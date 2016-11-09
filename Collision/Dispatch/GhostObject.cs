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

        /**
         * This method is mainly for expert/internal use only.
         */
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
        public void removeOverlappingObjectInternal(BroadphaseProxy otherProxy, Dispatcher dispatcher, BroadphaseProxy thisProxy)
        {
            CollisionObject otherObject = otherProxy.clientObject;

            int index = overlappingObjects.IndexOf(otherObject);
            if (index != -1)
            {
                overlappingObjects[index] = overlappingObjects[overlappingObjects.Count - 1];
                overlappingObjects.RemoveAt(overlappingObjects.Count - 1);
            }
        }

        public void convexSweepTest(ConvexShape castShape, VIntTransform convexFromWorld, VIntTransform convexToWorld, CollisionWorld.ConvexResultCallback resultCallback, VFixedPoint allowedCcdPenetration)
        {
		    VInt3 castShapeAabbMin = VInt3.zero;
		    VInt3 castShapeAabbMax = VInt3.zero;

            // compute AABB that encompasses angular movement

            VInt3 linVel = VInt3.zero;
			VInt3 angVel = VInt3.zero;
			TransformUtil.calculateVelocity(convexFromWorld, convexToWorld, VFixedPoint.One, ref linVel, ref angVel);
			VIntTransform R = VIntTransform.Identity;
            R.rotation = convexFromWorld.rotation;
            castShape.calculateTemporalAabb(R, linVel, angVel, 1f, castShapeAabbMin, castShapeAabbMax);


            VIntTransform tmpTrans = VIntTransform.Identity;

		    // go over all objects, and if the ray intersects their aabb + cast shape aabb,
		    // do a ray-shape query using convexCaster (CCD)
		    for (int i = 0; i<overlappingObjects.Count; i++)
            {
			    CollisionObject collisionObject = overlappingObjects[i];

			    // only perform raycast if filterMask matches
			    if (resultCallback.needsCollision(collisionObject.getBroadphaseHandle()))
                {
				    //RigidcollisionObject* collisionObject = ctrl->GetRigidcollisionObject();
				    VInt3 collisionObjectAabbMin = VInt3.zero;
				    VInt3 collisionObjectAabbMax = VInt3.zero;
				    collisionObject.getCollisionShape().getAabb(collisionObject.getWorldTransform(out tmpTrans), collisionObjectAabbMin, collisionObjectAabbMax);
				    AabbUtil2.aabbExpand(collisionObjectAabbMin, collisionObjectAabbMax, castShapeAabbMin, castShapeAabbMax);
				    VFixedPoint[] hitLambda = new VFixedPoint[] { VFixedPoint.One }; // could use resultCallback.closestHitFraction, but needs testing
                    VInt3 hitNormal = VInt3.forward;
				    if (AabbUtil2.rayAabb(convexFromWorld.position, convexToWorld.position, collisionObjectAabbMin, collisionObjectAabbMax, hitLambda, hitNormal))
                    {
					    CollisionWorld.objectQuerySingle(castShape, convexFromWorld, convexToWorld,
					                                     collisionObject,
					                                     collisionObject.getCollisionShape(),
					                                     collisionObject.getWorldTransform(out tmpTrans),
					                                     resultCallback,
					                                     allowedCcdPenetration);
				    }
			    }
		    }
	    }

        public void rayTest(VInt3 rayFromWorld, VInt3 rayToWorld, CollisionWorld.RayResultCallback resultCallback)
        {
            VIntTransform rayFromTrans = VIntTransform.Identity;
		    rayFromTrans.position = rayFromWorld;
            VIntTransform rayToTrans = VIntTransform.Identity;
		    rayToTrans.position = rayToWorld;

            VIntTransform tmpTrans = VIntTransform.Identity;

		    for (int i = 0; i<overlappingObjects.Count; i++)
            {
			    CollisionObject collisionObject = overlappingObjects[i];
			
			    // only perform raycast if filterMask matches
			    if (resultCallback.needsCollision(collisionObject.getBroadphaseHandle()))
                {
				    CollisionWorld.rayTestSingle(rayFromTrans, rayToTrans,
				                                 collisionObject,
				                                 collisionObject.getCollisionShape(),
				                                 collisionObject.getWorldTransform(out tmpTrans),
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
