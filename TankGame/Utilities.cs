using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Input;
using System.IO;
using System.Text;

namespace Utils
{
    public abstract class BaseObject
    {
        public  Vector2 position;
        private Vector2 m_fwd = new Vector2(1, 0);
        public  Vector2 forward
        {
            get { return m_fwd; }
            set
            {
                value.Normalize();

                //Automatically update potential mesh orientations
                if (mesh != null && mesh.Count > 0)
                    for (int i = 0; i < mesh.Count; ++i)
                        mesh[i].forward = mesh[i].forward.Rotate(ExtensionMethods.Angle(forward, value));

                m_fwd = value;
            }
        }
        public  Vector2 right    { get { return forward.GetNormal(); } }

        public List<Mesh> mesh { get; set; }

        public BaseObject(Vector2 p, Vector2 f)
        {
            position    = p;
            m_fwd       = f;

            TankGame.TankGame.OnUpdate += Update;
        }
        public BaseObject()
        {
            position    = Vector2.Zero;
            m_fwd       = new Vector2(1,0);

            TankGame.TankGame.OnUpdate += Update;
        }

        public void Destroy()
        {
            for (int i = 0; i < mesh.Count; ++i)
                TankGame.TankGame.RemoveMeshFromRenderStack(mesh[i]);

            TankGame.TankGame.OnUpdate -= Update;     
        }

        public void Rotate(float radians)
        {
            forward = forward.Rotate(radians);
        }

        public abstract void Update();
    }

    public class Mesh
    {
        public PrimitiveType renderMode;
        public Color         color;

        public  BaseObject  parent { get; set; }
        public  Vector2     offset { get; set; }
        public  Vector2     worldPosition
        {
            get { return parent.position + offset.TransformPoint(forward, right); }
        }

        private Vector2     m_fwd   = new Vector2(1, 0);
        public  Vector2     forward
        {
            get
            {
                return m_fwd;
            }
            set
            {
                m_fwd = value;
                m_fwd.Normalize();
            }
        }
        public  Vector2     right    { get { return forward.GetNormal(); } }

        public  Vector2[]   vertices { get; set; }

        public Mesh(int vertexCount,    BaseObject p)
        {
            parent      = p;
            vertices    = new Vector2[vertexCount];
        }
        public Mesh(int vertexCount,    BaseObject p, Color c, PrimitiveType t)
        {
            parent      = p;
            vertices    = new Vector2[vertexCount];
            color       = c;
            renderMode  = t;
        }
        public Mesh(Vector2[] v,        BaseObject p, Color c, PrimitiveType t)
        {
            parent      = p;
            vertices    = v;
            color       = c;
            renderMode  = t;
        }
        public Mesh(Vector2[] v,        BaseObject p, Color c)
        {
            parent      = p;
            vertices    = v;
            color       = c;
            renderMode  = PrimitiveType.LineStrip;
        }
        public Mesh(Vector2[] v,        BaseObject p, PrimitiveType t)
        {
            parent      = p;
            vertices    = v;
            color       = Color.White;
            renderMode  = t;
        }
        public Mesh(Vector2[] v,        BaseObject p)
        {
            parent      = p;
            vertices    = v;
            color       = Color.White;
            renderMode  = PrimitiveType.LineStrip;
        }

        public void Rotate(float radians)
        {
            m_fwd = m_fwd.Rotate(radians);
        }

        public void Draw()
        {
            GL.Begin(renderMode);
            GL.Color4(color);

            for (int i = 0; i < vertices.Length; ++i) GL.Vertex2(parent.position + (offset + vertices[i]).TransformPoint(right, forward));

            GL.End();
        }
    }

    [Serializable]
    public class InputScheme
    {
        public enum Preset { Player1, Player2 }

        public Key leftUp;
        public Key leftDown;
        public Key rightUp;
        public Key rightDown;

        public Key rightTurn;
        public Key leftTurn;

