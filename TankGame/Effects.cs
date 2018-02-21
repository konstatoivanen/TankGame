
using OpenTK;
using System;
using System.Collections.Generic;
using Utils;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using Utils.Physics;


namespace TankGame
{
    public class Explosion : BaseObject
    {
        private float killTime;

        public Explosion(Vector2 pos, Vector2 fwd)
        {
            Vector2[] verts = { new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0) };

            Mesh ExplosionMesh = new Mesh(verts, this, Color.OrangeRed, PrimitiveType.Quads);

            meshes.Add(ExplosionMesh);

            position = pos;
            forward = fwd;

            killTime = Time.time + 0.25f;

            Initialize();
        }

        public override void Update()
        {
            for (int i = 0; i < meshes[0].vertices.Length; i++)
                meshes[0].vertices[i] = meshes[0].vertices[i] + TankGame.random.OnUnitCircle() * 0.4f + forward * 0.05f;

            if (Time.time > killTime)
                Destroy();
        }
    }

    public class Dot
    {
        public Vector2 position;
        public Vector2 velocity;

        public float killTime;
        public float damping;
        public bool alive;

        public Dot(Vector2 pos, Vector2 vel, float lt, float d)
        {
            position = pos;
            velocity = vel;
            killTime = Time.time + lt;
            damping = d;
            alive = true;
        }

        public void Update(float delta)
        {
            if (!alive)
                return;

            position += velocity * delta;
            velocity = ExtensionMethods.MoveTowards(velocity, Vector2.Zero, damping * delta);

            if (Time.time > killTime)
                alive = false;
        }
    }

    public class Sparks : BaseObject
    {

        private List<Dot> dots;
        private int prevLength;

        public Sparks(Vector2 pos, int count, float maxSpeed, Vector2 mimMaxLifeTime, float damping)
        {
            position = pos;

            dots = new List<Dot>();

            for (int i = 0; i < count; ++i) dots.Add(new Dot(pos, TankGame.random.OnScaledCircle(-maxSpeed, maxSpeed), TankGame.random.Range(mimMaxLifeTime.X, mimMaxLifeTime.Y), damping));

            meshes.Add(new Mesh(count, this, Color.Orange, PrimitiveType.Points));

            prevLength = count;

            Initialize();
        }

        public override void Update()
        {
            for (int i = 0; i < dots.Count; ++i) if (!dots[i].alive) dots.Remove(dots[i]);

            if (prevLength != dots.Count)
                meshes[0].vertices = new Vector2[dots.Count];

            prevLength = dots.Count;

            if (prevLength == 0)
            {
                Destroy();
                return;
            }

            for (int i = 0; i < dots.Count; ++i)
            {
                dots[i].Update(Time.deltatime);
                meshes[0].vertices[i] = position - dots[i].position;
            }
        }
    }
    public class MuzzleFlashv2 : BaseObject
    {
        public MuzzleFlashv2(Vector2 p, Vector2 f)
        {
            Mesh m = new Mesh(new Vector2[] { new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1) }, null, Color.Orange, PrimitiveType.Quads);
            new SpinningObj(p, f, f * 0.1f, m, 0.1f, 1f);
        }

        public override void Update()
        {
            Destroy();
        }
    }
    public class SpinningObj : BaseObject
    {
        float spin;
        float lifeTime;
        Vector2 moveDir;
        Vector3 colorCurrent;

        public SpinningObj(Vector2 p, Vector2 f, Vector2 mv, Mesh m, float s, float lt)
        {
            position = p;
            forward = f;
            meshes.Add(m);
            spin = s;
            moveDir = mv;
            lifeTime = lt;
            meshes[0].parent = this;
            colorCurrent = m.color.ToVector();
            Debug.Log(colorCurrent.ToString());
            Initialize();
        }

        public override void Update()
        {
            lifeTime -= Time.deltatime;
            if (lifeTime <= 0)
                Destroy();
            position += moveDir;
            meshes[0].RotateVertices(spin, ExtensionMethods.GetPolyCenter(meshes[0].vertices));
            meshes[0].Scale(0.02f);
            //Move current color towards 0
            colorCurrent = ExtensionMethods.MoveTowards(colorCurrent, Vector3.Zero, Time.deltatime * (1f / lifeTime));

            //Convert vector3 back to color
            meshes[0].color = colorCurrent.ToColor();
        }
    }
    public class SpinningThing : ShatterMesh
    {
        List<int> spinDir = new List<int>();

        public SpinningThing(Mesh m) : base(m)
        {

        }

        public override void Update()
        {
            base.Update();
            for (int i = 0; i < meshes.Count; i++)
            {
                while (i > spinDir.Count - 1)
                {
                    spinDir.Add(ExtensionMethods.Range(TankGame.random, 0f, 2f) < 1 ? -1 : 1);
                }
                meshes[i].RotateVertices(0.01f * spinDir[i], ExtensionMethods.GetPolyCenter(meshes[i].vertices));
            }
        }
        public override void Destroy()
        {
            base.Destroy();
            for (int i = 0; i < meshes.Count; i++)
            {
                new ExplodeLineLoopToDots(meshes[i], 2, TankGame.random.Range(0.5f, 2f));
            }
        }
    }

    public class ShatterMesh : BaseObject
    {
        float SplitInterval = 1f;
        float LifeTime = 2f;
        float LastSplit;

        public ShatterMesh(Mesh m)
        {
            //position = m.parent.position;
            //forward = m.parent.forward;
            meshes = SplitMesh(m);
            for (int i = 0; i < meshes.Count; i++)
            {
                meshes[i].parent = this;
            }
            
            Initialize();        
        }
        public override void Update()
        {
            LifeTime -= Time.deltatime;
            if (LifeTime <= 0) Destroy();
            for (int i = 0; i < meshes.Count; i++)
            {
                ShrinkMesh(meshes[i], 0.01f);
            }
            if (Time.time - LastSplit > SplitInterval)
            {
                LastSplit = Time.time;
                List<Mesh> temp = new List<Mesh>();

                for (int i = 0; i < meshes.Count; i++)
                {
                    temp.AddRange(SplitMesh(meshes[i]));
                }

                TankGame.RemoveMeshesFromRenderStack(meshes);
                meshes = temp;
                TankGame.AddMeshesToRenderStack(meshes);
            }
                
        }
        public List<Mesh> SplitMesh(Mesh m)
        {
            List<Mesh> list = new List<Mesh>();
            List<Vector2> midPoints = new List<Vector2>();
            Vector2 center = ExtensionMethods.GetPolyCenter(m.verticesWorldSpace);            

            for (int i = 0; i < m.verticesWorldSpace.Length; i++)
            {
                midPoints.Add((m.verticesWorldSpace[i + 1 == m.verticesWorldSpace.Length ? 0 : i + 1] + m.verticesWorldSpace[i])/2);
            }
            for (int i = 0; i < midPoints.Count; i++)
            {
                list.Add(new Mesh(new Vector2[] {m.verticesWorldSpace[i], midPoints[i], center, midPoints[i - 1 < 0 ? midPoints.Count - 1 : i - 1], m.verticesWorldSpace[i] }, m.parent, m.color));
            }
            return list;
        }

        public static void ShrinkMesh(Mesh m, float multiplier)
        {
            Vector2 center = ExtensionMethods.GetPolyCenter(m.vertices);

            for (int i = 0; i < m.vertices.Length; i++)
            {                
                m.vertices[i] = ExtensionMethods.Lerp(m.vertices[i], center, multiplier);
            }
        }   
    }
    

    public class MuzzleFlashSmoke : BaseObject
    {
        float timer = 0;
        float lifeTime = 0.25f;
        Vector3 colorCurrent;

        public MuzzleFlashSmoke(Vector2 pos, Vector2 fwd)
        {
            Mesh SmokeMesh = new Mesh(new Vector2[] { new Vector2(3, -1), new Vector2(3, 1), new Vector2(1, 0.5f), new Vector2(0, 0), new Vector2(1, -0.5f) }, this, Color.DarkGray, PrimitiveType.LineLoop);
            meshes.Add(SmokeMesh);

            position            = pos;
            SmokeMesh.forward   = fwd;

            //Convert color to vector3
            colorCurrent = SmokeMesh.color.ToVector();

            Initialize();
        }

        public override void Update()
        {
            for (int i = 0; i < meshes[0].vertices.Length; i++)
                meshes[0].vertices[i] = meshes[0].vertices[i] + TankGame.random.OnUnitCircle() * 0.1f + new Vector2(1, 0) * 0.005f;

            //Move current color towards 0
            colorCurrent = ExtensionMethods.MoveTowards(colorCurrent, Vector3.Zero, Time.deltatime * (1f / lifeTime));

            //Convert vector3 back to color
            meshes[0].color = colorCurrent.ToColor();

            timer += Time.deltatime;
            if (timer > lifeTime)
                Destroy();
        }
    }
    public class MuzzleFlashFire : BaseObject
    {
        float timer = 0;
        float lifeTime = 0.25f;
        Vector3 colorCurrent;

        public MuzzleFlashFire(Vector2 pos, Vector2 fwd)
        {
            Mesh MuzzleFlashMesh = new Mesh(new Vector2[] { new Vector2(3, -1), new Vector2(3, 1), new Vector2(1, 0.5f), new Vector2(0, 0), new Vector2(1, -0.5f) }, this, Color.Orange, PrimitiveType.LineLoop);

            meshes.Add(MuzzleFlashMesh);

            position                = pos;
            MuzzleFlashMesh.forward = fwd;

            colorCurrent = MuzzleFlashMesh.color.ToVector();

            Initialize();
        }

        public override void Update()
        {
            for (int i = 0; i < meshes[0].vertices.Length; i++)
                meshes[0].vertices[i] = meshes[0].vertices[i] + TankGame.random.OnUnitCircle() * 0.05f + new Vector2(1, 0) * 0.01f;

            //Move current color towards 0
            colorCurrent = ExtensionMethods.MoveTowards(colorCurrent, Vector3.Zero, Time.deltatime * (1f / lifeTime));

            //Convert vector3 back to color
            meshes[0].color = colorCurrent.ToColor();

            timer += Time.deltatime;
            if (timer > lifeTime)
                Destroy();
        }
    }

    public class MuzzleFlash
    {
        public MuzzleFlash(Vector2 pos, Vector2 fwd)
        {
            MuzzleFlashSmoke smoke0 = new MuzzleFlashSmoke(pos - fwd * 0.5f, fwd.Rotate(0.4f));
            MuzzleFlashSmoke smoke1 = new MuzzleFlashSmoke(pos - fwd * 0.5f, fwd.Rotate(-0.4f));
            MuzzleFlashSmoke smoke2 = new MuzzleFlashSmoke(pos + fwd * 0.8f, fwd);
            MuzzleFlashFire fire0 = new MuzzleFlashFire(pos - fwd * 0.5f, fwd.Rotate(0.2f));
            MuzzleFlashFire fire1 = new MuzzleFlashFire(pos - fwd * 0.5f, fwd.Rotate(-0.2f));
            MuzzleFlashFire fire2 = new MuzzleFlashFire(pos + fwd * 0.2f, fwd);
        }
    }

    public class Obstacle : BaseObject
    {
        public Obstacle(int angles, float minSize, float maxSize, Vector2 pos)
        {
            //Obsolete!!!

            Vector2[] v = new Vector2[angles];

            for (int i = 0; i < v.Length; ++i)
                v[i] = TankGame.random.OnScaledCircle(minSize, maxSize);

            Vector2 c = ExtensionMethods.GetPolyCenter(v);

            for (int i = 0; i < v.Length; ++i)
                v[i] -= c;

            v = ExtensionMethods.SortPolyClockwise(v, c);

            Mesh ObstacleMesh = new Mesh(v, this, Color.Gray, PrimitiveType.LineLoop);
            meshes.Add(ObstacleMesh);
            position = pos;

            collider = new Collider(this, meshes[0], PhysicsLayer.Default);

            Initialize();
        }
        public Obstacle(Vector2 size, Vector2 pos, bool destroyable)
        {
            Mesh ObstacleMesh = new Mesh(new Vector2[4] { size, new Vector2(size.X, -size.Y), -size, new Vector2(-size.X, size.Y), }, this, destroyable? Color.Beige : Color.Gray, PrimitiveType.LineLoop);
            meshes.Add(ObstacleMesh);
            position = pos;

            collider = new Collider(this, meshes[0], destroyable? PhysicsLayer.Destroyable : PhysicsLayer.Default);

            Initialize();
        }
        public override void Destroy()
        {
            for (int i = 0; i < meshes.Count; i++)
                new SpinningThing(meshes[i]);

            base.Destroy();
        }
        public override void Update()
        {
            
        }
    }

    public class ExplodeLineLoopToDots : BaseObject
    {
        List<Vector2[]> points = new List<Vector2[]>();

        List<Dot> dots;

        int count;
        private int prevLength;

        public ExplodeLineLoopToDots(Mesh _mesh, int countPerLine)
        {
            for (int i = 0; i < _mesh.vertices.Length; ++i)
            {
                points.Add(ExtensionMethods.LineToDots(_mesh.vertices[i], _mesh.vertices[i < _mesh.vertices.Length -1? i + 1 : 0], countPerLine));
                count += countPerLine;
            }

            if (points.Count <= 0)
            {
                Destroy();
                return;
            }

            dots = new List<Dot>();

            for (int i = 0; i < points.Count; ++i)
                for (int e = 0; e < points[i].Length; ++e)
                    dots.Add(new Dot(points[i][e], TankGame.random.OnScaledCircle(-2, 2), TankGame.random.Range(2, 5), 1));

            meshes.Add(new Mesh(count, this, _mesh.color, PrimitiveType.Points));
            position        = _mesh.worldPosition;
            meshes[0].forward = _mesh.forward;

            Initialize();
        }
        public ExplodeLineLoopToDots(Mesh _mesh, int countPerLine, float lifeTime)
        {
            for (int i = 0; i < _mesh.vertices.Length; ++i)
            {
                points.Add(ExtensionMethods.LineToDots(_mesh.vertices[i], _mesh.vertices[i < _mesh.vertices.Length - 1 ? i + 1 : 0], countPerLine));
                count += countPerLine;
            }

            if (points.Count <= 0)
            {
                Destroy();
                return;
            }

            dots = new List<Dot>();

            for (int i = 0; i < points.Count; ++i)
                for (int e = 0; e < points[i].Length; ++e)
                    dots.Add(new Dot(points[i][e], TankGame.random.OnScaledCircle(-2, 2), lifeTime, 1));

            meshes.Add(new Mesh(count, this, _mesh.color, PrimitiveType.Points));
            position = _mesh.worldPosition;
            meshes[0].forward = _mesh.forward;

            Initialize();
        }

        public override void Update()
        {
            for (int i = 0; i < dots.Count; ++i) if (!dots[i].alive) dots.Remove(dots[i]);

            if (prevLength != dots.Count)
                meshes[0].vertices = new Vector2[dots.Count];

            prevLength = dots.Count;

            if (prevLength == 0)
            {
                Destroy();
                return;
            }

            for (int i = 0; i < dots.Count; ++i)
            {
                dots[i].Update(Time.deltatime);
                meshes[0].vertices[i] = dots[i].position;
            }
        }
    }
}
