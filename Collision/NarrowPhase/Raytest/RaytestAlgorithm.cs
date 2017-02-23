using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public delegate void RaytestAlgorithm(VInt3 FromPos, VInt3 ToPos,
            CollisionObject collisionObject,
            RayResultCallback resultCallback);
}
