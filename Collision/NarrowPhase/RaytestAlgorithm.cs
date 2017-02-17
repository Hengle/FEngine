using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class RaytestAlgorithm
    {
        public abstract void rayTestSingle(VIntTransform rayFromTrans, VIntTransform rayToTrans,
            CollisionObject collisionObject,
            RayResultCallback resultCallback);
    }
}
