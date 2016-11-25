using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class SphereBoxCollisionAlgorithm: CollisionAlgorithm
    {
        bool ownManifold;
        PersistentManifold manifoldPtr;
        bool isSwapped;

        public void init(PersistentManifold mf, CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1, bool isSwapped)
        {
            base.init(ci);
            ownManifold = false;
            manifoldPtr = mf;
            this.isSwapped = isSwapped;

            CollisionObject sphereObj = isSwapped ? body1 : body0;
            CollisionObject boxObj = isSwapped ? body0 : body1;

            if(manifoldPtr == null && dispatcher.needsCollision(sphereObj, boxObj))
            {
                manifoldPtr = dispatcher.getNewManifold(sphereObj, boxObj);
                ownManifold = true;
            }
        }

        public override void destroy()
        {
            if (ownManifold)
            {
                if (manifoldPtr != null)
                {
                    dispatcher.releaseManifold(manifoldPtr);
                }
                manifoldPtr = null;
            }
        }

        public override void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut)
        {
            if(manifoldPtr == null)
            {
                return;
            }

            CollisionObject sphereObj = isSwapped ? body1 : body0;
            CollisionObject boxObj = isSwapped ? body0 : body1;

            VInt3 pOnBox;

            VInt3 normalOnSurfaceB;
            VFixedPoint penetrationDepth;
            VInt3 sphereCenter = sphereObj.getWorldTransform().position;
            SphereShape sphere0  = (SphereShape)sphereObj.getCollisionShape();
            VFixedPoint radius = sphere0.getRadius();
            VFixedPoint maxContactDistance = manifoldPtr.getContactBreakingThreshold();

            resultOut.setPersistentManifold(manifoldPtr);

            if(getSphereDistance(boxObj, out pOnBox, out normalOnSurfaceB, out penetrationDepth, sphereCenter, radius, maxContactDistance))
            {
                resultOut.addContactPoint(normalOnSurfaceB, pOnBox, penetrationDepth);
            }

            if(ownManifold)
            {
                if(manifoldPtr.getNumContacts() != 0)
                {
                    resultOut.refreshContactPoints();
                }
            }
        }

        public override VFixedPoint calculateTimeOfImpact(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut)
        {
            return VFixedPoint.One;
        }

        public override void getAllContactManifolds(List<PersistentManifold> manifoldArray)
        {
            if(manifoldPtr != null && ownManifold)
            {
                manifoldArray.Add(manifoldPtr);
            }
        }

        public bool getSphereDistance(CollisionObject boxObj, out VInt3 pointOnBox, out VInt3 normal, out VFixedPoint penetrationDepth, VInt3 sphereCenter, VFixedPoint radius, VFixedPoint maxContactDistance)
        {
            BoxShape boxShape = (BoxShape)boxObj.getCollisionShape();
            VInt3 boxHalfExtent = boxShape.getHalfExtentsWithoutMargin();
            VFixedPoint boxMargin = boxShape.getMargin();
            penetrationDepth = VFixedPoint.One;

            // convert the sphere position to the box's local space
            VIntTransform m44T = boxObj.getWorldTransform();
            VInt3 sphereRelPos = m44T.InverseTransformPoint(sphereCenter);

            // Determine the closest point to the sphere center in the box
            VInt3 closestPoint = sphereRelPos;
            closestPoint.x = (FMath.Min(boxHalfExtent.x, closestPoint.x));
            closestPoint.x = (FMath.Max(-boxHalfExtent.x, closestPoint.x));
            closestPoint.y = (FMath.Min(boxHalfExtent.y, closestPoint.y));
            closestPoint.y = (FMath.Max(-boxHalfExtent.y, closestPoint.y));
            closestPoint.z = (FMath.Min(boxHalfExtent.z, closestPoint.z));
            closestPoint.z = (FMath.Max(-boxHalfExtent.z, closestPoint.z));

            VFixedPoint intersectionDist = radius + boxMargin;
            VFixedPoint contactDist = intersectionDist + maxContactDistance;
            normal = sphereRelPos - closestPoint;

            //if there is no penetration, we are done
            VFixedPoint dist2 = normal.sqrMagnitude;
            if (dist2 > contactDist * contactDist)
            {
                pointOnBox = VInt3.zero;
                return false;
            }

            VFixedPoint distance;

            //special case if the sphere center is inside the box
            if (dist2 <= Globals.EPS)
            {
                distance = -getSpherePenetration(boxHalfExtent, sphereRelPos, ref closestPoint, out normal);
            }
            else //compute the penetration details
            {
                distance = normal.magnitude;
                normal /= distance;
            }

            pointOnBox = closestPoint + normal * boxMargin;
            //	v3PointOnSphere = sphereRelPos - (normal * fRadius);	
            penetrationDepth = distance - intersectionDist;

            // transform back in world space
            VInt3 tmp = m44T.TransformPoint(pointOnBox);
            pointOnBox = tmp;
            //	tmp = m44T(v3PointOnSphere);
            //	v3PointOnSphere = tmp;
            tmp = m44T.TransformVector(normal);
            normal = tmp;

            return true;
        }

        public VFixedPoint getSpherePenetration(VInt3 boxHalfExtent, VInt3 sphereRelPos, ref VInt3 closestPoint, out VInt3 normal)
        {
            //project the center of the sphere on the closest face of the box
            VFixedPoint faceDist = boxHalfExtent.x - sphereRelPos.x;
            VFixedPoint minDist = faceDist;
            closestPoint.x = boxHalfExtent.x;
            normal = VInt3.right;

            faceDist = boxHalfExtent.x + sphereRelPos.x;
            if (faceDist < minDist)
            {
                minDist = faceDist;
                closestPoint = sphereRelPos;
                closestPoint.x = -boxHalfExtent.x;
                normal. = -VInt3.right;
            }

            faceDist = boxHalfExtent.y - sphereRelPos.y;
            if (faceDist < minDist)
            {
                minDist = faceDist;
                closestPoint = sphereRelPos;
                closestPoint.y = boxHalfExtent.y;
                normal = VInt3.up;
            }

            faceDist = boxHalfExtent.y + sphereRelPos.y;
            if (faceDist < minDist)
            {
                minDist = faceDist;
                closestPoint = sphereRelPos;
                closestPoint.y = -boxHalfExtent.y;
                normal = -VInt3.up;
            }

            faceDist = boxHalfExtent.z - sphereRelPos.z;
            if (faceDist < minDist)
            {
                minDist = faceDist;
                closestPoint = sphereRelPos;
                closestPoint.z = boxHalfExtent.z;
                normal = VInt3.forward;
            }

            faceDist = boxHalfExtent.z + sphereRelPos.z;
            if (faceDist < minDist)
            {
                minDist = faceDist;
                closestPoint = sphereRelPos;
                closestPoint.z = -boxHalfExtent.z;
                normal = -VInt3.forward;
            }

            return minDist;
        }

        public class CreateFunc : CollisionAlgorithmCreateFunc
        {
            private ObjectPool<SphereBoxCollisionAlgorithm> pool = new ObjectPool<SphereBoxCollisionAlgorithm>();

            public override CollisionAlgorithm createCollisionAlgorithm(CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1)
            {
                SphereBoxCollisionAlgorithm algo = pool.Get();
                algo.init(null, ci, body0, body1, swapped);
                return algo;
            }

            public override void releaseCollisionAlgorithm(CollisionAlgorithm algo)
            {
                pool.Release((SphereBoxCollisionAlgorithm)algo);
            }
        };
    }
}
