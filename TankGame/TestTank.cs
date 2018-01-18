using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGame
{
    class TestTank
    {
        public List<Mesh> CreateMeshes()
        {
            List<Mesh> Meshes = new List<Mesh>();

            Mesh bodyMesh = new Mesh(4);
            bodyMesh.Vertices[0] = new Vector3(-2, 1, 0);
            bodyMesh.Vertices[1] = new Vector3(2, 1, 0);
            bodyMesh.Vertices[2] = new Vector3(2, -1, 0);
            bodyMesh.Vertices[3] = new Vector3(-2, -1, 0);
            Meshes.Add(bodyMesh);
            
            //Tower
            Mesh towerMesh = new Mesh(4);
            towerMesh.Vertices[0] = new Vector3(-.75f, .75f, 0);
            towerMesh.Vertices[1] = new Vector3(.75f, .75f, 0);
            towerMesh.Vertices[2] = new Vector3(.75f, -.75f, 0);
            towerMesh.Vertices[3] = new Vector3(-.75f, -.75f, 0);
            Meshes.Add(towerMesh);

            //Cannon
            Mesh cannonMesh = new Mesh(4);
            cannonMesh.Vertices[0] = new Vector3(.75f, .15f, 0);
            cannonMesh.Vertices[1] = new Vector3(3, .15f, 0);
            cannonMesh.Vertices[2] = new Vector3(3, -.15f, 0);
            cannonMesh.Vertices[3] = new Vector3(.75f, -.15f, 0);
            Meshes.Add(cannonMesh);

            return Meshes;
        }
    }
}
