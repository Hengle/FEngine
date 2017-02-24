using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public delegate void RaytestAlgorithm(VInt3 FromPos, VInt3 ToPos,
            CollisionObject collisionObject,
            RayResultCallback resultCallback);


    public abstract class RayResultCallback
    {
        public VFixedPoint closestHitFraction;

        public abstract bool hasHit();
        public abstract VFixedPoint addSingleResult(CollisionObject collisionObject, VInt3 hitNormalLocal, VFixedPoint hitFraction);
    }
}
