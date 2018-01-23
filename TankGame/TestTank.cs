using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace TankGame
{
    class TestTank : BaseObject
    {
        public List<Mesh> CreateMeshes()
        {
            List<Mesh> Meshes = new List<Mesh>();

            Mesh bodyMesh = new Mesh(4);
            bodyMesh.vertices[0] = new Vector2(-2, 1);
            bodyMesh.vertices[1] = new Vector2(2, 1);
            bodyMesh.vertices[2] = new Vector2(2, -1);
            bodyMesh.vertices[3] = new Vector2(-2, -1);
            bodyMesh.color = System.Drawing.Color.AliceBlue;
            bodyMesh.renderMode = OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop;
            bodyMesh.forward = new Vector2(10, 10);
            bodyMesh.forward = bodyMesh.forward.Rotate(1);
            Meshes.Add(bodyMesh);
            
            //Tower
            Mesh towerMesh = new Mesh(4);
            towerMesh.vertices[0] = new Vector2(-.75f, .75f);
            towerMesh.vertices[1] = new Vector2(.75f, .75f);
            towerMesh.vertices[2] = new Vector2(.75f, -.75f);
            towerMesh.vertices[3] = new Vector2(-.75f, -.75f);
            towerMesh.color = System.Drawing.Color.AliceBlue;
            towerMesh.renderMode = OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop;
            Meshes.Add(towerMesh);

            //Cannon
            Mesh cannonMesh = new Mesh(4);
            cannonMesh.vertices[0] = new Vector2(.75f, .15f);
            cannonMesh.vertices[1] = new Vector2(3, .15f);
            cannonMesh.vertices[2] = new Vector2(3, -.15f);
            cannonMesh.vertices[3] = new Vector2(.75f, -.15f);
            cannonMesh.color = System.Drawing.Color.AliceBlue;
            cannonMesh.renderMode = OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop;
            Meshes.Add(cannonMesh);

            return Meshes;
        }

        public override void Update()
        {
            
        }
    }
}
