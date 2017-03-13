using MobaGame.FixedMath;
using System.Text;

namespace MobaGame.Collision
{
    public abstract class BroadphaseInterface
    {
        public abstract BroadphaseProxy createProxy(VInt3 aabbMin, VInt3 aabbMax, BroadphaseNativeType shapeType, CollisionObject collisionObject, short collisionFilterGroup, short collisionFilterMask, Dispatcher dispatcher);

        public abstract void destroyProxy(BroadphaseProxy proxy, Dispatcher dispatcher);

        public abstract void setAabb(BroadphaseProxy proxy, VInt3 aabbMin, VInt3 aabbMax, Dispatcher dispatcher);

        ///calculateOverlappingPairs is optional: incremental algorithms (sweep and prune) might do it during the set aabb
        public abstract void calculateOverlappingPairs(Dispatcher dispatcher);

        public abstract OverlappingPairCache getOverlappingPairCache();

        ///getAabb returns the axis aligned bounding box in the 'global' coordinate frame
        ///will add some transform later
        public abstract void getBroadphaseAabb(out VInt3 aabbMin, out VInt3 aabbMax);

        public abstract void rayTest(BroadphaseRayCallback rayCallback, Dispatcher dispatcher, VInt3 aabbMin, VInt3 aabbMax, short collisionFilterGroup, short collisionFilterMask);

	    public abstract void aabbTest(VInt3 aabbMin, VInt3 aabbMax, BroadphaseAabbCallback callback, Dispatcher dispatcher, short collisionFilterGroup, short collisionFilterMask);
    }

    public abstract class BroadphaseAabbCallback
    {
        protected VInt3 aabbMin;

        protected VInt3 aabbMax;

        public BroadphaseAabbCallback(CollisionObject colObj)
        {
            colObj.getCollisionShape().getAabb(colObj.getWorldTransform(), out aabbMin, out aabbMax);
        }

        public abstract bool process(BroadphaseProxy proxy);
    }

    public abstract class BroadphaseRayCallback
    {
        public VInt3 rayFrom;
        public VInt3 rayTo;


        public BroadphaseRayCallback(VInt3 rayFrom, VInt3 rayTo)
        {
            this.rayFrom = rayFrom;
            this.rayTo = rayTo;
        }

        public abstract bool process(BroadphaseProxy proxy);
    }
}