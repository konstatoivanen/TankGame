using OpenTK;
using Utils;
using Utils.Physics;
using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace TankGame
{
    class Projectile : BaseObject
    {
        public PhysicsLayer Mask;
        public RayCastHit Hit;
        public float speed;

        private float killTime;
        Vector2 previousPos;

        public Projectile(Vector2 pos, Vector2 fwd, float _speed, PhysicsLayer mask)
        {
            Mesh projectileMesh = new Mesh(4, this, Color.Orange, PrimitiveType.LineLoop);

            projectileMesh.vertices[0] = new Vector2(-.3f, .2f);
            projectileMesh.vertices[1] = new Vector2(.3f, .2f);
            projectileMesh.vertices[2] = new Vector2(.3f, -.2f);
            projectileMesh.vertices[3] = new Vector2(-.3f, -.2f);

            meshes.Add(projectileMesh);

            position    = pos;
            forward     = fwd;
            Mask        = mask;
            speed       = _speed;

            killTime = Time.time + 2;

            if (Physics.PointMeshCollision(position, Mask))
            {
                base.Destroy();
                return;
            }

            Initialize();
        }

        public override void Update()
        {
            previousPos  = position;
            position    += forward * speed * Time.deltatime;

            if (Time.time > killTime)
                Destroy();

            PhysicsUpdate();
        }
        public void PhysicsUpdate()
        {
            if (!Physics.RayCast(previousPos, position - previousPos, Mask, ref Hit))
                return;

            Vector2 dir = (position - previousPos).Normalized();
            forward     = ExtensionMethods.Reflect(dir, Hit.contact.normal);
            position    = Hit.contact.point + forward * 0.05f;
            previousPos = position;

            Sparks s = new Sparks(Hit.contact.point, 64, 4, new Vector2(0.05f, 1f), 1f);

            if (Math.Abs(ExtensionMethods.Angle(dir, forward)) < 2)
                return;

            //Destroy players
            //if (Hit.other.Layer != PhysicsLayer.Default)
                Hit.other.parent.Destroy();

            Destroy();
        }

        public override void Destroy()
        {
            for (int i = 0; i < meshes.Count; i++)
                new ExplodeLineLoopToDots(meshes[i], 100);

            base.Destroy();
        }
    }
}
