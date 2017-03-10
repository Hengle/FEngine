using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    public class DbvtBroadphase: BroadphaseInterface
    {
        public static readonly VFixedPoint DBVT_BP_MARGIN = VFixedPoint.One / VFixedPoint.Create(20);

        public static readonly int DYNAMIC_SET = 0; // Dynamic set index
        public static readonly int FIXED_SET = 1;
        public static readonly int STAGECOUNT = 2;

        public readonly Dbvt[] sets = new Dbvt[STAGECOUNT];                        // Dbvt sets
        public DbvtProxy[] stageRoots = new DbvtProxy[STAGECOUNT]; // Stages list
        public OverlappingPairCache paircache;                         // Pair cache
        public VFixedPoint predictedframes;                                  // Frames predicted
        public int fupdates;                                           // % of fixed updates per frame
        public int dupdates;                                           // % of dynamic updates per frame
        public bool releasepaircache;                               // Release pair cache on delete

        public DbvtBroadphase(OverlappingPairCache paircache)
        {
            releasepaircache = (paircache != null? false : true);
            predictedframes = VFixedPoint.Two;
            fupdates = 1;
            dupdates = 1;
            this.paircache = paircache != null ? paircache : new HashedOverlappingPairCache();
            for (int i = 0; i < STAGECOUNT; i++)
            {
                sets[i] = new Dbvt();
                stageRoots[i] = null;
            }
        }

        public void collide(Dispatcher dispatcher)
        {
            DbvtTreeCollider collider = new DbvtTreeCollider(this);

            //collide dynamics:
            {
                Dbvt.collideTT(sets[DYNAMIC_SET].root, sets[FIXED_SET].root, dispatcher, collider);
                Dbvt.collideTT(sets[DYNAMIC_SET].root, sets[DYNAMIC_SET].root, dispatcher, collider);
            }

            //dynamic -> fixed set:
            DbvtProxy current = stageRoots[DYNAMIC_SET];
            if (current != null)
            {
                do
                {
                    DbvtProxy next = current.links[1];
                    stageRoots[current.stage] = listremove(current, stageRoots[current.stage]);
                    stageRoots[FIXED_SET] = listappend(current, stageRoots[FIXED_SET]);
                    sets[DYNAMIC_SET].remove(current.leaf);
                    current.leaf = sets[FIXED_SET].insert(current.aabb, current);
                    current.stage = FIXED_SET;
                    current = next;
                }
                while (current != null);
            }

            // clean up:
            {
                List<BroadphasePair> pairs = paircache.getOverlappingPairArray();
                if (pairs.Count > 0)
                {
                    for (int i=0, ni=pairs.Count; i<ni; i++)
                    {
                        BroadphasePair p = pairs[i];
                        DbvtProxy pa = (DbvtProxy) p.pProxy0;
                        DbvtProxy pb = (DbvtProxy) p.pProxy1;
                        if (!DbvtAabbMm.Intersect(pa.aabb, pb.aabb))
                        {
                            if (pa.getUid() > pb.getUid())
                            {
                                DbvtProxy tmp = pa;
                                pa = pb;
                                pb = tmp;
                            }
                            paircache.removeOverlappingPair(pa, pb);
                            ni--;
                            i--;
                        }
                    }
                }
            }
        }

        private static DbvtProxy listappend(DbvtProxy item, DbvtProxy list) {
            item.links[0] = null;
            item.links[1] = list;
            if (list != null) list.links[0] = item;
            list = item;
            return list;
        }

        private static DbvtProxy listremove(DbvtProxy item, DbvtProxy list) {
            if (item.links[0] != null) {
                item.links[0].links[1] = item.links[1];
            }
            else {
                list = item.links[1];
            }

            if (item.links[1] != null) {
                item.links[1].links[0] = item.links[0];
            }
            return list;
        }

        public override BroadphaseProxy createProxy(VInt3 aabbMin, VInt3 aabbMax, BroadphaseNativeType shapeType, CollisionObject collisionObject, short collisionFilterGroup, short collisionFilterMask, Dispatcher dispatcher) {
            DbvtProxy proxy = new DbvtProxy(collisionObject, collisionFilterGroup, collisionFilterMask);
            DbvtAabbMm.FromMM(aabbMin, aabbMax, proxy.aabb);
            proxy.leaf = sets[DYNAMIC_SET].insert(proxy.aabb, proxy);
            proxy.stage = DYNAMIC_SET;
            proxy.uniqueId = UUID.GetNextUUID();
            stageRoots[DYNAMIC_SET] = listappend(proxy, stageRoots[DYNAMIC_SET]);
            return (proxy);
        }

        public override void destroyProxy(BroadphaseProxy absproxy, Dispatcher dispatcher) {
            DbvtProxy proxy = (DbvtProxy)absproxy;
            sets[proxy.stage].remove(proxy.leaf);
            
            stageRoots[proxy.stage] = listremove(proxy, stageRoots[proxy.stage]);
            paircache.removeOverlappingPairsContainingProxy(proxy);
        }

        public override void setAabb(BroadphaseProxy absproxy, VInt3 aabbMin, VInt3 aabbMax, Dispatcher dispatcher) {
            DbvtProxy proxy = (DbvtProxy)absproxy;
            DbvtAabbMm aabb = DbvtAabbMm.FromMM(aabbMin, aabbMax, new DbvtAabbMm());
            
            if(aabb != proxy.leaf.volume)
            {
                if (proxy.stage == FIXED_SET)
                {
                    sets[FIXED_SET].remove(proxy.leaf);
                    proxy.leaf = sets[DYNAMIC_SET].insert(aabb, proxy);
                }
                else if (DbvtAabbMm.Intersect(proxy.leaf.volume, aabb))
                {
                    // Moving	
                    VInt3 delta = (aabbMin + aabbMax) * VFixedPoint.Half;
                    delta -= proxy.aabb.Center();
                    delta *= predictedframes;
                    sets[DYNAMIC_SET].update(proxy.leaf, aabb, delta, DBVT_BP_MARGIN);
                }
                else
                {
                    // teleporting:
                    sets[DYNAMIC_SET].update(proxy.leaf, aabb);
                }
                listremove(proxy, stageRoots[proxy.stage]);
                proxy.aabb.set(aabb);
                proxy.stage = DYNAMIC_SET;
                listappend(proxy, stageRoots[DYNAMIC_SET]);
            }
        }

        public override void calculateOverlappingPairs(Dispatcher dispatcher)
        {
            collide(dispatcher);
        }

        public override OverlappingPairCache getOverlappingPairCache()
        {
            return paircache;
        }

        public override void getBroadphaseAabb(out VInt3 aabbMin, out VInt3 aabbMax)
        {
            DbvtAabbMm bounds = new DbvtAabbMm();
            if (!sets[DYNAMIC_SET].empty())
            {
                if(!sets[FIXED_SET].empty())
                {
                    DbvtAabbMm.Merge(sets[DYNAMIC_SET].root.volume, sets[FIXED_SET].root.volume, bounds);
                }
                else
                {
                    bounds.set(sets[DYNAMIC_SET].root.volume);
                }
            }
            else if(!sets[FIXED_SET].empty())
            {
                bounds.set(sets[FIXED_SET].root.volume);
            }
            else
            {
                DbvtAabbMm.FromCR(VInt3.zero, VFixedPoint.Zero, bounds);
            }
            aabbMin = bounds.Mins();
            aabbMax = bounds.Maxs();
        }

        class BroadphaseRayTester: Dbvt.ICollide
        {
            BroadphaseRayCallback rayCallback;
            public BroadphaseRayTester(BroadphaseRayCallback callback)
            {
                rayCallback = callback;
            }

            public override void Process(Dbvt.Node n)
            {
                DbvtProxy proxy = n.data;
                rayCallback.process(proxy);
            }
        }

        public override void rayTest(BroadphaseRayCallback rayCallback, Dispatcher dispatcher, VInt3 aabbMin, VInt3 aabbMax, short collisionFilterGroup, short collisionFilterMask)
        {
            BroadphaseRayTester callback = new BroadphaseRayTester(rayCallback);

            VInt3 rayDirectionInverse;

            sets[DYNAMIC_SET].rayTestInternal(sets[DYNAMIC_SET].root,
                dispatcher,
                rayCallback.rayFrom,
                rayCallback.rayTo,
                aabbMin,
                aabbMax,
                collisionFilterGroup,
                collisionFilterMask,
                callback);

            sets[FIXED_SET].rayTestInternal(sets[FIXED_SET].root,
                dispatcher,
                rayCallback.rayFrom,
                rayCallback.rayTo,
                aabbMin,
                aabbMax,
                collisionFilterGroup,
                collisionFilterMask,
                callback);

        }

        class BroadphaseAabbTester : Dbvt.ICollide
        {
            BroadphaseAabbCallback m_aabbCallback;
            public BroadphaseAabbTester(BroadphaseAabbCallback orgCallback)
            {
                m_aabbCallback = orgCallback;
            }

            public override void Process(Dbvt.Node leaf)
            {
                m_aabbCallback.process(leaf.data);
            }
        }
        

        public override void aabbTest(VInt3 aabbMin, VInt3 aabbMax, BroadphaseAabbCallback aabbCallback, Dispatcher dispatcher, short collisionFilterGroup, short collisionFilterMask)
        {
            BroadphaseAabbTester callback = new BroadphaseAabbTester(aabbCallback);

            DbvtAabbMm bounds = new DbvtAabbMm();
            DbvtAabbMm.FromMM(aabbMin, aabbMax, bounds);
            //process all children, that overlap with  the given AABB bounds
            sets[DYNAMIC_SET].collideTV(sets[DYNAMIC_SET].root, dispatcher, bounds, collisionFilterGroup, collisionFilterMask, callback);
            sets[FIXED_SET].collideTV(sets[FIXED_SET].root, dispatcher, bounds, collisionFilterGroup, collisionFilterMask, callback);
        }
    }
}