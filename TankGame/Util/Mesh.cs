using OpenTK;

namespace TankGame
{
    class Mesh
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        // Vertex vectors should be offsets of position
        public Vector2[] Vertices { get; set; }

        public Mesh(int vertexCount)
        {
            Vertices = new Vector2[vertexCount];
        }
    }
}