        public Key fire;

        public InputScheme(Preset p)
        {
            switch(p)
            {
                case Preset.Player1:
                    leftUp      = Key.Keypad7;
                    leftDown    = Key.Keypad4;
                    rightUp     = Key.Keypad8;
                    rightDown   = Key.Keypad5;
                    rightTurn   = Key.Keypad6;
                    leftTurn    = Key.Keypad9;
                    fire        = Key.KeypadAdd;
                    break;

                case Preset.Player2:
                    leftUp      = Key.Q;
                    leftDown    = Key.A;
                    rightUp     = Key.W;
                    rightDown   = Key.S;
                    rightTurn   = Key.E;
                    leftTurn    = Key.R;
                    fire        = Key.Space;
                    break;
            }
        }
    }

    public static class Debug
    {
        static StringBuilder sb = new StringBuilder();
        public static void UpdateLog()
        {
            if (sb.Length <= 0)
                return;
            File.AppendAllText(Directory.GetCurrentDirectory() + "/log.txt", sb.ToString());
            sb.Clear();
        }
        public static void Log(String str)
        {
            sb.Append(str + System.Environment.NewLine);
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
        public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDelta)
        {
            return new Vector2(MoveTowards(current.X, target.X, maxDelta), MoveTowards(current.Y, target.Y, maxDelta));
        }
        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDelta)
        {
            return new Vector3(MoveTowards(current.X, target.X, maxDelta), MoveTowards(current.Y, target.Y, maxDelta), MoveTowards(current.Z, target.Z, maxDelta));
        }
        public static float   Lerp(float a, float b, float t)
        {
            return a + (b - a) * t.Clamp01();
        }

        public static float   LerpUnclamped(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
        public static float   Clamp01(this float f)
        {
            return f > 1 ? 1 : f < 0 ? 0 : f;
        }
        public static float   Clamp(float f, float min, float max)
        {
            return f < min ? min : f > max ? max : f;
        }
        public static float   MoveTowards(float current, float target, float maxDelta)
        {
            float result;
            if (Math.Abs(target - current) <= maxDelta)
            {
                result = target;
            }
            else
            {
                result = current + Math.Sign(target - current) * maxDelta;
            }
            return result;
        }
        
        public static Color   ToColor(this Vector3 v)
        {
            Vector3 v1 = v;
            v1  *= 255;

            return Color.FromArgb(1, (int)Math.Round(v1.X), (int)Math.Round(v1.X), (int)Math.Round(v1.X));
        }
        public static Vector3 ToVector(this Color c)
        {
            return new Vector3((float)c.R / 255, (float)c.G / 255, (float)c.B / 255);
        }

        public static float CrossProduct(Vector2 v, Vector2 r)
        {
            return v.X * r.Y - v.Y * r.X;
        }
        public static float Angle(Vector2 from, Vector2 to)
        {
            Vector2 normalized  = from.Normalized();
            Vector2 normalized2 = to.Normalized();

            float num   = (float)Math.Acos(Clamp(Vector2.Dot(normalized, normalized2), -1f, 1f));

            float num2  = Math.Sign(normalized.X * normalized2.Y - normalized.Y * normalized2.X);

            return num * num2;
        }
        public static Vector2[] LineToDots(Vector2 start, Vector2 end, int count)
        {
            Vector2[] dots = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                dots[i] = start + (end - start) * ((float)i / (count - 1));
            }
            return dots;
        }

        public static float   Range(this Random r, float min, float max)
        {
            return Lerp(min, max, (float)r.NextDouble());
        }
        public static Vector2 OnUnitCircle(this Random r)
        {
            return new Vector2((float)r.NextDouble() - 0.5f, (float)r.NextDouble() - 0.5f).Normalized();
        }
        public static Vector2 OnScaledCircle(this Random r, float min, float max)
        {
            return OnUnitCircle(r) * Range(r, min, max);
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
