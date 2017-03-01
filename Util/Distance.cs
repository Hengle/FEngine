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

        public static VInt3 closestPtPointTriangle2(VInt3 p, VInt3 a, VInt3 b, VInt3 c)
        {
            VInt3 ab = b - a;
            VInt3 ac = c - a;
            VInt3 ap = p - a;
            VFixedPoint d1 = VInt3.Dot(ab, ap);
            VFixedPoint d2 = VInt3.Dot(ac, ap);

            if (d1 <= VFixedPoint.Zero && d2 <= VFixedPoint.Zero)
                return a;

            VInt3 bp = p - b;
            VFixedPoint d3 = VInt3.Dot(ab, bp);
            VFixedPoint d4 = VInt3.Dot(ac, bp);

            if (d3 >= VFixedPoint.Zero && d4 <= d3)
                return b;

            VFixedPoint vc = d1 * d4 - d3 * d2;
            if (vc <= VFixedPoint.Zero && d1 >= VFixedPoint.Zero && d3 <= VFixedPoint.Zero)
            {
                VFixedPoint v = d1 / (d1 - d3);
                return a + ab * v;  // barycentric coords (1-v, v, 0)
            }

            VInt3 cp = p - c;
            VFixedPoint d5 = VInt3.Dot(ab, cp);
            VFixedPoint d6 = VInt3.Dot(ac, cp);
            if (d6 >= VFixedPoint.Zero && d5 <= d6)
                return c;   // Barycentric coords 0,0,1

            // Check if P in edge region of AC, if so return projection of P onto AC
            VFixedPoint vb = d5 * d2 - d1 * d6;
            if (vb <= VFixedPoint.Zero && d2 >= VFixedPoint.Zero && d6 <= VFixedPoint.Zero)
            {
                VFixedPoint w = d2 / (d2 - d6);
                return a + ac * w;  // barycentric coords (1-w, 0, w)
            }

            // Check if P in edge region of BC, if so return projection of P onto BC
            VFixedPoint va = d3 * d6 - d5 * d4;
            if (va <= VFixedPoint.Zero && (d4 - d3) >= VFixedPoint.Zero && (d5 - d6) >= VFixedPoint.Zero)
            {
                VFixedPoint w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                return b + (c - b) * w; // barycentric coords (0, 1-w, w)
            }


            // P inside face region. Compute Q through its barycentric coords (u,v,w)
            VFixedPoint denom = VFixedPoint.One / (va + vb + vc);
            VFixedPoint v = vb * denom;
            VFixedPoint w = vc * denom;
            return a + ab * v + ac * w;
        }
    }
}
