namespace MobaGame.Collision
{
    public class DbvtLeafCollider: Dbvt.ICollide
    {
        public DbvtBroadphase pbp;
        public DbvtProxy ppx;

        public DbvtLeafCollider(DbvtBroadphase p, DbvtProxy px) {
            this.pbp = p;
            this.ppx = px;
        }

        public override void Process(Dbvt.Node na)
        {
            Dbvt.Node nb = ppx.leaf;
            if (nb != na) {
                DbvtProxy pa = na.data;
                DbvtProxy pb = nb.data;

                if (DbvtAabbMm.Intersect(pa.aabb, pb.aabb))
                {
                    if (pa.getUid() > pb.getUid()) {
                        DbvtProxy tmp = pa;
                        pa = pb;
                        pb = tmp;
                    }
                    pbp.paircache.addOverlappingPair(pa, pb);
                }
            }
        }
    }
}