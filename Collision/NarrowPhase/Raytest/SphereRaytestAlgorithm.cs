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
            if((fromPos - spherePosition).sqrMagnitude < radius * radius)
            {
                //if start point in sphere, not contact
                return false;
            }

            VInt3 m = fromPos - spherePosition;
            VInt3 d = toPos - fromPos;
            VFixedPoint a = d.sqrMagnitude;
            VFixedPoint b = VInt3.Dot(m, d);
            VFixedPoint c = m.sqrMagnitude - radius * radius;
            if(c > VFixedPoint.Zero && b > VFixedPoint.Zero)
                return false;

            VFixedPoint discr = b * b - a * c;
            if (discr < VFixedPoint.Zero)
                return false;

            t0 = (-b - FMath.Sqrt(discr)) / a;
            if (t0 < VFixedPoint.Zero)
                return false;

            VInt3 q = fromPos + d * t0;
            hitNormal = (q - spherePosition) / radius;
            return true;
        }
    }
}
