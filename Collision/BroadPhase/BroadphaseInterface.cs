﻿using MobaGame.FixedMath;
using System.Text;

namespace MobaGame.Collision
{
    public abstract class IBroadphaseInterface 
    {
        public abstract BroadphaseProxy createProxy(VInt3 aabbMin, VInt3 aabbMax, BroadphaseNativeType shapeType, short collisionFilterGroup, short collisionFilterMask, Dispatcher dispatcher);

        public abstract void destroyProxy(BroadphaseProxy proxy, Dispatcher dispatcher);

        public abstract void setAabb(BroadphaseProxy proxy, VInt3 aabbMin, VInt3 aabbMax, Dispatcher dispatcher);

        ///calculateOverlappingPairs is optional: incremental algorithms (sweep and prune) might do it during the set aabb
        public abstract void calculateOverlappingPairs(Dispatcher dispatcher);

        public abstract OverlappingPairCache getOverlappingPairCache();

        ///getAabb returns the axis aligned bounding box in the 'global' coordinate frame
        ///will add some transform later
        public abstract void getBroadphaseAabb(VInt3 aabbMin, VInt3 aabbMax);

        public abstract void printStats();
    }
}