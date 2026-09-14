using System;
using System.Numerics;
using Vortice.Mathematics;
using Xenon.Context;
using Xenon.NodeSystem;

namespace TestGame
{
    public partial class Camera : Camera3D
    {
        private float _headBobTimer = 0f;
        private float _currentBobOffset = 0f;
        private const float BobFrequency = 8.0f;
        private const float BobAmplitude = 0.25f;

        private Vector3 _lastParentPosition;
        private bool _isInitialized = false;

        private Raycast3D raycast = new Raycast3D();

        public override void Ready(RenderContext context)
        {
            AddChild(raycast);
            base.Ready(context);
            if (Parent is Node3D parent3D)
            {
                _lastParentPosition = parent3D.Position;
                _isInitialized = true;
            }
            
        }

        public override void Update(float deltaTime)
        {
            // LOG FIRST: Read the result of the previous frame's collision pass here.
            Xenon.XEN.Logger.Log(raycast.IsColliding.ToString(), "Player");

            if (Parent is Node3D player)
            {
                if (!_isInitialized)
                {
                    _lastParentPosition = player.Position;
                    _isInitialized = true;
                }

                Vector3 currentPos = player.Position;
                Vector2 horizontalMovement = new Vector2(currentPos.X - _lastParentPosition.X, currentPos.Z - _lastParentPosition.Z);
                float speed = horizontalMovement.Length() / Math.Max(deltaTime, 0.0001f);
                float baseEyeHeight = 0.6f;

                if (speed > 0.5f)
                {
                    _headBobTimer += deltaTime * BobFrequency;
                    _currentBobOffset = MathF.Abs(MathF.Sin(_headBobTimer)) * BobAmplitude;
                }
                else
                {
                    _headBobTimer = 0f;
                    _currentBobOffset = MathHelper.Lerp(_currentBobOffset, 0f, 15f * deltaTime);
                }

                // Update local Camera Position
                Position = new Vector3(0, baseEyeHeight + _currentBobOffset, 0);

                _lastParentPosition = currentPos;
            }

            // EXTRACT TRUE GLOBAL TRANSFORM: Ensure Origin is perfectly aligned in world space.
            Matrix4x4.Decompose(GetGlobalTransform(), out Vector3 scale, out Quaternion globalRotation, out Vector3 globalPosition);
            raycast.Origin = globalPosition;

            // NORMALIZE GLOBAL FORWARD: Math will fail if the direction vector length isn't exactly 1.
            raycast.Direction = Vector3.Normalize(Vector3.Transform(new Vector3(0, 0, -1), globalRotation));

            // BASE UPDATE LAST: This calls Raycast3D.Update(), which clears the hit data 
            // to prepare for the engine's upcoming Collision System pass.
            base.Update(deltaTime);
        }
    }
}