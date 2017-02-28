using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class DbvtAabbMm
    {
        private VInt3 mi = new VInt3();
        private VInt3 mx = new VInt3();

        public DbvtAabbMm()
        {
        }

        public DbvtAabbMm(DbvtAabbMm o)
        {
            set(o);
        }

        public void set(DbvtAabbMm o)
        {
            mi = o.mi;
            mx = o.mx;
        }

        public static void swap(DbvtAabbMm p1, DbvtAabbMm p2)
        {
            VInt3 tmp = p1.mi; 
            p1.mi = p2.mi;
		    p2.mi = tmp;

		    tmp = p1.mx;
		    p1.mx = p2.mx;
		    p2.mx = tmp;
	    }

        public VInt3 Center()
        {
            VInt3 output = (mi + mx) * VFixedPoint.Half;
            return output;
        }

        public VInt3 Lengths()
        {
            VInt3 output = mx - mi;
            return output;
        }

        public VInt3 Extents()
        {
            VInt3 output = (mx - mi) * VFixedPoint.Half;
            return output;
        }

        public VInt3 Mins()
        {
            return mi;
        }

        public VInt3 Maxs()
        {
            return mx;
        }

        public static DbvtAabbMm FromCE(VInt3 c, VInt3 e, DbvtAabbMm output)
        {
            DbvtAabbMm box = output;
            box.mi = c - e;
            box.mx = c + e;
            return box;
        }

        public static DbvtAabbMm FromCR(VInt3 c, VFixedPoint r, DbvtAabbMm output)
        {
            VInt3 tmp = new VInt3(r, r, r);
		    return FromCE(c, tmp, output);
        }

        public static DbvtAabbMm FromMM(VInt3 mi, VInt3 mx, DbvtAabbMm output)
        {
            DbvtAabbMm box = output;
            box.mi = mi;
            box.mx = mx;
            return box;
        }

        public static DbvtAabbMm FromVec(VInt3 from, VInt3 to, DbvtAabbMm output)
        {
            DbvtAabbMm box = output;
            box.mi.x = FMath.Min(from.x, to.x);box.mi.y = FMath.Min(from.y, to.y);box.mi.x = FMath.Min(from.z, to.z);
            box.mx.x = FMath.Max(from.x, to.x);box.mx.y = FMath.Max(from.y, to.y);box.mx.x = FMath.Max(from.z, to.z);
            return box;

        }

        public void Expand(VInt3 e)
        {
            mi -= e;
            mx += e;
        }

        public void SignedExpand(VInt3 e)
        {
            if (e.x > VFixedPoint.Zero)
            {
                mx.x += e.x;
            }
            else {
                mi.x += e.x;
            }

            if (e.y > VFixedPoint.Zero)
            {
                mx.y += e.y;
            }
            else {
                mi.y += e.y;
            }

            if (e.z > VFixedPoint.Zero)
            {
                mx.z += e.z;
            }
            else {
                mi.z += e.z;
            }
        }

        public bool Contain(DbvtAabbMm a)
        {
            return ((mi.x <= a.mi.x) &&
                    (mi.y <= a.mi.y) &&
                    (mi.z <= a.mi.z) &&
                    (mx.x >= a.mx.x) &&
                    (mx.y >= a.mx.y) &&
                    (mx.z >= a.mx.z));
        }

        public static bool Intersect(DbvtAabbMm a, DbvtAabbMm b)
        {
            return ((a.mi.x <= b.mx.x) &&
                    (a.mx.x >= b.mi.x) &&
                    (a.mi.y <= b.mx.y) &&
                    (a.mx.y >= b.mi.y) &&
                    (a.mi.z <= b.mx.z) &&
                    (a.mx.z >= b.mi.z));
        }

        public static VFixedPoint Proximity(DbvtAabbMm a, DbvtAabbMm b)
        {
		    VInt3 d = a.mi + a.mx;
            VInt3 tmp = b.mi - b.mx;
		    d -= tmp;
		    return d.x.Abs() + d.y.Abs() + d.z.Abs();
	    }

	    public static void Merge(DbvtAabbMm a, DbvtAabbMm b, DbvtAabbMm r)
        {
            r.mi.x = FMath.Min(a.mi.x, b.mi.x);
            r.mx.x = FMath.Max(a.mx.x, b.mx.x);

            r.mi.y = FMath.Min(a.mi.y, b.mi.y);
            r.mx.y = FMath.Max(a.mx.y, b.mx.y);

            r.mi.z = FMath.Min(a.mi.z, b.mi.z);
            r.mx.z = FMath.Max(a.mx.z, b.mx.z);
        }

        public static bool operator != (DbvtAabbMm a, DbvtAabbMm b)
        {
            return ((a.mi.x != b.mi.x) ||
                    (a.mi.y != b.mi.y) ||
                    (a.mi.z != b.mi.z) ||
                    (a.mx.x != b.mx.x) ||
                    (a.mx.y != b.mx.y) ||
                    (a.mx.z != b.mx.z));
        }

        public static bool operator ==(DbvtAabbMm a, DbvtAabbMm b)
        {
            return ((a.mi.x == b.mi.x) &&
                    (a.mi.y == b.mi.y) &&
                    (a.mi.z == b.mi.z) &&
                    (a.mx.x == b.mx.x) &&
                    (a.mx.y == b.mx.y) &&
                    (a.mx.z == b.mx.z));
        }
    }
}
