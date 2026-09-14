using System.Numerics;
using Xenon.Context;

namespace Xenon.NodeSystem
{
    public class Node3D : Node
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Vector3 Scale { get; set; } = Vector3.One;

        // Local directional vectors
        public Vector3 Forward => Vector3.Transform(new Vector3(0, 0, -1), Rotation);
        public Vector3 Right => Vector3.Transform(new Vector3(1, 0, 0), Rotation);
        public Vector3 Up => Vector3.Transform(new Vector3(0, 1, 0), Rotation);

        // Global directional vectors
        public Vector3 GlobalForward
        {
            get
            {
                Matrix4x4.Decompose(GetGlobalTransform(), out _, out Quaternion globalRot, out _);
                return Vector3.Transform(new Vector3(0, 0, -1), globalRot);
            }
        }

        public Matrix4x4 GetLocalTransform()
        {
            return Matrix4x4.CreateScale(Scale) *
                   Matrix4x4.CreateFromQuaternion(Rotation) *
                   Matrix4x4.CreateTranslation(Position);
        }

        public Matrix4x4 GetGlobalTransform()
        {
            if (Parent is Node3D parent3D)
                return GetLocalTransform() * parent3D.GetGlobalTransform();
            return GetLocalTransform();
        }
    }
}