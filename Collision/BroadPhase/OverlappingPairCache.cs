using System;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public abstract class OverlappingPairCache: OverlappingPairCallback
    {
        public abstract List<BroadphasePair> getOverlappingPairArray();

        public abstract int getNumOverlappingPairs();

        public abstract void setOverlapFilterCallback(OverlapFilterCallback overlapFilterCallback);

        public abstract void processAllOverlappingPairs(OverlapCallback callback);

        public abstract BroadphasePair findPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1);
    }
}
