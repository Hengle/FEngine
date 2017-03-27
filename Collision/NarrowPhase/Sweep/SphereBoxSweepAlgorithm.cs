﻿using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class SphereBoxSweepAlgorithm
    {
        public static bool sweepSphereBox(SphereShape sphere, VInt3 fromPos, VInt3 ToPos, BoxShape box, VIntTransform boxTransform, 
            ref VFixedPoint dist, ref VInt3 normal)
        {
            VFixedPoint tmp;

            VInt3 dir = ToPos - fromPos;
            if(SphereBoxCollisionAlgorithm.getSphereDistance(box, boxTransform, fromPos, sphere.getRadius(), out normal, out tmp))
            {
                normal = (fromPos - ToPos).Normalize();
                dist = VFixedPoint.Zero;
                return true;
            }

            VInt3 aabbMin = boxTransform.position - box.getHalfExtent(), aabbMax = boxTransform.position + box.getHalfExtent();
            aabbMax += VInt3.one * sphere.getRadius();
            aabbMin -= VInt3.one * sphere.getRadius();
            return BoxRaytestAlgorithm.rayTestBox(fromPos, ToPos, aabbMax, boxTransform, ref dist, ref normal);
        }

        public static void objectQuerySingle(CollisionObject castObject, VInt3 ToPos, CollisionObject collisionObject, List<CastResult> results, VFixedPoint allowedPenetration)
        {
            VIntTransform castTransform = castObject.getWorldTransform();
            bool needSwap = castObject.getCollisionShape() is BoxShape;
            BoxShape box = (BoxShape)(needSwap ? castObject.getCollisionShape() : collisionObject.getCollisionShape());
            VIntTransform boxTransform = needSwap ? castTransform : collisionObject.getWorldTransform();
            SphereShape sphere = (SphereShape)(needSwap ? collisionObject.getCollisionShape() : castObject.getCollisionShape());
            VInt3 spherePos = needSwap ? collisionObject.getWorldTransform().position : castObject.getWorldTransform().position; 

            VInt3 toPos = needSwap ? spherePos - (ToPos - castTransform.position) : ToPos;

            VFixedPoint dist = VFixedPoint.Zero;
            VInt3 normal = VInt3.zero;
            if(sweepSphereBox(sphere, spherePos, toPos, box, boxTransform, ref dist, ref normal))
            {
                CastResult result = new CastResult();
                result.hitObject = collisionObject;
                result.fraction = dist;
                result.normal = normal * (needSwap ? -1 : 1);
                results.Add(result);
            }
        }
    }
}
