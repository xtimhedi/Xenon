using System;
using Xenon.Context;
using Xenon.CS;

namespace Xenon.NodeSystem
{
    public abstract class Collider3D : Node
    {
        public Action<Collider3D> OnCollision;

        public override void Ready(RenderContext context)
        {
            base.Ready(context);
            CollisionSystem.Register(this);
        }

        new public void QueueFree()
        {
            CollisionSystem.Unregister(this);
            base.QueueFree();
        }

        // Every child shape must implement how it collides with generic others
        public abstract bool Intersects(Collider3D other);
    }
}