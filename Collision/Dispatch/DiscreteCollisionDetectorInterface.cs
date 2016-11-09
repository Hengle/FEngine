using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class DiscreteCollisionDetectorInterface
    {
        public abstract class Result
        {
            ///setShapeIdentifiers provides experimental support for per-triangle material / custom material combiner
            public abstract void setShapeIdentifiers(int partId0, int index0, int partId1, int index1);

            public abstract void addContactPoint(VInt3 normalOnBInWorld, VInt3 pointInWorld, VFixedPoint depth);
        }

        public class ClosestPointInput
        {
            public VIntTransform transformA = VIntTransform.Identity;
            public VIntTransform transformB = VIntTransform.Identity;
            public VFixedPoint maximumDistanceSquared;
            //btStackAlloc* m_stackAlloc;

            public ClosestPointInput()
            {
                init();
            }

            public void init()
            {
                maximumDistanceSquared = VFixedPoint.MaxValue;
            }
        }

        /**
         * Give either closest points (distance > 0) or penetration (distance)
         * the normal always points from B towards A.
         */
        public void getClosestPoints(ClosestPointInput input, Result output)
        {
            getClosestPoints(input, output, false);
        }

        /**
         * Give either closest points (distance > 0) or penetration (distance)
         * the normal always points from B towards A.
         */
        public abstract void getClosestPoints(ClosestPointInput input, Result output, bool swapResults);
    }
}
