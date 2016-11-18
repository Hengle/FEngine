using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    public class DbvtBroadphase: BroadphaseInterface
    {
        public static readonly VFixedPoint DBVT_BP_MARGIN = VFixedPoint.One / VFixedPoint.Create(20);

        public static readonly int DYNAMIC_SET = 0; // Dynamic set index
        public static readonly int FIXED_SET   = 1; // Fixed set index
        public static readonly int STAGECOUNT  = 2; // Number of stages

        public readonly Dbvt[] sets = new Dbvt[2];                        // Dbvt sets
        public DbvtProxy[] stageRoots = new DbvtProxy[STAGECOUNT + 1]; // Stages list
        public OverlappingPairCache paircache;                         // Pair cache
        public VFixedPoint predictedframes;                                  // Frames predicted
        public int stageCurrent;                                       // Current stage
        public int fupdates;                                           // % of fixed updates per frame
        public int dupdates;                                           // % of dynamic updates per frame
        public int pid;                                                // Parse id
        public int gid;                                                // Gen id
        public bool releasepaircache;                               // Release pair cache on delete

        public DbvtBroadphase():this(null)
        {

        }

        public DbvtBroadphase(OverlappingPairCache paircache)
        {
            sets[0] = new Dbvt();
            sets[1] = new Dbvt();

            //Dbvt.benchmark();
            releasepaircache = (paircache != null? false : true);
            predictedframes = VFixedPoint.Two;
            stageCurrent = 0;
            fupdates = 1;
            dupdates = 1;
            this.paircache = paircache != null ? paircache : new HashedOverlappingPairCache();
            gid = 0;
            pid = 0;

            for (int i=0; i<=STAGECOUNT; i++)
            {
                stageRoots[i] = null;
            }

        }

        public void collide(Dispatcher dispatcher)
        {
            // optimize:
            sets[0].optimizeIncremental(1 + (sets[0].leaves * dupdates) / 100);
            sets[1].optimizeIncremental(1 + (sets[1].leaves * fupdates) / 100);

            // dynamic -> fixed set:
            stageCurrent = (stageCurrent + 1) % STAGECOUNT;
            DbvtProxy current = stageRoots[stageCurrent];
            if (current != null)
            {
                DbvtTreeCollider collider = new DbvtTreeCollider(this);
                do {
                    DbvtProxy next = current.links[1];
                    stageRoots[current.stage] = listremove(current, stageRoots[current.stage]);
                    stageRoots[STAGECOUNT] = listappend(current, stageRoots[STAGECOUNT]);
                    Dbvt.collideTT(sets[1].root, current.leaf, collider);
                    sets[0].remove(current.leaf);
                    current.leaf = sets[1].insert(current.aabb, current);
                    current.stage = STAGECOUNT;
                    current = next;
                } while (current != null);
            }

            // collide dynamics:
            {
                DbvtTreeCollider collider = new DbvtTreeCollider(this);
                Dbvt.collideTT(sets[0].root, sets[1].root, collider);
                Dbvt.collideTT(sets[0].root, sets[0].root, collider);
            }

            // clean up:
            {
                List<BroadphasePair> pairs = paircache.getOverlappingPairArray();
                if (pairs.Count > 0) {
                    for (int i=0, ni=pairs.Count; i<ni; i++) {
                        BroadphasePair p = pairs[i];
                        DbvtProxy pa = (DbvtProxy) p.pProxy0;
                        DbvtProxy pb = (DbvtProxy) p.pProxy1;
                        if (!DbvtAabbMm.Intersect(pa.aabb, pb.aabb)) {
                            //if(pa>pb) btSwap(pa,pb);
                            if (pa.getUid() > pb.getUid())
                            {
                                DbvtProxy tmp = pa;
                                pa = pb;
                                pb = tmp;
                            }
                            paircache.removeOverlappingPair(pa, pb, dispatcher);
                            ni--;
                            i--;
                        }
                    }
                }
            }
            pid++;
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
            proxy.leaf = sets[0].insert(proxy.aabb, proxy);
            proxy.stage = stageCurrent;
            proxy.uniqueId = UUID.GetNextUUID();
            stageRoots[stageCurrent] = listappend(proxy, stageRoots[stageCurrent]);
            return (proxy);
        }

        public override void destroyProxy(BroadphaseProxy absproxy, Dispatcher dispatcher) {
            DbvtProxy proxy = (DbvtProxy)absproxy;
            if (proxy.stage == STAGECOUNT) {
                sets[1].remove(proxy.leaf);
            }
            else {
                sets[0].remove(proxy.leaf);
            }
            stageRoots[proxy.stage] = listremove(proxy, stageRoots[proxy.stage]);
            paircache.removeOverlappingPairsContainingProxy(proxy, dispatcher);
        }

        public override void setAabb(BroadphaseProxy absproxy, VInt3 aabbMin, VInt3 aabbMax, Dispatcher dispatcher) {
            DbvtProxy proxy = (DbvtProxy)absproxy;
            DbvtAabbMm aabb = DbvtAabbMm.FromMM(aabbMin, aabbMax, new DbvtAabbMm());
            if (proxy.stage == STAGECOUNT) {
                // fixed -> dynamic set
                sets[1].remove(proxy.leaf);
                proxy.leaf = sets[0].insert(aabb, proxy);
            }
            else {
                // dynamic set:
                if (DbvtAabbMm.Intersect(proxy.leaf.volume, aabb)) {/* Moving				*/
                    VInt3 delta = (aabbMin + aabbMax) / VFixedPoint.Two;
                    delta -= proxy.aabb.Center();
                    delta *= predictedframes;
                    sets[0].update(proxy.leaf, aabb, delta, DBVT_BP_MARGIN);
                }
                else {
                    // teleporting:
                    sets[0].update(proxy.leaf, aabb);
                }
            }

            stageRoots[proxy.stage] = listremove(proxy, stageRoots[proxy.stage]);
            proxy.aabb.set(aabb);
            proxy.stage = stageCurrent;
            stageRoots[stageCurrent] = listappend(proxy, stageRoots[stageCurrent]);
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
            if (!sets[0].empty()) {
                if (!sets[1].empty()) {
                    DbvtAabbMm.Merge(sets[0].root.volume, sets[1].root.volume, bounds);
                }
                else {
                    bounds.set(sets[0].root.volume);
                }
            }
            else if (!sets[1].empty()) {
                bounds.set(sets[1].root.volume);
            }
            else {
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

        public override void rayTest(VInt3 rayFrom, VInt3 rayTo, BroadphaseRayCallback rayCallback, VInt3 aabbMin, VInt3 aabbMax)
        {
            BroadphaseRayTester callback = new BroadphaseRayTester(rayCallback);

            sets[0].rayTestInternal(sets[0].root,
                rayFrom,
                rayTo,
                rayCallback.rayDirectionInverse,
                rayCallback.signs,
                rayCallback.lambdaMax,
                aabbMin,
                aabbMax,
                callback);

            sets[1].rayTestInternal(sets[1].root,
                rayFrom,
                rayTo,
                rayCallback.rayDirectionInverse,
                rayCallback.signs,
                rayCallback.lambdaMax,
                aabbMin,
                aabbMax,
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
        

        public override void aabbTest(VInt3 aabbMin, VInt3 aabbMax, BroadphaseAabbCallback aabbCallback)
        {
            BroadphaseAabbTester callback = new BroadphaseAabbTester(aabbCallback);

            DbvtAabbMm bounds = new DbvtAabbMm();
            DbvtAabbMm.FromMM(aabbMin, aabbMax, bounds);
            //process all children, that overlap with  the given AABB bounds
            sets[0].collideTV(sets[0].root, bounds, callback);
            sets[1].collideTV(sets[1].root, bounds, callback);
        }
    }
}