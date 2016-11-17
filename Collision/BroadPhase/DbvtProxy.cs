namespace MobaGame.Collision
{
    public class DbvtProxy: BroadphaseProxy
    {
        public readonly DbvtAabbMm aabb = new DbvtAabbMm();
        public Dbvt.Node leaf;
        public readonly DbvtProxy[] links = new DbvtProxy[2];
        public int stage;

        public DbvtProxy(short collisionFilterGroup, short collisionFilterMask)
            :base(collisionFilterGroup, collisionFilterMask)
        {

        }

    }
}