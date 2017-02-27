using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class CapsuleRaytestAlgorithm
    {
        //Interset segment sa, sb against cylinder specified by p, d and r
        public static bool IntersectSegmentCylinder(VInt3  sa, VInt3 sb, VInt3 p, VInt3 q, VFixedPoint r, ref VInt3 normal, ref VFixedPoint t)
        {
            if (r < Globals.EPS2) return false;

            VInt3 d = q - p, m = sa - p, n = sb - sa;
            VFixedPoint md = VInt3.Dot(m, d);
            VFixedPoint nd = VInt3.Dot(n, d);
            VFixedPoint dd = d.sqrMagnitude;

            if (md < VFixedPoint.Zero && md + nd < VFixedPoint.Zero) return false;
            if (md > dd && md + nd > dd) return false;

            VFixedPoint nn = n.sqrMagnitude;
            VFixedPoint mn = VInt3.Dot(m, n);
            VFixedPoint a = dd * nn - nd * nd;
            VFixedPoint k = m.sqrMagnitude - r * r;
            VFixedPoint c = dd * k - md * md;
            //if start point is within cylinder, not intersect
            if (c < VFixedPoint.Zero) return false;
            //if segment is parallel to cylinder, they do not intersect with each other
            /*if (a.Abs() < Globals.EPS)
            {
                if (c > VFixedPoint.Zero) return false;
                if (md < VFixedPoint.Zero) t = -mn / nn;
                else if (md > dd) t = (nd - mn) / nn;
                else t = VFixedPoint.Zero;
                return true;
            }*/

            VFixedPoint b = dd * mn - nd * md;
            VFixedPoint discr = b * b - a * c;
            if (discr < VFixedPoint.Zero) return false;
            t = (-b - FMath.Sqrt(discr)) / a;
            if (t < VFixedPoint.Zero || t > VFixedPoint.One) return false;
            //we don't need result of segment and endcap
            /*if(md + t * nd < VFixedPoint.Zero)
            {
                if (nd <= VFixedPoint.Zero) return false;
                t = -md / nd;
                return k + t * (mn + t * nn) * 2 <= VFixedPoint.Zero;
            }
            else if(md + t * nd > dd)
            {
                if (nd >= VFixedPoint.Zero) return false;
                t = (dd - md) / nd;
                return k + dd - md * 2 + t * ((md - nd) * 2 + t * nn) <= VFixedPoint.Zero;
            }*/

            VInt3 parallelDirVector = d * VInt3.Dot(d, n) / d.sqrMagnitude;
            VInt3 tmpNormal = (m - parallelDirVector).Normalize();
            return true;
        }

        public static void rayTestSingle(VInt3 fromPos, VInt3 toPos, CollisionObject collisionObject, RayResultCallback resultCallback)
        {
            CapsuleShape capsule = (CapsuleShape)collisionObject.getCollisionShape();
            VInt3 p0 = collisionObject.getWorldTransform().TransformPoint(capsule.getUpAxis() * capsule.getHalfHeight());
            VInt3 p1 = collisionObject.getWorldTransform().TransformPoint(capsule.getUpAxis() * -capsule.getHalfHeight());
            VInt3 capsDir = p0 - p1;
            VFixedPoint kW = capsDir.sqrMagnitude;

            if(kW < Globals.EPS2)
            {
                VFixedPoint t0 = VFixedPoint.Zero;
                VInt3 hitNormal = VInt3.zero;
                if(SphereRaytestAlgorithm.rayTestSphere(fromPos, toPos, collisionObject.getWorldTransform().position, capsule.getRadius(), ref hitNormal, ref t0 ))
                {
                    resultCallback.addSingleResult(collisionObject, hitNormal.Normalize(), t0);
                }
                else
                {
                    return;
                }
            }

            VFixedPoint t = VFixedPoint.One, tmpT = VFixedPoint.One;
            VInt3 normal = VInt3.zero, tmpNormal = VInt3.one;
            bool hitCylinder = IntersectSegmentCylinder(fromPos, toPos, p0, p1, capsule.getRadius(), ref normal, ref t);
            bool hitSphere1 = SphereRaytestAlgorithm.rayTestSphere(fromPos, toPos, p0, capsule.getRadius(), ref tmpNormal, ref tmpT);
            if(hitSphere1 && tmpT < t)
            {
                t = tmpT; normal = tmpNormal;
            }
            bool hitSphere2 = SphereRaytestAlgorithm.rayTestSphere(fromPos, toPos, p1, capsule.getRadius(), ref tmpNormal, ref tmpT);
            if (hitSphere2 && tmpT < t)
            {
                t = tmpT; normal = tmpNormal;
            }

            if(hitCylinder || hitSphere1 || hitSphere2)
            {
                resultCallback.addSingleResult(collisionObject, normal, t);
            }
        }
    }
}
