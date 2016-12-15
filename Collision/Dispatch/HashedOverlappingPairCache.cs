using MobaGame.Framework;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class HashedOverlappingPairCache: OverlappingPairCache
    {
        private ObjectPool<BroadphasePair> pairsPool = new ObjectPool<BroadphasePair>();
	
	    private static readonly int NULL_PAIR = -1;

        private List<BroadphasePair> overlappingPairArray = new List<BroadphasePair>(16);
        private OverlapFilterCallback overlapFilterCallback;

        private Dictionary<int, int> hashTable = new Dictionary<int, int>();

        public override BroadphasePair addOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            if (!needsBroadphaseCollision(proxy0, proxy1))
            {
                return null;
            }

            base.addOverlappingPair(proxy0, proxy1);
            return internalAddPair(proxy0, proxy1);
        }

        public override void removeOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1, Dispatcher dispatcher)
        {
            base.removeOverlappingPair(proxy0, proxy1, dispatcher);

            if (proxy0.getUid() > proxy1.getUid())
            {
                BroadphaseProxy tmp = proxy0;
                proxy0 = proxy1;
                proxy1 = tmp;
            }
            UUID proxyId1 = proxy0.getUid();
            UUID proxyId2 = proxy1.getUid();


            int hash = getHash(proxyId1, proxyId2);

            BroadphasePair pair = internalFindPair(proxy0, proxy1, hash);
            if (pair == null)
            {
                return;
            }

            cleanOverlappingPair(pair, dispatcher);

            int pairIndex = hashTable[hash];

            // Remove the pair from the hash table.
            hashTable.Remove(hash);

            // We now move the last pair into spot of the
            // pair being removed. We need to fix the hash
            // table indices to support the move.

            int lastPairIndex = overlappingPairArray.Count - 1;
         
            // If the removed pair is the last pair, we are done.
            if (lastPairIndex == pairIndex)
            {
                overlappingPairArray.RemoveAt(overlappingPairArray.Count - 1);
                return;
            }

            BroadphasePair last = overlappingPairArray[lastPairIndex];

            overlappingPairArray[pairIndex] = overlappingPairArray[lastPairIndex];
            overlappingPairArray.RemoveAt(lastPairIndex);

            int lastHash = getHash(last.pProxy0.getUid(), last.pProxy1.getUid());
            hashTable[lastHash] = pairIndex;

            

            return;
        }

        public bool needsBroadphaseCollision(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            if (overlapFilterCallback != null)
            {
                return overlapFilterCallback.needBroadphaseCollision(proxy0, proxy1);
            }

            bool collides = (proxy0.collisionFilterGroup & proxy1.collisionFilterMask) != 0;
            collides = collides && (proxy1.collisionFilterGroup & proxy0.collisionFilterMask) != 0;

            return collides;
        }

        public override void processAllOverlappingPairs(OverlapCallback callback, Dispatcher dispatcher)
        {
            for (int i = 0; i < overlappingPairArray.Count;)
            {
                BroadphasePair pair = overlappingPairArray[i];
                if (callback.processOverlap(pair))
                {
                    removeOverlappingPair(pair.pProxy0, pair.pProxy1, dispatcher);
                }
                else {
                    i++;
                }
            }
        }

        public override void removeOverlappingPairsContainingProxy(BroadphaseProxy proxy, Dispatcher dispatcher)
        {
            processAllOverlappingPairs(new RemovePairCallback(proxy), dispatcher);
        }

        public override void cleanProxyFromPairs(BroadphaseProxy proxy, Dispatcher dispatcher)
        {
            processAllOverlappingPairs(new CleanPairCallback(proxy, this, dispatcher), dispatcher);
        }

        public override List<BroadphasePair> getOverlappingPairArray()
        {
            return overlappingPairArray;
        }

        public override void cleanOverlappingPair(BroadphasePair pair, Dispatcher dispatcher)
        {
            if (pair.algorithm != null)
            {
                //pair.algorithm.destroy();
                dispatcher.freeCollisionAlgorithm(pair.algorithm);
                pair.algorithm = null;
            }
        }

        public override BroadphasePair findPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            if (proxy0.getUid() > proxy1.getUid())
            {
                BroadphaseProxy tmp = proxy0;
                proxy0 = proxy1;
                proxy1 = proxy0;
            }
            UUID proxyId1 = proxy0.getUid();
            UUID proxyId2 = proxy1.getUid();

            int hash = getHash(proxyId1, proxyId2);

            if (!hashTable.ContainsKey(hash))
            {
                return null;
            }

            int index = hashTable[hash];
            if (index >= overlappingPairArray.Count)
                return null;

            return overlappingPairArray[index];
        }

        public int getCount()
        {
            return overlappingPairArray.Count;
        }

        public OverlapFilterCallback getOverlapFilterCallback()
        {
            return overlapFilterCallback;
        }

        public override void setOverlapFilterCallback(OverlapFilterCallback overlapFilterCallback)
        {
            this.overlapFilterCallback = overlapFilterCallback;
        }

        public override int getNumOverlappingPairs()
        {
            return overlappingPairArray.Count;
        }

        public override bool hasDeferredRemoval()
        {
            return false;
        }

        private BroadphasePair internalAddPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            if (proxy0.getUid() > proxy1.getUid())
            {
                BroadphaseProxy tmp = proxy0;
                proxy0 = proxy1;
                proxy1 = tmp;
            }
            UUID proxyId1 = proxy0.getUid();
            UUID proxyId2 = proxy1.getUid();

            int hash = getHash(proxyId1, proxyId2); // New hash value with new mask

            BroadphasePair pair = internalFindPair(proxy0, proxy1, hash);
            if (pair != null)
            {
                return pair;
            }

            // hash with new capacity
            hash = getHash(proxyId1, proxyId2);
            hashTable[hash] = overlappingPairArray.Count - 1;

            pair = new BroadphasePair(proxy0, proxy1);
            pair.algorithm = null;

            overlappingPairArray.Add(pair);
            return pair;
        }

        private bool equalsPair(BroadphasePair pair, UUID proxyId1, UUID proxyId2)
        {
            return pair.pProxy0.getUid() == proxyId1 && pair.pProxy1.getUid() == proxyId2;
        }

        private int getHash(UUID proxyId1, UUID proxyId2)
        {
            int key = (proxyId1.id) | (proxyId2.id << 16);
            // Thomas Wang's hash

            key += ~(key << 15);
            key ^= (key >> 10);
            key += (key << 3);
            key ^= (key >> 6);
            key += ~(key << 11);
            key ^= (key >> 16);
            return key;
        }

        private BroadphasePair internalFindPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1, int hash)
        {
            UUID proxyId1 = proxy0.getUid();
            UUID proxyId2 = proxy1.getUid();

            if (!hashTable.ContainsKey(hash))
                return null;

            int index = hashTable[hash];
            if (index >= overlappingPairArray.Count)
                return null;

            return overlappingPairArray[index];
        }

        private class RemovePairCallback: OverlapCallback
        {

            private BroadphaseProxy obsoleteProxy;

            public RemovePairCallback(BroadphaseProxy obsoleteProxy)
            {
                this.obsoleteProxy = obsoleteProxy;
            }

            public override bool processOverlap(BroadphasePair pair)
            {
                return ((pair.pProxy0 == obsoleteProxy) ||
                        (pair.pProxy1 == obsoleteProxy));
            }
        }

        private class CleanPairCallback : OverlapCallback
        {
            private BroadphaseProxy cleanProxy;
            private OverlappingPairCache pairCache;
            private Dispatcher dispatcher;

            public CleanPairCallback(BroadphaseProxy cleanProxy, OverlappingPairCache pairCache, Dispatcher dispatcher)
            {
                this.cleanProxy = cleanProxy;
                this.pairCache = pairCache;
                this.dispatcher = dispatcher;
            }

            public override bool processOverlap(BroadphasePair pair)
            {
                if ((pair.pProxy0 == cleanProxy) ||
                        (pair.pProxy1 == cleanProxy))
                {
                    pairCache.cleanOverlappingPair(pair, dispatcher);
                }
                return false;
            }
        }
    }
}
