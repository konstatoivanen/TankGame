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
        Random random = new Random();

        public Explosion(Vector2 pos, Vector2 fwd)
        {
            Vector2[] verts = { new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0) };

            Mesh ExplosionMesh = new Mesh(verts, this, Color.OrangeRed, PrimitiveType.Quads);

            meshes.Add(ExplosionMesh);

            position    = pos;
            forward     = fwd;

            killTime = Time.time + 0.25f;

            Initialize();
        }

        public override void Update()
        {
            for (int i = 0; i < meshes[0].vertices.Length; i++)
                meshes[0].vertices[i] = meshes[0].vertices[i] + random.OnUnitCircle() * 0.4f + forward * 0.05f;

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
            position    = pos;
            velocity    = vel;
            killTime    = Time.time + lt;
            damping     = d;
            alive       = true;
        }

        public void Update(float delta)
        {
            if (!alive)
                return;

            position   += velocity * delta;
            velocity    = ExtensionMethods.MoveTowards(velocity, Vector2.Zero, damping * delta);

            if (Time.time > killTime)
                alive = false;
        }
    }

    public class Sparks : BaseObject
    {
        Random random = new Random();

        private List<Dot> dots;
        private int prevLength;

        public Sparks(Vector2 pos, int count, float maxSpeed, Vector2 mimMaxLifeTime, float damping)
        {
            position = pos;

            dots = new List<Dot>();

            for (int i = 0; i < count; ++i) dots.Add(new Dot(pos, random.OnScaledCircle(-maxSpeed, maxSpeed), random.Range(mimMaxLifeTime.X, mimMaxLifeTime.Y), damping));

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

    public class MuzzleFlashSmoke : BaseObject
    {
        float timer = 0;
        float lifeTime = 0.25f;
        Vector3 colorCurrent;

        Random random = new Random();

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
                meshes[0].vertices[i] = meshes[0].vertices[i] + random.OnUnitCircle() * 0.1f + new Vector2(1, 0) * 0.005f;

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

        Random random = new Random();

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
                meshes[0].vertices[i] = meshes[0].vertices[i] + random.OnUnitCircle() * 0.05f + new Vector2(1, 0) * 0.01f;

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
        Random random = new Random();

        public Obstacle(float size, float sizeVar, Vector2 pos)
        {
            
            Vector2[] v = new Vector2[3];

            Vector2 c = ExtensionMethods.GetPolyCenter(v);

            for (int i = 0; i < v.Length; ++i)
                v[i] = random.OnScaledCircle(size, size + sizeVar);

            for (int i = 0; i < v.Length; ++i)
                v[i] -= c;

            v = ExtensionMethods.SortPolyClockwise(v, c);

            Mesh ObstacleMesh = new Mesh(v, this, Color.Gray, PrimitiveType.LineLoop);
            meshes.Add(ObstacleMesh);
            position = pos;

            collider = new Collider(this, meshes[0], PhysicsLayer.Default);

            Initialize();
        }

        public override void Update()
        {
            
        }
    }

    public class ExplodeLineLoopToDots : BaseObject
    {
        List<Vector2[]> points = new List<Vector2[]>();

        Random random = new Random();
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
                    dots.Add(new Dot(points[i][e], random.OnScaledCircle(-2, 2), random.Range(2, 5), 1));

            meshes.Add(new Mesh(count, this, _mesh.color, PrimitiveType.Points));
            position        = _mesh.worldPosition;
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
