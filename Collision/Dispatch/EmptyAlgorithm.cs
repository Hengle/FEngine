using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class EmptyAlgorithm: CollisionAlgorithm
    {
        private static EmptyAlgorithm INSTANCE = new EmptyAlgorithm();

        public override void destroy() {
        }

        public override void processCollision(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut) {
        }
    }
}