using OpenTK;
using System;
using System.Collections.Generic;
using Utils;
using System.Drawing;
using OpenTK.Graphics.OpenGL;


namespace TankGame
{
    public class Explosion : BaseObject
    {
        int tickCount = 0;
        int lifeTime = 40;
        Random random = new Random();

        public Explosion(Vector2 pos, Vector2 fwd)
        {
            Vector2[] verts = { new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0) };

            Mesh ExplosionMesh = new Mesh(verts.Length, this);
            ExplosionMesh.vertices = verts;
            ExplosionMesh.color = Color.OrangeRed;
            ExplosionMesh.renderMode = PrimitiveType.Quads;
            mesh = new List<Mesh>();
            mesh.Add(ExplosionMesh);
            position = pos;
            forward = fwd;

            TankGame.AddMeshesToRenderStack(mesh);

            TankGame.OnUpdate += Update;
        }

        public override void Update()
        {
            for (int i = 0; i < mesh[0].vertices.Length; i++)
            {
                mesh[0].vertices[i] = mesh[0].vertices[i] + new Vector2((float)random.NextDouble() - (float)random.NextDouble(), (float)random.NextDouble() - (float)random.NextDouble()) * 0.4f + forward * 0.05f;
            }
            tickCount++;
            if (tickCount > lifeTime)
                Destroy();
        }
    }

    public class Sparks : BaseObject
    {
        Random random = new Random();

        public class Dot
        {
            public Vector2 position;
            public Vector2 velocity;

            public float lifeTimeLeft;
            public float damping;
            public bool alive;

            public Dot(Vector2 pos, Vector2 vel, float lt, float d)
            {
                position = pos;
                velocity = vel;
                lifeTimeLeft = lt;
                damping = d;
                alive = true;
            }

            public void Update(float delta)
            {
                if (!alive)
                    return;

                position += velocity * delta;
                lifeTimeLeft -= delta;
                velocity = ExtensionMethods.MoveTowards(velocity, Vector2.Zero, damping * delta);

                if (lifeTimeLeft <= 0) alive = false;
            }
        }

        private List<Dot> dots;
        private int prevLength;

        public Sparks(Vector2 pos, int count, float maxSpeed, Vector2 mimMaxLifeTime, float damping)
        {
            position = pos;

            dots = new List<Dot>();

            for (int i = 0; i < count; ++i) dots.Add(new Dot(pos, new Vector2(ExtensionMethods.Lerp(-maxSpeed, maxSpeed, (float)random.NextDouble()), ExtensionMethods.Lerp(-maxSpeed, maxSpeed, (float)random.NextDouble())), ExtensionMethods.Lerp(mimMaxLifeTime.X, mimMaxLifeTime.Y, (float)random.NextDouble()), damping));

            mesh = new List<Mesh>();

            mesh.Add(new Mesh(count, this, Color.Orange, PrimitiveType.Points));

            prevLength = count;

            TankGame.AddMeshesToRenderStack(mesh);

            TankGame.OnUpdate += Update;
        }

        public override void Update()
        {
            for (int i = 0; i < dots.Count; ++i) if (!dots[i].alive) dots.Remove(dots[i]);

            if (prevLength != dots.Count)
                mesh[0].vertices = new Vector2[dots.Count];

            prevLength = dots.Count;

            if (prevLength == 0)
            {
                Destroy();
                return;
            }

            for (int i = 0; i < dots.Count; ++i)
            {
                dots[i].Update(Time.deltatime);
                mesh[0].vertices[i] = position - dots[i].position;
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
            mesh = new List<Mesh>();
            mesh.Add(SmokeMesh);

            position = pos;

            // Fixes mesh facing wrong way when tank spawns and hasn't rotated at all
            SmokeMesh.forward = fwd;
            //forward = fwd;

            //Convert color to vector3
            colorCurrent = SmokeMesh.color.ToVector();

            TankGame.AddMeshesToRenderStack(mesh);

            TankGame.OnUpdate += Update;
        }

        public override void Update()
        {
            for (int i = 0; i < mesh[0].vertices.Length; i++)
            {
                mesh[0].vertices[i] = mesh[0].vertices[i] + new Vector2((float)random.NextDouble() - (float)random.NextDouble(), (float)random.NextDouble() - (float)random.NextDouble()) * 0.1f + new Vector2(1, 0) * 0.005f;
            }

            //Move current color towards 0
            colorCurrent = ExtensionMethods.MoveTowards(colorCurrent, Vector3.Zero, Time.deltatime * (1f / lifeTime));

            //Convert vector3 back to color
            mesh[0].color = colorCurrent.ToColor();

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

            mesh = new List<Mesh>();
            mesh.Add(MuzzleFlashMesh);

            position = pos;

            // Fixes mesh facing wrong way when tank spawns and hasn't rotated at all
            MuzzleFlashMesh.forward = fwd;
            //forward = fwd;

            colorCurrent = MuzzleFlashMesh.color.ToVector();

            TankGame.AddMeshesToRenderStack(mesh);

            TankGame.OnUpdate += Update;
        }

        public override void Update()
        {
            for (int i = 0; i < mesh[0].vertices.Length; i++)
            {
                mesh[0].vertices[i] = mesh[0].vertices[i] + new Vector2((float)random.NextDouble() - (float)random.NextDouble(), (float)random.NextDouble() - (float)random.NextDouble()) * 0.05f + new Vector2(1, 0) * 0.01f;
            }

            //Move current color towards 0
            colorCurrent = ExtensionMethods.MoveTowards(colorCurrent, Vector3.Zero, Time.deltatime * (1f / lifeTime));

            //Convert vector3 back to color
            mesh[0].color = colorCurrent.ToColor();

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

    public class ExplodeLineLoopToDots : BaseObject
    {
        List<Vector2[]> points = new List<Vector2[]>();
        Random random = new Random();
        float maxSpeed = 2f;
        Vector2 mimMaxLifeTime = new Vector2(2, 5);
        float damping = 1f;
        List<Sparks.Dot> dots;
        int count;
        private int prevLength;
        public ExplodeLineLoopToDots(Mesh _mesh, int countPerLine)
        {
            for (int i = 0; i < _mesh.vertices.Length; i++)
            {
                if (i + 1 == _mesh.vertices.Length)
                    points.Add(ExtensionMethods.LineToDots(_mesh.vertices[i], _mesh.vertices[0], countPerLine));
                else
                    points.Add(ExtensionMethods.LineToDots(_mesh.vertices[i], _mesh.vertices[i + 1], countPerLine));
                count += countPerLine;
            }
            if (points.Count > 0)
            {
                dots = new List<Sparks.Dot>();
                for (int i = 0; i < points.Count; i++)
                {
                    for (int e = 0; e < points[i].Length; e++)
                    {
                        dots.Add(new Sparks.Dot(points[i][e], new Vector2(ExtensionMethods.Lerp(-maxSpeed, maxSpeed, (float)random.NextDouble()), ExtensionMethods.Lerp(-maxSpeed, maxSpeed, (float)random.NextDouble())), ExtensionMethods.Lerp(mimMaxLifeTime.X, mimMaxLifeTime.Y, (float)random.NextDouble()), damping));
                    }

                }
                mesh = new List<Mesh>();

                mesh.Add(new Mesh(count, this, _mesh.color, PrimitiveType.Points));
                position = _mesh.worldPosition;
                mesh[0].forward = _mesh.forward;
                TankGame.AddMeshesToRenderStack(mesh);

                TankGame.OnUpdate += Update;
            }
        }            

        public override void Update()
        {
            for (int i = 0; i < dots.Count; ++i) if (!dots[i].alive) dots.Remove(dots[i]);

            if (prevLength != dots.Count)
                mesh[0].vertices = new Vector2[dots.Count];

            prevLength = dots.Count;

            if (prevLength == 0)
            {
                Destroy();
                return;
            }

            for (int i = 0; i < dots.Count; ++i)
            {
                dots[i].Update(Time.deltatime);
                mesh[0].vertices[i] = dots[i].position;
            }
        }
    }
}
