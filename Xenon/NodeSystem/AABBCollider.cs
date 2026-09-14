using System.Numerics;

namespace Xenon.NodeSystem
{
    public class AABBCollider : Collider3D
    {
        public AABB LocalBounds;
        public AABB WorldBounds { get; private set; }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            // Assumes your 3D nodes inherit from a Node3D class that has a Position
            if (Parent is Node3D parentNode)
            {
                WorldBounds = new AABB(
                    LocalBounds.Min + parentNode.Position,
                    LocalBounds.Max + parentNode.Position
                );
            }
        }

        public override bool Intersects(Collider3D other)
        {
            if (other is AABBCollider otherAABB)
            {
                return WorldBounds.Intersects(otherAABB.WorldBounds);
            }
            else if (other is SphereCollider otherSphere)
            {
                // Box vs Sphere Math
                Vector3 closestPoint = Vector3.Clamp(otherSphere.WorldCenter, WorldBounds.Min, WorldBounds.Max);
                float distanceSquared = Vector3.DistanceSquared(otherSphere.WorldCenter, closestPoint);
                return distanceSquared <= (otherSphere.Radius * otherSphere.Radius);
            }
            return false;
        }
    }

    public class SphereCollider : Collider3D
    {
        public float Radius;
        public Vector3 LocalCenter = Vector3.Zero;
        public Vector3 WorldCenter { get; private set; }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            if (Parent is Node3D parentNode)
            {
                WorldCenter = parentNode.Position + LocalCenter;
            }
        }

        public override bool Intersects(Collider3D other)
        {
            if (other is SphereCollider otherSphere)
            {
                // Sphere vs Sphere Math
                float distanceSquared = Vector3.DistanceSquared(WorldCenter, otherSphere.WorldCenter);
                float radiusSum = Radius + otherSphere.Radius;
                return distanceSquared <= (radiusSum * radiusSum);
            }
            else if (other is AABBCollider otherAABB)
            {
                // Delegate back to AABB's math for Sphere vs Box
                return otherAABB.Intersects(this);
            }
            return false;
        }
    }
}