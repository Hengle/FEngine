using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    public class DbvtBroadphase: BroadphaseInterface
    {
        public static readonly VFixedPoint DBVT_BP_MARGIN = VFixedPoint.One / VFixedPoint.Create(20);

        public static readonly int DYNAMIC_SET = 0; // Dynamic set index

        public readonly Dbvt sets;                        // Dbvt sets
        public DbvtProxy stageRoots; // Stages list
        public OverlappingPairCache paircache;                         // Pair cache
        public VFixedPoint predictedframes;                                  // Frames predicted
        public int stageCurrent;                                       // Current stage
        public int updates;                                           // % of dynamic updates per frame
        public bool releasepaircache;                               // Release pair cache on delete

        public DbvtBroadphase():this(null)
        {

        }

        public DbvtBroadphase(OverlappingPairCache paircache)
        {
            sets = new Dbvt();
            releasepaircache = (paircache != null? false : true);
            predictedframes = VFixedPoint.Two;
            stageCurrent = 0;
            updates = 1;
            this.paircache = paircache != null ? paircache : new HashedOverlappingPairCache();
        }

        public void collide(Dispatcher dispatcher)
        {
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
                            paircache.removeOverlappingPair(pa, pb, dispatcher);
                            ni--;
                            i--;
                        }
                    }
                }
            }

            // optimize:
            sets.optimizeIncremental(1 + (sets.leaves * updates) / 100);

            // collide dynamics:
            {
                DbvtTreeCollider collider = new DbvtTreeCollider(this);
                Dbvt.collideTT(sets.root, sets.root, collider);
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
            proxy.leaf = sets.insert(proxy.aabb, proxy);
            proxy.uniqueId = UUID.GetNextUUID();
            stageRoots = listappend(proxy, stageRoots);
            return (proxy);
        }

        public override void destroyProxy(BroadphaseProxy absproxy, Dispatcher dispatcher) {
            DbvtProxy proxy = (DbvtProxy)absproxy;
            sets.remove(proxy.leaf);
            
            stageRoots = listremove(proxy, stageRoots);
            paircache.removeOverlappingPairsContainingProxy(proxy, dispatcher);
        }

        public override void setAabb(BroadphaseProxy absproxy, VInt3 aabbMin, VInt3 aabbMax, Dispatcher dispatcher) {
            DbvtProxy proxy = (DbvtProxy)absproxy;
            DbvtAabbMm aabb = DbvtAabbMm.FromMM(aabbMin, aabbMax, new DbvtAabbMm());
            
            if (DbvtAabbMm.Intersect(proxy.leaf.volume, aabb))
            {   
                // Moving	
                VInt3 delta = (aabbMin + aabbMax) * VFixedPoint.Half;
                delta -= proxy.aabb.Center();
                delta *= predictedframes;
                sets.update(proxy.leaf, aabb, delta, DBVT_BP_MARGIN);
            }
            else
            {
                // teleporting:
                sets.update(proxy.leaf, aabb);
            }
            
            proxy.aabb.set(aabb);
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
            if (!sets.empty())
            {
                bounds.set(sets.root.volume);
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

        public override void rayTest(VInt3 rayFrom, VInt3 rayTo, BroadphaseRayCallback rayCallback, VInt3 aabbMin, VInt3 aabbMax)
        {
            BroadphaseRayTester callback = new BroadphaseRayTester(rayCallback);

            sets.rayTestInternal(sets.root,
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
            sets.collideTV(sets.root, bounds, callback);
        }
    }
}