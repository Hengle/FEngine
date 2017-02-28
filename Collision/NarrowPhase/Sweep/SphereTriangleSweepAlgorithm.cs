using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{ 
    class SphereTriangleSweepAlgorithm
    {
        static int rayTriSpecial(VInt3 orig, VInt3 dir, VInt3 vert0, VInt3 edge1, VInt3 edge2, ref VFixedPoint t, ref VFixedPoint u, ref VFixedPoint v)
        {
            VInt3 pvec = VInt3.Cross(dir, edge2);
            VFixedPoint det = VInt3.Dot(edge1, pvec);
            //triangle lies in plane of triangle
            if(det > -Globals.EPS && det < Globals.EPS)
            {
                return 0;
            }

            VFixedPoint oneOverDet = VFixedPoint.One / det;

            VInt3 tvec = orig - vert0;

            u = VInt3.Dot(tvec, pvec) * oneOverDet;

            VInt3 qvec = VInt3.Cross(tvec, edge1);

            v = VInt3.Dot(dir, qvec) * oneOverDet;

            if (u < VFixedPoint.Zero || u > VFixedPoint.One) return 1;
            if (v < VFixedPoint.Zero || u + v > VFixedPoint.One) return 1;

            t = VInt3.Dot(edge2, qvec) * oneOverDet;
            return 2;
        }

        static bool edgeOrVertexTest()
    }
}
