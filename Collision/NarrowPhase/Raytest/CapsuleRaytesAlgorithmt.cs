using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class CapsuleRaytestAlgorithm
    {
        //Interset segment sa, sb against cylinder specified by p, d and r
        static bool IntersectSegmentCylinder(VInt3  sa, VInt3 sb, VInt3 p, VInt3 q, VFixedPoint r, ref VInt3 normal, ref VFixedPoint t)
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
            if (a.Abs() < Globals.EPS)
            {
                return false;
            }

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

            VInt3 hitPoint = sa + n * t;
            VFixedPoint param = VFixedPoint.Zero;
            Distance.distancePointSegmentSquared(p, q, hitPoint, ref param);
            if (param <= VFixedPoint.Zero || param >= VFixedPoint.One)
                return false;
            VInt3 closestPoint = p * (VFixedPoint.One - param) + q * param;
            normal = (hitPoint - closestPoint) / r;
            return true;
        }

        public static bool raycastCapsule(VInt3 from, VInt3 to, VInt3 p0, VInt3 p1, VFixedPoint radius, ref VInt3 hitNormal, ref VFixedPoint t)
        {
            t = VFixedPoint.One;
            VInt3 capPos = (p0 + p1) * VFixedPoint.Half;
            VInt3 capsDir = p0 - p1;
            VFixedPoint kW = capsDir.sqrMagnitude;

            if (kW < Globals.EPS2)
            {
                t = VFixedPoint.Zero;
                hitNormal = VInt3.zero;
                if (SphereRaytestAlgorithm.rayTestSphere(from, to, capPos, radius, ref hitNormal, ref t))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            VFixedPoint tmp = VFixedPoint.Zero;
            if (Distance.distancePointSegmentSquared(p0, p1, from, ref tmp) < radius * radius)
                return false;

            VFixedPoint tmpT = VFixedPoint.One;
            VInt3 tmpNormal = VInt3.zero;
            bool hitCyclinder = IntersectSegmentCylinder(from, to, p0, p1, radius, ref tmpNormal, ref tmpT);
            if(hitCyclinder)
            {
                hitNormal = tmpNormal;
                t = tmpT;
            }

            bool hitSphere1 = SphereRaytestAlgorithm.rayTestSphere(from, to, p0, radius, ref tmpNormal, ref tmpT);
            if (hitSphere1 && tmpT < t)
            {
                t = tmpT;
                hitNormal = tmpNormal;
            }
            bool hitSphere2 = SphereRaytestAlgorithm.rayTestSphere(from, to, p1, radius, ref tmpNormal, ref tmpT);
            if (hitSphere2 && tmpT < t)
            {
                t = tmpT;
                hitNormal = tmpNormal;
            }

            return hitCyclinder || hitSphere1 || hitSphere2; 
        }

        public static void rayTestSingle(VInt3 fromPos, VInt3 toPos, CollisionObject collisionObject, RayResultCallback resultCallback)
        {
            CapsuleShape capsule = (CapsuleShape)collisionObject.getCollisionShape();
            VInt3 p0 = collisionObject.getWorldTransform().TransformPoint(capsule.getUpAxis() * capsule.getHalfHeight());
            VInt3 p1 = collisionObject.getWorldTransform().TransformPoint(capsule.getUpAxis() * -capsule.getHalfHeight());

            VInt3 normal = VInt3.zero; VFixedPoint t = VFixedPoint.Zero;
            if(raycastCapsule(fromPos, toPos, p0, p1, capsule.getRadius(), ref normal, ref t))
            {
                resultCallback.addSingleResult(collisionObject, normal, t);
            }            
        }
    }
}
