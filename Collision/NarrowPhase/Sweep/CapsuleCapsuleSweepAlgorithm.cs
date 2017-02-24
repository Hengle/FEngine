using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class CapsuleCapsuleSweepAlgorithm
    {

        public static bool rayQuad(VInt3 orig, VInt3 dir, VInt3 vert0, VInt3 vert1, VInt3 vert2, ref VFixedPoint t, ref VFixedPoint u, ref VFixedPoint v, bool cull)
        {
            VInt3 edge1 = vert1 - vert0;
            VInt3 edge2 = vert2 - vert0;
            VInt3 pvec = VInt3.Cross(dir, edge2);
            VFixedPoint det = VInt3.Dot(edge1, pvec);

            if (cull)
            {
                if (det < Globals.EPS)
                {
                    return false;
                }

                VInt3 tvec = orig - vert0;
                u = VInt3.Dot(tvec, pvec);
                if (u < VFixedPoint.Zero || u > det)
                {
                    return false;
                }

                VInt3 qvec = VInt3.Cross(tvec, edge1);

                v = VInt3.Dot(dir, qvec);
                if (v < VFixedPoint.Zero || v > det)
                {
                    return false;
                }

                t = VInt3.Dot(edge2, qvec);
                VFixedPoint oneOverDet = VFixedPoint.One / det;
                t *= oneOverDet;
                u *= oneOverDet;
                v *= oneOverDet;
            }
            else
            {
                if(det > -Globals.EPS && det < Globals.EPS)
                {
                    return false;
                }

                VFixedPoint oneOverDet = VFixedPoint.One / det;
                VInt3 tvec = orig - vert0;

                u = VInt3.Dot(tvec, pvec) * oneOverDet;
                if(u < VFixedPoint.Zero || u > VFixedPoint.One)
                {
                    return false;
                }

                VInt3 qvec = VInt3.Cross(tvec, edge1);
                v = VInt3.Dot(dir, qvec) * oneOverDet;
                if(v < VFixedPoint.Zero || v > VFixedPoint.One)
                {
                    return false;
                }

                t = (VInt3.Dot(edge2, qvec)) * oneOverDet;
            }
            return true;
        }
    }
}
