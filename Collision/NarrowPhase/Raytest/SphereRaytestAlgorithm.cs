using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class SphereRaytestAlgorithm
    {
        public static void rayTestSingle(VInt3 fromPos, VInt3 toPos, CollisionObject collisionObject, RayResultCallback resultCallback)
        {
            SphereShape sphereShape = (SphereShape)collisionObject.getCollisionShape();
            VInt3 objectPosition = collisionObject.getWorldTransform().position;
            VInt3 hitNormal = VInt3.zero;
            VFixedPoint t0 = VFixedPoint.Zero;
            if(rayTestSphere(fromPos, toPos, objectPosition, sphereShape.getRadius(), ref hitNormal, ref t0))
            {
                resultCallback.addSingleResult(collisionObject, hitNormal.Normalize(), t0);
            }
        }

        public static bool rayTestSphere(VInt3 fromPos, VInt3 toPos, VInt3 spherePosition, VFixedPoint radius, ref VInt3 hitNormal, ref VFixedPoint t0)
        { 
            if((fromPos - toPos).sqrMagnitude < radius * radius)
            {
                //if start point in sphere, not contact
                return false;
            }

            VInt3 d = (toPos - fromPos).Normalize();
            VInt3 p = fromPos;

            VFixedPoint tm = -VInt3.Dot(p, d);
            VFixedPoint lm2 = VInt3.Dot(p, p) - tm * tm;
            VFixedPoint deltaT = FMath.Sqrt(VFixedPoint.One - lm2);
            t0 = tm + deltaT;
            if(t0 > VFixedPoint.Zero && t0 < VFixedPoint.One)
            {
                VInt3 contactPoint = p + d * t0;
                hitNormal = contactPoint - spherePosition;
                return true;
            }
            return false;
        }
    }
}
