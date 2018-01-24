using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace TankGame
{
    class Projectile : BaseObject //Lauri
    {
        public Projectile(Vector2 pos, Vector2 fwd)
        {
            Mesh projectileMesh = new Mesh(4);
            projectileMesh.vertices[0] = new Vector2(-.3f, .2f);
            projectileMesh.vertices[1] = new Vector2(.3f, .2f);
            projectileMesh.vertices[2] = new Vector2(.3f, -.2f);
            projectileMesh.vertices[3] = new Vector2(-.3f, -.2f);
            projectileMesh.color = System.Drawing.Color.Red;
            projectileMesh.renderMode = OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop;
            mesh = new List<Mesh>();
            mesh.Add(projectileMesh);
            Translate(pos);
            Rotate((fwd.Y < 0? -1: 1) * ExtensionMethods.Angle(forward, fwd));
            forward = fwd;

            TankGame.OnUpdate += Update;
        }
        int tickCount = 0;
        int maxLifeTime = 1000;
        public override void Update()
        {
            Move(5f * Time.deltatime);
            tickCount++;
            if (tickCount > maxLifeTime)
                Destroy();


        }
        public void Move(float speed)
        {
            Translate(position + forward * speed);
        }
    }
}
