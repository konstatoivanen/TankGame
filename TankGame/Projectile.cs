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
        public Projectile(Vector2 pos, Vector2 fwd) // Vectors from parent
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
            if(fwd.Y < 0)
                Rotate(-MathShit.Angle(forward, fwd));
            else
                Rotate(MathShit.Angle(forward, fwd));


            TankGame.OnUpdate += Update;
        }
        public override void Update(float delta)
        {
            Move(5f*delta);
        }
        public void Move(float speed)
        {
            Translate(position + forward*speed);
        }
    }
}
