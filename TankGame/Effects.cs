using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

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
            
            Mesh ExplosionMesh = new Mesh(verts.Length);
            ExplosionMesh.vertices = verts;
            ExplosionMesh.color = System.Drawing.Color.OrangeRed;
            ExplosionMesh.renderMode = OpenTK.Graphics.OpenGL.PrimitiveType.Quads;
            mesh = new List<Mesh>();
            mesh.Add(ExplosionMesh);
            Translate(pos);
            if (fwd.Y < 0)
                Rotate(-ExtensionMethods.Angle(forward, fwd));
            else
                Rotate(ExtensionMethods.Angle(forward, fwd));
            forward = fwd;


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
}
