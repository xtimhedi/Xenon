using System;
using System.Numerics;
using Xenon.Context;
using Xenon.CS;

namespace Xenon.NodeSystem
{
    public class Raycast3D : Collider3D
    {
        // Ray Configuration
        public Vector3 Origin { get; set; }
        public Vector3 Direction { get; set; } = new Vector3(0, 0, -1);
        public float MaxDistance { get; set; } = 1000f;

        // Hit Result Data
        public bool IsColliding { get; private set; }
        public Collider3D HitCollider { get; private set; }
        public Vector3 HitPoint { get; private set; }
        public float HitDistance { get; private set; }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            // Clear previous frame's hit data before the CollisionSystem processes intersections
            ClearHit();
        }

        public override bool Intersects(Collider3D other)
        {
            if (other is AABBCollider aabb)
            {
                if (CheckAABBIntersection(aabb, out float distance))
                {
                    RegisterHit(aabb, distance);
                    return true;
                }
            }
            else if (other is SphereCollider sphere)
            {
                if (CheckSphereIntersection(sphere, out float distance))
                {
                    RegisterHit(sphere, distance);
                    return true;
                }
            }

            return false;
        }

        private bool CheckAABBIntersection(AABBCollider aabb, out float distance)
        {
            distance = 0f;
            Vector3 min = aabb.WorldBounds.Min;
            Vector3 max = aabb.WorldBounds.Max;

            // X axis
            float tMin = (min.X - Origin.X) / Direction.X;
            float tMax = (max.X - Origin.X) / Direction.X;
            if (tMin > tMax) Swap(ref tMin, ref tMax);

            // Y axis
            float tyMin = (min.Y - Origin.Y) / Direction.Y;
            float tyMax = (max.Y - Origin.Y) / Direction.Y;
            if (tyMin > tyMax) Swap(ref tyMin, ref tyMax);

            if ((tMin > tyMax) || (tyMin > tMax)) return false;
            if (tyMin > tMin) tMin = tyMin;
            if (tyMax < tMax) tMax = tyMax;

            // Z axis
            float tzMin = (min.Z - Origin.Z) / Direction.Z;
            float tzMax = (max.Z - Origin.Z) / Direction.Z;
            if (tzMin > tzMax) Swap(ref tzMin, ref tzMax);

            if ((tMin > tzMax) || (tzMin > tMax)) return false;
            if (tzMin > tMin) tMin = tzMin;
            if (tzMax < tMax) tMax = tzMax;

            // Enforce max distance
            if (tMin < 0 || tMin > MaxDistance) return false;

            distance = tMin;
            return true;
        }

        private bool CheckSphereIntersection(SphereCollider sphere, out float distance)
        {
            distance = 0f;
            Vector3 offset = Origin - sphere.WorldCenter;

            float a = Direction.LengthSquared();
            float b = 2.0f * Vector3.Dot(offset, Direction);
            float c = offset.LengthSquared() - (sphere.Radius * sphere.Radius);

            float discriminant = (b * b) - (4 * a * c);

            if (discriminant < 0) return false;

            float sqrtDiscriminant = (float)Math.Sqrt(discriminant);
            float t1 = (-b - sqrtDiscriminant) / (2.0f * a);
            float t2 = (-b + sqrtDiscriminant) / (2.0f * a);

            float t = t1;
            if (t < 0) t = t2;

            if (t < 0 || t > MaxDistance) return false;

            distance = t;
            return true;
        }

        private void RegisterHit(Collider3D collider, float distance)
        {
            // Only overwrite if this collision is closer than a previously registered one this frame
            if (distance < HitDistance)
            {
                IsColliding = true;
                HitCollider = collider;
                HitDistance = distance;
                HitPoint = Origin + (Direction * distance);

                OnCollision?.Invoke(collider);
            }
        }

        public void ClearHit()
        {
            IsColliding = false;
            HitCollider = null;
            HitDistance = float.MaxValue;
            HitPoint = Vector3.Zero;
        }

        private void Swap(ref float a, ref float b)
        {
            float temp = a;
            a = b;
            b = temp;
        }
    }
}