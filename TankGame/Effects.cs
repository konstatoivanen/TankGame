using OpenTK;
using System;
using System.Collections.Generic;
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
            
            Mesh ExplosionMesh = new Mesh(verts.Length, this);
            ExplosionMesh.vertices = verts;
            ExplosionMesh.color = System.Drawing.Color.OrangeRed;
            ExplosionMesh.renderMode = OpenTK.Graphics.OpenGL.PrimitiveType.Quads;
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
}
