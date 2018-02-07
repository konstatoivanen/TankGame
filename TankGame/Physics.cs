using System;
using System.Collections.Generic;
using OpenTK;

namespace Utils.Physics
{
    public enum     ColliderType
    {
        Circle,
        Box,
        Mesh
    }
    [Flags]
    public enum     PhysicsLayer
    {
        Default,
        Player1,
        Player2        
    }
    public struct   RayCastHit
    {
        public ContactPoint cp;
        public Collider other;

        public RayCastHit(ContactPoint _cp, Collider _other)
        {
            cp = _cp;
            other = _other;
        }
    }

    public class    Collider
    {
        public ColliderType Type;
        public PhysicsLayer Layer;
        public Vector2 center;
        public Vector2 extents;
        public float radius;
        public Mesh mesh;
        public BaseObject parent;

        //circle
        public Collider(BaseObject p, Vector2 c, float rad, PhysicsLayer layer)
        {
            parent = p;
            center = c;
            radius = rad;
            Type = ColliderType.Circle;
            Layer = layer;
        }
        //box
        public Collider(BaseObject p, Vector2 c, Vector2 ext, PhysicsLayer layer)
        {
            parent = p;
            center = c;
            extents = ext;
            Type = ColliderType.Box;
            Layer = layer;
        }
        //mesh
        public Collider(BaseObject p, Vector2 c, Mesh m, PhysicsLayer layer)
        {
            parent = p;
            center = c;
            mesh = m;
            Type = ColliderType.Mesh;
            Layer = layer;
        }
    }

    public class    Physics
    {
        /*
        public bool DetectCollision(CirlceCollider c1, CirlceCollider c2, ref ContactPoint cp)
        {
            //Axis-aligned bounding box
            if (c1.center.X + c1.radius   + c2.radius > c2.center.X
             && c1.center.X < c2.center.X + c1.radius + c2.radius
             && c1.center.Y + c1.radius   + c2.radius > c2.center.Y
             && c1.center.Y < c2.center.Y + c1.radius + c2.radius)
            {
                //AABBs overlap
                float distance = Math.Abs((c1.center - c2.center).Length);
                if (distance < c1.radius + c2.radius)
                {
                    //Circles collide WIP
                    cp.normal = Vector2.Normalize(c2.center - c1.center);
                    cp.point = (c1.center + (cp.normal * c1.radius));
                    return true;
                }
                else
                {
                    //AABBs overlap, but circles do not collide
                    return false;
                }
            }
            else
            {
                //AABBs do not overlap
                return false;
            }
        }*/
        #region physics Variables
        private static List<Collider> m_colliderList = new List<Collider>();
        public  static void AddCollider(Collider c)
        {
            if (m_colliderList.Contains(c))
                return;

            m_colliderList.Add(c);
        }
        public  static void RemoveCollider(Collider c)
        {
            if (!m_colliderList.Contains(c))
                return;

            m_colliderList.Remove(c);
        }
        public  static void ClearColliders()
        {
            m_colliderList.Clear();
        }
        #endregion

        public static bool RayCast(Vector2 c, Vector2 d, PhysicsLayer mask, ref RayCastHit hit)
        {
            if (m_colliderList == null || m_colliderList.Count <= 0)
                return false;

            List<RayCastHit> hits = new List<RayCastHit>();

            float sign;

            for (int i = 0; i < m_colliderList.Count; ++i)
            {
                //Is the collider in the cast mask
                if (m_colliderList[i].Layer.HasFlag(mask))
                    continue;
                    
                //Collider specific intersection methods
                switch(m_colliderList[i].Type)
                {
                    case ColliderType.Mesh:

                        //Does the cast ray intersect with the collider
                        if (!ExtensionMethods.MeshIntersection(m_colliderList[i].mesh, c, d, ref hit.cp))
                            break;

                        //Contact point was inside the collider
                        if (Vector2.Dot(hit.cp.point - ExtensionMethods.GetPolyCenter(m_colliderList[i].mesh.vertices), hit.cp.normal) > 0)
                            break;

                        hits.Add(new RayCastHit(hit.cp, m_colliderList[i]));

                        break;

                    default: return false;
                }
            }

            //Find the closest hit point

            if (hits.Count <= 0)
                return false;

            float d0 = float.PositiveInfinity;
            float d1;

            for (int i = 0; i < hits.Count; ++i)
            {

                d1 = (c - hits[i].cp.point).LengthSquared;

                if (d1 > d0)
                    continue;

                d0 = d1;

                hit = hits[i];
            }

            return true;

        }
    }
}
