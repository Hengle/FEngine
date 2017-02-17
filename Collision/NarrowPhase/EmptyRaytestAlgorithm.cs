using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    class EmptyRaytestAlgorithm : RaytestAlgorithm
    {
        public override void rayTestSingle(VIntTransform rayFromTrans, VIntTransform rayToTrans, CollisionObject collisionObject, RayResultCallback resultCallback)
        {
            
        }
    }
}
