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
                    getBroadphase().destroyProxy(bp, dispatcher1);
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

                // only update aabb of active objects
                if (colObj.isActive())
                {
                    updateSingleAabb(colObj);
                }
            }
        }

        public static void rayTestSingle(VIntTransform rayFromTrans, VIntTransform rayToTrans,
            CollisionObject collisionObject,
            RayResultCallback resultCallback)
        {
            CollisionShape collisionShape = collisionObject.getCollisionShape();
            VIntTransform colObjWorldTransform = collisionObject.getWorldTransform();
            SphereShape pointShape = new SphereShape(VFixedPoint.Zero);
            pointShape.setMargin(VFixedPoint.Zero);
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
            SingleRayCallback rayCB = new SingleRayCallback(rayFromWorld, rayToWorld, resultCallback);
            broadphasePairCache.rayTest(rayCB, VInt3.zero, VInt3.zero);
        }


        public static void objectQuerySingle(ConvexShape castShape, VIntTransform convexFromTrans, VIntTransform convexToTrans,
					  CollisionObject collisionObject,
					  ConvexResultCallback resultCallback, VFixedPoint allowedPenetration)
        {
            CollisionShape collisionShape = collisionObject.getCollisionShape();
            VIntTransform colObjWorldTransform = collisionObject.getWorldTransform();
            CastResult castResult = new CastResult();
            castResult.allowedPenetration = allowedPenetration;
            castResult.fraction = resultCallback.m_closestHitFraction;//btScalar(1.);//??

            VoronoiSimplexSolver simplexSolver = new VoronoiSimplexSolver();
            ConvexCast castPtr = new GjkConvexCast(castShape, (ConvexShape)collisionShape,simplexSolver);

            if (castPtr.calcTimeOfImpact(convexFromTrans, convexToTrans, colObjWorldTransform, colObjWorldTransform, castResult))
            {
                //add hit
                if (castResult.normal.sqrMagnitude > Globals.EPS)
                {
                    if (castResult.fraction < resultCallback.m_closestHitFraction)
                    {
                        castResult.normal = castResult.normal.Normalize();
                        LocalConvexResult localConvexResult = new LocalConvexResult
                            (
                            collisionObject,
                            castResult.normal,
                            castResult.hitPoint,
                            castResult.fraction
        					);

                        bool normalInWorldSpace = true;
                        resultCallback.addSingleResult(localConvexResult, normalInWorldSpace);

                    }
                }
            }
        }

        public void convexSweepTest(ConvexShape castShape, VIntTransform convexFromWorld, VIntTransform convexToWorld, ConvexResultCallback resultCallback, VFixedPoint allowedCcdPenetration)
        {
            VInt3 castShapeAabbMin, castShapeAabbMax;

            // Compute AABB that encompasses angular movement
            VInt3 linVel = new VInt3();
            VInt3 angVel = new VInt3();
			TransformUtil.calculateVelocity(convexFromWorld, convexToWorld, VFixedPoint.One, ref linVel, ref angVel);
			VIntTransform R = VIntTransform.Identity;
            R.rotation = convexFromWorld.rotation;
            castShape.calculateTemporalAabb(R, linVel, angVel, VFixedPoint.One, out castShapeAabbMin, out castShapeAabbMax);

            SingleSweepCallback convexCB = new SingleSweepCallback(castShape, convexFromWorld, convexToWorld, resultCallback, allowedCcdPenetration);

            broadphasePairCache.rayTest(convexCB, castShapeAabbMin, castShapeAabbMax);
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

    public class LocalRayResult
    {
        public CollisionObject collisionObject;
        public VInt3 hitNormalLocal;
        public VFixedPoint hitFraction;

        public LocalRayResult(CollisionObject collisionObject, VInt3 hitNormalLocal, VFixedPoint hitFraction)
        {
            this.collisionObject = collisionObject;
            this.hitNormalLocal = hitNormalLocal;
            this.hitFraction = hitFraction;
        }
    }

    public class SingleRayCallback : BroadphaseRayCallback
    {
        RayResultCallback m_resultCallback;

        public SingleRayCallback(VInt3 rayFromWorld, VInt3 rayToWorld, RayResultCallback resultCallback)
        {
            rayFromTrans = VIntTransform.Identity;
            rayFromTrans.position = rayFromWorld;
            rayToTrans = VIntTransform.Identity;
            rayToTrans.position = rayToWorld;

            VInt3 rayDir = (rayToWorld - rayFromWorld).Normalize();

            ///what about division by zero? --> just set rayDirection[i] to INF/BT_LARGE_FLOAT
            rayDirectionInverse.x = rayDir[0] == VFixedPoint.Zero ? VFixedPoint.LARGE_NUMBER : VFixedPoint.One / rayDir[0];
            rayDirectionInverse.y = rayDir[1] == VFixedPoint.Zero ? VFixedPoint.LARGE_NUMBER : VFixedPoint.One / rayDir[1];
            rayDirectionInverse.z = rayDir[2] == VFixedPoint.Zero ? VFixedPoint.LARGE_NUMBER : VFixedPoint.One / rayDir[2];
            signs[0] = rayDirectionInverse.x < VFixedPoint.Zero ? 1u : 0 ;
            signs[1] = rayDirectionInverse.y < VFixedPoint.Zero ? 1u : 0;
            signs[2] = rayDirectionInverse.z < VFixedPoint.Zero ? 1u : 0;

            lambdaMax = VInt3.Dot(rayDir, (rayToWorld - rayFromWorld));
            m_resultCallback = resultCallback;
        }

        public override bool process(BroadphaseProxy proxy)
	    {
		    ///terminate further ray tests, once the closestHitFraction reached zero
		    if (m_resultCallback.closestHitFraction == VFixedPoint.Zero)
			    return false;

		    CollisionObject collisionObject = proxy.clientObject;

		    //only perform raycast if filterMask matches
		    if(m_resultCallback.needsCollision(collisionObject.getBroadphaseHandle())) 
		    {

				    CollisionWorld.rayTestSingle(rayFromTrans, rayToTrans,
                        collisionObject,
					    m_resultCallback);
			}
		    return true;
	    }
    }

    public class SingleSweepCallback: BroadphaseRayCallback
    {
        public VInt3 m_hitNormal;
        public ConvexResultCallback m_resultCallback;
        public VFixedPoint m_allowedCcdPenetration;
        public ConvexShape m_castShape;

        public SingleSweepCallback(ConvexShape castShape, VIntTransform convexFromTrans, VIntTransform convexToTrans, ConvexResultCallback resultCallback, VFixedPoint allowedPenetration)
        {
            m_castShape = castShape;
            rayFromTrans = convexFromTrans;
            rayToTrans = convexToTrans;
            m_resultCallback = resultCallback;
            m_allowedCcdPenetration = allowedPenetration;

            VInt3 unnormalizedRayDir = rayToTrans.position - rayFromTrans.position;
            VInt3 rayDir = unnormalizedRayDir.Normalize();

            rayDirectionInverse.x = rayDir.x == VFixedPoint.Zero ? VFixedPoint.LARGE_NUMBER : VFixedPoint.One / rayDir.x;
            rayDirectionInverse.y = rayDir.y == VFixedPoint.Zero ? VFixedPoint.LARGE_NUMBER : VFixedPoint.One / rayDir.y;
            rayDirectionInverse.z = rayDir.z == VFixedPoint.Zero ? VFixedPoint.LARGE_NUMBER : VFixedPoint.One / rayDir.z;
            signs[0] = rayDirectionInverse.x < VFixedPoint.Zero ? 1u : 0;
            signs[1] = rayDirectionInverse.y < VFixedPoint.Zero ? 1u : 0;
            signs[2] = rayDirectionInverse.z < VFixedPoint.Zero ? 1u : 0;
            lambdaMax = VInt3.Dot(rayDir, unnormalizedRayDir);
        }

        public override bool process(BroadphaseProxy proxy)
        {
            if (m_resultCallback.m_closestHitFraction == VFixedPoint.Zero)
                return false;
            CollisionObject collisionObject = proxy.clientObject;

            if(m_resultCallback.needsCollision(collisionObject.getBroadphaseHandle()))
            {
                CollisionWorld.objectQuerySingle(m_castShape, rayFromTrans, rayToTrans,
                collisionObject,
                m_resultCallback,
                m_allowedCcdPenetration);
            }
            return true;
        }
    }

    public abstract class ConvexResultCallback
    {
        public VFixedPoint m_closestHitFraction = VFixedPoint.One;
        public short m_collisionFilterGroup = CollisionFilterGroups.DEFAULT_FILTER;
        public short m_collisionFilterMask = CollisionFilterGroups.ALL_FILTER;

        public bool hasHit()
		{
			return m_closestHitFraction < VFixedPoint.One;
		}

        public virtual bool needsCollision(BroadphaseProxy proxy0)
		{
			bool collides = (proxy0.collisionFilterGroup & m_collisionFilterMask) != 0;
            collides = collides && (m_collisionFilterGroup & proxy0.collisionFilterMask) != 0;
			return collides;
		}

        public abstract VFixedPoint addSingleResult(LocalConvexResult convexResult, bool normalInWorldSpace);
    };

    public class LocalConvexResult
    {
        public LocalConvexResult(CollisionObject hitCollisionObject,
			VInt3		hitNormalLocal,
            VInt3 hitPointLocal,
			VFixedPoint hitFraction
			)
        {
            m_hitCollisionObject = hitCollisionObject;
            m_hitNormalLocal = hitNormalLocal;
            m_hitPointLocal = hitPointLocal;
            m_hitFraction = hitFraction;
        }

        public CollisionObject m_hitCollisionObject;
        public VInt3 m_hitNormalLocal;
        public VInt3 m_hitPointLocal;
        public VFixedPoint m_hitFraction;
    }
}