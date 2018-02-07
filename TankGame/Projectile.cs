using OpenTK;
using System.Collections.Generic;
using Utils;
using Physics;
using System;

namespace TankGame
{
    class Projectile : BaseObject //Lauri
    {
        public PhysicsLayer Mask;
        public RayCastHit Hit;
        public Projectile(Vector2 pos, Vector2 fwd, PhysicsLayer mask)
        {
            Mesh projectileMesh = new Mesh(4, this);
            projectileMesh.vertices[0] = new Vector2(-.3f, .2f);
            projectileMesh.vertices[1] = new Vector2(.3f, .2f);
            projectileMesh.vertices[2] = new Vector2(.3f, -.2f);
            projectileMesh.vertices[3] = new Vector2(-.3f, -.2f);
            projectileMesh.color = System.Drawing.Color.Orange;
            projectileMesh.renderMode = OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop;
            mesh = new List<Mesh>();
            mesh.Add(projectileMesh);
            position = pos;
            forward = fwd;
            Mask = mask;

            TankGame.AddMeshesToRenderStack(mesh);

            TankGame.OnUpdate += Update;
        }
        int tickCount = 0;
        int maxLifeTime = 1000;
        Vector2 previousPos;
        public override void Update()
        {
            previousPos = position;
            Move(15f * Time.deltatime);

            tickCount++;
            if (tickCount > maxLifeTime)
                Destroy();

            PhysicsUpdate();
        }
        public void PhysicsUpdate()
        {
            if (!Physics.Physics.RayCast(previousPos, position - previousPos, Mask, ref Hit))
                return;

            Vector2 dir = (position - previousPos).Normalized();
            forward = ExtensionMethods.Reflect(dir, Hit.cp.normal);
            position = Hit.cp.point + forward * 0.05f;
            previousPos = position;

            Sparks s = new Sparks(Hit.cp.point, 64, 4, new Vector2(0.05f, 1f), 1f);

            if (Math.Abs(ExtensionMethods.Angle(dir, forward)) < 2)
                return;
            
            Hit.other.parent.Destroy();
            Destroy();
        }

        public override void Destroy()
        {
            for (int i = 0; i < mesh.Count; i++)
                new ExplodeLineLoopToDots(mesh[i], 100);
            base.Destroy();
        }
        public void Move(float speed)
        {
            position += forward * speed;
        }
    }
}
