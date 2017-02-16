﻿namespace MobaGame.Collision
{
    public class GhostPairCallback: OverlappingPairCallback
    {
        public override BroadphasePair addOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            CollisionObject colObj0 = proxy0.clientObject;
            CollisionObject colObj1 = proxy1.clientObject;
            GhostObject ghost0 = colObj0 as GhostObject;
            GhostObject ghost1 = colObj1 as GhostObject;

            if (ghost0 != null)
            {
                ghost0.addOverlappingObjectInternal(proxy1, proxy0);
            }
            if (ghost1 != null)
            {
                ghost1.addOverlappingObjectInternal(proxy0, proxy1);
            }
            return null;
        }

        public override void removeOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            CollisionObject colObj0 = proxy0.clientObject;
            CollisionObject colObj1 = proxy1.clientObject;
            GhostObject ghost0 = colObj0 as GhostObject;
            GhostObject ghost1 = colObj1 as GhostObject;

            if (ghost0 != null)
            {
                ghost0.removeOverlappingObjectInternal(proxy1);
            }
            if (ghost1 != null)
            {
                ghost1.removeOverlappingObjectInternal(proxy0);
            }
        }

        public override void removeOverlappingPairsContainingProxy(BroadphaseProxy proxy0)
        {

        }
    }
}
