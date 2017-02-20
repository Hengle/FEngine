using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class PointCollector : DiscreteCollisionDetectorInterface.Result
    {
        public VInt3 normalOnBInWorld = VInt3.zero;
        public VInt3 pointInWorld = VInt3.zero;
        public VFixedPoint distance = VFixedPoint.MaxValue; // negative means penetration

        public bool hasResult = false;

        public override void addContactPoint(VInt3 normalOnBInWorld, VFixedPoint depth)
        {
            if (depth < distance)
            {
                hasResult = true;
                this.normalOnBInWorld = normalOnBInWorld;
                // negative means penetration
                distance = depth;
            }
        }
    }
}
