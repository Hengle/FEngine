using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class SphereCapsuleSweepAlgorithm
    {
        //sphere move to collide capsule
        static bool sweepSphereCapsule(SphereShape sphere, VIntTransform sphereTransform, VInt3 end,
            CapsuleShape capsule, VIntTransform capsuleTransform, ref VInt3 normal, ref VFixedPoint t)
        {
            VInt3 move = end - sphereTransform.position;
            VFixedPoint radiusSum = sphere.getRadius() + capsule.getRadius();
            VInt3 capsuleP0 = capsuleTransform.TransformPoint(capsule.getUpAxis() * capsule.getHalfHeight());
            VInt3 capsuleP1 = capsuleTransform.TransformPoint(capsule.getUpAxis() * -capsule.getHalfHeight());
            VInt3 spherePosition = sphereTransform.position;
            VFixedPoint tmp = VFixedPoint.Zero;
            if(Distance.distancePointSegmentSquared(capsuleP0, capsuleP1, spherePosition, ref tmp) < radiusSum * radiusSum)
            {
                t = VFixedPoint.Zero;
                normal = -move.Normalize();
                return true;
            }

            VFixedPoint u0 = VFixedPoint.Zero;
            VInt3 tmpNormal = VInt3.zero;

            if (capsuleP0 == capsuleP1)
            {
                VInt3 ToPos = spherePosition + move;
                if(SphereSphereSweepAlgorithm.sphereSphereSweep(sphere.getRadius(), spherePosition, ToPos, capsule.getRadius(), capsuleTransform.position, ref u0, ref tmp, ref tmpNormal))
                {
                    t = u0;
                    normal = tmpNormal;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(CapsuleRaytestAlgorithm.raycastCapsule(spherePosition, end, capsuleP0, capsuleP1, radiusSum, ref normal, ref u0))
            {
                t = u0;
                VFixedPoint param = VFixedPoint.Zero;
                VInt3 movedSphereCenter = spherePosition + (end - spherePosition) * u0;
                Distance.distancePointSegmentSquared(capsuleP0, capsuleP1, movedSphereCenter, ref param);
                normal = movedSphereCenter - (capsuleP0 * (VFixedPoint.One - param) + capsuleP1 * param);
                normal = normal.Normalize();
                return true;
            }
            
            return false;
        }

        public static void objectQuerySingle(CollisionObject castObject, VInt3 ToPos, CollisionObject collisionObject, List<CastResult> results, VFixedPoint allowedPenetration)
        {
            bool needSwap = castObject.getCollisionShape() is CapsuleShape;
            CollisionObject sphereObject = needSwap ? collisionObject : castObject;
            CollisionObject capsuleObject = !needSwap ? castObject : collisionObject;
            SphereShape sphere = (SphereShape)sphereObject.getCollisionShape();
            CapsuleShape capsule = (CapsuleShape)capsuleObject.getCollisionShape();

            VInt3 toPos = needSwap ? sphereObject.getWorldTransform().position - (ToPos - castObject.getWorldTransform().position) : ToPos;

            VFixedPoint t = VFixedPoint.Zero;
            VInt3 normal = VInt3.zero;

            if(sweepSphereCapsule(sphere, sphereObject.getWorldTransform(), ToPos, capsule, capsuleObject.getWorldTransform(), ref normal, ref t))
            {
                CastResult result = new CastResult();
                result.hitObject = collisionObject;
                result.fraction = t;
                result.normal = normal * (needSwap ? -1 : 1);
                results.Add(result);
            }
        }
    }    
}
