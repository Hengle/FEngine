using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class CollisionWorld
    {
        protected List<CollisionObject> collisionObjects = new List<CollisionObject>();
        protected Dispatcher dispatcher1;
        
        protected BroadphaseInterface broadphasePairCache;

        public CollisionWorld(Dispatcher dispatcher, BroadphaseInterface broadphasePairCache)
        {
            this.dispatcher1 = dispatcher;
            this.broadphasePairCache = broadphasePairCache;
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
            updateAabbs();
            broadphasePairCache.calculateOverlappingPairs(dispatcher1);
            Dispatcher dispatcher = getDispatcher();
            {
                if (dispatcher != null)
                {
                    dispatcher.releaseAllManifold();
                    dispatcher.dispatchAllCollisionPairs(broadphasePairCache.getOverlappingPairCache());
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
            broadphasePairCache = pairCache;
        }

        public BroadphaseInterface getBroadphase()
        {
            return broadphasePairCache;
        }

        public OverlappingPairCache getPairCache()
        {
            return broadphasePairCache.getOverlappingPairCache();
        }

        public Dispatcher getDispatcher()
        {
            return dispatcher1;
        }

        public void updateSingleAabb(CollisionObject colObj)
        {
            VInt3 minAabb, maxAabb;

            colObj.getCollisionShape().getAabb(colObj.getWorldTransform(), out minAabb, out maxAabb);
            // need to increase the aabb for contact thresholds
            VInt3 contactThreshold = new VInt3(Globals.getContactBreakingThreshold(), Globals.getContactBreakingThreshold(), Globals.getContactBreakingThreshold());
            minAabb -= contactThreshold;
            maxAabb += contactThreshold;

            BroadphaseInterface bp = broadphasePairCache;

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

        public void rayTest(VInt3 rayFromWorld, VInt3 rayToWorld, RayResultCallback resultCallback)
        {
            VIntTransform rayFromTrans = VIntTransform.Identity;
            rayFromTrans.position = rayFromWorld;
            VIntTransform rayToTrans = VIntTransform.Identity;
            rayToTrans.position = rayToWorld;
            SingleRayCallback rayCB = new SingleRayCallback(rayFromTrans, rayToTrans, dispatcher1, resultCallback);
            broadphasePairCache.rayTest(rayCB, VInt3.zero, VInt3.zero);
        }

        public void OverlapTest(CollisionObject testObject, List<ManifoldResult> results)
        {
            SingleOverlapCallback overlapCB = new SingleOverlapCallback(testObject, dispatcher1, results);
        }
    }

    public abstract class RayResultCallback
    {
        public short collisionFilterGroup = CollisionFilterGroups.DEFAULT_FILTER;
        public short collisionFilterMask = CollisionFilterGroups.ALL_FILTER;
        public VFixedPoint closestHitFraction;

        public abstract bool hasHit();
        public abstract VFixedPoint addSingleResult(CollisionObject collisionObject, VInt3 hitNormalLocal, VFixedPoint hitFraction);
    }

    class SingleRayCallback : BroadphaseRayCallback
    {
        RayResultCallback m_resultCallback;
        Dispatcher dispatcher;

        public SingleRayCallback(VIntTransform rayFromWorld, VIntTransform rayToWorld, Dispatcher dispatcher, RayResultCallback resultCallback):base(rayFromWorld, rayToWorld)
        {
            this.dispatcher = dispatcher;
            m_resultCallback = resultCallback;
        }

        public override bool process(BroadphaseProxy proxy)
	    {
		    CollisionObject collisionObject = proxy.clientObject;

		    //only perform raycast if filterMask matches
		    if(dispatcher.needsCollision(collisionObject, m_resultCallback)) 
		    {
                RaytestAlgorithm algorithm = dispatcher.findAlgorithm(collisionObject);
                algorithm.rayTestSingle(rayFromTrans, rayToTrans,
                        collisionObject,
					    m_resultCallback);
			}
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
            if (dispatcher.needsCollision(collisionObject, this.collisionObject))
            {
                CollisionAlgorithm algorithm = dispatcher.findAlgorithm(collisionObject, this.collisionObject);
                algorithm.processCollision(collisionObject, this.collisionObject,
                        dispatcher.getDispatchInfo(),
                        result);

                if(result.hasContact)
                {
                    results.Add(result);
                }
            }
            return true;
        }
    }
}