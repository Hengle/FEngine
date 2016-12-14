using System;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public abstract class OverlappingPairCache: OverlappingPairCallback
    {
        protected OverlappingPairCallback ghostPairCallback = new GhostPairCallback();

        public abstract List<BroadphasePair> getOverlappingPairArray();

        public abstract void cleanOverlappingPair(BroadphasePair pair, Dispatcher dispatcher);

        public abstract int getNumOverlappingPairs();

        public abstract void cleanProxyFromPairs(BroadphaseProxy proxy, Dispatcher dispatcher);

        public abstract void setOverlapFilterCallback(OverlapFilterCallback overlapFilterCallback);

        public abstract void processAllOverlappingPairs(OverlapCallback callback, Dispatcher dispatcher);

        public abstract BroadphasePair findPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1);

        public abstract bool hasDeferredRemoval();

        public void setInternalGhostPairCallback(OverlappingPairCallback ghostPairCallback)
        {
            this.ghostPairCallback = ghostPairCallback;
        }

        public override BroadphasePair addOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            // this is where we add an actual pair, so also call the 'ghost'
            if (ghostPairCallback != null)
            {
                return ghostPairCallback.addOverlappingPair(proxy0, proxy1);
            }
            return null;
        }

        public override void removeOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1, Dispatcher dispatcher)
        {
            if (ghostPairCallback != null)
            {
                ghostPairCallback.removeOverlappingPair(proxy0, proxy1, dispatcher);
            }
        }
    }
}
