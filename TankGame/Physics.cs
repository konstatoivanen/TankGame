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
        public ContactPoint contact;
        public Collider other;

        public RayCastHit(ContactPoint _contact, Collider _other)
        {
            contact = _contact;
            other   = _other;
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
            parent  = p;
            center  = c;
            radius  = rad;
            Type    = ColliderType.Circle;
            Layer   = layer;
        }
        //box
        public Collider(BaseObject p, Vector2 c, Vector2 ext, PhysicsLayer layer)
        {
            parent  = p;
            center  = c;
            extents = ext;
            Type    = ColliderType.Box;
            Layer   = layer;
        }
        //mesh
        public Collider(BaseObject p, Mesh m, PhysicsLayer layer)
        {
            parent  = p;
            center  = Vector2.Zero;
            mesh    = m;
            Type    = ColliderType.Mesh;
            Layer   = layer;
        }
    }

    public class    Physics
    {
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

        public static bool RayCast(Vector2 c, Vector2 d, PhysicsLayer mask, ref RayCastHit hit)
        {
            if (m_colliderList == null || m_colliderList.Count <= 0)
                return false;

            bool            r   = false;
            ContactPoint    cp  = new ContactPoint();
            float           d0  = float.PositiveInfinity;
            float           d1;

            for (int i = 0; i < m_colliderList.Count; ++i)
            {
                //Is the collider in the cast mask
                if (m_colliderList[i].Layer.HasFlag(mask))
                    continue;
                    
                //Collider specific intersection methods
                switch(m_colliderList[i].Type)
                {
                    case ColliderType.Mesh:

                        if (!ExtensionMethods.MeshIntersection(m_colliderList[i].mesh, c, d, ref cp))
                            continue;

                        //Contact point was inside the collider
                     //   if (Vector2.Dot(cp.point - ExtensionMethods.GetPolyCenter(m_colliderList[i].mesh.verticesWorldSpace), d) > 0)
                       //     continue;

                        break;

                    case ColliderType.Circle:
                        if (!ExtensionMethods.CircleIntersection(m_colliderList[i].center, m_colliderList[i].radius, c, d, ref cp))
                            continue;

                        break;

                    default: return false;
                }

                d1 = (c - cp.point).LengthSquared;

                //Current hit is further away than last
                if (d1 > d0)
                    continue;

                hit.contact = cp;
                hit.other   = m_colliderList[i];
                r           = true;
            }

            return r;
        }
        public static bool PointMeshCollision(Vector2 p, PhysicsLayer mask)
        {
            for (int i = 0; i < m_colliderList.Count; ++i)
            {
                //Is the collider in the cast mask
                if (m_colliderList[i].Layer.HasFlag(mask))
                    continue;

                if (m_colliderList[i].Type != ColliderType.Mesh)
                    continue;

                //Cannot fire a ray inside collider
                if (ExtensionMethods.MeshContainsPoint(m_colliderList[i].mesh, p))
                    return true;
            }

            return false;
        }

        public static Vector2   CollisionMesh(Collider c)
        {
            if (c.Type != ColliderType.Mesh)
                return Vector2.Zero;

            if (m_colliderList == null || m_colliderList.Count <= 0)
                return Vector2.Zero;

            List<ContactPoint> contacts = new List<ContactPoint>();

            for (int i = 0; i < m_colliderList.Count; ++i)
            {
                //Collider is not a mesh
                if (m_colliderList[i].Type != ColliderType.Mesh)
                    continue;

                //Dont test against self
                if (m_colliderList[i] == c)
                    continue;

                //Is the collider in the mask
                if (m_colliderList[i].Layer.HasFlag(c.Layer))
                   continue;

                //Do the meshes intersect?
                if (!ExtensionMethods.MeshIntersection(c.mesh, m_colliderList[i].mesh, ref contacts))
                    continue;

                //Only solve collisions with 2 contact points
                if (contacts.Count != 2)
                    continue;

                List<Vector2> p0 = new List<Vector2>();
                List<Vector2> p1 = new List<Vector2>();

                ExtensionMethods.IntersectingVertices(c.mesh, m_colliderList[i].mesh, ref p0, ref p1);

                if (p0.Count == 0 && p1.Count == 0)
                    continue;

                Vector2 d   = (contacts[1].point - contacts[0].point).Normalized();
                Vector2 p   = Vector2.Zero;
                Vector2 n1  = Vector2.Zero;
                Vector2 n2  = Vector2.Zero;

                float f0 = 0, f1 = 0;

                if(p0.Count > 0)
                    for(int j = 0; j < p0.Count; ++j)
                    {
                        p   = contacts[1].point + d * Vector2.Dot(d, p0[j] - contacts[1].point);
                        f1  = (p - p0[j]).LengthSquared;

                        if (f1 < f0)
                            continue;

                        f0 = f1;
                        n1 = p0[j] - p;
                    }

                f1 = 0;
                f0 = 0;

                if (p1.Count > 0)
                    for (int j = 0; j < p1.Count; ++j)
                    {
                        p   = contacts[1].point + d * Vector2.Dot(d, p1[j] - contacts[1].point);
                        f1  = (p - p1[j]).LengthSquared;

                        if (f1 < f0)
                            continue;

                        f0 = f1;
                        n2 = p1[j] - p;
                    }

                p = n1 + n2;

                //Avoid Nan vectors
                if (float.IsNaN(p.X + p.Y))
                    return Vector2.Zero;

                d  = (m_colliderList[i].parent.position - c.parent.position).Normalized();
                p *= Math.Sign(-Vector2.Dot(p, d));

                return p;
            }

            return Vector2.Zero;
        }
        public static bool      CollisionMesh(Collider c, ref ContactPoint result)
        {
            if (c.Type != ColliderType.Mesh)
                return false;

            if (m_colliderList == null || m_colliderList.Count <= 0)
                return false;

            List<ContactPoint> contacts = new List<ContactPoint>();

            for (int i = 0; i < m_colliderList.Count; ++i)
            {
                //Collider is not a mesh
                if (m_colliderList[i].Type != ColliderType.Mesh)
                    continue;

                //Dont test against self
                if (m_colliderList[i] == c)
                    continue;

                //Is the collider in the mask
                if (m_colliderList[i].Layer.HasFlag(c.Layer))
                   continue;

                //Do the meshes intersect?
                if (!ExtensionMethods.MeshIntersection(c.mesh, m_colliderList[i].mesh, ref contacts))
                    continue;

                //Only solve collisions with 2 contact points
                if (contacts.Count != 2)
                    continue;

                List<Vector2> p0 = new List<Vector2>();
                List<Vector2> p1 = new List<Vector2>();

                ExtensionMethods.IntersectingVertices(c.mesh, m_colliderList[i].mesh, ref p0, ref p1);

                if (p0.Count == 0 && p1.Count == 0)
                    continue;

                Vector2 d   = (contacts[1].point - contacts[0].point).Normalized();
                Vector2 p   = Vector2.Zero;
                Vector2 n1  = Vector2.Zero;
                Vector2 n2  = Vector2.Zero;

                float f0 = 0, f1 = 0;

                if(p0.Count > 0)
                    for(int j = 0; j < p0.Count; ++j)
                    {
                        p   = contacts[1].point + d * Vector2.Dot(d, p0[j] - contacts[1].point);
                        f1  = (p - p0[j]).LengthSquared;

                        if (f1 < f0)
                            continue;

                        f0 = f1;
                        n1 = p0[j] - p;
                    }

                f1 = 0;
                f0 = 0;

                if (p1.Count > 0)
                    for (int j = 0; j < p1.Count; ++j)
                    {
                        p   = contacts[1].point + d * Vector2.Dot(d, p1[j] - contacts[1].point);
                        f1  = (p - p1[j]).LengthSquared;

                        if (f1 < f0)
                            continue;

                        f0 = f1;
                        n2 = p1[j] - p;
                    }

                p = n1 + n2;

                //Avoid Nan vectors
                if (float.IsNaN(p.X + p.Y))
                    return false;


                d  = m_colliderList[i].parent.position - c.parent.position;
                p *= Math.Sign(-Vector2.Dot(p, d));

                result = new ContactPoint(ExtensionMethods.Lerp(contacts[0].point, contacts[1].point, 0.5f), p);

                return true;
            }

            return false;
        }
        public static void      CollisonSolve_Mesh(BaseObject b, float rotationAmount)
        {
            if (b.collider == null || b.collider.Type != ColliderType.Mesh)
                return;

            ContactPoint    cp      = new ContactPoint();
            Vector2         dp      = Vector2.Zero;

            if (ExtensionMethods.MapBoundsIntersection(b.collider.mesh, ref dp))
                b.position += dp;

            if (!CollisionMesh(b.collider, ref cp))
                return;

            b.position += cp.normal;

            cp.normal = b.position - (cp.point - cp.normal);

            float angle = ExtensionMethods.Angle(b.forward, cp.normal);

            b.forward = b.forward.Rotate(angle * rotationAmount);
        }
    }
}
