using System;
using System.Numerics;
using NeoVeldrid;
using Xenon.Context;

namespace Xenon.NodeSystem
{
    public class SpriteNode : Node2D, IDisposable
    {
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private DeviceBuffer _worldBuffer;
        private ResourceSet _resourceSet;

        public void SetMeshData(RenderContext context, VertexPositionColor[] vertices, ushort[] indices)
        {
            ResourceFactory factory = context.Device.ResourceFactory;

            _vertexBuffer = factory.CreateBuffer(new BufferDescription(
                (uint)(vertices.Length * VertexPositionColor.SizeInBytes), BufferUsage.VertexBuffer));
            _indexBuffer = factory.CreateBuffer(new BufferDescription(
                (uint)(indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));

            context.Device.UpdateBuffer(_vertexBuffer, 0, vertices);
            context.Device.UpdateBuffer(_indexBuffer, 0, indices);

            _worldBuffer = factory.CreateBuffer(new BufferDescription(
                64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            _resourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                context.MatrixLayout, _worldBuffer));
        }

        public override void Draw(RenderContext context)
        {
            if (_vertexBuffer != null)
            {
                Matrix4x4 world = GetGlobalTransform();
                context.CommandList.UpdateBuffer(_worldBuffer, 0, ref world);

                context.CommandList.SetPipeline(context.Pipeline);
                context.CommandList.SetGraphicsResourceSet(0, _resourceSet);
                context.CommandList.SetVertexBuffer(0, _vertexBuffer);
                context.CommandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

                context.CommandList.DrawIndexed(6, 1, 0, 0, 0);
            }

            base.Draw(context);
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _worldBuffer?.Dispose();
            _resourceSet?.Dispose();
        }
    }
}