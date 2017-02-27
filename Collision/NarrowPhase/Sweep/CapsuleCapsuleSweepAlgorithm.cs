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
        
        public static bool sweepCapsuleCapsule(CapsuleShape lss0, VIntTransform transform0, VInt3 FromPos, VInt3 toPos, CapsuleShape lss1, VIntTransform transform1, ref VFixedPoint dist, ref VInt3 hitNormal)
        {
            transform0.position = FromPos;
            VFixedPoint radiusSun = lss0.getRadius() + lss1.getRadius();
            VFixedPoint length = (toPos - FromPos).magnitude;
            VInt3 dir = (toPos - FromPos) / length;

            VInt3 lss0p0 = transform0.TransformPoint(lss0.getUpAxis() * lss0.getHalfHeight()), lss0p1 = transform0.TransformPoint(lss0.getUpAxis() * -lss0.getHalfHeight());
            VInt3 lss1p0 = transform1.TransformPoint(lss1.getUpAxis() * lss1.getHalfHeight()), lss1p1 = transform1.TransformPoint(lss1.getUpAxis() * -lss1.getHalfHeight());

            bool initialOverlapStatus = false;
            VFixedPoint tmp = VFixedPoint.Zero;
            if(lss0.getHalfHeight() < Globals.EPS)
            {
                initialOverlapStatus = Distance.distancePointSegmentSquared(lss1p0, lss1p1, lss0p0, ref tmp) < radiusSun * radiusSun;
            }
            else if(lss1.getHalfHeight() < Globals.EPS)
            {
                initialOverlapStatus = Distance.distancePointSegmentSquared(lss0p0, lss0p1, lss1p0, ref tmp) < radiusSun * radiusSun;
            }
            else
            {
                VInt3 x, y;
                initialOverlapStatus = Distance.SegmentSegmentDist2(lss0p0, lss0p1 - lss0p0, lss1p0, lss1p1 - lss1p0, out x, out y) < radiusSun * radiusSun;
            }

            if(initialOverlapStatus)
            {
                dist = VFixedPoint.Zero;
                hitNormal = (FromPos - toPos).Normalize();
                return true;
            }

            // 1. Extrude lss0 by lss1's length
            // 2. Inflate extruded shape by lss1's radius
            // 3. Raycast against resulting quad
            VInt3 D = (lss1p1 - lss1p0) * VFixedPoint.Half;
            VInt3 p0 = lss0p0 - D, p1 = lss0p1 - D, p0b = lss0p0 + D, p1b = lss0p1 + D;
            VInt3 normal = VInt3.Cross(p1b - p0b, p1 - p0b); normal = normal.Normalize();
            dist = VFixedPoint.One; bool status = false;

            VInt3 pa, pb, pc;
            if(VInt3.Dot(normal, dir) >= VFixedPoint.Zero)
            {
                pc = p0 - normal * radiusSun;
                pa = p1 - normal * radiusSun;
                pb = p1b - normal * radiusSun;
            }
            else
            {
                pc = p0 + normal * radiusSun;
                pa = p1 + normal * radiusSun;
                pb = p1b + normal * radiusSun;
            }
            VFixedPoint t = VFixedPoint.Zero, u = VFixedPoint.Zero, v = VFixedPoint.Zero;
            if(rayQuad(transform1.position, dir, pa, pb, pc, ref t, ref u, ref v, true) && t >= VFixedPoint.Zero && t < length)
            {
                dist = t / length;
                status = true;
            }

            if(!status)
            {
                VInt3[] caps = new VInt3[]
                {
                    p0, p1, p1, p1b, p1b, p0b, p0, p0b
                };
                VInt3 tmpNormal = VInt3.zero;
                for(int i = 0; i < 4; i++)
                {
                    VFixedPoint s = VFixedPoint.Zero;
                    if(CapsuleRaytestAlgorithm.raycastCapsule(FromPos, toPos, caps[i * 2], caps[i * 2 + 1], radiusSun, ref tmpNormal, ref s))
                    {
                        if(s > VFixedPoint.Zero && s < VFixedPoint.One)
                        {
                            dist = s;
                            status = true;
                        }
                    }
                }
            }

            if(status)
            {
                VInt3 x, y;
                Distance.SegmentSegmentDist2(lss0p0 + dir * length * dist, lss0p1 - lss0p0, lss1p0, lss1p1 - lss1p0, out x, out y);
                hitNormal = (y - x).Normalize();
            }
            return status;
        }

        public static void objectQuerySingle(CollisionObject castObject, VInt3 FromPos, VInt3 ToPos, CollisionObject collisionObject, List<CastResult> results, VFixedPoint allowedPenetration)
        {
            CapsuleShape lss0 = (CapsuleShape)castObject.getCollisionShape();
            CapsuleShape lss1 = (CapsuleShape)collisionObject.getCollisionShape();
            VFixedPoint t = VFixedPoint.One; VInt3 hitNormal = VInt3.zero;
            if(sweepCapsuleCapsule(lss0, castObject.getWorldTransform(), FromPos, ToPos, lss1, collisionObject.getWorldTransform(), ref t, ref hitNormal))
            {
                CastResult result = new CastResult();
                result.fraction = t;
                result.hitObject = collisionObject;
                result.hitPoint = hitNormal;
                results.Add(result);
            }
        }
    }
}
