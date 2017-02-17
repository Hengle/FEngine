using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class GhostObject: CollisionObject
    {
        protected List<CollisionObject> overlappingObjects = new List<CollisionObject>();

        public GhostObject()
        {
            this.internalType = CollisionObjectType.GHOST_OBJECT;
        }

        public void addOverlappingObjectInternal(BroadphaseProxy otherProxy, BroadphaseProxy thisProxy)
        {
            CollisionObject otherObject = otherProxy.clientObject;

            // if this linearSearch becomes too slow (too many overlapping objects) we should add a more appropriate data structure
            int index = overlappingObjects.IndexOf(otherObject);
            if (index == -1)
            {
                // not found
                overlappingObjects.Add(otherObject);
            }
        }

        /**
         * This method is mainly for expert/internal use only.
         */
        public void removeOverlappingObjectInternal(BroadphaseProxy otherProxy)
        {
            CollisionObject otherObject = otherProxy.clientObject;

            int index = overlappingObjects.IndexOf(otherObject);
            if (index != -1)
            {
                overlappingObjects[index] = overlappingObjects[overlappingObjects.Count - 1];
                overlappingObjects.RemoveAt(overlappingObjects.Count - 1);
            }
        }

        public void convexSweepTest(ConvexShape castShape, VIntTransform convexFromWorld, VIntTransform convexToWorld, ConvexResultCallback resultCallback, VFixedPoint allowedCcdPenetration)
        {
		    VInt3 castShapeAabbMin;
		    VInt3 castShapeAabbMax;

            // compute AABB that encompasses angular movement

            VInt3 linVel = VInt3.zero;
			VInt3 angVel = VInt3.zero;
			TransformUtil.calculateVelocity(convexFromWorld, convexToWorld, VFixedPoint.One, ref linVel, ref angVel);
			VIntTransform R = VIntTransform.Identity;
            R.rotation = convexFromWorld.rotation;
            castShape.calculateTemporalAabb(R, linVel, angVel, VFixedPoint.One, out castShapeAabbMin, out castShapeAabbMax);

		    // go over all objects, and if the ray intersects their aabb + cast shape aabb,
		    // do a ray-shape query using convexCaster (CCD)
		    for (int i = 0; i<overlappingObjects.Count; i++)
            {
			    CollisionObject collisionObject = overlappingObjects[i];

			    // only perform raycast if filterMask matches
			    if (resultCallback.needsCollision(collisionObject.getBroadphaseHandle()))
                {
				    VInt3 collisionObjectAabbMin = VInt3.zero;
				    VInt3 collisionObjectAabbMax = VInt3.zero;
				    collisionObject.getCollisionShape().getAabb(collisionObject.getWorldTransform(), out collisionObjectAabbMin, out collisionObjectAabbMax);
				    VFixedPoint hitLambda = VFixedPoint.One; // could use resultCallback.closestHitFraction, but needs testing
                    VInt3 hitNormal = VInt3.forward;
				    if (AabbUtils.RayAabb(convexFromWorld.position, convexToWorld.position, collisionObjectAabbMin, collisionObjectAabbMax, ref hitLambda, ref hitNormal))
                    {
					    CollisionWorld.objectQuerySingle(castShape, convexFromWorld, convexToWorld,
					                                     collisionObject,
					                                     resultCallback,
					                                     allowedCcdPenetration);
				    }
			    }
		    }
	    }

        public void rayTest(VInt3 rayFromWorld, VInt3 rayToWorld, RayResultCallback resultCallback)
        {
            VIntTransform rayFromTrans = VIntTransform.Identity;
		    rayFromTrans.position = rayFromWorld;
            VIntTransform rayToTrans = VIntTransform.Identity;
		    rayToTrans.position = rayToWorld;

		    for (int i = 0; i<overlappingObjects.Count; i++)
            {
			    CollisionObject collisionObject = overlappingObjects[i];
			
			    // only perform raycast if filterMask matches
			    if (resultCallback.needsCollision(collisionObject.getBroadphaseHandle()))
                {
				    CollisionWorld.rayTestSingle(rayFromTrans, rayToTrans,
				                                 collisionObject,
				                                 resultCallback);
			    }
            }
	    }

        public int getNumOverlappingObjects()
        {
            return overlappingObjects.Count;
        }

        public CollisionObject getOverlappingObject(int index)
        {
            return overlappingObjects[index];
        }

        public List<CollisionObject> getOverlappingPairs()
        {
            return overlappingObjects;
        }
    }
}
