﻿using MobaGame.FixedMath;
using System.Text;

namespace MobaGame.Collision
{
    public abstract class BroadphaseInterface
    {
        public abstract BroadphaseProxy createProxy(VInt3 aabbMin, VInt3 aabbMax, BroadphaseNativeType shapeType, CollisionObject collisionObject, short collisionFilterGroup, short collisionFilterMask, Dispatcher dispatcher);

        public abstract void destroyProxy(BroadphaseProxy proxy, Dispatcher dispatcher);

        public abstract void setAabb(BroadphaseProxy proxy, VInt3 aabbMin, VInt3 aabbMax, Dispatcher dispatcher);

        ///calculateOverlappingPairs is optional: incremental algorithms (sweep and prune) might do it during the set aabb
        public abstract void calculateOverlappingPairs(Dispatcher dispatcher);

        public abstract OverlappingPairCache getOverlappingPairCache();

        ///getAabb returns the axis aligned bounding box in the 'global' coordinate frame
        ///will add some transform later
        public abstract void getBroadphaseAabb(out VInt3 aabbMin, out VInt3 aabbMax);

        public abstract void rayTest(BroadphaseRayCallback rayCallback, VInt3 aabbMin, VInt3 aabbMax);

	    public abstract void aabbTest(VInt3 aabbMin, VInt3 aabbMax, BroadphaseAabbCallback callback);
    }

    public abstract class BroadphaseAabbCallback
    {
        public abstract bool process(BroadphaseProxy proxy);
    }

    public abstract class BroadphaseRayCallback: BroadphaseAabbCallback
    {
        public VIntTransform rayFromTrans;
        public VIntTransform rayToTrans;
        public VInt3 rayDirectionInverse;
        public uint[] signs = new uint[3];
        public VFixedPoint lambdaMax;

        public BroadphaseRayCallback(VIntTransform rayFromTrans, VIntTransform rayToTrans)
        {
            this.rayFromTrans = rayFromTrans;
            this.rayToTrans = rayToTrans;

            VInt3 rayDir = (rayToTrans.position - rayFromTrans.position).Normalize();

            ///what about division by zero? --> just set rayDirection[i] to INF/BT_LARGE_FLOAT
            rayDirectionInverse.x = rayDir[0] == VFixedPoint.Zero ? VFixedPoint.LARGE_NUMBER : VFixedPoint.One / rayDir[0];
            rayDirectionInverse.y = rayDir[1] == VFixedPoint.Zero ? VFixedPoint.LARGE_NUMBER : VFixedPoint.One / rayDir[1];
            rayDirectionInverse.z = rayDir[2] == VFixedPoint.Zero ? VFixedPoint.LARGE_NUMBER : VFixedPoint.One / rayDir[2];
            signs[0] = rayDirectionInverse.x < VFixedPoint.Zero ? 1u : 0;
            signs[1] = rayDirectionInverse.y < VFixedPoint.Zero ? 1u : 0;
            signs[2] = rayDirectionInverse.z < VFixedPoint.Zero ? 1u : 0;

            lambdaMax = VInt3.Dot(rayDir, (rayToTrans.position - rayFromTrans.position));
        }
    }



}