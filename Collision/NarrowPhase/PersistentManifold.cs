using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class PersistentManifold
    {
        public const int MANIFOLD_CACHE_SIZE = 4;

        ManifoldPoint[] pointCache = new ManifoldPoint[MANIFOLD_CACHE_SIZE];

        public CollisionObject body0, body1;
        int cachedPoints;

        public PersistentManifold()
        {
            for(int i = 0; i < pointCache.Length; i++)
            {
                pointCache[i] = new ManifoldPoint();
            }
        }

        public PersistentManifold(CollisionObject body0, CollisionObject body1): this()
        {
            init(body0, body1);
        }

        public void init(CollisionObject body0, CollisionObject body1)
        {
            this.body0 = body0; this.body1 = body1;
            cachedPoints = 0;
        }

        public void clearManifold()
        {
            cachedPoints = 0;
        }
        
        public int getContactPointsNum()
        {
            return cachedPoints;
        }

        public int addManifoldPoint(ManifoldPoint newPoint)
        {
            int insertIndex = cachedPoints;
            if(insertIndex == MANIFOLD_CACHE_SIZE)
            {
                insertIndex = 0;
            }
            else
            {
                cachedPoints++;
            }
            newPoint.localPointA = body0.getWorldTransform().InverseTransformPoint(newPoint.positionWorldOnA);
            newPoint.localPointB = body0.getWorldTransform().InverseTransformPoint(newPoint.positionWorldOnB);
            pointCache[insertIndex] = newPoint;
            return insertIndex;
        }

        public void removeContactPoint(int index)
        {
            int lastUsedIndex = cachedPoints - 1;
            if(index != lastUsedIndex)
            {
                pointCache[index].set(pointCache[lastUsedIndex]);
            }

            cachedPoints--;
        }

        public void refreshContactPoints(VIntTransform trA, VIntTransform trB)
        {
            VInt3 tmp = VInt3.zero;
            for(int i = cachedPoints - 1; i >= 0; i--)
            {
                ManifoldPoint manifoldPoint = pointCache[i];
                manifoldPoint.positionWorldOnA = trA.TransformPoint(manifoldPoint.localPointA);
                manifoldPoint.positionWorldOnB = trB.TransformPoint(manifoldPoint.localPointB);
                manifoldPoint.distance = VInt3.Dot(manifoldPoint.normalWorldOnB, manifoldPoint.positionWorldOnA - manifoldPoint.positionWorldOnB);
            }

            VFixedPoint distance2d = VFixedPoint.Zero;
            VInt3 projectedDifference = VInt3.zero, projectedPoint = VInt3.zero;

            for(int i = cachedPoints - 1; i >= 0; i--)
            {
                ManifoldPoint manifoldPoint = pointCache[i];
                if(!validContactDistance(manifoldPoint))
                {
                    removeContactPoint(i);
                }
            }
        }
        
        VFixedPoint getContactBreakingThreshold()
        {
            return Globals.getContactBreakingThreshold();
        }

        bool validContactDistance(ManifoldPoint pt)
        {
            return pt.distance <= getContactBreakingThreshold();
        }

        public ManifoldPoint getManifoldPoint(int index)
        {
            return pointCache[index];
        }
    }


    public class ManifoldPoint
    {
        public VInt3 localPointA;
        public VInt3 localPointB;
        public VInt3 positionWorldOnA;
        public VInt3 positionWorldOnB;
        public VInt3 normalWorldOnB;
        public VFixedPoint distance;

        public ManifoldPoint()
        {

        }

        public ManifoldPoint(VInt3 pointA, VInt3 pointB, VInt3 normal, VFixedPoint distance)
        {
            init(pointA, pointB, normal, distance);
        }

        public void init(VInt3 pointA, VInt3 pointB, VInt3 normal, VFixedPoint distance)
        {
            positionWorldOnA = pointA;
            positionWorldOnB = pointB;
            normalWorldOnB = normal;
            this.distance = distance;
        }
    }
}
