using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Utils.Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TankGame;
using Voronoi2;

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
                if (meshes != null && meshes.Count > 0)
                    for (int i = 0; i < meshes.Count; ++i)
                        meshes[i].forward = meshes[i].forward.Rotate(ExtensionMethods.Angle(forward, value));

                m_fwd = value;
            }
        }
        public  Vector2 right    { get { return forward.GetNormal(); } }

        public List<Mesh> meshes { get; set; }
        public Collider collider;

        public BaseObject()
        {
            meshes      = new List<Mesh>(); // avoid null reference
            position    = Vector2.Zero;
            m_fwd       = new Vector2(1,0);

            TankGame.TankGame.OnUpdate  += Update;
            TankGame.TankGame.OnRestart += DestroyImmediate;
        }

        public virtual void Initialize()
        {
            if (collider != null) Physics.Physics.AddCollider(collider);

            if (meshes != null && meshes.Count > 0) TankGame.TankGame.AddMeshesToRenderStack(meshes);
        }

        public void DestroyImmediate()
        {
            TankGame.TankGame.OnUpdate  -= Update;
            TankGame.TankGame.OnRestart -= DestroyImmediate;

            for (int i = 0; i < meshes.Count; ++i)
                TankGame.TankGame.RemoveMeshFromRenderStack(meshes[i]);

            if (collider != null) Physics.Physics.RemoveCollider(collider);
        }


        public virtual void Destroy()
        {
            DestroyImmediate();
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
            get
            {
                return parent.position + offset.TransformPoint(forward, right);
            }
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
        public  Vector2[]   verticesWorldSpace
        {
            get
            {
                Vector2[] v = new Vector2[vertices.Length];

                for (int i = 0; i < v.Length; ++i) v[i] = TransformPoint(vertices[i]);

                return v;
            }
        }

        public Mesh(int vertexCount,    BaseObject p)
        {
            parent      = p;
            vertices    = new Vector2[vertexCount];
        }
        public Mesh(Vector2[] v,        BaseObject p)
        {
            parent = p;
            vertices = v;
            color = Color.White;
            renderMode = PrimitiveType.LineStrip;
        }
        public Mesh(Vector2[] v,        BaseObject p, PrimitiveType t)
        {
            parent = p;
            vertices = v;
            color = Color.White;
            renderMode = t;
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
        public Mesh(Mesh m)
        {
            parent = m.parent;
            vertices = new Vector2[m.vertices.Length];
            for (int i = 0; i < m.vertices.Length; i++)
            {
                vertices[i] = m.vertices[i];
            }
            color = m.color;
            renderMode = m.renderMode;
        }

        public void Rotate(float radians)
        {
            m_fwd = m_fwd.Rotate(radians);
        }
        public void Rotate(float radians, Vector2 center)
        {
            m_fwd = m_fwd.Rotate(radians, center);
        }
        public void RotateVertices(float radians)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = vertices[i].Rotate(radians);
            }
        }
        public void RotateVertices(float radians, Vector2 center)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = vertices[i].Rotate(radians, center);
            }
        }
        public void Draw()
        {
            GL.Begin(renderMode);
            GL.Color4(color);

            for (int i = 0; i < vertices.Length; ++i) GL.Vertex2(TransformPoint(vertices[i]));

            GL.End();
        }

        public Vector2 TransformPoint(Vector2 p)
        {
            return parent.position + (offset + p).TransformPoint(right, forward);
        }

    }

    public class DebugMesh
    {
        public PrimitiveType RenderMode;
        public Color Color;

        public Vector2[] vertices { get; set; }

        public DebugMesh(Vector2[] v, Color color, PrimitiveType renderMode)
        {
            vertices    = v;
            Color       = color;
            RenderMode  = renderMode;
        }
        public DebugMesh(Mesh m)
        {
            vertices = m.vertices;
            Color = m.color;
            RenderMode = m.renderMode;
        }

        public void Draw()
        {
            GL.Begin(RenderMode);
            GL.Color4(Color);

            for (int i = 0; i < vertices.Length; ++i) GL.Vertex2(vertices[i]);

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

        public static void DrawLine(Vector2 origin, Vector2 end, Color color)
        {
            DebugMesh mesh = new DebugMesh(new Vector2[] { origin, end}, color, PrimitiveType.Lines);
            TankGame.TankGame.DrawDebugMesh(mesh);
        }
        public static void DrawNormals(Mesh m, Color color)
        {
            Vector2[] normals = new Vector2[m.vertices.Length];
            Vector2[] origins = new Vector2[m.vertices.Length];
            Vector2[] v = m.verticesWorldSpace;
            
            for (int i = 0; i < m.vertices.Length; i++)
            {
                if (i == m.vertices.Length - 1)
                {
                    normals[i] = (v[0] - v[i]).GetNormal();
                    origins[i] = (v[i] + v[0]) / 2;
                }
                else
                {
                    normals[i] = (v[i+1] - v[i]).GetNormal();
                    origins[i] = (v[i] + v[i+1]) / 2;
                }
                normals[i].Normalize();

                DrawLine(origins[i], origins[i] + normals[i], color);
            }
        }
        public static void DrawBounds(Mesh m, Color color)
        {
            Bounds b = new Bounds(m);

            DrawLine(b.topLeft, b.topRight, color);
            DrawLine(b.topRight, b.bottomRight, color);
            DrawLine(b.bottomRight, b.bottomLeft, color);
            DrawLine(b.bottomLeft, b.topLeft, color);
        }
        public static void DrawBounds(Bounds b, Color color)
        {
            DrawLine(b.topLeft, b.topRight, color);
            DrawLine(b.topRight, b.bottomRight, color);
            DrawLine(b.bottomRight, b.bottomLeft, color);
            DrawLine(b.bottomLeft, b.topLeft, color);
        }
    }

    public struct ContactPoint
    {
        public Vector2 point;
        public Vector2 normal;

        public ContactPoint(Vector2 p, Vector2 n)
        {
            point   = p;
            normal  = n;
        }
    }

    public struct Bounds
    {
        public Vector2 center, extents;
        public Vector2 topLeft
        {
            get
            {
                return center + extents;
            }
        }
        public Vector2 bottomRight
        {
            get
            {
                return center - extents;
            }
        }
        public Vector2 topRight
        {
            get
            {
                return center + new Vector2(-extents.X, extents.Y);
            }
        }
        public Vector2 bottomLeft
        {
            get
            {
                return center + new Vector2(extents.X, -extents.Y);
            }
        }
        public float Left
        {
            get
            {
                return topLeft.X;
            }
        }
        public float Right
        {
            get
            {
                return topRight.X;
            }
        }
        public float Top
        {
            get
            {
                return topLeft.Y;
            }
        }
        public float Bottom
        {
            get
            {
                return bottomLeft.Y;
            }
        }

        public Bounds(Mesh m)
        {
            Vector2[] v = m.verticesWorldSpace;
            float minX = float.PositiveInfinity, maxX = float.NegativeInfinity, minY = float.PositiveInfinity, maxY = float.NegativeInfinity;

            for (int i = 0; i < v.Length; i++)
            {
                if (v[i].X < minX)
                    minX = v[i].X;

                if (v[i].X > maxX)
                    maxX = v[i].X;

                if (v[i].Y > maxY)
                    maxY = v[i].Y;

                if (v[i].Y < minY)
                    minY = v[i].Y;
            }
            center = new Vector2(ExtensionMethods.Lerp(minX, maxX, 0.5f), ExtensionMethods.Lerp(minY, maxY, 0.5f));
            extents = new Vector2(maxX - minX, maxY - minY) * 0.5f;
        }

        public Bounds(Vector2 c, Vector2 ext)
        {
            center = c;
            extents = ext;
        }
    }

    public static class LevelGeneration
    {
        public static List<Obstacle> GenerateGrid(Vector2 start, Vector2 end, int countX, int countY, float scaling)
        {
              Vector2 baseScale       = end - start;
                      baseScale.X    /= countX;
                      baseScale.Y    /= countY;
                      baseScale      *= 0.5f;

              Vector2 pos;

              List<Obstacle> result = new List<Obstacle>();

              for(int i = 0; i < countY; ++i)
              {
                  pos.Y = ExtensionMethods.Lerp(start.Y, end.Y, (float)i / (countY - 1));

                  for (int j = 0; j < countX; ++j)
                  {
                      pos.X = ExtensionMethods.Lerp(start.X, end.X, (float)j / (countX - 1));

                      result.Add(new Obstacle(baseScale * scaling, pos, TankGame.TankGame.random.NextDouble() > 0.75));
                      result[result.Count - 1].Rotate(TankGame.TankGame.random.Range(-3, 3));
                  }
              }

            return result;
        }
        public static List<Obstacle> GenerateVoronoiCells()
        {
            List<Obstacle> result = new List<Obstacle>();

            Vector2[] v2;

            List<GraphEdge> graph = GenerateVoronoi((int)Math.Ceiling(TankGame.TankGame.battlefieldSize.X * 0.5f), out v2);
            List<Vector2[]> polys = IsolateVoronoiCells(graph, v2);

            Mesh[] meshes = new Mesh[polys.Count];

            for (int i = 0; i < meshes.Length; i++)
            {
                result.Add(new Obstacle(polys[i], false));
                result[i].meshes[0].Scale(-0.5f);
            }

            return result;
        }

        public static List<Vector2[]> IsolateVoronoiCells(List<GraphEdge> edges, Vector2[] sites)
        {
            List<Vector2[]> result = new List<Vector2[]>();
            List<Vector2> poly = new List<Vector2>();

            for (int i = 0; i < sites.Length; ++i)
            {
                poly.Clear();

                for (int j = 0; j < edges.Count; ++j)
                {
                    if (edges[j].site1 != i && edges[j].site2 != i)
                        continue;

                    poly.Add(edges[j].p1);
                    poly.Add(edges[j].p2);
                }


                result.Add(ExtensionMethods.SortPolyClockwise(ExtensionMethods.MergerVertices(poly.ToArray(), 0.05f)));
            }

            return result;
        }

        public static List<GraphEdge> GenerateVoronoi(int count)
        {

            double[] xVal = new double[count];
            double[] yVal = new double[count];

            for (int i = 0; i < xVal.Length; i++)
            {
                xVal[i] = TankGame.TankGame.random.Range(-TankGame.TankGame.battlefieldSize.X * 0.5f, TankGame.TankGame.battlefieldSize.X * 0.5f);
                yVal[i] = TankGame.TankGame.random.Range(-TankGame.TankGame.battlefieldSize.Y * 0.5f, TankGame.TankGame.battlefieldSize.Y * 0.5f);
            }

            Voronoi voroObject = new Voronoi(0.1);

            return voroObject.generateVoronoi(xVal, yVal, -TankGame.TankGame.battlefieldSize.X, TankGame.TankGame.battlefieldSize.X, -TankGame.TankGame.battlefieldSize.Y, TankGame.TankGame.battlefieldSize.Y);
        }
        public static List<GraphEdge> GenerateVoronoi(int count, out Vector2[] vertices)
        {

            double[] xVal = new double[count];
            double[] yVal = new double[count];

            for (int i = 0; i < xVal.Length; i++)
            {
                xVal[i] = TankGame.TankGame.random.Range(-TankGame.TankGame.battlefieldSize.X * 0.5f, TankGame.TankGame.battlefieldSize.X * 0.5f);
                yVal[i] = TankGame.TankGame.random.Range(-TankGame.TankGame.battlefieldSize.Y * 0.5f, TankGame.TankGame.battlefieldSize.Y * 0.5f);
            }

            Voronoi voroObject = new Voronoi(0.1);

            List<GraphEdge> result = voroObject.generateVoronoi(xVal, yVal, -TankGame.TankGame.battlefieldSize.X, TankGame.TankGame.battlefieldSize.X, -TankGame.TankGame.battlefieldSize.Y, TankGame.TankGame.battlefieldSize.Y);

            vertices = voroObject.sitesToVertices;

            return result;
        }

        public static List<Obstacle> CullRandom(this List<Obstacle> o, float chance)
        {
            for (int i = 0; i < o.Count; ++i) if (TankGame.TankGame.random.NextDouble() < chance) { o[i].DestroyImmediate(); o.Remove(o[i]); }
            return o;
        }
    }

    public static class ExtensionMethods
    {
        public static Bounds    Bounds(this Mesh m)
        {
            return new Bounds(m);
        }
        public static void      Scale(this Mesh m, float multiplier)
        {
            Vector2 center = GetPolyCenter(m.vertices);

            for (int i = 0; i < m.vertices.Length; i++)
            {
                if (multiplier < 0)
                    m.vertices[i] = Lerp(m.vertices[i], center, -multiplier);
                else
                    m.vertices[i] = Lerp(m.vertices[i], m.vertices[i] + m.vertices[i] - center, multiplier);
            }
        }

        public static void      IntersectingVertices(Mesh m1, Mesh m2, ref List<Vector2> m1v, ref List<Vector2> m2v)
        {
            Vector2[] v1 = m1.verticesWorldSpace;
            Vector2[] v2 = m2.verticesWorldSpace;

            v1 = SortPolyClockwise(v1);
            v2 = SortPolyClockwise(v2);

            Vector2 n;
            Vector2 d;

            int count = 0;

            //Are any of the v1 vertices inside v2
            for(int i = 0; i < v1.Length; ++i)
            {
                count = 0;

                for(int j = 0; j < v2.Length; ++j)
                {
                    n = v2[j] - v2[j == v2.Length -1? 0 : j+1];
                    n = n.GetNormal();
                    d = v1[i] - v2[j];

                    if (Vector2.Dot(n, d) <= 0)
                        continue;

                    ++count;
                }

                if (count != v2.Length)
                    continue;

                m1v.Add(v1[i]);
            }

            //Are any of the v2 vertices inside v1
            for (int i = 0; i < v2.Length; ++i)
            {
                count = 0;

                for (int j = 0; j < v1.Length; ++j)
                {
                    n = v1[j] - v1[j == v1.Length - 1 ? 0 : j + 1];
                    n = n.GetNormal();
                    d = v2[i] - v1[j];

                    if (Vector2.Dot(n, d) <= 0)
                        continue;

                    ++count;
                }

                if (count != v1.Length)
                    continue;

                m2v.Add(v2[i]);
            }
        }
        public static bool      MapBoundsIntersection(Mesh m, ref Vector2 dep)
        {
            Bounds b = new Bounds(m);

            Vector2 tl = TankGame.TankGame.battlefieldSize*0.5f;

            float dL, dR, dT, dB;

            dL = Math.Max(Math.Abs(b.Left)      - tl.X, 0); 
            dR = Math.Max(Math.Abs(b.Right)     - tl.X, 0);
            dT = Math.Max(Math.Abs(b.Top)       - tl.Y, 0);
            dB = Math.Max(Math.Abs(b.Bottom)    - tl.Y, 0);

            if (dL == 0 && dR == 0 && dT == 0 && dB == 0)
                return false;

            dep = new Vector2(-dL + dR, -dT + dB);
            return true;
        }

        public static bool      CircleIntersection(Vector2 center, float radius, Vector2 point, Vector2 dir, ref ContactPoint result)
        {
            //Line start is within circle
            if ((center - point).LengthSquared < radius * radius)
                return false;

            Vector2 nDir    = dir.Normalized();
            Vector2 nPoint  = Vector2.Dot(nDir, point - center) * nDir;
            float   nDist   = (nPoint - center).LengthSquared;
            float   sqrRad  = radius * radius;

            if (nDist > sqrRad) //NO INTERSECTION 
                return false;

            nDist = (float)Math.Sqrt(sqrRad - nDist);

            result.point    = nPoint - dir * nDist;
            result.normal   = (result.point - center).Normalized();

            return true;
        }
        public static bool      MeshIntersection(Mesh m1, Mesh m2, ref List<ContactPoint> contacts)
        {
            Vector2[] v1 = m1.verticesWorldSpace;
            Vector2[] v2 = m2.verticesWorldSpace;

            v1 = SortPolyClockwise(v1);
            v2 = SortPolyClockwise(v2);

            Vector2 a0, a1, b0, b1;

            bool r = false;

            ContactPoint cp = new ContactPoint();

            for (int i = 0; i < v1.Length; ++i)
            {
                a0 = v1[i];
                a1 = v1[i == v1.Length - 1 ? 0 : i + 1];

                for (int j = 0; j < v2.Length; ++j)
                {
                    b0 = v2[j];
                    b1 = v2[j == v2.Length - 1 ? 0 : j + 1];

                    if (!LineIntersection(a0, a1, b0, b1, ref cp))
                        continue;

                    r = true;
                    contacts.Add(cp);
                }
            }

            return r;
        }
        public static bool      MeshIntersection(Mesh m, Vector2 point, Vector2 dir, ref Vector2 result)
        {
            Vector2[] v = m.verticesWorldSpace;

            v = SortPolyClockwise(v);

            bool    b   = false;
            float   d0  = float.PositiveInfinity;
            float   d1  = 0;
            Vector2 cr  = Vector2.Zero;

            for(int i = 0; i < m.vertices.Length; ++i)
                if (LineIntersection(v[i], v[i == m.vertices.Length - 1 ? 0 : i + 1], point, point + dir, ref cr))
                {
                    d1 = (cr - point).LengthSquared;
                    b  = true;

                    //Find the closest intersection
                    if (d1 > d0)
                        continue;

                    d0      = d1;
                    result  = cr;
                }      

            return b;
        }
        public static bool      MeshIntersection(Mesh m, Vector2 point, Vector2 dir, ref ContactPoint cp)
        {
            Vector2[] v = m.verticesWorldSpace;

            v = SortPolyClockwise(v);

            bool  b             = false;
            float d0            = float.PositiveInfinity;
            float d1            = 0;
            ContactPoint cp1    = new ContactPoint();

            for (int i = 0; i < m.vertices.Length; ++i)
                if (LineIntersection(v[i], v[i == m.vertices.Length - 1 ? 0 : i + 1], point, point + dir, ref cp1))
                {
                    d1 = (cp1.point - point).LengthSquared;
                    b = true;

                    //Find the closest intersection
                    if (d1 > d0)
                        continue;

                    d0 = d1;
                    cp = cp1;
                }

            return b;
        }
        public static bool      LineIntersection(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1, ref Vector2 point)
        {
            float d0 = DistanceFromPlaneDir(a0 - a1, a0, b0);
            float d1 = DistanceFromPlaneDir(a0 - a1, a0, b1);

            //both points are on either side of the line normal
            if ((d0 > 0 && d1 > 0) || (d0 < 0 && d1 < 0))
                return false;

            Vector2 x   = Lerp(b0, b1, Math.Abs(d0) / (Math.Abs(d0) + Math.Abs(d1)));
            Vector2 c   = Lerp(a0, a1, 0.5f);
                    d1  = (a0 - a1).Length / 2;

            //Distance from center of line a
            d0 = DistanceFromPlaneNor(a0 - a1, c, x);
            d0 = Math.Abs(d0);

            //If distance from line center is greater than its half extent there was no intersection
            if (d0 > d1)
                return false;

            point = x;

            return true;
        }
        public static bool      LineIntersection(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1, ref ContactPoint cp)
        {
            float d0 = DistanceFromPlaneDir(a0 - a1, a0, b0);
            float d1 = DistanceFromPlaneDir(a0 - a1, a0, b1);

            //both points are on either side of the line normal
            if ((d0 > 0 && d1 > 0) || (d0 < 0 && d1 < 0))
                return false;

            Vector2 x = Lerp(b0, b1, Math.Abs(d0) / (Math.Abs(d0) + Math.Abs(d1)));
            Vector2 c = Lerp(a0, a1, 0.5f);
            d1 = (a0 - a1).Length / 2;

            //Distance from center of line a
            d0 = DistanceFromPlaneNor(a0 - a1, c, x);
            d0 = Math.Abs(d0);

            //If distance from line center is greater than its half extent there was no intersection
            if (d0 > d1)
                return false;

            cp = new ContactPoint(x, (a1 - a0).GetNormal().Normalized());

            return true;
        }

        public static float     DistanceFromPlaneDir(Vector2 planeDir, Vector2 planePoint, Vector2 point)
        {
            planeDir = new Vector2(-planeDir.Y, planeDir.X);
            planeDir.Normalize();
            return Vector2.Dot(planeDir, point - planePoint);
        }
        public static float     DistanceFromPlaneNor(Vector2 planeNormal, Vector2 planePoint, Vector2 point)
        {
            return Vector2.Dot(planeNormal.Normalized(), point - planePoint);
        }

        public static bool      MeshContainsPoint(Mesh m, Vector2 p)
        {
            Vector2[] v = m.verticesWorldSpace;

            v = SortPolyClockwise(v);

            Vector2 n;
            Vector2 d;

            int count = 0;

            for (int i = 0; i < v.Length; ++i)
            {
                n = v[i] - v[i == v.Length -1? 0 : i+1];
                n = n.GetNormal();
                d = v[i] - p;

                if (Vector2.Dot(n, d) > 0)
                    continue;

                ++count;
            }

            return count == v.Length;
        }
        public static Vector2[] SortPolyClockwise(Vector2[] v)
        {
            Vector2 c = GetPolyCenter(v);
            Array.Sort(v, new ClockwiseComparer(c));
            return v;
        }
        public static Vector2[] SortPolyClockwise(Vector2[] v, Vector2 c)
        {
            Array.Sort(v, new ClockwiseComparer(c));
            return v;
        }
        public static Vector2[] MergerVertices(Vector2[] v, float threshold)
        {
            List<Vector2> result = new List<Vector2>();

            bool b = false;

            for (int i = 0; i < v.Length; ++i)
            {
                b = true;

                if(result.Count != 0)
                    for (int j = 0; j < result.Count; ++j)
                    {
                        if ((result[j] - v[i]).Length > threshold)
                            continue;

                        b = false;
                    }

                if(b) result.Add(v[i]);
            }

            return result.ToArray();
        }
        public static Vector2   GetPolyCenter(Vector2[] points)
        {
            if (points.Length == 0) return default(Vector2);
            if (points.Length < 2)  return points[0];

            Vector2 center = Vector2.Zero;

            for (int i = 0; i < points.Length; i++) center += points[i];

            center /= points.Length;

            return center;
        }

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
        public static Vector2 Rotate(this Vector2 v, float radians, Vector2 center)
        {
            float sin = (float)Math.Sin(radians);
            float cos = (float)Math.Cos(radians);

            v.X -= center.X;
            v.Y -= center.Y;

            float tx = v.X * cos - v.Y * sin;
            float ty = v.X * sin + v.Y * cos;

            v.X = tx + center.X;
            v.Y = ty + center.Y;
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
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return new Vector2(Lerp(a.X, b.X, t.Clamp01()), Lerp(a.Y, b.Y, t.Clamp01()));
        }
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(Lerp(a.X, b.X, t.Clamp01()), Lerp(a.Y, b.Y, t.Clamp01()), Lerp(a.Z, b.Z, t.Clamp01()));
        }
        public static Vector2 Reflect(Vector2 dir, Vector2 nor)
        {
            return -2f * Vector2.Dot(nor, dir) * nor + dir;
        }
        public static void    Set(this Vector2 v, float x, float y)
        {
            v.X = x;
            v.Y = y;
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
        public static float   Repeat01(float f)
        {
            return f - (float)Math.Floor(f);
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
            return Color.FromArgb((int)Math.Round(v1.X), (int)Math.Round(v1.X), (int)Math.Round(v1.X));
        }
        public static Vector3 ToVector(this Color c)
        {
            return new Vector3((float)c.R / 255, (float)c.G / 255, (float)c.B / 255);
        }

        public static float     CrossProduct(Vector2 v, Vector2 r)
        {
            return v.X * r.Y - v.Y * r.X;
        }
        public static float     Angle(Vector2 from, Vector2 to)
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
                dots[i] = Lerp(start, end, (float)i / (count - 1));

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
