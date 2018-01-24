using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Physics
{
    public class CirlceCollider
    {
        public Vector2 center;
        public float   radius;
    }

    public struct ContactPoint
    {
        public Vector2 point;
        public Vector2 normal;
    }

    public class Physics
    {
        public bool DetectCollision(CirlceCollider c1, CirlceCollider c2, ref ContactPoint cp)
        {
            //Axis-aligned bounding box
            if (c1.center.X + c1.radius   + c2.radius > c2.center.X
             && c1.center.X < c2.center.X + c1.radius + c2.radius
             && c1.center.Y + c1.radius   + c2.radius > c2.center.Y
             && c1.center.Y < c2.center.Y + c1.radius + c2.radius)
            {
                //AABBs overlap
                float distance = Math.Abs((c1.center - c2.center).Length);
                if (distance < c1.radius + c2.radius)
                {
                    //Circles collide WIP
                    cp.normal = Vector2.Normalize(c2.center - c1.center);
                    cp.point = (c1.center + (cp.normal * c1.radius));
                    return true;
                }
                else
                {
                    //AABBs overlap, but circles do not collide
                    return false;
                }
            }
            else
            {
                //AABBs do not overlap
                return false;
            }
        }
    }
}
