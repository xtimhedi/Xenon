using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xenon.NodeSystem;

namespace TestGame
{
    public class CameraAnchor : Node3D
    {
        public float RotationSpeed { get; set; } = 1.5f;

        public override void Update(float deltaTime)
        {
            // 1. Calculate rotation delta for this frame using deltaTime
            float yawAmount = RotationSpeed * deltaTime;
            Quaternion deltaRotation = Quaternion.CreateFromYawPitchRoll(yawAmount, 0f, 0f);

            // 2. Combine rotations using Multiplication (*), NOT Addition (+)
            // (Order matters: deltaRotation * Rotation rotates around global axis;
            //  Rotation * deltaRotation rotates around local axis)
            Rotation = deltaRotation * Rotation;

            base.Update(deltaTime);
        }
    }
}
