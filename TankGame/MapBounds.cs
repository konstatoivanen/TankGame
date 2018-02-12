using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using Utils.Physics;

namespace TankGame
{
    class MapBounds : BaseObject
    {

        List<BaseObject> Walls = new List<BaseObject>();

        public MapBounds()
        {
            TankGame.OnResize += Resize;
        }

        class Wall : BaseObject
        {
            public Wall(Vector2[] v, BaseObject p)
            {
                Vector2 center = Vector2.Zero;
                for (int i = 0; i < v.Length; i++)
                {
                    center += v[i];
                }
                center = center / v.Length;

                mesh = new List<Mesh>();
                mesh.Add(new Mesh(v, p, PrimitiveType.LineLoop));
                collider = new Collider(this, center, mesh[0], PhysicsLayer.Default);
                Initialize();
            }

            public override void Update()
            {
                //Debug.DrawLine(Vector2.Zero, collider.center, Color.Red);
                Debug.DrawNormals(mesh[0], Color.Red);
            }
        }
        

        public void Resize()
        {
            for (int i = 0; i < Walls.Count; i++)
            {
                Walls[i].Destroy();
            }
            Walls = GenerateWalls();
        }

        public override void Update()
        {
            /*for (int i = 0; i < Walls[0].mesh[0].vertices.Count(); i++)
            {
                if (i == Walls[0].mesh[0].vertices.Count() - 1)
                    Debug.DrawLine((Walls[0].mesh[0].vertices[i] + Walls[0].mesh[0].vertices[0]) / 2, (Walls[0].mesh[0].vertices[i] - Walls[0].mesh[0].vertices[0]).GetNormal(), Color.Red);
                else
                    Debug.DrawLine((Walls[0].mesh[0].vertices[i] + Walls[0].mesh[0].vertices[i + 1]) / 2, (Walls[0].mesh[0].vertices[i] - Walls[0].mesh[0].vertices[i + 1]).GetNormal(), Color.Red);
            }*/
        }

        public List<BaseObject> GenerateWalls()
        {
            Vector2[] corners = GetCorners();
            List<BaseObject> Walls = new List<BaseObject>();
            for (int i = 0; i < corners.Length; i++)
            {
                if (i == corners.Length - 1)
                    Walls.Add(new Wall(new Vector2[] { corners[i], corners[0], corners[0] + corners[0]*0.1f, corners[i] + corners[i] * 0.1f }, this));
                else
                    Walls.Add(new Wall(new Vector2[] {corners[i], corners[i+1], corners[i+1] + corners[i+1] * 0.1f, corners[i] + corners[i] * 0.1f }, this));
            }
            return Walls;
        }

        public Vector2[] GetCorners()
        {
            Vector2 topLeft = TankGame.battlefieldSize / 2 - new Vector2(1,1);
            Vector2 bottomRight = -topLeft;
            Vector2 topRight = new Vector2(bottomRight.X, topLeft.Y);
            Vector2 bottomLeft = new Vector2(topLeft.X, bottomRight.Y);
            Vector2[] corners = { topLeft, topRight, bottomRight, bottomLeft };

            return corners;
        }
    }
}
