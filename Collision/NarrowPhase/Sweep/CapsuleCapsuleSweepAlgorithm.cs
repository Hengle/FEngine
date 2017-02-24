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
        
        public static bool sweepCapsuleCapsule(CapsuleShape lss0, VIntTransform transform0, VInt3 toPos, CapsuleShape lss1, VIntTransform transform1, ref VFixedPoint dist, ref VInt3 normal)
        {
            VFixedPoint radiusSun = lss0.getRadius() + lss1.getRadius();
            VInt3 center = transform1.position;

            bool initialOverlapStatus = false;
            if(lss0.getHalfHeight() < Globals.EPS)
            {

            }
            else if(lss1.getHalfHeight() < Globals.EPS)
            {

            }
            else
            {

            }

            if(initialOverlapStatus)
            {
                dist = VFixedPoint.Zero;
                normal = (transform0.position - toPos).Normalize();
                return true;
            }

            // 1. Extrude lss0 by lss1's length
            // 2. Inflate extruded shape by lss1's radius
            // 3. Raycast against resulting quad
        }
    }
}
