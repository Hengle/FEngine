using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    public class BoxBoxDetector: DiscreteCollisionDetectorInterface
    {
        BoxShape polyA;
        BoxShape polyB;

        public BoxBoxDetector(BoxShape box1, BoxShape box2)
        {
            polyA = box1;
            polyB = box2;
        }

        // for all 15 possible separating axes:
        //   * see if the axis separates the boxes. if so, return 0.
        //   * find the depth of the penetration along the separating axis (s2)
        //   * if this is the mininum depth so far, record it.
        // the normal vector will be set to the separating axis with the smallest
        // depth. note: normalR is set to point to a column of R1 or R2 if that is
        // the smallest depth normal so far. otherwise normalR is 0 and normalC is
        // set to a vector relative to body 1. invert_normal is 1 if the sign of
        // the normal should be flipped.

        public override void getClosestPoints(ClosestPointInput input, Result output)
        {
            
        }

    }
}
