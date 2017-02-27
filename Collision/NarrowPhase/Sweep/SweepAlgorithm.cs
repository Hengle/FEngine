using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public delegate void SweepAlgorithm(CollisionObject castObject, VInt3 start, VInt3 end,
                      CollisionObject collisionObject,
                      List<CastResult> results, VFixedPoint allowedPenetration);

    public class CastResult
    {
        public CollisionObject hitObject;
        public VInt3 normal;
        public VInt3 hitPoint;
        public VFixedPoint fraction = VFixedPoint.MaxValue; // input and output
    }
}
