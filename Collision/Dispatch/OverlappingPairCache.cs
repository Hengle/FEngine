using System;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public abstract class OverlappingPairCache: OverlappingPairCallback
    {
        public abstract List<BroadphasePair> getOverlappingPairArray();

        public abstract void cleanOverlappingPair(BroadphasePair pair, Dispatcher dispatcher);

        public abstract int getNumOverlappingPairs();

        public abstract void cleanProxyFromPairs(BroadphaseProxy proxy, Dispatcher dispatcher);

        public abstract void setOverlapFilterCallback(OverlapFilterCallback overlapFilterCallback);

        public abstract void processAllOverlappingPairs(OverlapCallback callback, Dispatcher dispatcher);

        public abstract BroadphasePair findPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1);

        public abstract bool hasDeferredRemoval();
    }
}
