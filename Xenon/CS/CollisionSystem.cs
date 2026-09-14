using System.Collections.Generic;
using Xenon.CS;
using Xenon.NodeSystem;

namespace Xenon.CS
{
    public static class CollisionSystem
    {
        private static List<Collider3D> _colliders = new List<Collider3D>();

        public static void Register(Collider3D collider)
        {
            if (!_colliders.Contains(collider))
                _colliders.Add(collider);
        }

        public static void Unregister(Collider3D collider)
        {
            _colliders.Remove(collider);
        }

        public static void ProcessCollisions()
        {
            // O(n^2) broad check. 
            for (int i = 0; i < _colliders.Count; i++)
            {
                for (int j = i + 1; j < _colliders.Count; j++)
                {
                    Collider3D a = _colliders[i];
                    Collider3D b = _colliders[j];

                    // Don't collide objects with themselves or objects attached to the same parent (compound colliders)
                    if (a.Parent == b.Parent) continue;

                    if (a.Intersects(b))
                    {
                        a.OnCollision?.Invoke(b);
                        b.OnCollision?.Invoke(a);
                    }
                }
            }
        }
    }
}