using System;
using System.Collections.Generic;
using System.Numerics;
using NeoVeldrid;
using Xenon.Context;
using Xenon.NodeSystem;

namespace Editor
{
    public class EditorCamera3D : Camera3D
    {
        // Settings
        public float MoveSpeed { get; set; } = 15f;
        public float LookSpeed { get; set; } = 0.003f;
        public float PanSpeed { get; set; } = 0.02f;
        public float ZoomSpeed { get; set; } = 2.0f;

        // Camera State
        private float _yaw = 0f;
        private float _pitch = 0f;

        // Input State Tracking
        private Vector2 _lastMousePos;
        private bool _isFirstFrame = true;
        private HashSet<Key> _heldKeys = new HashSet<Key>();
        private bool _isRightMouseDown = false;
        private bool _isMiddleMouseDown = false;

        public override void Ready(RenderContext context)
        {
            base.Ready(context);

            // Note: If you set initial rotation in Program.cs, you should extract 
            // the initial pitch/yaw here so the camera doesn't snap on first click.
        }

        public override void Input(InputSnapshot snapshot)
        {
            base.Input(snapshot);

            // 1. Track persistent button states across frames
            foreach (KeyEvent ke in snapshot.KeyEvents)
            {
                if (ke.Down) _heldKeys.Add(ke.Key);
                else _heldKeys.Remove(ke.Key);
            }

            foreach (MouseEvent me in snapshot.MouseEvents)
            {
                if (me.MouseButton == MouseButton.Right) _isRightMouseDown = me.Down;
                if (me.MouseButton == MouseButton.Middle) _isMiddleMouseDown = me.Down;
            }

            // 2. Calculate Mouse Delta
            Vector2 currentMousePos = snapshot.MousePosition;
            if (_isFirstFrame)
            {
                _lastMousePos = currentMousePos;
                _isFirstFrame = false;
            }
            Vector2 mouseDelta = currentMousePos - _lastMousePos;
            _lastMousePos = currentMousePos;

            // 3. Process Fly Mode Rotation (Right-Click Drag)
            if (_isRightMouseDown)
            {
                _yaw -= mouseDelta.X * LookSpeed;
                _pitch -= mouseDelta.Y * LookSpeed;

                // Clamp pitch to prevent the camera from flipping upside down
                _pitch = Math.Clamp(_pitch, -1.57f, 1.57f); // ~90 degrees

                Rotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0f);
            }

            // 4. Process Panning (Middle-Click Drag)
            if (_isMiddleMouseDown)
            {
                // Calculate local right and up axes based on current rotation
                Vector3 right = Vector3.Transform(Vector3.UnitX, Rotation);
                Vector3 up = Vector3.Transform(Vector3.UnitY, Rotation);

                // Move opposite to mouse movement to drag the "world"
                Position -= right * mouseDelta.X * PanSpeed;
                Position += up * mouseDelta.Y * PanSpeed;
            }

            // 5. Process Zoom (Scroll Wheel)
            if (snapshot.WheelDelta != 0)
            {
                Vector3 forward = Vector3.Transform(-Vector3.UnitZ, Rotation);
                Position += forward * snapshot.WheelDelta * ZoomSpeed;
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            // 6. Process Continuous Movement (WASD) only while in Fly Mode
            if (_isRightMouseDown)
            {
                Vector3 forward = Vector3.Transform(-Vector3.UnitZ, Rotation);
                Vector3 right = Vector3.Transform(Vector3.UnitX, Rotation);
                Vector3 up = Vector3.Transform(Vector3.UnitY, Rotation);

                Vector3 movement = Vector3.Zero;

                if (_heldKeys.Contains(Key.W)) movement += forward;
                if (_heldKeys.Contains(Key.S)) movement -= forward;
                if (_heldKeys.Contains(Key.A)) movement -= right;
                if (_heldKeys.Contains(Key.D)) movement += right;
                if (_heldKeys.Contains(Key.E)) movement += up;
                if (_heldKeys.Contains(Key.Q)) movement -= up;

                if (movement.LengthSquared() > 0)
                {
                    movement = Vector3.Normalize(movement);

                    // Hold Shift to move faster
                    float currentSpeed = _heldKeys.Contains(Key.ShiftLeft) ? MoveSpeed * 2.5f : MoveSpeed;
                    Position += movement * currentSpeed * deltaTime;
                }
            }
        }
    }
}