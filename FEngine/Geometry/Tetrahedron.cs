using MobaGame.FixedMath;
using System.Text;

namespace MobaGame.Collision
{
    class Tetrahedron
    {
        public static VInt3 getPointOnTetrahedronClosestToPoint(VInt3 p, VInt3 a, VInt3 b, VInt3 c, VInt3 d)
        {
            VInt3 closestPt = p;
            VFixedPoint bestSqDist = VFixedPoint.MaxValue;
            if(PointOUtsideOfPlane(p, a, b, c, d))
            {
                VInt3 q = Triangle.getPointOnTriangleClosestToPoint(p, a, b, c);
                VFixedPoint sqDist = (q - p).sqrMagnitude;
                if(sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPt = q;
                }
            }

            if (PointOUtsideOfPlane(p, a, c, d, b))
            {
                VInt3 q = Triangle.getPointOnTriangleClosestToPoint(p, a, c, d);
                VFixedPoint sqDist = (q - p).sqrMagnitude;
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPt = q;
                }
            }

            if (PointOUtsideOfPlane(p, a, d, b, c))
            {
                VInt3 q = Triangle.getPointOnTriangleClosestToPoint(p, a, d, b);
                VFixedPoint sqDist = (q - p).sqrMagnitude;
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPt = q;
                }
            }

            if (PointOUtsideOfPlane(p, b, c, d, a))
            {
                VInt3 q = Triangle.getPointOnTriangleClosestToPoint(p, b, c, d);
                VFixedPoint sqDist = (q - p).sqrMagnitude;
                if (sqDist < bestSqDist)
                {
                    bestSqDist = sqDist;
                    closestPt = q;
                }
            }

            return closestPt;
        }

        public static bool PointOUtsideOfPlane(VInt3 p, VInt3 a, VInt3 b, VInt3 c, VInt3 d)
        {
            VFixedPoint signp = VInt3.Dot(p - a, VInt3.Cross(b - a, c - a));
            VFixedPoint signd = VInt3.Dot(d - a, VInt3.Cross(b - a, c - a));

            return signd * signp < VFixedPoint.Zero;
        }
    }
}
