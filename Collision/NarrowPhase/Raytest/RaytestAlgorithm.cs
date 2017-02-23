using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public delegate void RaytestAlgorithm(VIntTransform rayFromTrans, VIntTransform rayToTrans,
            CollisionObject collisionObject,
            RayResultCallback resultCallback);
}
