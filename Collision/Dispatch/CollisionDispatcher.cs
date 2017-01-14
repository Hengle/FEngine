using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class CollisionDispatcher:Dispatcher
    {
	    private static readonly int MAX_BROADPHASE_COLLISION_TYPES = (int)BroadphaseNativeType.MAX_BROADPHASE_COLLISION_TYPES;
        private NearCallback nearCallback;
        private readonly CollisionAlgorithmCreateFunc[,] doubleDispatch = new CollisionAlgorithmCreateFunc[MAX_BROADPHASE_COLLISION_TYPES,MAX_BROADPHASE_COLLISION_TYPES];
	    private CollisionConfiguration collisionConfiguration;

        private CollisionAlgorithmConstructionInfo tmpCI = new CollisionAlgorithmConstructionInfo();

        public CollisionDispatcher(CollisionConfiguration collisionConfiguration)
        {
            this.collisionConfiguration = collisionConfiguration;

            setNearCallback(new DefaultNearCallback());

            for (int i = 0; i < MAX_BROADPHASE_COLLISION_TYPES; i++)
            {
                for (int j = 0; j < MAX_BROADPHASE_COLLISION_TYPES; j++)
                {
                    doubleDispatch[i, j] = collisionConfiguration.getCollisionAlgorithmCreateFunc(
                        (BroadphaseNativeType)i,
                        (BroadphaseNativeType)j
                    );
                }
            }
        }

        public void registerCollisionCreateFunc(int proxyType0, int proxyType1, CollisionAlgorithmCreateFunc createFunc)
        {
            doubleDispatch[proxyType0,proxyType1] = createFunc;
        }

        public NearCallback getNearCallback()
        {
            return nearCallback;
        }

        public void setNearCallback(NearCallback nearCallback)
        {
            this.nearCallback = nearCallback;
        }

        public CollisionConfiguration getCollisionConfiguration()
        {
            return collisionConfiguration;
        }

        public void setCollisionConfiguration(CollisionConfiguration collisionConfiguration)
        {
            this.collisionConfiguration = collisionConfiguration;
        }

        public override CollisionAlgorithm findAlgorithm(CollisionObject body0, CollisionObject body1)
        {
            CollisionAlgorithmConstructionInfo ci = tmpCI;
            ci.dispatcher1 = this;
            CollisionAlgorithmCreateFunc createFunc = doubleDispatch[(int)body0.getCollisionShape().getShapeType(), (int)body1.getCollisionShape().getShapeType()];
            CollisionAlgorithm algo = createFunc.createCollisionAlgorithm(ci, body0, body1);
            algo.internalSetCreateFunc(createFunc);

            return algo;
        }

        public override void freeCollisionAlgorithm(CollisionAlgorithm algo)
        {
            CollisionAlgorithmCreateFunc createFunc = algo.internalGetCreateFunc();
            algo.internalSetCreateFunc(null);
            createFunc.releaseCollisionAlgorithm(algo);
            algo.destroy();
        }

        public override bool needsCollision(CollisionObject body0, CollisionObject body1)
        {
            bool needsCollision = true;

            if ((!body0.isActive()) && (!body1.isActive()))
            {
                needsCollision = false;
            }
            else if (!body0.CheckCollideWith(body1))
            {
                needsCollision = false;
            }

            return needsCollision;
        }

        public override bool needsResponse(CollisionObject body0, CollisionObject body1)
        {
            //here you can do filtering
            bool hasResponse = (body0.hasContactResponse() && body1.hasContactResponse());
            //no response between two static/kinematic bodies:
            hasResponse = hasResponse && ((!body0.isStaticOrKinematicObject()) || (!body1.isStaticOrKinematicObject()));
            return hasResponse;
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
                return !dispatcher.getNearCallback().handleCollision(pair, dispatcher, dispatchInfo);
            }
        }

        private CollisionPairCallback collisionPairCallback = new CollisionPairCallback();

        public override void dispatchAllCollisionPairs(OverlappingPairCache pairCache, DispatcherInfo dispatchInfo, Dispatcher dispatcher)
        {
            collisionPairCallback.init(dispatchInfo, this);
            pairCache.processAllOverlappingPairs(collisionPairCallback, dispatcher);
        }
    }
}
