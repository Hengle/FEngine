using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class ManifoldResult : DiscreteCollisionDetectorInterface.Result
    {
        protected readonly ObjectPool<ManifoldPoint> pointsPool = new ObjectPool<ManifoldPoint>();

        public ManifoldPoint manifoldPoint;

        // we need this for compounds
        public CollisionObject body0;
        public CollisionObject body1;
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
        }

        public override void setShapeIdentifiers(int partId0, int index0, int partId1, int index1)
        {
            this.partId0 = partId0;
            this.partId1 = partId1;
            this.index0 = index0;
            this.index1 = index1;
        }

        public override void addContactPoint(VInt3 normalOnBInWorld, VFixedPoint depth)
        {
            ManifoldPoint newPt = pointsPool.Get();
            newPt.init(normalOnBInWorld, depth);

            // BP mod, store contact triangles.
            newPt.partId0 = partId0;
            newPt.partId1 = partId1;
            newPt.index0 = index0;
            newPt.index1 = index1;

            manifoldPoint = newPt;
            pointsPool.Release(newPt);
        }
    }
}
