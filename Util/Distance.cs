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

        /*
        A segment is defined by S1(t) = P * t + a * (1 - t), S2(t) = q * t + b * (1 - t) with 0 <= t <= 1
        */
        public static VFixedPoint SegmentSegmentDist2 (VInt3 p, VInt3 a, //seg 1 origin, vector
                                               VInt3 q, VInt3 b, //seg 2 origin, vector
                                               out VInt3 x, out VInt3 y//closet points
                                                )
        {
            VInt3 T = q - p;
            VFixedPoint aDotA = a.sqrMagnitude;
            VFixedPoint bDotB = b.sqrMagnitude;
            VFixedPoint aDotB = VInt3.Dot(a, b);
            VFixedPoint aDotT = VInt3.Dot(a, T);
            VFixedPoint bDotT = VInt3.Dot(b, T);

            VFixedPoint Denom = aDotA * bDotB - aDotB * aDotB;
            VFixedPoint t = VFixedPoint.Zero;

            if(Denom != VFixedPoint.Zero)
            {
                t = (aDotT * bDotB - bDotT * aDotB) / Denom;
                if (t < VFixedPoint.Zero) t = VFixedPoint.Zero;
                else if (t > VFixedPoint.One) t = VFixedPoint.One;
            }
            else
            {
                t = VFixedPoint.Zero;
            }

            VFixedPoint u = VFixedPoint.Zero;
            if(bDotB != VFixedPoint.Zero)
            {
                u = (t * aDotB - bDotT) / bDotB;

                if(u < VFixedPoint.Zero)
                {
                    u = VFixedPoint.Zero;
                    if(aDotA != VFixedPoint.Zero)
                    {
                        t = aDotT / aDotA;
                        if (t < VFixedPoint.Zero) t = VFixedPoint.Zero;
                        else if (t > VFixedPoint.One) t = VFixedPoint.One;
                    }
                    else
                    {
                        t = VFixedPoint.Zero;
                    }
                }
                else if(u > VFixedPoint.One)
                {
                    u = VFixedPoint.One;
                    if(aDotA != VFixedPoint.One)
                    {
                        t = (aDotB + aDotT) / aDotA;
                        if (t < VFixedPoint.Zero) t = VFixedPoint.Zero;
                        else if (t > VFixedPoint.One) t = VFixedPoint.One;
                    }
                    else
                    {
                        t = VFixedPoint.Zero;
                    }
                }
            }
            else
            {
                u = VFixedPoint.One;
                if(aDotA != VFixedPoint.Zero)
                {
                    t = (aDotB + aDotT) / aDotA;
                    if (t < VFixedPoint.Zero) t = VFixedPoint.Zero;
                    else if (t > VFixedPoint.One) t = VFixedPoint.One;
                }
                else
                {
                    t = VFixedPoint.Zero;
                }
            }
            x = p + a * t;
            y = q + b * t;
            return (x - y).sqrMagnitude;
       }
    }
}
