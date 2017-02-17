using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class CollisionDispatcher:Dispatcher
    {
	    private static readonly int MAX_BROADPHASE_COLLISION_TYPES = (int)BroadphaseNativeType.MAX_BROADPHASE_COLLISION_TYPES;
        private NearCallback nearCallback;
        private readonly CollisionAlgorithm[,] doubleDispatch = new CollisionAlgorithm[MAX_BROADPHASE_COLLISION_TYPES,MAX_BROADPHASE_COLLISION_TYPES];
        private readonly RaytestAlgorithm[] raytestDispatch = new RaytestAlgorithm[MAX_BROADPHASE_COLLISION_TYPES];
        private readonly ObjectQueryAlgorithm[, ] objectQueryDispatch = new ObjectQueryAlgorithm[MAX_BROADPHASE_COLLISION_TYPES, MAX_BROADPHASE_COLLISION_TYPES];

        private ObjectPool<ManifoldResult> manifoldPool;
        private List<ManifoldResult> manifolds;

        public CollisionDispatcher(CollisionConfiguration collisionConfiguration)
        {
            manifoldPool = new ObjectPool<ManifoldResult>();
            manifolds = new List<ManifoldResult>();


            setNearCallback(new DefaultNearCallback());

            for (int i = 0; i < MAX_BROADPHASE_COLLISION_TYPES; i++)
            {
                for (int j = 0; j < MAX_BROADPHASE_COLLISION_TYPES; j++)
                {
                    doubleDispatch[i, j] = collisionConfiguration.getCollisionAlgorithmCreateFunc(
                        (BroadphaseNativeType)i,
                        (BroadphaseNativeType)j
                    );

                    objectQueryDispatch[i, j] = collisionConfiguration.getObjectQueryAlgorithm((BroadphaseNativeType)i, (BroadphaseNativeType)j);
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

        public override ObjectQueryAlgorithm findObjectQueryAlgorithm(CollisionObject body0, CollisionObject body1)
        {
            ObjectQueryAlgorithm algo = objectQueryDispatch[(int)body0.getCollisionShape().getShapeType(), (int)body1.getCollisionShape().getShapeType()];
            return algo;
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
                if(dispatcher.getNearCallback().handleCollision(pair, dispatcher, dispatchInfo))
                {
                    if(dispatcher.ghostPairCallback != null)
                    {
                        dispatcher.ghostPairCallback.addOverlappingPair(pair.pProxy0, pair.pProxy1);
                    }
                }
                else
                {
                    if (dispatcher.ghostPairCallback != null)
                    {
                        dispatcher.ghostPairCallback.removeOverlappingPair(pair.pProxy0, pair.pProxy1);
                    }
                }
                return false;
            }
        }

        private CollisionPairCallback collisionPairCallback = new CollisionPairCallback();

        public override void dispatchAllCollisionPairs(OverlappingPairCache pairCache, DispatcherInfo dispatchInfo, Dispatcher dispatcher)
        {
            releaseAllManifold();
            collisionPairCallback.init(dispatchInfo, this);
            pairCache.processAllOverlappingPairs(collisionPairCallback);
        }

        public override ManifoldResult applyManifold()
        {
            ManifoldResult newManifold = manifoldPool.Get();
            manifolds.Add(newManifold);
            return newManifold;
        }

        public override void releaseManifold(ManifoldResult result)
        {
            if(manifolds.Contains(result))
            {
                manifolds.Remove(result);
            }
            manifoldPool.Release(result);
        }

        public override void releaseAllManifold()
        {
            for(int i = 0; i < manifolds.Count; i++)
            {
                manifoldPool.Release(manifolds[i]);
            }
            manifolds.Clear();
        }

        public override List<ManifoldResult> getAllManifolds()
        {
            return manifolds;
        }
    }
}
