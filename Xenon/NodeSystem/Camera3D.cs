using System.Numerics;
using Xenon.Context;

namespace Xenon.NodeSystem
{
    public class Camera3D : Node3D
    {
        public float FieldOfView { get; set; } = 1.05f; // ~60 degrees
        public float AspectRatio { get; set; } = 16f / 9f;
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 99999999f;
        public override void Ready(RenderContext context)
        {
            base.Ready(context);
            context.CurrentCamera = this;
            XEN.Logger.Log($"Camera added! Current camera is {this.ToString()}", "Camera");
        }
        public Matrix4x4 GetViewMatrix()
        {
            Matrix4x4.Invert(GetGlobalTransform(), out Matrix4x4 view);
            return view;
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
        }
    }
}