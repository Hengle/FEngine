using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class BarycentricCoordinates
    {
        public static void barycentricCoordinates(VInt3 p, VInt3 a, VInt3 b, ref VFixedPoint v)
        {
            VInt3 v0 = a - p;
            VInt3 v1 = b - p;
            VInt3 d = v1 - v0;
            VFixedPoint denominator = VInt3.Dot(d, d);
            VFixedPoint numerator = VInt3.Dot(-v0, d);
            v = numerator / denominator;
        }

        public static void barycentricCoordinates(VInt3 p, VInt3 a, VInt3 b, VInt3 c, ref VFixedPoint v, ref VFixedPoint w)
        {
            VInt3 ab = b - a;
            VInt3 ac = c - a;

            VInt3 n = VInt3.Cross(ab, ac);

            VInt3 bCrossC = VInt3.Cross(b, c);
            VInt3 cCrossA = VInt3.Cross(c, a);
            VInt3 aCrossB = VInt3.Cross(a, b);

            VFixedPoint va = VInt3.Dot(n, bCrossC);//edge region of BC, signed area rbc, u = S(rbc)/S(abc) for a
            VFixedPoint vb = VInt3.Dot(n, cCrossA);//edge region of AC, signed area rac, v = S(rca)/S(abc) for b
            VFixedPoint vc = VInt3.Dot(n, aCrossB);//edge region of AB, signed area rab, w = S(rab)/S(abc) for c
            VFixedPoint totalArea = va + vb + vc;

            VFixedPoint denom = totalArea == VFixedPoint.Zero ? VFixedPoint.Zero : VFixedPoint.One / totalArea;
            v = vb * denom;
            w = vc * denom;
        }
    }
}
