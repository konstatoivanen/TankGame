using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Collections.Generic;
using Utils;
using Utils.Physics;

namespace TankGame
{
    public class Tank : BaseObject
    {
        private float acceleration;
        private float aimAcceleration;
        private float turnFactor;

        private ContactPoint collisionContact;
        private Vector2 tempTest;

        private bool  triggerDownPrev;

        InputScheme input;

        private float lts_current; //left track speed current
        private float rts_current; //right track speed current
        private float lts_target; //left track speed target
        private float rts_target; //left track speed target
        private float as_current; //aim speed current
        private float as_target; // aim speed target

        public Tank(float acceleration0, float aimAcceleration0, float turnFactor0, Vector2 position0, Vector2 direction, Color color, InputScheme input0, PhysicsLayer layer)
        {
            List<Mesh> Meshes = new List<Mesh>();

            //Hull
            Meshes.Add(new Mesh(new Vector2[4] { new Vector2(-2, 1), new Vector2(2, 1), new Vector2(2, -1), new Vector2(-2, -1) }, this));

            //Tower
            Meshes.Add(new Mesh(new Vector2[4] { new Vector2(-.75f, .75f), new Vector2(.75f, .75f), new Vector2(.75f, -.75f), new Vector2(-.75f, -.75f) }, this));

            //Cannon
            Meshes.Add(new Mesh(new Vector2[4] { new Vector2(.75f, .15f), new Vector2(3, .15f), new Vector2(3, -.15f), new Vector2(.75f, -.15f) }, this));

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

            collider = new Collider(this, position, mesh[0], layer);

            Initialize();
        }

        public override void Update()
        {
            InputUpdate();
            LocomotionUpdate(Time.deltatime);

            if (!Physics.CollisionMesh(collider, ref collisionContact))
                return;

            position += collisionContact.normal;

            tempTest = position - (collisionContact.point - collisionContact.normal);
            tempTest.Normalize();

            float angle = ExtensionMethods.Angle(forward, tempTest);

            forward = forward.Rotate(angle * Time.deltatime * 0.5f);
        }

        private void InputUpdate()
        {
            int axisL = TankGame.game.Keyboard[input.leftUp] ? 1 : 0;
            if (TankGame.game.Keyboard[input.leftDown]) axisL -= 1;

            int axisR = TankGame.game.Keyboard[input.rightUp] ? 1 : 0;
            if (TankGame.game.Keyboard[input.rightDown]) axisR -= 1;

            int axisT = TankGame.game.Keyboard[input.leftTurn] ? 1 : 0;
            if (TankGame.game.Keyboard[input.rightTurn]) axisT -= 1;

            SetLocomotiontarget(axisL, axisR, axisT);

            if(TankGame.game.Keyboard[input.fire] != triggerDownPrev)
            {
                triggerDownPrev = TankGame.game.Keyboard[input.fire];
                if (triggerDownPrev) Fire();
            }
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
        public  void SetLocomotiontarget(float tl, float tr, float t)
        {
            lts_target = tl;
            rts_target = tr;
            as_target  = t;
        }
        private void Fire()
        {
			Projectile proj = new Projectile(mesh[2].worldPosition + mesh[2].forward * 3.2f, mesh[2].forward, collider.Layer);
            MuzzleFlash flash = new MuzzleFlash(mesh[2].worldPosition + mesh[2].forward * 3.2f, mesh[2].forward);
        }

        public override void Destroy()
        {
            for (int i = 0; i < mesh.Count; i++)
                new ExplodeLineLoopToDots(mesh[i], 100);

            base.Destroy();
        }
    }
}
