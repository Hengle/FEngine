namespace MobaGame.Collision
{
    public class DbvtProxy: BroadphaseProxy
    {
        public readonly DbvtAabbMm aabb = new DbvtAabbMm();
        public Dbvt.Node leaf;
        public readonly DbvtProxy[] links = new DbvtProxy[2];

        public DbvtProxy(CollisionObject collisionObject, short collisionFilterGroup, short collisionFilterMask)
            :base(collisionObject, collisionFilterGroup, collisionFilterMask)
        {

        }

    }
}