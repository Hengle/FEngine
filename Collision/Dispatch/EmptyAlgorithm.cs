﻿using MobaGame.FixedMath;
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

        public override VFixedPoint calculateTimeOfImpact(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut) {
            return VFixedPoint.One;
        }

        public override void getAllContactManifolds(List<PersistentManifold> manifoldArray) {
        }

        ////////////////////////////////////////////////////////////////////////////

        public class CreateFunc: CollisionAlgorithmCreateFunc
        {
            public override CollisionAlgorithm createCollisionAlgorithm(CollisionAlgorithmConstructionInfo ci, CollisionObject body0, CollisionObject body1) {
                return INSTANCE;
            }

            public override void releaseCollisionAlgorithm(CollisionAlgorithm algo) {
            }
        };
    }
}