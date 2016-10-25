using MobaGame.FixedMath;
using System.Text;

namespace MobaGame.Collision
{
    public class Triangle
    {
        public static VInt3 getPointOnTriangleClosestToPoint(VInt3 p, VInt3 a, VInt3 b, VInt3 c)
        {
            VInt3 ab = b - a;
            VInt3 ac = c - a;
            VInt3 ap = p - a;

            VFixedPoint d1 = VInt3.Dot(ab, ap);
            VFixedPoint d2 = VInt3.Dot(ac, ap);
            if(d1 <= VFixedPoint.Zero && d2 <= VFixedPoint.Zero)
            {
                return a;
            }

            VInt3 bp = p - b;
            VFixedPoint d3 = VInt3.Dot(ab, bp);
            VFixedPoint d4 = VInt3.Dot(ac, bp);
            if(d3 >= VFixedPoint.Zero && d4 <= d3)
            {
                return b;
            }

            VFixedPoint vc = d1 * d4 - d3 * d2;
            if(vc <= VFixedPoint.Zero && d1 >= VFixedPoint.Zero && d3 <= VFixedPoint.Zero)
            {
                return a + ab * (d1 / (d1 - d3));
            }

            VInt3 cp = p - c;
            VFixedPoint d5 = VInt3.Dot(ab, cp);
            VFixedPoint d6 = VInt3.Dot(ac, cp);
            if(d6 >= VFixedPoint.Zero && d5 <= d6)
            {
                return c;
            }

            VFixedPoint vb = d5 * d2 - d1 * d6;
            if(vb <= VFixedPoint.Zero && d2 >= VFixedPoint.Zero && d6 <= VFixedPoint.Zero)
            {
                return a + ac * (d2 / (d2 - d6));
            }

            VFixedPoint va = d3 * d6 - d5 * d4;
            if(va <= VFixedPoint.Zero && (d4 - d3) >= VFixedPoint.Zero && (d5 - d6) >= VFixedPoint.Zero)
            {
                return b + (c - b) * ((d4 - d3) / ((d4 - d3) + (d5 - d6)));
            }

            VFixedPoint denom = VFixedPoint.One / (va + vb + vc);
            VFixedPoint v = vb * denom;
            VFixedPoint w = vc * denom;
            return a + ab * v + ac * w;
        }
    }
}
