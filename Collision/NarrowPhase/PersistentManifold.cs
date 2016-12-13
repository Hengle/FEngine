using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class PersistentManifold
    {
        public static readonly int MANIFOLD_CACHE_SIZE = 4;
        private readonly ManifoldPoint[] pointCache = new ManifoldPoint[MANIFOLD_CACHE_SIZE];

        private CollisionObject body0;
        private CollisionObject body1;
        private int cachedPoints;

        public int index1a;

        public PersistentManifold()
        {
            for (int i = 0; i < pointCache.Length; i++)
            {
                pointCache[i] = new ManifoldPoint();
            }
        }

        public PersistentManifold(CollisionObject body0, CollisionObject body1, int bla):this()
        {
            init(body0, body1, bla);
        }

        public void init(CollisionObject body0, CollisionObject body1, int bla)
        {
            this.body0 = body0;
            this.body1 = body1;
            cachedPoints = 0;
            index1a = 0;
        }

        /// sort cached points so most isolated points come first
        private int sortCachedPoints(ManifoldPoint pt)
        {
            //calculate 4 possible cases areas, and take biggest area
            //also need to keep 'deepest'

            int maxPenetrationIndex = -1;
            //#define KEEP_DEEPEST_POINT 1
            //#ifdef KEEP_DEEPEST_POINT
            VFixedPoint maxPenetration = pt.getDistance();
            for (int i = 0; i < 4; i++)
            {
                if (pointCache[i].getDistance() < maxPenetration)
                {
                    maxPenetrationIndex = i;
                    maxPenetration = pointCache[i].getDistance();
                }
            }
            //#endif //KEEP_DEEPEST_POINT

            VFixedPoint maxVal = VFixedPoint.MinValue;
            int maxNum = -1;

            if (maxPenetrationIndex != 0)
            {
                VInt3 a0 = pt.localPointA - pointCache[1].localPointA;
                VInt3 b0 = pointCache[3].localPointA - pointCache[2].localPointA;
                VInt3 cross = VInt3.Cross(a0, b0);
			    VFixedPoint res = cross.sqrMagnitude;
                if(maxVal > res)
                {
                    maxVal = res;
                    maxNum = 0;
                }
		    }

		    if (maxPenetrationIndex != 1) {
			    VInt3 a1 = pt.localPointA - pointCache[0].localPointA;
			    VInt3 b1 = pointCache[3].localPointA - pointCache[2].localPointA;
                VInt3 cross = VInt3.Cross(a1, b1);
                VFixedPoint res = cross.sqrMagnitude;
                if (maxVal > res)
                {
                    maxVal = res;
                    maxNum = 1;
                }
            }

		    if (maxPenetrationIndex != 2) {
                VInt3 a2 = pt.localPointA - pointCache[0].localPointA;
                VInt3 b2 = pointCache[3].localPointA - pointCache[1].localPointA;
                VInt3 cross = VInt3.Cross(a2, b2);
                VFixedPoint res = cross.sqrMagnitude;
                if (maxVal > res)
                {
                    maxVal = res;
                    maxNum = 2;
                }
            }

		    if (maxPenetrationIndex != 3) {
                VInt3 a3 = pt.localPointA - pointCache[0].localPointA;
                VInt3 b3 = pointCache[3].localPointA - pointCache[2].localPointA;
                VInt3 cross = VInt3.Cross(a3, b3);
                VFixedPoint res = cross.sqrMagnitude;
                if (maxVal > res)
                {
                    maxVal = res;
                    maxNum = 3;
                }
            }

            return maxNum;
	    }

        public CollisionObject getBody0()
        {
            return body0;
        }

        public CollisionObject getBody1()
        {
            return body1;
        }

        public void setBodies(CollisionObject body0, CollisionObject body1)
        {
            this.body0 = body0;
            this.body1 = body1;
        }

        public void clearUserCache(ManifoldPoint pt)
        {
            pt.userPersistentData = null;
        }

        public int getNumContacts()
        {
            return cachedPoints;
        }

        public ManifoldPoint getContactPoint(int index)
        {
            return pointCache[index];
        }

        public VFixedPoint getContactBreakingThreshold()
        {
            return Globals.getContactBreakingThreshold();
        }

        public int getCacheEntry(ManifoldPoint newPoint)
        {
            VFixedPoint shortestDist = getContactBreakingThreshold() * getContactBreakingThreshold();
            int size = getNumContacts();
            int nearestPoint = -1;
            VInt3 diffA = new VInt3();
		    for (int i = 0; i<size; i++)
            {
			    ManifoldPoint mp = pointCache[i];
                diffA = mp.localPointA - newPoint.localPointA;

			    VFixedPoint distToManiPoint = diffA.sqrMagnitude;
			    if (distToManiPoint<shortestDist) {
				    shortestDist = distToManiPoint;
				    nearestPoint = i;
			    }
            }
		    return nearestPoint;
	    }

	    public int addManifoldPoint(ManifoldPoint newPoint)
        {
            int insertIndex = getNumContacts();
            if (insertIndex == MANIFOLD_CACHE_SIZE)
            {
                if (MANIFOLD_CACHE_SIZE >= 4)
                {
                    //sort cache so best points come first, based on area
                    insertIndex = sortCachedPoints(newPoint);
                }
                else
                {
                    insertIndex = 0;
                }
                clearUserCache(pointCache[insertIndex]);
            }
            else {
                cachedPoints++;
            }

            pointCache[insertIndex].set(newPoint);
            return insertIndex;
        }

        public void removeContactPoint(int index)
        {
            clearUserCache(pointCache[index]);

            int lastUsedIndex = getNumContacts() - 1;
            //		m_pointCache[index] = m_pointCache[lastUsedIndex];
            if (index != lastUsedIndex)
            {
                // TODO: possible bug
                pointCache[index].set(pointCache[lastUsedIndex]);
                //get rid of duplicated userPersistentData pointer
                pointCache[lastUsedIndex].userPersistentData = null;
                pointCache[lastUsedIndex].appliedImpulse = VFixedPoint.Zero;
                pointCache[lastUsedIndex].lateralFrictionInitialized = false;
                pointCache[lastUsedIndex].appliedImpulseLateral1 = VFixedPoint.Zero;
                pointCache[lastUsedIndex].appliedImpulseLateral2 = VFixedPoint.Zero;
                pointCache[lastUsedIndex].lifeTime = 0;
            }

            cachedPoints--;
        }

        public void replaceContactPoint(ManifoldPoint newPoint, int insertIndex)
        {
            int lifeTime = pointCache[insertIndex].getLifeTime();
            VFixedPoint appliedImpulse = pointCache[insertIndex].appliedImpulse;
            VFixedPoint appliedLateralImpulse1 = pointCache[insertIndex].appliedImpulseLateral1;
            VFixedPoint appliedLateralImpulse2 = pointCache[insertIndex].appliedImpulseLateral2;

            ConstraintPersistentData cache = pointCache[insertIndex].userPersistentData;

            pointCache[insertIndex].set(newPoint);
            pointCache[insertIndex].userPersistentData = cache;
            pointCache[insertIndex].appliedImpulse = appliedImpulse;
            pointCache[insertIndex].appliedImpulseLateral1 = appliedLateralImpulse1;
            pointCache[insertIndex].appliedImpulseLateral2 = appliedLateralImpulse2;

            pointCache[insertIndex].lifeTime = lifeTime;

        }

        private bool validContactDistance(ManifoldPoint pt)
        {
            return pt.distance1 <= getContactBreakingThreshold();
        }

        public void refreshContactPoints(VIntTransform trA, VIntTransform trB)
        {
            VInt3 tmp = new VInt3();

		    // first refresh worldspace positions and distance
		    for (int i = getNumContacts() - 1; i >= 0; i--) {
			    ManifoldPoint manifoldPoint = pointCache[i];

                manifoldPoint.positionWorldOnA = manifoldPoint.localPointA;
			    trA.TransformPoint(manifoldPoint.positionWorldOnA);

			    manifoldPoint.positionWorldOnB = manifoldPoint.localPointB;
			    trB.TransformPoint(manifoldPoint.positionWorldOnB);

			    tmp = manifoldPoint.positionWorldOnA - manifoldPoint.positionWorldOnB;
			    manifoldPoint.distance1 = VInt3.Dot(tmp, manifoldPoint.normalWorldOnB);

			    manifoldPoint.lifeTime++;
		    }

            // then 
            VFixedPoint distance2d;
            VInt3 projectedDifference = new VInt3();
            VInt3 projectedPoint = new VInt3();

		    for (i = getNumContacts() - 1; i >= 0; i--) {

			    ManifoldPoint manifoldPoint = pointCache[i];
			    // contact becomes invalid when signed distance exceeds margin (projected on contactnormal direction)
			    if (!validContactDistance(manifoldPoint)) {
                    removeContactPoint(i);
                }
			    else {
				    // contact also becomes invalid when relative movement orthogonal to normal exceeds margin
				    tmp = manifoldPoint.normalWorldOnB * manifoldPoint.distance1;
				    projectedPoint = manifoldPoint.positionWorldOnA - tmp;
				    projectedDifference = manifoldPoint.positionWorldOnB - projectedPoint;
				    distance2d = VInt3.Dot(projectedDifference, projectedDifference);
				    if (distance2d > getContactBreakingThreshold() * getContactBreakingThreshold()) {
                        removeContactPoint(i);
				    }
			    }
		    }
	    }

        public void clearManifold()
        {
            cachedPoints = 0;
            for (int i = 0; i < cachedPoints; i++)
            {
                clearUserCache(pointCache[i]);
            }
        }

    }
}
