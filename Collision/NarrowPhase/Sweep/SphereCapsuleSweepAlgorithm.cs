using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class SphereCapsuleSweepAlgorithm
    {
        //sphere move to collide capsule
        static bool sweepSphereCapsule(CollisionObject sphereObject, CollisionObject capsuleObject,
            VFixedPoint length, VInt3 dir
            , CastResult result)
        {
            VInt3 move = dir * length;
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
                result.normal = -dir;
                return true;
            }

            if(capsuleP0 == capsuleP1)
            {
                VInt3 ToPos = spherePosition + move;
                VFixedPoint u0 = VFixedPoint.Zero;
                if(SphereSphereSweepAlgorithm.sphereSphereSweep(sphere.getRadius(), spherePosition, ToPos, capsule.getRadius(), capsuleObject.getWorldTransform().position, ref u0, ref tmp))
                {
                    result.fraction = u0;
                    if (u0 == VFixedPoint.Zero)
                    {
                        result.normal = -dir * length;
                    }
                    else
                    {
                        result.normal = capsuleObject.getWorldTransform().position - (spherePosition + move * u0);
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
    }
}
