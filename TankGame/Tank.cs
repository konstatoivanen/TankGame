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
    public class Tank : BaseObject
    {
        private float acceleration;
        private float aimAcceleration;
        private float turnFactor;

        private float lts_current; //left track speed current
        private float rts_current; //right track speed current
        private float lts_target; //left track speed target
        private float rts_target; //left track speed target
        private float as_current; //aim speed current
        private float as_target; // aim speed target

        public Tank(float acceleration0, float aimAcceleration0, float turnFactor0)
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

            mesh        = Meshes;
            position    = Vector2.Zero;
            forward     = new Vector2(1, 0);

            acceleration    = acceleration0;
            aimAcceleration = aimAcceleration0;
            turnFactor      = turnFactor0;

            TankGame.OnUpdate += Update;
        }

        public override void Update(float delta)
        {
            int axisL = TankGame.game.Keyboard[Key.Keypad7]? 1 : 0;
            if (TankGame.game.Keyboard[Key.Keypad4]) axisL -= 1;

            int axisR = TankGame.game.Keyboard[Key.Keypad8] ? 1 : 0;
            if (TankGame.game.Keyboard[Key.Keypad5]) axisR -= 1;

            int axisT = TankGame.game.Keyboard[Key.Keypad9] ? 1 : 0;
            if (TankGame.game.Keyboard[Key.Keypad6]) axisT -= 1;

            SetLocomotiontarget(axisL, axisR, axisT);

            LocomotionUpdate(delta);

            if (TankGame.game.Keyboard[Key.Enter])
            {
                Projectile proj = new Projectile(mesh[2].position + mesh[2].forward * 3.2f, mesh[2].forward);
                TankGame.AddMeshesToRenderStack(proj.mesh);
            }
        }

        private void LocomotionUpdate(float delta)
        {
            lts_current = ExtensionMethods.MoveTowards(lts_current, lts_target, acceleration * delta);
            rts_current = ExtensionMethods.MoveTowards(rts_current, rts_target, acceleration * delta);
            as_current = ExtensionMethods.MoveTowards(as_current, as_target, aimAcceleration * delta);

            TurnTower(as_current * delta);

            TurnHull((lts_current - rts_current) * delta * turnFactor);

            MoveHull((lts_current + rts_current) * delta);
        }

        public void SetLocomotiontarget(float tl, float tr, float t)
        {
            lts_target = tl;
            rts_target = tr;
            as_target  = t;
        }

        private void TurnTower(float radians)
        {
            mesh[1].Rotate(radians);
            mesh[2].Rotate(radians);
        }
        private void TurnHull(float radians)
        {
            Rotate(radians);
        }
        private void MoveHull(float speed)
        {
            Translate(position + forward*speed);
        }        
    }
}
