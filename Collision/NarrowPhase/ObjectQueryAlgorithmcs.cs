using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class ObjectQueryAlgorithm
    {
        public abstract void objectQuerySingle(ConvexShape castShape, VIntTransform convexFromTrans, VIntTransform convexToTrans,
                      CollisionObject collisionObject,
                      ConvexResultCallback resultCallback, VFixedPoint allowedPenetration);
    }
}
