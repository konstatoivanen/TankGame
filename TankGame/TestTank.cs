using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace TankGame
{
    class TestTank
    {
        public List<Mesh> CreateMeshes()
        {
            List<Mesh> Meshes = new List<Mesh>();

            Mesh bodyMesh = new Mesh(4);
            bodyMesh.Vertices[0] = new Vector2(-2, 1);
            bodyMesh.Vertices[1] = new Vector2(2, 1);
            bodyMesh.Vertices[2] = new Vector2(2, -1);
            bodyMesh.Vertices[3] = new Vector2(-2, -1);
            Meshes.Add(bodyMesh);
            
            //Tower
            Mesh towerMesh = new Mesh(4);
            towerMesh.Vertices[0] = new Vector2(-.75f, .75f);
            towerMesh.Vertices[1] = new Vector2(.75f, .75f);
            towerMesh.Vertices[2] = new Vector2(.75f, -.75f);
            towerMesh.Vertices[3] = new Vector2(-.75f, -.75f);
            Meshes.Add(towerMesh);

            //Cannon
            Mesh cannonMesh = new Mesh(4);
            cannonMesh.Vertices[0] = new Vector2(.75f, .15f);
            cannonMesh.Vertices[1] = new Vector2(3, .15f);
            cannonMesh.Vertices[2] = new Vector2(3, -.15f);
            cannonMesh.Vertices[3] = new Vector2(.75f, -.15f);
            Meshes.Add(cannonMesh);

            return Meshes;
        }
    }
}
