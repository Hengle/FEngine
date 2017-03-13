using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public static class SphereBoxCollisionAlgorithm
    {
        public static void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, PersistentManifold resultOut)
        {
            bool isSwapped = body0.getCollisionShape() is SphereShape;
            CollisionObject sphereObj = isSwapped ? body1 : body0;
            CollisionObject boxObj = isSwapped ? body0 : body1;

            VInt3 normalOnSurfaceB;
            VFixedPoint penetrationDepth;
            VInt3 sphereCenter = sphereObj.getWorldTransform().position;
            SphereShape sphere0  = (SphereShape)sphereObj.getCollisionShape();
            VFixedPoint radius = sphere0.getRadius();

            if(getSphereDistance((BoxShape)boxObj.getCollisionShape(), boxObj.getWorldTransform(), sphereCenter, radius, out normalOnSurfaceB, out penetrationDepth))
            {
                VInt3 worldPosOnSphere = sphereCenter - normalOnSurfaceB * radius;
                VInt3 worldPosOnBox = worldPosOnSphere + normalOnSurfaceB * penetrationDepth;
                ManifoldPoint contactPoint = new ManifoldPoint(isSwapped ? worldPosOnBox : worldPosOnSphere, isSwapped ? worldPosOnSphere : worldPosOnBox, normalOnSurfaceB * (isSwapped ? -1 : 1), penetrationDepth);
                resultOut.addManifoldPoint(contactPoint);
            }
        }

        public static bool getSphereDistance(BoxShape boxShape, VIntTransform m44T, VInt3 sphereCenter, VFixedPoint radius, out VInt3 normal, out VFixedPoint penetrationDepth)
        {
            VInt3 boxHalfExtent = boxShape.getHalfExtent();
            VFixedPoint boxMargin = boxShape.getMargin();
            penetrationDepth = VFixedPoint.Zero;

            // convert the sphere position to the box's local space
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
            VFixedPoint contactDist = intersectionDist;
            normal = sphereRelPos - closestPoint;

            //if there is no penetration, we are done
            VFixedPoint dist2 = normal.sqrMagnitude;
            if (dist2 > contactDist * contactDist)
            {
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

            //	v3PointOnSphere = sphereRelPos - (normal * fRadius);	
            penetrationDepth = distance - intersectionDist;

            VInt3 tmp = m44T.TransformDirection(normal);
            normal = tmp;

            return true;
        }

        static VFixedPoint getSpherePenetration(VInt3 boxHalfExtent, VInt3 sphereRelPos, ref VInt3 closestPoint, out VInt3 normal)
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
                normal = -VInt3.right;
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
    }
}
