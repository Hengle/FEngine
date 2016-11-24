using MobaGame.Framework;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class HashedOverlappingPairCache<T>: OverlappingPairCache where T: BroadphasePair, new()
    {
        private ObjectPool<T> pairsPool = new ObjectPool<T>();
	
	    private static readonly int NULL_PAIR = -1;

        private List<BroadphasePair> overlappingPairArray = new List<BroadphasePair>();
        private OverlapFilterCallback overlapFilterCallback;
        private bool blockedForChanges = false;

        private IntArrayList hashTable = new IntArrayList();
        private IntArrayList next = new IntArrayList();
        protected OverlappingPairCallback ghostPairCallback;

        public override BroadphasePair addOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {

            if (!needsBroadphaseCollision(proxy0, proxy1))
            {
                return null;
            }

            return internalAddPair(proxy0, proxy1);
        }

        public override void removeOverlappingPair(BroadphaseProxy proxy0, BroadphaseProxy proxy1, Dispatcher dispatcher)
        {
            if (proxy0.getUid() > proxy1.getUid())
            {
                BroadphaseProxy tmp = proxy0;
                proxy0 = proxy1;
                proxy1 = tmp;
            }
            UUID proxyId1 = proxy0.getUid();
            UUID proxyId2 = proxy1.getUid();


            int hash = getHash(proxyId1, proxyId2) & (overlappingPairArray.Capacity - 1);

            BroadphasePair pair = internalFindPair(proxy0, proxy1, hash);
            if (pair == null)
            {
                return;
            }

            cleanOverlappingPair(pair, dispatcher);

            object userData = pair;

            // JAVA TODO: optimize
            //int pairIndex = int(pair - &m_overlappingPairArray[0]);
            int pairIndex = overlappingPairArray.IndexOf(pair);

            // Remove the pair from the hash table.
            int index = hashTable.get(hash);

            int previous = NULL_PAIR;
            while (index != pairIndex)
            {
                previous = index;
                index = next.get(index);
            }

            if (previous != NULL_PAIR)
            {
                next.set(previous, next.get(pairIndex));
            }
            else {
                hashTable.set(hash, next.get(pairIndex));
            }

            // We now move the last pair into spot of the
            // pair being removed. We need to fix the hash
            // table indices to support the move.

            int lastPairIndex = overlappingPairArray.Count - 1;

            if (ghostPairCallback != null)
            {
                ghostPairCallback.removeOverlappingPair(proxy0, proxy1, dispatcher);
            }

            // If the removed pair is the last pair, we are done.
            if (lastPairIndex == pairIndex)
            {
                overlappingPairArray.RemoveAt(overlappingPairArray.Count - 1);
                return;
            }

            // Remove the last pair from the hash table.
            BroadphasePair last = overlappingPairArray[lastPairIndex];
            /* missing swap here too, Nat. */
            int lastHash = getHash(last.pProxy0.getUid(), last.pProxy1.getUid()) & (overlappingPairArray.Capacity - 1);

            index = hashTable.get(lastHash);

            previous = NULL_PAIR;
            while (index != lastPairIndex)
            {
                previous = index;
                index = next.get(index);
            }

            if (previous != NULL_PAIR)
            {
                next.set(previous, next.get(lastPairIndex));
            }
            else {
                hashTable.set(lastHash, next.get(lastPairIndex));
            }

            // Copy the last pair into the remove pair's spot.
            overlappingPairArray[pairIndex] = overlappingPairArray[lastPairIndex];

            // Insert the last pair into the hash table
            next.set(pairIndex, hashTable.get(lastHash));
            hashTable.set(lastHash, pairIndex);

            overlappingPairArray.RemoveAt(overlappingPairArray.Count - 1);

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
            //	printf("m_overlappingPairArray.size()=%d\n",m_overlappingPairArray.size());
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

            /*if (proxyId1 > proxyId2) 
                btSwap(proxyId1, proxyId2);*/

            int hash = getHash(proxyId1, proxyId2) & (overlappingPairArray.Capacity - 1);

            if (hash >= hashTable.Size())
            {
                return null;
            }

            int index = hashTable.get(hash);
            while (index != NULL_PAIR && equalsPair(overlappingPairArray[index], proxyId1, proxyId2) == false)
            {
                index = next.get(index);
            }

            if (index == NULL_PAIR)
            {
                return null;
            }


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

            /*if (proxyId1 > proxyId2) 
            btSwap(proxyId1, proxyId2);*/

            int hash = getHash(proxyId1, proxyId2) & (overlappingPairArray.Capacity - 1); // New hash value with new mask

            BroadphasePair pair = internalFindPair(proxy0, proxy1, hash);
            if (pair != null)
            {
                return pair;
            }
            /*for(int i=0;i<m_overlappingPairArray.size();++i)
            {
            if(	(m_overlappingPairArray[i].m_pProxy0==proxy0)&&
            (m_overlappingPairArray[i].m_pProxy1==proxy1))
            {
            printf("Adding duplicated %u<>%u\r\n",proxyId1,proxyId2);
            internalFindPair(proxy0, proxy1, hash);
            }
            }*/
            int count = overlappingPairArray.Count;
            int oldCapacity = overlappingPairArray.Capacity;
            overlappingPairArray.Add(null);

            // this is where we add an actual pair, so also call the 'ghost'
            if (ghostPairCallback != null)
            {
                ghostPairCallback.addOverlappingPair(proxy0, proxy1);
            }

            int newCapacity = overlappingPairArray.Capacity;

            if (oldCapacity < newCapacity)
            {
                growTables();
                // hash with new capacity
                hash = getHash(proxyId1, proxyId2) & (overlappingPairArray.Capacity - 1);
            }

            pair = new BroadphasePair(proxy0, proxy1);
            //	pair->m_pProxy0 = proxy0;
            //	pair->m_pProxy1 = proxy1;
            pair.algorithm = null;

            overlappingPairArray[overlappingPairArray.Count - 1] = pair;

            next.set(count, hashTable.get(hash));
            hashTable.set(hash, count);

            return pair;
        }

        private void growTables()
        {
            int newCapacity = overlappingPairArray.Capacity;

            if (hashTable.Size() < newCapacity)
            {
                // grow hashtable and next table
                int curHashtableSize = hashTable.Size();

                hashTable.resize(newCapacity, 0);
                next.resize(newCapacity, 0);

                for (int i = 0; i < newCapacity; ++i)
                {
                    hashTable.set(i, NULL_PAIR);
                }
                for (int i = 0; i < newCapacity; ++i)
                {
                    next.set(i, NULL_PAIR);
                }

                for (int i = 0; i < curHashtableSize; i++)
                {

                    BroadphasePair pair = overlappingPairArray[i];
                    UUID proxyId1 = pair.pProxy0.getUid();
                    UUID proxyId2 = pair.pProxy1.getUid();
                    /*if (proxyId1 > proxyId2) 
                    btSwap(proxyId1, proxyId2);*/
                    int hashValue = getHash(proxyId1, proxyId2) & (overlappingPairArray.Capacity - 1); // New hash value with new mask
                    next.set(i, hashTable.get(hashValue));
                    hashTable.set(hashValue, i);
                }
            }
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
            //#if 0 // wrong, 'equalsPair' use unsorted uids, copy-past devil striked again. Nat.
            //if (proxyId1 > proxyId2) 
            //	btSwap(proxyId1, proxyId2);
            //#endif

            int index = hashTable.get(hash);

            while (index != NULL_PAIR && equalsPair(overlappingPairArray[index], proxyId1, proxyId2) == false)
            {
                index = next.get(index);
            }

            if (index == NULL_PAIR)
            {
                return null;
            }

            return overlappingPairArray[index];
        }

        public override void setInternalGhostPairCallback(OverlappingPairCallback ghostPairCallback)
        {
            this.ghostPairCallback = ghostPairCallback;
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
