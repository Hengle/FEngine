using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class BoxBoxCollisionAlgorithm
    {
        static BoxBoxDetector detector = new BoxBoxDetector();

        public static void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut)
        {
            detector.init((BoxShape)body0.getCollisionShape(), (BoxShape)body1.getCollisionShape());
            ClosestPointInput input = new ClosestPointInput();
            input.maximumDistanceSquared = VFixedPoint.LARGE_NUMBER;
            input.transformA = body0.getWorldTransform();
            input.transformB = body1.getWorldTransform();

            detector.getClosestPoints(input, resultOut);
        }
    }
}
