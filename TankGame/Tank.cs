using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Collections.Generic;
using Utils;

namespace TankGame
{
    public class Tank : BaseObject
    {
        private float acceleration;
        private float aimAcceleration;
        private float turnFactor;

        InputScheme input;

        private float lts_current; //left track speed current
        private float rts_current; //right track speed current
        private float lts_target; //left track speed target
        private float rts_target; //left track speed target
        private float as_current; //aim speed current
        private float as_target; // aim speed target

        public Tank(float acceleration0, float aimAcceleration0, float turnFactor0, Vector2 position0, Vector2 direction, Color color, InputScheme input0)
        {
            List<Mesh> Meshes = new List<Mesh>();

            Mesh bodyMesh           = new Mesh(4, this);
            bodyMesh.vertices[0]    = new Vector2(-2, 1);
            bodyMesh.vertices[1]    = new Vector2(2, 1);
            bodyMesh.vertices[2]    = new Vector2(2, -1);
            bodyMesh.vertices[3]    = new Vector2(-2, -1);
            Meshes.Add(bodyMesh);
            
            //Tower
            Mesh towerMesh          = new Mesh(4, this);
            towerMesh.vertices[0]   = new Vector2(-.75f, .75f);
            towerMesh.vertices[1]   = new Vector2(.75f, .75f);
            towerMesh.vertices[2]   = new Vector2(.75f, -.75f);
            towerMesh.vertices[3]   = new Vector2(-.75f, -.75f);
            Meshes.Add(towerMesh);

            //Cannon
            Mesh cannonMesh         = new Mesh(4, this);
            cannonMesh.vertices[0]  = new Vector2(.75f, .15f);
            cannonMesh.vertices[1]  = new Vector2(3, .15f);
            cannonMesh.vertices[2]  = new Vector2(3, -.15f);
            cannonMesh.vertices[3]  = new Vector2(.75f, -.15f);
            Meshes.Add(cannonMesh);

            mesh            = Meshes;
            forward         = direction;
            position        = position0;

            acceleration    = acceleration0;
            aimAcceleration = aimAcceleration0;
            turnFactor      = turnFactor0;
            input           = input0;

            for(int i = 0; i < mesh.Count; ++i)
            {
                mesh[i].forward     = direction;
                mesh[i].color       = color;
                mesh[i].renderMode = PrimitiveType.LineLoop;
            }

            TankGame.AddMeshesToRenderStack(mesh);

            TankGame.OnUpdate += Update;
        }

        public override void Update()
        {
            InputUpdate();
            LocomotionUpdate(Time.deltatime);
            //Physics stuff here
        }

        private void InputUpdate()
        {
            int axisL = TankGame.game.Keyboard[input.leftUp] ? 1 : 0;
            if (TankGame.game.Keyboard[input.leftDown]) axisL -= 1;

            int axisR = TankGame.game.Keyboard[input.rightUp] ? 1 : 0;
            if (TankGame.game.Keyboard[input.rightDown]) axisR -= 1;

            int axisT = TankGame.game.Keyboard[input.rightTurn] ? 1 : 0;
            if (TankGame.game.Keyboard[input.leftTurn]) axisT -= 1;

            SetLocomotiontarget(axisL, axisR, axisT);

            if (TankGame.game.Keyboard[input.fire]) Fire();
        }

        private void LocomotionUpdate(float delta)
        {
            lts_current = ExtensionMethods.MoveTowards(lts_current, lts_target, acceleration * delta);
            rts_current = ExtensionMethods.MoveTowards(rts_current, rts_target, acceleration * delta);
            as_current = ExtensionMethods.MoveTowards(as_current, as_target, aimAcceleration * delta);

            //Rotate Tower
            mesh[1].Rotate(as_current * delta);
            mesh[2].Rotate(as_current * delta);

            //Rotate
            forward = forward.Rotate((lts_current - rts_current) * delta * turnFactor);

            //Move
            position += forward * (lts_current + rts_current) * delta;
        }

        public void SetLocomotiontarget(float tl, float tr, float t)
        {
            lts_target = tl;
            rts_target = tr;
            as_target  = t;
        }

        private void Fire()
        {
			Projectile proj = new Projectile(mesh[2].worldPosition + mesh[2].forward * 3.2f, mesh[2].forward);
			TankGame.AddMeshesToRenderStack(proj.mesh);
			Explosion exp = new Explosion(mesh[2].worldPosition + mesh[2].forward * 3.2f, mesh[2].forward);
			TankGame.AddMeshesToRenderStack(exp.mesh);
        }
    }
}
