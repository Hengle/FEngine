using System;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class CollisionDispatcher:Dispatcher
    {
	    private static readonly int MAX_BROADPHASE_COLLISION_TYPES = (int)BroadphaseNativeType.MAX_BROADPHASE_COLLISION_TYPES;
        private NearCallback nearCallback;
        private readonly CollisionAlgorithm[,] doubleDispatch = new CollisionAlgorithm[MAX_BROADPHASE_COLLISION_TYPES,MAX_BROADPHASE_COLLISION_TYPES];
        private readonly RaytestAlgorithm[] raytestDispatch = new RaytestAlgorithm[MAX_BROADPHASE_COLLISION_TYPES];
        private readonly SweepAlgorithm[,] doubleSweepDispatch = new SweepAlgorithm[MAX_BROADPHASE_COLLISION_TYPES, MAX_BROADPHASE_COLLISION_TYPES];

        public CollisionDispatcher(CollisionConfiguration collisionConfiguration)
        {
            setNearCallback(new DefaultNearCallback());

            for (int i = 0; i < MAX_BROADPHASE_COLLISION_TYPES; i++)
            {
                for (int j = 0; j < MAX_BROADPHASE_COLLISION_TYPES; j++)
                {
                    doubleDispatch[i, j] = collisionConfiguration.getCollisionAlgorithmCreateFunc(
                        (BroadphaseNativeType)i,
                        (BroadphaseNativeType)j
                    );

                    doubleSweepDispatch[i, j] = collisionConfiguration.getSweepAlgorithmCreateFunc(
                        (BroadphaseNativeType)i,
                        (BroadphaseNativeType)j
                    );
                }

                raytestDispatch[i] = collisionConfiguration.getRaytestAlgorithm((BroadphaseNativeType)i);
            }
        }

        public NearCallback getNearCallback()
        {
            return nearCallback;
        }

        public void setNearCallback(NearCallback nearCallback)
        {
            this.nearCallback = nearCallback;
        }

        public override CollisionAlgorithm findAlgorithm(CollisionObject body0, CollisionObject body1)
        {
            CollisionAlgorithm algo = doubleDispatch[(int)body0.getCollisionShape().getShapeType(), (int)body1.getCollisionShape().getShapeType()];
            return algo;
        }

        public override RaytestAlgorithm findAlgorithm(CollisionObject body)
        {
            RaytestAlgorithm algo = raytestDispatch[(int)body.getCollisionShape().getShapeType()];
            return algo;
        }

        public override SweepAlgorithm findSweepAlgorithm(CollisionObject body0, CollisionObject body1)
        {
            return doubleSweepDispatch[(int)body0.getCollisionShape().getShapeType(), (int)body1.getCollisionShape().getShapeType()];
        }

        public override bool needsCollision(short collisionFilterGroup0, short collisionFilterMask0, short collisionFilterGroup1, short collisionFilterMask1)
        {
            bool collides = ((collisionFilterGroup0 & collisionFilterMask1) & 0xFFFF) != 0;
            collides = collides && ((collisionFilterGroup1 & collisionFilterMask0) & 0xFFFF) != 0;
            return collides;
        }

        private class CollisionPairCallback: OverlapCallback
        {

            private DispatcherInfo dispatchInfo;
            private CollisionDispatcher dispatcher;

            public void init(DispatcherInfo dispatchInfo, CollisionDispatcher dispatcher)
            {
                this.dispatchInfo = dispatchInfo;
                this.dispatcher = dispatcher;
            }

            public override bool processOverlap(BroadphasePair pair)
            {
                dispatcher.getNearCallback().handleCollision(pair, dispatcher);
                return false;
            }
        }

        private CollisionPairCallback collisionPairCallback = new CollisionPairCallback();

        public override void dispatchAllCollisionPairs(OverlappingPairCache pairCache)
        {
            collisionPairCallback.init(dispatchInfo, this);
            pairCache.processAllOverlappingPairs(collisionPairCallback);
        }
    }
}
