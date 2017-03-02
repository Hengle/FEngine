using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public static class BoxPointDistance
    {
        public static VFixedPoint distancePointBoxSquared(VInt3 point, VIntTransform boxTransform, VInt3 boxExtent, ref VInt3 boxParam)
        {
            VInt3 closest = boxTransform.InverseTransformPoint(point);

            VFixedPoint sqrDistance = VFixedPoint.Zero;
            for (int ax = 0; ax < 3; ax++)
            {
                if (closest[ax] < -boxExtent[ax])
                {
                    VFixedPoint delta = closest[ax] + boxExtent[ax];
                    sqrDistance += delta * delta;
                    closest[ax] = -boxExtent[ax];
                }
                else if (closest[ax] > boxExtent[ax])
                {
                    VFixedPoint delta = closest[ax] - boxExtent[ax];
                    sqrDistance += delta * delta;
                    closest[ax] = boxExtent[ax];
                }
            }
            boxParam = closest;

            return sqrDistance;
        }
    }
}
