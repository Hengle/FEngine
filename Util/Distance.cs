using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public static class Distance
    {
        /*
        A segment is defined by S(t) = mP0 * (1 - t) + mP1 * t, with 0 <= t <= 1
        Alternatively, a segment is S(t) = Origin + t * Direction for 0 <= t <= 1.
        Direction is not necessarily unit length. The end points are Origin = mP0 and Origin + Direction = mP1.
        */
        public static VFixedPoint distancePointSegmentSquared(VInt3 p0, VInt3 p1, VInt3 point, ref VFixedPoint param)
        {
            VInt3 Diff = point - p0;
            VInt3 Dir = p1 - p0;
            VFixedPoint ft = VInt3.Dot(Diff, Dir);
            if(ft <= VFixedPoint.Zero)
            {
                ft = VFixedPoint.Zero;
            }
            else
            {
                VFixedPoint sqrLen = Dir.sqrMagnitude;
                if(ft >= sqrLen)
                {
                    ft = VFixedPoint.One;
                    Diff -= Dir;
                }
                else
                {
                    ft /= sqrLen;
                    Diff -= Dir * ft;
                }
            }

            param = ft;

            return Diff.sqrMagnitude;
        }
    }
}
