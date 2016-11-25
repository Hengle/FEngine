using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    public class BoxBoxDetector: DiscreteCollisionDetectorInterface
    {
        BoxShape box1;
        BoxShape box2;

        public BoxBoxDetector(BoxShape box1, BoxShape box2)
        {
            this.box1 = box1;
            this.box2 = box2;
        }

        public override void getClosestPoints(ClosestPointInput input, Result output)
        {
            throw new NotImplementedException();
        }
    }
}
