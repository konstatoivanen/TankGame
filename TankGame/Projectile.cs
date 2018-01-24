using OpenTK;
using System.Collections.Generic;
using Utils;

namespace TankGame
{
    class Projectile : BaseObject //Lauri
    {
        public Projectile(Vector2 pos, Vector2 fwd)
        {
            Mesh projectileMesh = new Mesh(4, this);
            projectileMesh.vertices[0] = new Vector2(-.3f, .2f);
            projectileMesh.vertices[1] = new Vector2(.3f, .2f);
            projectileMesh.vertices[2] = new Vector2(.3f, -.2f);
            projectileMesh.vertices[3] = new Vector2(-.3f, -.2f);
            projectileMesh.color = System.Drawing.Color.Orange;
            projectileMesh.renderMode = OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop;
            mesh = new List<Mesh>();
            mesh.Add(projectileMesh);
            position = pos;
            forward = fwd;

            TankGame.AddMeshesToRenderStack(mesh);

            TankGame.OnUpdate += Update;
        }
        int tickCount = 0;
        int maxLifeTime = 1000;
        public override void Update()
        {
            Move(15f * Time.deltatime);

            tickCount++;
            if (tickCount > maxLifeTime)
                Destroy();
        }
        public void Move(float speed)
        {
            position += forward * speed;
        }
    }
}
