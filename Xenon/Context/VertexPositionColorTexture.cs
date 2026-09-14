using System.Numerics;
using NeoVeldrid;

namespace Xenon.Context
{
    public struct VertexPositionColorTexture
    {
        public Vector3 Position;
        public RgbaFloat Color;
        public Vector2 UV;

        public const uint SizeInBytes = 36; // 12 + 16 + 8 bytes

        public VertexPositionColorTexture(Vector3 position, RgbaFloat color, Vector2 uv)
        {
            Position = position;
            Color = color;
            UV = uv;
        }
    }
}