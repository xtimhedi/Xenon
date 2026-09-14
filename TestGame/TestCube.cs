using System.Numerics;
using NeoVeldrid;
using Xenon.Context;
using Xenon.NodeSystem;

namespace TestGame
{
    public class MovableCube : MeshInstance3D
    {
        private AABBCollider _collider;

        public override void Ready(RenderContext context)
        {
            // Setup the collider FIRST
            _collider = new AABBCollider();

            if (Mesh != null)
            {
                AABB bounds = Mesh.CalculateBounds();
                _collider.LocalBounds = new AABB(bounds.Min * Scale, bounds.Max * Scale);
            }
            else
            {
                _collider.LocalBounds = new AABB(new Vector3(-0.5f), new Vector3(0.5f));
            }

            AddChild(_collider);

            // Call base.Ready() LAST so the new collider gets registered to the CollisionSystem
            base.Ready(context);
        }
    }
}