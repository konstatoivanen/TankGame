using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Utils
{
    public abstract class BaseObject
    {
        public  Vector2 position { get; set; }
        private Vector2 m_fwd = new Vector2(1, 0);
        public  Vector2 forward  { get { return m_fwd; } set { m_fwd = value; m_fwd.Normalize(); } }
        public  Vector2 right    { get { return forward.GetNormal(); } }

        public List<Mesh> mesh { get; set; }

        public BaseObject(Vector2 p, Vector2 f)
        {
            position = p;
            m_fwd = f;

            TankGame.TankGame.OnUpdate += Update;
        }
        public BaseObject()
        {
            position = Vector2.Zero;
            m_fwd = new Vector2(1,0);

            TankGame.TankGame.OnUpdate += Update;
        }


        public void Translate(Vector2 destination)
        {
            if (mesh != null && mesh.Count > 0)
            {
                for (int i = 0; i < mesh.Count; i++)
                {
                    mesh[i].position += destination - position;
                }
            }
            
            position = destination;
        }
        public void Rotate(float radians)
        {
            m_fwd = m_fwd.Rotate(radians);
            if (mesh == null || mesh.Count <= 0)
                return;
            for (int i = 0; i < mesh.Count; i++)
            {
                mesh[i].forward = mesh[i].forward.Rotate(radians);
            }
        }

        public abstract void Update(float delta);
    }

    public class Mesh
    {
        public PrimitiveType renderMode;
        public Color         color;

        public  Vector2     position { get; set; }
        private Vector2     m_fwd = new Vector2(1, 0);
        public  Vector2     forward  { get { return m_fwd; } set { m_fwd = value; m_fwd.Normalize(); } }
        public  Vector2     right    { get { return forward.GetNormal(); } }
        public  Vector2[]   vertices { get; set; }

        public Mesh(int vertexCount)
        {
            vertices = new Vector2[vertexCount];
        }
        public Mesh(Vector2[] v, Color c, PrimitiveType t)
        {
            vertices = v;
            color = c;
            renderMode = t;
        }
        public Mesh(Vector2[] v, Color c)
        {
            vertices = v;
            color = c;
            renderMode = PrimitiveType.LineStrip;
        }
        public Mesh(Vector2[] v, PrimitiveType t)
        {
            vertices = v;
            color = Color.White;
            renderMode = t;
        }
        public Mesh(Vector2[] v)
        {
            vertices = v;
            color = Color.White;
            renderMode = PrimitiveType.LineStrip;
        }
        public void Translate(Vector2 destination)
        {
            position = destination;
        }
        public void Rotate(float radians)
        {
            m_fwd = m_fwd.Rotate(radians);
        }

        public void Draw()
        {
            GL.Begin(renderMode);
            GL.Color4(color);

            for (int i = 0; i < vertices.Length; ++i) GL.Vertex2(position + vertices[i].TransformPoint(right, forward));

            GL.End();
        }
    }

    public static class ExtensionMethods
    {
        public static Vector2 Rotate(this Vector2 v, float radians)
        {
            float sin = (float)Math.Sin(radians);
            float cos = (float)Math.Cos(radians);

            float tx = v.X;
            float ty = v.Y;
            v.X = (cos * tx) - (sin * ty);
            v.Y = (sin * tx) + (cos * ty);
            return v;
        }
        public static Vector2 TransformPoint(this Vector2 v, Vector2 r, Vector2 f)
        {
            return r * v.Y + f * v.X;
        }
        public static Vector2 GetNormal(this Vector2 v)
        {
            return new Vector2(-v.Y, v.X);
        }
    }

    public class MathShit // Lauri
    {
        public static float CrossProduct(Vector2 v, Vector2 r)
        {
            return v.X * r.Y - v.Y * r.X;
        }
        public static float Angle(Vector2 v, Vector2 r)
        {
            return (float)Math.Acos(Vector2.Dot(v, r) / (v.Length * r.Length));
        }
    }

    public class ClockwiseComparer : IComparer
    {
        private Vector2 m_Origin;
        public Vector2 origin { get { return m_Origin; } set { m_Origin = value; } }

        public ClockwiseComparer(Vector2 origin)
        {
            m_Origin = origin;
        }

        public int Compare(object first, object second)
        {
            Vector2 pointA = (Vector2)first;
            Vector2 pointB = (Vector2)second;

            return IsClockwise(pointB, pointA, m_Origin);
        }

        /// <summary>
        ///     Returns 1 if first comes before second in clockwise order.
        ///     Returns -1 if second comes before first.
        ///     Returns 0 if the points are identical.
        /// </summary>
        public static int IsClockwise(Vector2 first, Vector2 second, Vector2 origin)
        {
            if (first == second)
                return 0;

            Vector2 firstOffset = first - origin;
            Vector2 secondOffset = second - origin;

            double angle1 = Math.Atan2(firstOffset.X, firstOffset.Y);
            double angle2 = Math.Atan2(secondOffset.X, secondOffset.Y);

            if (angle1 < angle2)
                return 1;

            if (angle1 > angle2)
                return -1;

            return (firstOffset.Length < secondOffset.Length) ? 1 : -1;
        }

    }
}
