using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class CollisionWorld
    {
        protected List<CollisionObject> collisionObjects = new List<CollisionObject>();
        protected Dispatcher dispatcher1;
        
        protected BroadphaseInterface broadphase;

        public CollisionWorld(Dispatcher dispatcher, BroadphaseInterface broadphase)
        {
            this.dispatcher1 = dispatcher;
            this.broadphase = broadphase;
        }

        public void destroy()
        {
            // clean up remaining objects
            for (int i = 0; i < collisionObjects.Count; i++)
            {
                CollisionObject collisionObject = collisionObjects[i];
                BroadphaseProxy bp = collisionObject.getBroadphaseHandle();
                if (bp != null)
                {
                    getBroadphase().destroyProxy(bp, dispatcher1);
                }
            }
        }

        public virtual void Tick(VFixedPoint dt)
        {
            performDiscreteCollisionDetection();
        }

        protected void performDiscreteCollisionDetection()
        {
            //updateAabbs();
            broadphase.calculateOverlappingPairs(dispatcher1);
            Dispatcher dispatcher = getDispatcher();
            {
                if (dispatcher != null)
                {
                    dispatcher.releaseAllManifold();
                    dispatcher.dispatchAllCollisionPairs(broadphase.getOverlappingPairCache());
                }
            }
        }

        public List<CollisionObject> getCollisionObjectArray()
        {
            return collisionObjects;
        }

        public void addCollisionObject(CollisionObject collisionObject)
        {
            addCollisionObject(collisionObject, CollisionFilterGroups.DEFAULT_FILTER, CollisionFilterGroups.ALL_FILTER);
        }

        public void addCollisionObject(CollisionObject collisionObject, short collisionFilterGroup, short collisionFilterMask)
        {
            collisionObjects.Add(collisionObject);

            // calculate new AABB
            // TODO: check if it's overwritten or not
            VIntTransform trans = collisionObject.getWorldTransform();

            VInt3 minAabb = new VInt3();
            VInt3 maxAabb = new VInt3();
            collisionObject.getCollisionShape().getAabb(trans, out minAabb, out maxAabb);

            BroadphaseNativeType type = collisionObject.getCollisionShape().getShapeType();
            collisionObject.setBroadphaseHandle(getBroadphase().createProxy(
                minAabb,
                maxAabb,
                type,
                collisionObject,
                collisionFilterGroup,
                collisionFilterMask,
                dispatcher1));
        }

        public void removeCollisionObject(CollisionObject collisionObject)
        {
            BroadphaseProxy bp = collisionObject.getBroadphaseHandle();
            if (bp != null)
            {
                //
                // only clear the cached algorithms
                //
                getBroadphase().destroyProxy(bp, dispatcher1);
                collisionObject.setBroadphaseHandle(null);
            }

            //swapremove
            collisionObjects.Remove(collisionObject);
        }

        public void setBroadphase(BroadphaseInterface pairCache)
        {
            broadphase = pairCache;
        }

        public BroadphaseInterface getBroadphase()
        {
            return broadphase;
        }

        public OverlappingPairCache getPairCache()
        {
            return broadphase.getOverlappingPairCache();
        }

        public Dispatcher getDispatcher()
        {
            return dispatcher1;
        }

        protected void updateSingleAabb(CollisionObject colObj)
        {
            VInt3 minAabb, maxAabb;

            colObj.getCollisionShape().getAabb(colObj.getWorldTransform(), out minAabb, out maxAabb);
            // need to increase the aabb for contact thresholds
            VInt3 contactThreshold = new VInt3(Globals.getContactBreakingThreshold(), Globals.getContactBreakingThreshold(), Globals.getContactBreakingThreshold());
            minAabb -= contactThreshold;
            maxAabb += contactThreshold;

            BroadphaseInterface bp = broadphase;

            // moving objects should be moderately sized, probably something wrong if not
            VInt3 tmp = maxAabb - minAabb; // TODO: optimize
            if (colObj.isStaticObject() || (tmp.sqrMagnitude < VFixedPoint.LARGE_NUMBER))
            {
                bp.setAabb(colObj.getBroadphaseHandle(), minAabb, maxAabb, dispatcher1);
            }
        }

        public void updateAabbs()
        {
            for (int i = 0; i < collisionObjects.Count; i++)
            {
                CollisionObject colObj = collisionObjects[i];

                updateSingleAabb(colObj);
                
            }
        }

        public void rayTest(VInt3 rayFromWorld, VInt3 rayToWorld, RayResultCallback resultCallback, short collisionFilterMask = CollisionFilterGroups.ALL_FILTER, short collisionFilterGroup = CollisionFilterGroups.ALL_FILTER)
        {
            SingleRayCallback rayCB = new SingleRayCallback(rayFromWorld, rayToWorld, dispatcher1, resultCallback);
            broadphase.rayTest(rayCB, dispatcher1, VInt3.zero, VInt3.zero, collisionFilterGroup, collisionFilterMask);
        }

        public void OverlapTest(CollisionObject testObject, List<ManifoldResult> results, short collisionFilterGroup = CollisionFilterGroups.DEFAULT_FILTER, short collisionFilterMask = CollisionFilterGroups.ALL_FILTER)
        {
            SingleOverlapCallback overlapCB = new SingleOverlapCallback(testObject, dispatcher1, results);
            VInt3 aabbMin = VInt3.zero, aabbMax = VInt3.zero;
            testObject.getCollisionShape().getAabb(testObject.getWorldTransform(), out aabbMin, out aabbMax);
            broadphase.aabbTest(aabbMin, aabbMax, overlapCB, dispatcher1, collisionFilterGroup, collisionFilterMask);
        }

        public void SweepTest(CollisionObject testObject, VInt3 start, VInt3 end, List<CastResult> results, short collisionFilterGroup = CollisionFilterGroups.DEFAULT_FILTER, short collisionFilterMask = CollisionFilterGroups.ALL_FILTER)
        {
            VInt3 aabbMin, aabbMax;
            testObject.getCollisionShape().getAabb(testObject.getWorldTransform(), out aabbMin, out aabbMax);

            VInt3 dir = end - start;
            DbvtAabbMm aabb = new DbvtAabbMm();
            aabb = DbvtAabbMm.FromVec(start, end, aabb);
            aabb.Expand((aabbMax - aabbMin) * VFixedPoint.Half);

            SingleSweepCallback sweepCB = new SingleSweepCallback(testObject, start, end, dispatcher1, results);
            broadphase.aabbTest(aabbMin, aabbMax, sweepCB, dispatcher1, collisionFilterGroup, collisionFilterMask);
        }
    }

    class SingleRayCallback : BroadphaseRayCallback
    {
        RayResultCallback m_resultCallback;
        Dispatcher dispatcher;

        public SingleRayCallback(VInt3 rayFromWorld, VInt3 rayToWorld, Dispatcher dispatcher, RayResultCallback resultCallback):base(rayFromWorld, rayToWorld)
        {
            this.dispatcher = dispatcher;
            m_resultCallback = resultCallback;
        }

        public override bool process(BroadphaseProxy proxy)
	    {
		    CollisionObject collisionObject = proxy.clientObject;

		    //only perform raycast if filterMask matches
            RaytestAlgorithm algorithm = dispatcher.findAlgorithm(collisionObject);
            algorithm(rayFrom, rayTo,
                    collisionObject,
					m_resultCallback);
			
		    return true;
	    }
    }

    class SingleOverlapCallback : BroadphaseAabbCallback
    {
        List<ManifoldResult> results;
        Dispatcher dispatcher;
        CollisionObject collisionObject;

        public SingleOverlapCallback(CollisionObject collisionObject, Dispatcher dispatcher, List<ManifoldResult> results):base(collisionObject)
        {
            this.dispatcher = dispatcher;
            this.results = results;
            this.collisionObject = collisionObject;
        }

        public override bool process(BroadphaseProxy proxy)
        {
            ///terminate further ray tests, once the closestHitFraction reached zero
            if (aabbMin == aabbMax)
                return false;

            CollisionObject collisionObject = proxy.clientObject;

            ManifoldResult result = new ManifoldResult();
            //only perform raycast if filterMask matches

            CollisionAlgorithm algorithm = dispatcher.findAlgorithm(collisionObject, this.collisionObject);
            algorithm(collisionObject, this.collisionObject,
                    dispatcher.getDispatchInfo(),
                    result);

            if(result.hasContact)
            {
                results.Add(result);
            }
            
            return true;
        }
    }

    class SingleSweepCallback : BroadphaseAabbCallback
    {
        List<CastResult> results;
        Dispatcher dispatcher;
        CollisionObject collisionObject;
        VInt3 start, end;

        public SingleSweepCallback(CollisionObject collisionObject, VInt3 start, VInt3 end, Dispatcher dispatcher, List<CastResult> results) : base(collisionObject)
        {
            this.dispatcher = dispatcher;
            this.results = results;
            this.collisionObject = collisionObject;
            this.start = start;
            this.end = end;
        }

        public override bool process(BroadphaseProxy proxy)
        {
            ///terminate further ray tests, once the closestHitFraction reached zero
            if (aabbMin == aabbMax)
                return false;

            CollisionObject collisionObject = proxy.clientObject;

            ManifoldResult result = new ManifoldResult();
            //only perform raycast if filterMask matches

            SweepAlgorithm algorithm = dispatcher.findSweepAlgorithm(collisionObject, this.collisionObject);
            algorithm(this.collisionObject, start, end, collisionObject, results, VFixedPoint.Create(0.01f));

            return true;
        }
    }
}