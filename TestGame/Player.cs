using System;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Vortice.Mathematics;
using Xenon.Context;
using Xenon.NodeSystem;

namespace TestGame
{
    public class Player : Node3D
    {
        private Vector3 _previousPosition;
        private Vector3 _velocity = Vector3.Zero;
        private bool _isGrounded = false;

        // Physics Tuning
        private const float Gravity = -20.0f;
        private const float JumpForce = 8.0f;
        private const float MoveSpeed = 16.0f;
        private const float GroundFriction = 15.0f;
        private const float AirFriction = 2.0f;

        private float _yaw = 0f;
        private float _targetYaw = 0f;
        private bool _isMouseLocked = false;
        private RenderContext _renderContext;
        private SphereCollider _collider;

        private const float MouseSensitivity = 0.002f;
        private const float CameraSmoothness = 25.0f;

        private bool _moveForward, _moveBackward, _moveLeft, _moveRight, _jumpPressed;

        public override void Ready(RenderContext context)
        {
            Globals.player = this;
            _renderContext = context;

            // The body owns the physical collider now
            _collider = new SphereCollider();
            _collider.Radius = 0.5f;
            _collider.OnCollision += HandleCollision;
            AddChild(_collider);

            Position = new Vector3(0, 2f, 5f);
            base.Ready(context);
        }

        private void HandleCollision(Collider3D other)
        {
            if (other is AABBCollider box)
            {
                if (_previousPosition.Y - _collider.Radius >= box.WorldBounds.Max.Y - 0.1f)
                {
                    _isGrounded = true;
                    _velocity.Y = 0f;
                    Position = new Vector3(Position.X, box.WorldBounds.Max.Y + _collider.Radius, Position.Z);
                }
                else
                {
                    Position = new Vector3(_previousPosition.X, Position.Y, _previousPosition.Z);
                    if (_velocity.Y > 0)
                    {
                        _velocity.Y = 0;
                        Position = new Vector3(Position.X, _previousPosition.Y, Position.Z);
                    }
                }
            }
            else
            {
                Position = _previousPosition;
            }
        }

        public override void Input(InputSnapshot snapshot)
        {
            base.Input(snapshot);

            if (!_isMouseLocked && snapshot.MouseEvents.Count > 0)
                LockMouse();

            foreach (KeyEvent ke in snapshot.KeyEvents)
            {
                switch (ke.Key)
                {
                    case Key.W: _moveForward = ke.Down; break;
                    case Key.S: _moveBackward = ke.Down; break;
                    case Key.A: _moveLeft = ke.Down; break;
                    case Key.D: _moveRight = ke.Down; break;
                    case Key.Space: if (ke.Down && _isGrounded) _jumpPressed = true; break;
                    case Key.Escape: if (ke.Down) UnlockMouse(); break;
                }
            }

            if (_isMouseLocked && _renderContext?.Window is Sdl2Window window)
            {
                Vector2 mouseDelta = window.MouseDelta;
                if (mouseDelta != Vector2.Zero)
                {
                    _targetYaw -= mouseDelta.X * MouseSensitivity;
                }
            }
        }

        public override void Update(float deltaTime)
        {
            _previousPosition = Position;

            // Smooth rotation
            float lerpFactor = 1.0f - MathF.Exp(-CameraSmoothness * deltaTime);
            _yaw = MathHelper.Lerp(_yaw, _targetYaw, lerpFactor);
            Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, _yaw);

            // Movement relative to body rotation
            Vector3 forward = Vector3.Transform(-Vector3.UnitZ, Rotation);
            Vector3 right = Vector3.Transform(Vector3.UnitX, Rotation);

            Vector3 moveInput = Vector3.Zero;
            if (_moveForward) moveInput += forward;
            if (_moveBackward) moveInput -= forward;
            if (_moveRight) moveInput += right;
            if (_moveLeft) moveInput -= right;

            if (moveInput.LengthSquared() > 0)
                moveInput = Vector3.Normalize(moveInput);

            Vector3 targetVelocity = moveInput * MoveSpeed;
            float friction = _isGrounded ? GroundFriction : AirFriction;

            _velocity.X = MathHelper.Lerp(_velocity.X, targetVelocity.X, friction * deltaTime);
            _velocity.Z = MathHelper.Lerp(_velocity.Z, targetVelocity.Z, friction * deltaTime);
            _velocity.Y += Gravity * deltaTime;

            if (_jumpPressed)
            {
                _velocity.Y = JumpForce;
                _jumpPressed = false;
                _isGrounded = false;
            }

            Position += _velocity * deltaTime;
            _isGrounded = false;

            base.Update(deltaTime);
        }

        private void LockMouse()
        {
            Sdl2Native.SDL_SetRelativeMouseMode(true);
            _isMouseLocked = true;
        }

        private void UnlockMouse()
        {
            Sdl2Native.SDL_SetRelativeMouseMode(false);
            _isMouseLocked = false;
        }
    }
}