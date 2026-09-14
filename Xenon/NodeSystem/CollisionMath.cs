using System;
using System.Numerics;

namespace Xenon.NodeSystem
{
    public static class CollisionMath
    {
        // Sphere vs Triangle
        public static bool SphereIntersectsTriangle(SphereCollider sphere, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 center = sphere.WorldCenter;
            Vector3 closest = ClosestPointOnTriangle(center, a, b, c);
            float distanceSquared = Vector3.DistanceSquared(center, closest);
            return distanceSquared <= (sphere.Radius * sphere.Radius);
        }

        // AABB vs Triangle (Separating Axis Theorem)
        public static bool AABBIntersectsTriangle(AABB aabb, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            // Center the AABB
            Vector3 center = (aabb.Max + aabb.Min) * 0.5f;
            Vector3 extents = (aabb.Max - aabb.Min) * 0.5f;

            // Move triangle to AABB origin
            v0 -= center;
            v1 -= center;
            v2 -= center;

            Vector3 f0 = v1 - v0;
            Vector3 f1 = v2 - v1;
            Vector3 f2 = v0 - v2;

            // 1. Test AABB axes (Face normals)
            if (Math.Max(Math.Max(v0.X, v1.X), v2.X) < -extents.X || Math.Min(Math.Min(v0.X, v1.X), v2.X) > extents.X) return false;
            if (Math.Max(Math.Max(v0.Y, v1.Y), v2.Y) < -extents.Y || Math.Min(Math.Min(v0.Y, v1.Y), v2.Y) > extents.Y) return false;
            if (Math.Max(Math.Max(v0.Z, v1.Z), v2.Z) < -extents.Z || Math.Min(Math.Min(v0.Z, v1.Z), v2.Z) > extents.Z) return false;

            // 2. Test Triangle normal
            Vector3 n = Vector3.Cross(f0, f1);
            float r = extents.X * Math.Abs(n.X) + extents.Y * Math.Abs(n.Y) + extents.Z * Math.Abs(n.Z);
            if (Math.Abs(Vector3.Dot(n, v0)) > r) return false;

            // 3. Test 9 Edge Cross Products
            if (!AxisTest(extents, v0, v1, v2, new Vector3(0, -f0.Z, f0.Y))) return false;
            if (!AxisTest(extents, v0, v1, v2, new Vector3(0, -f1.Z, f1.Y))) return false;
            if (!AxisTest(extents, v0, v1, v2, new Vector3(0, -f2.Z, f2.Y))) return false;

            if (!AxisTest(extents, v0, v1, v2, new Vector3(f0.Z, 0, -f0.X))) return false;
            if (!AxisTest(extents, v0, v1, v2, new Vector3(f1.Z, 0, -f1.X))) return false;
            if (!AxisTest(extents, v0, v1, v2, new Vector3(f2.Z, 0, -f2.X))) return false;

            if (!AxisTest(extents, v0, v1, v2, new Vector3(-f0.Y, f0.X, 0))) return false;
            if (!AxisTest(extents, v0, v1, v2, new Vector3(-f1.Y, f1.X, 0))) return false;
            if (!AxisTest(extents, v0, v1, v2, new Vector3(-f2.Y, f2.X, 0))) return false;

            return true;
        }

        private static bool AxisTest(Vector3 extents, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 axis)
        {
            float p0 = Vector3.Dot(v0, axis);
            float p1 = Vector3.Dot(v1, axis);
            float p2 = Vector3.Dot(v2, axis);

            float r = extents.X * Math.Abs(axis.X) + extents.Y * Math.Abs(axis.Y) + extents.Z * Math.Abs(axis.Z);
            return !(Math.Max(Math.Max(p0, p1), p2) < -r || Math.Min(Math.Min(p0, p1), p2) > r);
        }

        private static Vector3 ClosestPointOnTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 ap = p - a;

            float d1 = Vector3.Dot(ab, ap);
            float d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0.0f && d2 <= 0.0f) return a;

            Vector3 bp = p - b;
            float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0.0f && d4 <= d3) return b;

            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
            {
                float v = d1 / (d1 - d3);
                return a + v * ab;
            }

            Vector3 cp = p - c;
            float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0.0f && d5 <= d6) return c;

            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
            {
                float w = d2 / (d2 - d6);
                return a + w * ac;
            }

            float va = d3 * d6 - d5 * d4;
            if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
            {
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                return b + w * (c - b);
            }

            float denom = 1.0f / (va + vb + vc);
            float vn = vb * denom;
            float wn = vc * denom;
            return a + ab * vn + ac * wn;
        }
    }
}