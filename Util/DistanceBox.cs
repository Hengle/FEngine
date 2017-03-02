using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public static class DistanceBox
    {
        public static VFixedPoint distanceSegmentBoxSquared(VInt3 p0, VInt3 p1, VInt3 boxHalfExtent, VIntTransform boxTransform)
        {
            VFixedPoint lp = VFixedPoint.Zero;
            VInt3 bp = VInt3.zero;
            VFixedPoint sqrDistance = distanceLineBoxSquared(p0, p1 - p0, boxHalfExtent, boxTransform, ref lp, ref bp);

            if (lp >= VFixedPoint.Zero)
            {
                if (lp <= VFixedPoint.One)
                {
                    return sqrDistance;
                }
                else
                {
                    return distancePointBoxSquared(p1, boxTransform, boxHalfExtent);
                }
            }
            else
            {
                return distancePointBoxSquared(p0, boxTransform, boxHalfExtent);
            }
        }

        public static VFixedPoint distanceLineBoxSquared(VInt3 lineOrigin, VInt3 lineDirection,
								  VInt3 boxExtent, VIntTransform boxTransform, ref VFixedPoint lineParam, ref VInt3 boxParam)
        {
            // compute coordinates of line in box coordinate system
            VInt3 pnt = boxTransform.InverseTransformPoint(lineOrigin);
            VInt3 dir = boxTransform.InverseTransformDirection(lineDirection);

            // Apply reflections so that direction vector has nonnegative components.
            bool[] reflect = new bool[3];
            for (int i = 0; i < 3; i++)
            {
                if (dir[i] < VFixedPoint.Zero)
                {
                    pnt[i] = -pnt[i];
                    dir[i] = -dir[i];
                    reflect[i] = true;
                }
                else
                {
                    reflect[i] = false;
                }
            }

            VFixedPoint sqrDistance = VFixedPoint.Zero;

            if(dir.x > VFixedPoint.Zero)
            {
                if (dir.y > VFixedPoint.Zero)
                {
                    if (dir.z > VFixedPoint.Zero) caseNoZero(pnt, dir, boxExtent, ref lineParam, ref sqrDistance);
                    else case0(0, 1, 2, pnt, dir, boxExtent, ref lineParam, ref sqrDistance);
                }
                else
                {
                    if (dir.z > VFixedPoint.Zero) case0(0, 2, 1, pnt, dir, boxExtent, ref lineParam, ref sqrDistance);  // (+,0,+)
                    else case00(0, 1, 2, pnt, dir, boxExtent, ref lineParam, ref sqrDistance);	// (+,0,0)
                }
            }
            else
            {
                if (dir.y > VFixedPoint.Zero)
                {
                    if (dir.z > VFixedPoint.Zero) case0(1, 2, 0, pnt, dir, boxExtent, ref lineParam, ref sqrDistance);  // (0,+,+)
                    else case00(1, 0, 2, pnt, dir, boxExtent, ref lineParam, ref sqrDistance);  // (0,+,0)
                }
                else
                {
                    if (dir.z > VFixedPoint.Zero) case00(2, 0, 1, pnt, dir, boxExtent, ref lineParam, ref sqrDistance); // (0,0,+)
                    else
                    {
                        case000(pnt, boxExtent, sqrDistance);                                       // (0,0,0)
                        lineParam = VFixedPoint.Zero;
                    }
                }
            }

            for(int i = 0; i < 3; i++)
            {
                boxParam[i] = pnt[i] * (reflect[i] ? -1 : 1);
            }

            return sqrDistance;
        }

        public static VFixedPoint distancePointBoxSquared(VInt3 point, VIntTransform boxTransform, VInt3 boxExtent)
        {
            VInt3 closest = boxTransform.InverseTransformPoint(point);

            VFixedPoint sqrDistance = VFixedPoint.Zero;
            for(int ax = 0; ax < 3; ax++)
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

            return sqrDistance;
        }
    }
}
