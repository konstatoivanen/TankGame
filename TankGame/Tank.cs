﻿using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Collections.Generic;
using Utils;
using Utils.Physics;
using System;

namespace TankGame
{
    public class Tank : BaseObject
    {
        private float acceleration;
        private float aimAcceleration;
        private float turnFactor;
        private bool  triggerDownPrev;

        InputScheme input;

        private float lts_current; //left track speed current
        private float rts_current; //right track speed current
        private float lts_target; //left track speed target
        private float rts_target; //left track speed target
        private float as_current; //aim speed current
        private float as_target; // aim speed target

        private Vector2 prevPos;
        float trackGap = 0.5f;
        float currentGap = 0f;

        public Tank(float acceleration0, float aimAcceleration0, float turnFactor0, Vector2 position0, Vector2 direction, Color color, InputScheme input0, PhysicsLayer layer)
        {
            List<Mesh> Meshes = new List<Mesh>();

            //Hull
            Meshes.Add(new Mesh(new Vector2[4] { new Vector2(-2, 1), new Vector2(2, 1), new Vector2(2, -1), new Vector2(-2, -1) }, this, color, PrimitiveType.LineLoop));

            //Forward Indicator
            Meshes.Add(new Mesh(new Vector2[2] { new Vector2(1.75f, 1), new Vector2(1.75f, -1) }, this, color, PrimitiveType.Lines));

            //Tower
            Meshes.Add(new Mesh(new Vector2[4] { new Vector2(-.75f, .75f), new Vector2(.75f, .75f), new Vector2(.75f, -.75f), new Vector2(-.75f, -.75f) }, this, color, PrimitiveType.LineLoop));

            //Cannon
            Meshes.Add(new Mesh(new Vector2[4] { new Vector2(.75f, .15f), new Vector2(3, .15f), new Vector2(3, -.15f), new Vector2(.75f, -.15f) }, this, color, PrimitiveType.LineLoop));

            meshes          = Meshes;
            forward         = direction;
            position        = position0;
            prevPos         = position;

            acceleration    = acceleration0;
            aimAcceleration = aimAcceleration0;
            turnFactor      = turnFactor0;
            input           = input0;

            for(int i = 0; i < meshes.Count; ++i)
                meshes[i].forward = direction;
           
            collider = new Collider(this, meshes[0], layer);

            Initialize();
        }

        public override void Update()
        {
            InputUpdate();
            LocomotionUpdate(Time.deltatime);

            Physics.CollisonSolve_Mesh(this);      
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
        private void Fire()
        {
            //Dont fire when the muzzle is inside a collider
            if (Physics.PointMeshCollision(meshes[2].worldPosition + meshes[2].forward * 3.2f, collider.Layer))
                return;

            new Projectile(meshes[2].worldPosition + meshes[2].forward * 3.2f, meshes[2].forward, 20, collider.Layer);
            new MuzzleFlash(meshes[2].worldPosition + meshes[2].forward * 3.2f, meshes[2].forward);
        }

        private void LocomotionUpdate(float delta)
        {
            lts_current = ExtensionMethods.MoveTowards(lts_current, lts_target, acceleration * delta);
            rts_current = ExtensionMethods.MoveTowards(rts_current, rts_target, acceleration * delta);
            as_current = ExtensionMethods.MoveTowards(as_current, as_target, aimAcceleration * delta);

            //Rotate Tower
            meshes[2].Rotate(as_current * delta);
            meshes[3].Rotate(as_current * delta);

            //Rotate
            forward = forward.Rotate((lts_current - rts_current) * delta * turnFactor);

            //Move
            position += forward * (lts_current + rts_current) * delta;

            //Track marks
            currentGap += (position - prevPos).Length;
            
            if (currentGap >= trackGap)
            {       
                if (Vector2.Dot(forward, position - prevPos) > 0)
                { 
                    // forward
                    new TrackMarks(position - forward * 2f + right * 1f, -right, 0.8f, meshes[0].color, 5f);
                    new TrackMarks(position - forward * 2f + -right * 1f, right, 0.8f, meshes[0].color, 5f);
                }
                else
                {
                    // backward
                    new TrackMarks(position + forward * 2f + right * 1f, -right, 0.8f, meshes[0].color, 5f);
                    new TrackMarks(position + forward * 2f + -right * 1f, right, 0.8f, meshes[0].color, 5f);
                }

                currentGap = 0f;
            }

            prevPos = position;
        }
        public  void SetLocomotiontarget(float tl, float tr, float t)
        {
            lts_target = tl;
            rts_target = tr;
            as_target  = t;
        }

        public override void Destroy()
        {
            for (int i = 0; i < meshes.Count; i++)
                new ExplodeLineLoopToDots(meshes[i], 100);

            base.Destroy();
        }
    }
}
