using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace TankGame
{
    class Tank : BaseObject
    {
        public Tank()
        {
            List<Mesh> Meshes = new List<Mesh>();

            Mesh bodyMesh = new Mesh(4);
            bodyMesh.vertices[0] = new Vector2(-2, 1);
            bodyMesh.vertices[1] = new Vector2(2, 1);
            bodyMesh.vertices[2] = new Vector2(2, -1);
            bodyMesh.vertices[3] = new Vector2(-2, -1);
            bodyMesh.color = System.Drawing.Color.AliceBlue;
            bodyMesh.renderMode = OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop;
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

            mesh = Meshes;
            position = Vector2.Zero;
            forward = new Vector2(1, 0);

            TankGame.OnUpdate += Update;
        }

        public override void Update(float delta)
        {
            if (TankGame.game.Keyboard[Key.Left])
                TurnTower(-1f * delta);
            if (TankGame.game.Keyboard[Key.Right])
                TurnTower(1f * delta);
            float turnFloat = 0;
            if (TankGame.game.Keyboard[Key.U]) turnFloat += 1f;
            if (TankGame.game.Keyboard[Key.K]) turnFloat += 1f;
            if (TankGame.game.Keyboard[Key.J]) turnFloat -= 1f;
            if (TankGame.game.Keyboard[Key.I]) turnFloat -= 1f;

            if (turnFloat != 0)
                TurnHull(turnFloat * delta);
            else if (TankGame.game.Keyboard[Key.U] && TankGame.game.Keyboard[Key.I])
            {
                MoveHull(1f*delta);
            }
            else if (TankGame.game.Keyboard[Key.J] && TankGame.game.Keyboard[Key.K])
            {
                MoveHull(-1f * delta);
            }
        }

        public void TurnTower(float radians)
        {
            mesh[1].Rotate(radians);
            mesh[2].Rotate(radians);
        }
        public void TurnHull(float radians)
        {
            Rotate(radians);
        }
        public void MoveHull(float speed)
        {
            Translate(position + forward*speed);
        }        
    }
}
