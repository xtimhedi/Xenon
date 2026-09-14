using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Xenon.NodeSystem
{
    public partial class Node2D : Node
    {
        public Vector2 Position { get; set; } = Vector2.Zero;
        public float Rotation { get; set; } = 0f;
        public Vector2 Scale { get; set; } = Vector2.One; // Default to 1 to avoid zero-scale collapsing

        public Matrix4x4 GetLocalTransform()
        {
            return Matrix4x4.CreateScale(Scale.X, Scale.Y, 1f) *
                   Matrix4x4.CreateRotationZ(Rotation) *
                   Matrix4x4.CreateTranslation(Position.X, Position.Y, 0f);
        }

        public Matrix4x4 GetGlobalTransform()
        {
            if (Parent is Node2D parent2D)
            {
                // Local transform applies first, then parent's global transform
                return GetLocalTransform() * parent2D.GetGlobalTransform();
            }

            return GetLocalTransform();
        }
    }
}