using System;
using System.Numerics;
using NeoVeldrid;
using Xenon.Context;

namespace Xenon.NodeSystem
{
    public class GridNode3D : Node3D, IDisposable
    {
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private DeviceBuffer _worldBuffer;
        private ResourceSet _resourceSet;
        private Texture _gridTexture;
        private TextureView _gridTextureView;
        private uint _indexCount;
        private bool _isInitialized;

        // Custom parameters to control grid sizing
        public float GridSize { get; set; } = 100f;
        public float GridSpacing { get; set; } = 1.0f;

        public override void Ready(RenderContext context)
        {
            InitializeGpuResources(context);
            base.Ready(context);
        }

        public void InitializeGpuResources(RenderContext context)
        {
            if (_isInitialized) return;

            ResourceFactory factory = context.Device.ResourceFactory;

            // 1. Geometry data mapped out on the flat XZ ground plane
            float halfSize = GridSize / 2f;
            float maxUV = GridSize / GridSpacing;

            // Vertex Position, Color (White), and UV repetitions
            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[]
            {
                new VertexPositionColorTexture(new Vector3(-halfSize, 0f, -halfSize), RgbaFloat.White, new Vector2(0f, 0f)),
                new VertexPositionColorTexture(new Vector3( halfSize, 0f, -halfSize), RgbaFloat.White, new Vector2(maxUV, 0f)),
                new VertexPositionColorTexture(new Vector3( halfSize, 0f,  halfSize), RgbaFloat.White, new Vector2(maxUV, maxUV)),
                new VertexPositionColorTexture(new Vector3(-halfSize, 0f,  halfSize), RgbaFloat.White, new Vector2(0f, maxUV))
            };

            uint[] indices = new uint[] { 0, 1, 2, 0, 2, 3 };
            _indexCount = (uint)indices.Length;

            _vertexBuffer = factory.CreateBuffer(new BufferDescription(
                (uint)(vertices.Length * VertexPositionColorTexture.SizeInBytes), BufferUsage.VertexBuffer));
            _indexBuffer = factory.CreateBuffer(new BufferDescription(
                (uint)(indices.Length * sizeof(uint)), BufferUsage.IndexBuffer));

            context.Device.UpdateBuffer(_vertexBuffer, 0, vertices);
            context.Device.UpdateBuffer(_indexBuffer, 0, indices);

            _worldBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            // 2. Build a high-density, sharp 2D line pattern asset manually
            uint texDim = 64;
            uint lineWidth = 2;
            RgbaByte[] pixelData = new RgbaByte[texDim * texDim];

            for (uint y = 0; y < texDim; y++)
            {
                for (uint x = 0; x < texDim; x++)
                {
                    // Tint borders to form grid borders
                    if (x < lineWidth || y < lineWidth || x >= texDim - lineWidth || y >= texDim - lineWidth)
                    {
                        pixelData[y * texDim + x] = new RgbaByte(56, 56, 56, 190); // Solid gray line
                    }
                    else
                    {
                        pixelData[y * texDim + x] = new RgbaByte(0, 0, 0, 0); // Transparent void space
                    }
                }
            }

            // 3. Register Texture inside context pipeline structures
            _gridTexture = factory.CreateTexture(TextureDescription.Texture2D(
                texDim, texDim, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
            context.Device.UpdateTexture(_gridTexture, pixelData, 0, 0, 0, texDim, texDim, 1, 0, 0);

            _gridTextureView = factory.CreateTextureView(_gridTexture);

            _resourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                context.MatrixLayout,
                _worldBuffer,
                _gridTextureView,
                context.Device.LinearSampler
            ));

            _isInitialized = true;
        }

        public override void Draw(RenderContext context)
        {
            if (!_isInitialized)
            {
                InitializeGpuResources(context);
            }

            if (_vertexBuffer != null && context.CurrentCamera != null)
            {
                Matrix4x4 model = GetGlobalTransform();
                Matrix4x4 view = context.CurrentCamera.GetViewMatrix();
                Matrix4x4 projection = context.CurrentCamera.GetProjectionMatrix();

                Matrix4x4 mvp = model * view * projection;
                context.CommandList.UpdateBuffer(_worldBuffer, 0, ref mvp);

                // CRITICAL FIX: Route through Pipeline2D slot to use alpha blending and bypass backface culling!
                context.CommandList.SetPipeline(context.Pipeline2D);

                context.CommandList.SetGraphicsResourceSet(0, _resourceSet);
                context.CommandList.SetVertexBuffer(0, _vertexBuffer);
                context.CommandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt32);

                context.CommandList.DrawIndexed(_indexCount, 1, 0, 0, 0);
            }

            base.Draw(context);
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _worldBuffer?.Dispose();
            _resourceSet?.Dispose();
            _gridTextureView?.Dispose();
            _gridTexture?.Dispose();
            _isInitialized = false;
        }
    }
}
