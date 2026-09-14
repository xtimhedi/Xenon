using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Xenon
{
    public struct VertexPositionColor
    {
        public Vector3 Position; // This is the position, in normalized device coordinates.
        public RgbaFloat Color; // This is the color of the vertex.
        public VertexPositionColor(Vector3 position, RgbaFloat color)
        {
            Position = position;
            Color = color;
        }
        public const uint SizeInBytes = 28;
    }
}
