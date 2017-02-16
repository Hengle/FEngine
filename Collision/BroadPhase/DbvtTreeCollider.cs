namespace MobaGame.Collision
{
    public class DbvtTreeCollider : Dbvt.ICollide
    {
        public DbvtBroadphase pbp;

        public DbvtTreeCollider(DbvtBroadphase p)
        {
            this.pbp = p;
        }

        public override void Process(Dbvt.Node na, Dbvt.Node nb)
        {
            DbvtProxy pa = na.data;
            DbvtProxy pb = nb.data;
            if (DbvtAabbMm.Intersect(pa.aabb, pb.aabb))
            {
                if (pa.getUid() > pb.getUid())
                {
                    DbvtProxy tmp = pa;
                    pa = pb;
                    pb = tmp;
                }
                pbp.paircache.addOverlappingPair(pa, pb);
            }
        }
    }
}