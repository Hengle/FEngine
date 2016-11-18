using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class CollisionWorld
    {
        protected List<CollisionObject> collisionObjects = new List<CollisionObject>();
        protected Dispatcher dispatcher1;
        protected DispatcherInfo dispatchInfo = new DispatcherInfo();
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
                    //
                    // only clear the cached algorithms
                    //
                    getBroadphase().getOverlappingPairCache().cleanProxyFromPairs(bp, dispatcher1);
                    getBroadphase().destroyProxy(bp, dispatcher1);
                }
            }
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

        public void performDiscreteCollisionDetection()
        {
            updateAabbs();
            broadphasePairCache.calculateOverlappingPairs(dispatcher1);
            Dispatcher dispatcher = getDispatcher();
            {
                if (dispatcher != null)
                {
                    dispatcher.dispatchAllCollisionPairs(broadphasePairCache.getOverlappingPairCache(), dispatchInfo, dispatcher1);
                }
            }
        }

        public void removeCollisionObject(CollisionObject collisionObject)
        {
            BroadphaseProxy bp = collisionObject.getBroadphaseHandle();
            if (bp != null)
            {
                //
                // only clear the cached algorithms
                //
                getBroadphase().getOverlappingPairCache().cleanProxyFromPairs(bp, dispatcher1);
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

        public DispatcherInfo getDispatchInfo()
        {
            return dispatchInfo;
        }

        public void updateSingleAabb(CollisionObject colObj)
        {
            VInt3 minAabb, maxAabb;

            colObj.getCollisionShape().getAabb(colObj.getWorldTransform(), out minAabb, out maxAabb);
            // need to increase the aabb for contact thresholds
            VInt3 contactThreshold = new VInt3(BulletGlobals.getContactBreakingThreshold(), BulletGlobals.getContactBreakingThreshold(), BulletGlobals.getContactBreakingThreshold());
            minAabb -= contactThreshold;
            maxAabb += contactThreshold;

            BroadphaseInterface bp = broadphasePairCache;

            // moving objects should be moderately sized, probably something wrong if not
            VInt3 tmp = maxAabb - minAabb; // TODO: optimize
            if (colObj.isStaticObject() || (tmp.sqrMagnitude < 1e12f))
            {
                bp.setAabb(colObj.getBroadphaseHandle(), minAabb, maxAabb, dispatcher1);
            }
        }

        public void updateAabbs()
        {
            for (int i = 0; i < collisionObjects.Count; i++)
            {
                CollisionObject colObj = collisionObjects[i];

                // only update aabb of active objects
                if (colObj.isActive())
                {
                    updateSingleAabb(colObj);
                }
            }
        }

        public static void rayTestSingle(VIntTransform rayFromTrans, VIntTransform rayToTrans,
            CollisionObject collisionObject,
            CollisionShape collisionShape,
            VIntTransform colObjWorldTransform,
            RayResultCallback resultCallback)
        {
            SphereShape pointShape = new SphereShape(0f);
            pointShape.setMargin(0f);
            ConvexShape castShape = pointShape;


            CastResult castResult = new CastResult();
            castResult.fraction = resultCallback.closestHitFraction;

            ConvexShape convexShape = (ConvexShape)collisionShape;
            VoronoiSimplexSolver simplexSolver = new VoronoiSimplexSolver();

            SubsimplexConvexCast convexCaster = new SubsimplexConvexCast(castShape, convexShape, simplexSolver);
            if (convexCaster.calcTimeOfImpact(rayFromTrans, rayToTrans, colObjWorldTransform, colObjWorldTransform, castResult))
            {
                //add hit
                if (castResult.normal.sqrMagnitude > VFixedPoint.Zero)
                {
                    if (castResult.fraction < resultCallback.closestHitFraction)
                    {
                        //rotate normal into worldspace
                        rayFromTrans.TransformVector(castResult.normal);

                        castResult.normal = castResult.normal.Normalize();
                        LocalRayResult localRayResult = new LocalRayResult(
                                collisionObject,
                                null,
                                castResult.normal,
                                castResult.fraction);

                        bool normalInWorldSpace = true;
                        resultCallback.addSingleResult(localRayResult, normalInWorldSpace);
                    }
                }
            }
        }

        public void rayTest(VInt3 rayFromWorld, VInt3 rayToWorld, RayResultCallback resultCallback)
        {
            VIntTransform rayFromTrans = VIntTransform.Identity, rayToTrans = VIntTransform.Identity;
            rayFromTrans.position = rayFromWorld;
            rayToTrans.position = rayToWorld;

            // go over all objects, and if the ray intersects their aabb, do a ray-shape query using convexCaster (CCD)
            VInt3 collisionObjectAabbMin, collisionObjectAabbMax;
            VFixedPoint hitLambda;

            for (int i = 0; i < collisionObjects.Count; i++)
            {
                if (resultCallback.closestHitFraction == VFixedPoint.Zero)
                {
                    break;
                }

                CollisionObject collisionObject = collisionObjects[i];
                if (resultCallback.needsCollision(collisionObject.getBroadphaseHandle()))
                {
                    collisionObject.getCollisionShape().getAabb(collisionObject.getWorldTransform(), out collisionObjectAabbMin, out collisionObjectAabbMax);
                    hitLambda = resultCallback.closestHitFraction;
                    VInt3 hitNormal;
                    if (AabbUtil2.rayAabb(rayFromWorld, rayToWorld, collisionObjectAabbMin, collisionObjectAabbMax, hitLambda, out hitNormal))
                    {
                        rayTestSingle(rayFromTrans, rayToTrans,
                                collisionObject,
                                collisionObject.getCollisionShape(),
                                collisionObject.getWorldTransform(),
                                resultCallback);
                    }
                }
            }
        }
    }

    public abstract class RayResultCallback
    {
        public VFixedPoint closestHitFraction = VFixedPoint.One;
        public CollisionObject collisionObject;
        public short collisionFilterGroup = CollisionFilterGroups.DEFAULT_FILTER;
        public short collisionFilterMask = CollisionFilterGroups.ALL_FILTER;

        public bool hasHit()
        {
            return (collisionObject != null);
        }

        public bool needsCollision(BroadphaseProxy proxy0)
        {
            bool collides = ((proxy0.collisionFilterGroup & collisionFilterMask) & 0xFFFF) != 0;
            collides = collides && ((collisionFilterGroup & proxy0.collisionFilterMask) & 0xFFFF) != 0;
            return collides;
        }

        public abstract VFixedPoint addSingleResult(LocalRayResult rayResult, bool normalInWorldSpace);
    }

    public class LocalShapeInfo
    {
        public int shapePart;
        public int triangleIndex;
    }

    public class LocalRayResult
    {
        public CollisionObject collisionObject;
        public LocalShapeInfo localShapeInfo;
        public VInt3 hitNormalLocal;
        public VFixedPoint hitFraction;

        public LocalRayResult(CollisionObject collisionObject, LocalShapeInfo localShapeInfo, VInt3 hitNormalLocal, VFixedPoint hitFraction)
        {
            this.collisionObject = collisionObject;
            this.localShapeInfo = localShapeInfo;
            this.hitNormalLocal = hitNormalLocal;
            this.hitFraction = hitFraction;
        }
    }

    public class ClosestRayResultCallback: RayResultCallback
    {

        public VInt3 rayFromWorld; //used to calculate hitPointWorld from hitFraction
        public VInt3 rayToWorld;

        public VInt3 hitNormalWorld;
        public VInt3 hitPointWorld;

        public ClosestRayResultCallback(VInt3 rayFromWorld, VInt3 rayToWorld)
        {
            this.rayFromWorld = rayFromWorld;
            this.rayToWorld = rayToWorld;
        }

        public override VFixedPoint addSingleResult(LocalRayResult rayResult, bool normalInWorldSpace)
        {
            closestHitFraction = rayResult.hitFraction;
            collisionObject = rayResult.collisionObject;
            if (normalInWorldSpace)
            {
                hitNormalWorld = rayResult.hitNormalLocal;
            }
            else
            {
                // need to transform normal into worldspace
                hitNormalWorld = rayResult.hitNormalLocal;
                collisionObject.getWorldTransform().TransformVector(hitNormalWorld);
			}

                hitPointWorld = rayFromWorld * (VFixedPoint.One - rayResult.hitFraction) + rayToWorld * rayResult.hitFraction;
			    return rayResult.hitFraction;
		}
	}
}