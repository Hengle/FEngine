using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public static class Collisions
    {
        private static readonly int ESTIMATED_COLLISIONS_PER_OBJECT = 4;

        public static int getEstimatedCollisionPairs(int n)
        {
            return n * 4;
        }

        public static int getEstimatedCollisionsPerObject()
        {
            return 4;
        }

        public static int getEstimatedRaycastCollisions(int n)
        {
            return (int)FMath.Max(VFixedPoint.One, VFixedPoint.Create(2) / VFixedPoint.Create(100) * n).ToInt;
        }
    }
}
