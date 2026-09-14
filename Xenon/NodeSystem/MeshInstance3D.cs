using System;
using System.Numerics;
using Veldrid;
using Xenon.Context;

namespace Xenon.NodeSystem
{
    public class MeshInstance3D : Node3D, IDisposable
    {
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private DeviceBuffer _worldBuffer;
        private ResourceSet _resourceSet;
        private uint _indexCount;
        private bool _isInitialized;

        public Mesh Mesh { get; set; }
        public Texture Texture { get; set; }
        public TextureView TextureView { get; set; }
        public Sampler Sampler { get; set; }

        public override void Ready(RenderContext context)
        {
            InitializeGpuResources(context);
            base.Ready(context);
        }

        public void InitializeGpuResources(RenderContext context)
        {
            if (_isInitialized || Mesh == null) return;

            ResourceFactory factory = context.Device.ResourceFactory;

            uint[] indices = Mesh.indices;
            VertexPositionColorTexture[] vertices = Mesh.vertices;
            _indexCount = (uint)indices.Length;

            uint vertexByteSize = (uint)(vertices.Length * VertexPositionColorTexture.SizeInBytes);
            uint indexByteSize = (uint)(indices.Length * sizeof(uint));

            _vertexBuffer = factory.CreateBuffer(new BufferDescription(
                vertexByteSize, BufferUsage.VertexBuffer));

            _indexBuffer = factory.CreateBuffer(new BufferDescription(
                indexByteSize, BufferUsage.IndexBuffer));

            context.Device.UpdateBuffer(_vertexBuffer, 0, vertices);
            context.Device.UpdateBuffer(_indexBuffer, 0, indices);

            _worldBuffer = factory.CreateBuffer(new BufferDescription(
                64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            // Ensure every node creates its OWN distinct TextureView from the shared Texture
            if (TextureView == null)
            {
                Texture targetTexture = Texture ?? context.DefaultTexture;
                TextureView = factory.CreateTextureView(targetTexture);
            }

            // Create dedicated ResourceSet for THIS specific mesh instance
            _resourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                context.MatrixLayout,
                _worldBuffer,
                TextureView,
                Sampler ?? context.Device.Aniso4xSampler
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

                // Compute combined transformation matrix
                Matrix4x4 mvp = model * view * projection;

                context.CommandList.UpdateBuffer(_worldBuffer, 0, ref mvp);

                context.CommandList.SetPipeline(context.Pipeline);
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
            TextureView?.Dispose();
            _isInitialized = false;
        }
    }
}