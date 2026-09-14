using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;
using Xenon.Context;

namespace Xenon.NodeSystem
{
    public struct VertexPositionColorUV
    {
        public Vector3 Position;
        public RgbaFloat Color;
        public Vector2 UV;

        public VertexPositionColorUV(Vector3 position, RgbaFloat color, Vector2 uv)
        {
            Position = position;
            Color = color;
            UV = uv;
        }
    }

    public class Sprite2D : Node2D, IDisposable
    {
        public Texture Texture { get; set; }

        private TextureView _textureView;
        private Sampler _sampler;
        private ResourceSet _resourceSet;
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private DeviceBuffer _uniformBuffer;

        private bool _isInitialized = false;

        public override void Ready(RenderContext context)
        {
            InitializeGpuResources(context);
            base.Ready(context);
            
        }

        private void InitializeGpuResources(RenderContext ctx)
        {
            if (_isInitialized) return;

            ResourceFactory factory = ctx.Device.ResourceFactory;

            if (Texture != null)
            {
                _textureView = factory.CreateTextureView(Texture);
            }
            else
            {
                _textureView = ctx.DefaultTextureView;
            }

            _sampler = factory.CreateSampler(new SamplerDescription
            {
                AddressModeU = SamplerAddressMode.Clamp,
                AddressModeV = SamplerAddressMode.Clamp,
                AddressModeW = SamplerAddressMode.Clamp,
                Filter = SamplerFilter.MinLinear_MagLinear_MipLinear,
                LodBias = 0,
                MinimumLod = 0,
                MaximumLod = 0,
                MaximumAnisotropy = 0
            });

            _uniformBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            // Upright UVs mapped to screen-space quads
            VertexPositionColorUV[] vertices = new[]
            {
                new VertexPositionColorUV(new Vector3(-0.5f, -0.5f, 0.0f), RgbaFloat.White, new Vector2(0f, 0f)), // Top-Left
                new VertexPositionColorUV(new Vector3( 0.5f, -0.5f, 0.0f), RgbaFloat.White, new Vector2(1f, 0f)), // Top-Right
                new VertexPositionColorUV(new Vector3( 0.5f,  0.5f, 0.0f), RgbaFloat.White, new Vector2(1f, 1f)), // Bottom-Right
                new VertexPositionColorUV(new Vector3(-0.5f,  0.5f, 0.0f), RgbaFloat.White, new Vector2(0f, 1f))  // Bottom-Left
            };

            ushort[] indices = { 0, 1, 2, 0, 2, 3 };

            uint vertexBufferSize = (uint)(vertices.Length * Unsafe.SizeOf<VertexPositionColorUV>());
            uint indexBufferSize = (uint)(indices.Length * sizeof(ushort));

            _vertexBuffer = factory.CreateBuffer(new BufferDescription(vertexBufferSize, BufferUsage.VertexBuffer));
            _indexBuffer = factory.CreateBuffer(new BufferDescription(indexBufferSize, BufferUsage.IndexBuffer));

            ctx.Device.UpdateBuffer(_vertexBuffer, 0, vertices);
            ctx.Device.UpdateBuffer(_indexBuffer, 0, indices);

            _resourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                ctx.MatrixLayout,
                _uniformBuffer,
                _textureView,
                _sampler
            ));

            _isInitialized = true;
        }

        public override void Draw(RenderContext context)
        {
            base.Draw(context);

            if (!_isInitialized)
            {
                InitializeGpuResources(context);
            }

            float width = context.Device.SwapchainFramebuffer.Width;
            float height = context.Device.SwapchainFramebuffer.Height;

            // 2D Screen-space orthographic projection
            Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(
                left: 0f,
                right: width,
                bottom: height,
                top: 0f,
                zNearPlane: -1f,
                zFarPlane: 1f
            );

            Matrix4x4 model = GetGlobalTransform();
            Matrix4x4 mvp = model * projection;

            context.CommandList.UpdateBuffer(_uniformBuffer, 0, ref mvp);

            // Execute draw using dedicated 2D Alpha pipeline
            context.CommandList.SetPipeline(context.Pipeline2D);
            context.CommandList.SetGraphicsResourceSet(0, _resourceSet);
            context.CommandList.SetVertexBuffer(0, _vertexBuffer);
            context.CommandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            context.CommandList.DrawIndexed(6, 1, 0, 0, 0);
        }

        public void Dispose()
        {
            _resourceSet?.Dispose();
            if (Texture != null)
            {
                _textureView?.Dispose();
            }
            _sampler?.Dispose();
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _uniformBuffer?.Dispose();
        }
    }
}