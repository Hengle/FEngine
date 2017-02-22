using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class EmptyObjectQueryFunc : ObjectQueryAlgorithm
    {
        public override void objectQuerySingle(ConvexShape castShape, VIntTransform convexFromTrans, VIntTransform convexToTrans, CollisionObject collisionObject, ConvexResultCallback resultCallback, VFixedPoint allowedPenetration)
        {

        }
    }
}
