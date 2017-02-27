using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class SphereCapsuleSweepAlgorithm
    {
        //sphere move to collide capsule
        static bool sweepSphereCapsule(CollisionObject sphereObject, VInt3 start, VInt3 end,
            CollisionObject capsuleObject, CastResult result)
        {
            VInt3 move = end - start;
            SphereShape sphere = (SphereShape)sphereObject.getCollisionShape();
            CapsuleShape capsule = (CapsuleShape)capsuleObject.getCollisionShape();
            VFixedPoint radiusSum = sphere.getRadius() + capsule.getRadius();
            VIntTransform capsuleTransform = capsuleObject.getWorldTransform();
            VInt3 capsuleP0 = capsuleTransform.TransformPoint(capsule.getUpAxis() * capsule.getHalfHeight());
            VInt3 capsuleP1 = capsuleTransform.TransformPoint(capsule.getUpAxis() * -capsule.getHalfHeight());
            VInt3 spherePosition = sphereObject.getWorldTransform().position;
            VFixedPoint tmp = VFixedPoint.Zero;
            if(Distance.distancePointSegmentSquared(capsuleP0, capsuleP1, spherePosition, ref tmp) < radiusSum * radiusSum)
            {
                result.fraction = VFixedPoint.Zero;
                result.normal = -move.Normalize();
                return true;
            }

            VFixedPoint u0 = VFixedPoint.Zero;
            VInt3 normal = VInt3.zero;

            if (capsuleP0 == capsuleP1)
            {
                VInt3 ToPos = spherePosition + move;
                if(SphereSphereSweepAlgorithm.sphereSphereSweep(sphere.getRadius(), spherePosition, ToPos, capsule.getRadius(), capsuleObject.getWorldTransform().position, ref u0, ref tmp, ref normal))
                {
                    result.fraction = u0;
                    result.normal = normal;
                }
                else
                {
                    return false;
                }
            }
            else if(CapsuleRaytestAlgorithm.raycastCapsule(start, end, capsuleP0, capsuleP1, radiusSum, ref normal, ref u0))
            {
                result.fraction = u0;
                result.normal = normal;
                return true;
            }
            
            return false;
        }

        public static void objectQuerySingle(CollisionObject castObject, VInt3 FromPos, VInt3 ToPos, CollisionObject collisionObject, List<CastResult> results, VFixedPoint allowedPenetration)
        {
            bool needSwap = castObject.getCollisionShape() is CapsuleShape;
            CollisionObject sphereObject = needSwap ? collisionObject : castObject;
            CollisionObject capsuleObject = !needSwap ? castObject : collisionObject;

            CastResult result = new CastResult();
            if(sweepSphereCapsule(sphereObject, FromPos, ToPos, capsuleObject, result))
            {
                result.hitObject = collisionObject;
                result.normal = result.normal * (needSwap ? -1 : 1);
                results.Add(result);
            }
        }
    }    
}
