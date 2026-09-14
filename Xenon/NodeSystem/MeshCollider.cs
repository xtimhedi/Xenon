using System.Numerics;
using Xenon.Context;

namespace Xenon.NodeSystem
{
    public class MeshCollider : Collider3D
    {
        private Mesh _mesh;
        private AABB _localBounds;
        public AABB WorldBounds { get; private set; }

        public MeshCollider(Mesh mesh)
        {
            _mesh = mesh;
            CalculateLocalBounds();
        }

        private void CalculateLocalBounds()
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach (var vertex in _mesh.vertices)
            {
                min = Vector3.Min(min, vertex.Position);
                max = Vector3.Max(max, vertex.Position);
            }

            _localBounds = new AABB(min, max);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            // shift bounds by parent position
            if (Parent is Node3D parentNode)
            {
                WorldBounds = new AABB(
                    _localBounds.Min + parentNode.Position,
                    _localBounds.Max + parentNode.Position
                );
            }
        }

        public override bool Intersects(Collider3D other)
        {
            // BROAD PHASE: prevent m/m collisions, gets REALLY expensive
            // FIRE BROADSIDE
            if (other is MeshCollider)
            {
                XEN.Logger.LogWarn("Mesh vs Mesh collision is not supported due to performance constraints.", "Physics");
                return false;
            }

            Vector3 parentPos = (Parent as Node3D)?.Position ?? Vector3.Zero;

            // NARROW PHASE: check tris
            if (other is AABBCollider otherAABB)
            {
                // bp the AABB first
                if (!WorldBounds.Intersects(otherAABB.WorldBounds)) return false;

                for (int i = 0; i < _mesh.indices.Length; i += 3)
                {
                    Vector3 v0 = _mesh.vertices[_mesh.indices[i]].Position + parentPos;
                    Vector3 v1 = _mesh.vertices[_mesh.indices[i + 1]].Position + parentPos;
                    Vector3 v2 = _mesh.vertices[_mesh.indices[i + 2]].Position + parentPos;

                    if (CollisionMath.AABBIntersectsTriangle(otherAABB.WorldBounds, v0, v1, v2))
                        return true;
                }
            }
            else if (other is SphereCollider otherSphere)
            {
                // bp Sphere vs AABB check first
                Vector3 closestPoint = Vector3.Clamp(otherSphere.WorldCenter, WorldBounds.Min, WorldBounds.Max);
                if (Vector3.DistanceSquared(otherSphere.WorldCenter, closestPoint) > (otherSphere.Radius * otherSphere.Radius))
                    return false;

                for (int i = 0; i < _mesh.indices.Length; i += 3)
                {
                    Vector3 v0 = _mesh.vertices[_mesh.indices[i]].Position + parentPos;
                    Vector3 v1 = _mesh.vertices[_mesh.indices[i + 1]].Position + parentPos;
                    Vector3 v2 = _mesh.vertices[_mesh.indices[i + 2]].Position + parentPos;

                    if (CollisionMath.SphereIntersectsTriangle(otherSphere, v0, v1, v2))
                        return true;
                }
            }

            return false;
        }
    }
}