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
            
            Mesh ExplosionMesh          = new Mesh(verts.Length, this);
            ExplosionMesh.vertices      = verts;
            ExplosionMesh.color         = Color.OrangeRed;
            ExplosionMesh.renderMode    = PrimitiveType.Quads;
            mesh = new List<Mesh>();
            mesh.Add(ExplosionMesh);
            position    = pos;
            forward     = fwd;

            TankGame.AddMeshesToRenderStack(mesh);

            TankGame.OnUpdate += Update;
        }

        public override void Update()
        {
            for (int i = 0; i < mesh[0].vertices.Length; i++)
            {
                mesh[0].vertices[i] = mesh[0].vertices[i] + new Vector2((float)random.NextDouble() - (float)random.NextDouble(), (float)random.NextDouble() - (float)random.NextDouble())*0.4f + forward*0.05f;
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

            public float   lifeTimeLeft;
            public float   damping;
            public bool    alive;

            public Dot(Vector2 pos, Vector2 vel, float lt, float d)
            {
                position        = pos;
                velocity        = vel;
                lifeTimeLeft    = lt;
                damping         = d;
                alive           = true;
            }

            public void Update(float delta)
            {
                if (!alive)
                    return;

                position        += velocity * delta;
                lifeTimeLeft    -= delta;
                velocity         = ExtensionMethods.MoveTowards(velocity, Vector2.Zero, damping * delta);

                if (lifeTimeLeft <= 0) alive = false;
            }
        }

        private List<Dot> dots;
        private int prevLength;

        public Sparks(Vector2 pos, int count, float maxSpeed, Vector2 mimMaxLifeTime, float damping)
        {
            position = pos;

            dots = new List<Dot>();

            for(int i = 0; i < count; ++i) dots.Add(new Dot(pos, new Vector2(ExtensionMethods.Lerp(-maxSpeed, maxSpeed, (float)random.NextDouble()), ExtensionMethods.Lerp(-maxSpeed, maxSpeed, (float)random.NextDouble())), ExtensionMethods.Lerp(mimMaxLifeTime.X, mimMaxLifeTime.Y, (float)random.NextDouble()), damping));

            mesh = new List<Mesh>();

            mesh.Add(new Mesh(count, this, Color.Orange, PrimitiveType.Points ));

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
}
