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

        public override void addContactPoint(VInt3 normalOnBInWorld, VInt3 pointInWorld, VFixedPoint depth)
        {
            if (depth < distance)
            {
                hasResult = true;
                this.normalOnBInWorld = normalOnBInWorld;
                this.pointInWorld = pointInWorld;
                // negative means penetration
                distance = depth;
            }
        }

        public override void setShapeIdentifiers(int partId0, int index0, int partId1, int index1)
        {

        }
    }
}
