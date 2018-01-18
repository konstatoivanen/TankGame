using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGame
{
    class Mesh
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        // Vertex vectors should be offset of position
        public Vector3[] Vertices { get; set; }

        public Mesh(int vertexCount)
        {
            Vertices = new Vector3[vertexCount];
        }
    }
}
