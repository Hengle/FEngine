namespace MobaGame.Collision
{
    public class DbvtProxy: BroadphaseProxy
    {
        public DbvtAabbMm aabb = new DbvtAabbMm();
        public Dbvt.Node leaf;
        public DbvtProxy last, next;
        public int stage;

        public DbvtProxy(CollisionObject collisionObject, short collisionFilterGroup, short collisionFilterMask)
            :base(collisionObject, collisionFilterGroup, collisionFilterMask)
        {

        }

    }
}