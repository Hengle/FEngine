using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class ManifoldResult : DiscreteCollisionDetectorInterface.Result
    {
        protected readonly ObjectPool<ManifoldPoint> pointsPool = new ObjectPool<ManifoldPoint>();

        private PersistentManifold manifoldPtr;

        // we need this for compounds
        private VIntTransform rootTransA = VIntTransform.Identity;
        private VIntTransform rootTransB = VIntTransform.Identity;
        private CollisionObject body0;
        private CollisionObject body1;
        private int partId0;
        private int partId1;
        private int index0;
        private int index1;

        public ManifoldResult()
        {
        }

        public ManifoldResult(CollisionObject body0, CollisionObject body1)
        {
            init(body0, body1);
        }

        public void init(CollisionObject body0, CollisionObject body1)
        {
            this.body0 = body0;
            this.body1 = body1;
            this.rootTransA = body0.getWorldTransform();
            this.rootTransB = body1.getWorldTransform();
        }

        public PersistentManifold getPersistentManifold()
        {
            return manifoldPtr;
        }

        public void setPersistentManifold(PersistentManifold manifoldPtr)
        {
            this.manifoldPtr = manifoldPtr;
        }

        public override void setShapeIdentifiers(int partId0, int index0, int partId1, int index1)
        {
            this.partId0 = partId0;
            this.partId1 = partId1;
            this.index0 = index0;
            this.index1 = index1;
        }

        public override void addContactPoint(VInt3 normalOnBInWorld, VInt3 pointInWorld, VFixedPoint depth)
        {
            //order in manifold needs to match
            if (depth > manifoldPtr.getContactBreakingThreshold())
            {
                return;
            }

            bool isSwapped = manifoldPtr.getBody0() != body0;

            VInt3 pointA = normalOnBInWorld * depth + pointInWorld;

            VInt3 localA = new VInt3();
            VInt3 localB = new VInt3();

            if (isSwapped)
            {
                localA = rootTransB.InverseTransformPoint(pointA);
                localB = rootTransA.InverseTransformPoint(pointA);
            }
            else {
                localA = rootTransA.InverseTransformPoint(pointA);
                localB = rootTransB.InverseTransformPoint(pointA);
            }

            ManifoldPoint newPt = pointsPool.Get();
            newPt.init(localA, localB, normalOnBInWorld, depth);

            newPt.positionWorldOnA = pointA;
            newPt.positionWorldOnB = pointInWorld;

            int insertIndex = manifoldPtr.getCacheEntry(newPt);

            // BP mod, store contact triangles.
            newPt.partId0 = partId0;
            newPt.partId1 = partId1;
            newPt.index0 = index0;
            newPt.index1 = index1;

            /// todo, check this for any side effects
            if (insertIndex >= 0)
            {
                //const btManifoldPoint& oldPoint = m_manifoldPtr->getContactPoint(insertIndex);
                manifoldPtr.replaceContactPoint(newPt, insertIndex);
            }
            else {
                insertIndex = manifoldPtr.addManifoldPoint(newPt);
            }

            pointsPool.Release(newPt);
        }

        public void refreshContactPoints()
        {
            if (manifoldPtr.getNumContacts() == 0)
            {
                return;
            }

            bool isSwapped = manifoldPtr.getBody0() != body0;

            if (isSwapped)
            {
                manifoldPtr.refreshContactPoints(rootTransB, rootTransA);
            }
            else
            {
                manifoldPtr.refreshContactPoints(rootTransA, rootTransB);
            }
        }
    }
}
