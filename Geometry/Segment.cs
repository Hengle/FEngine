using MobaGame.FixedMath;
using System.Text;

namespace MobaGame.Collision
{
    public class Segment
    {
        public static VInt3 getPointOnSegmentClosestToPoint(VInt3 point, VInt3 linePoint1, VInt3 linePoint2)
        {
            VInt3 p1ToP = point - linePoint1;
            VInt3 line = linePoint2 - linePoint1;
            VFixedPoint ab2 = VInt3.Dot(line, line);
            VFixedPoint ap_ab = VInt3.Dot(p1ToP, line);
            VFixedPoint t = ap_ab / ab2;
            t = FMath.Clamp(t, VFixedPoint.Zero, VFixedPoint.One);
            return line * t + linePoint1;
        }
    }
}
