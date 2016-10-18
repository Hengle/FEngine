using MobaGame.FixedMath;
using System.Text;

namespace MobaGame.Collision
{
    public class AABB: Translatable
    {
        protected VInt3 min;
        protected VInt3 max;
  
        public AABB(VInt3 min, VInt3 max)
        {
            if ((min.x > max.x) || (min.y > max.y))
            {
                //Error
            }
            this.min = min;
            this.max = max;
        }

        public AABB(VInt3 center, VFixedPoint radius)
        {
            if (radius < VFixedPoint.Zero)
            {
                radius *= -1;
            }

            this.min = new VInt3(center.x - radius, center.y - radius, center.z - radius);
            this.max = new VInt3(center.x + radius, center.y + radius, center.z + radius);
            
        }

        public AABB(AABB aabb)
        {
            this.min = aabb.min;
            this.max = aabb.max;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("AABB[Min=").Append(this.min)
              .Append("|Max=").Append(this.max)
              .Append("]");
            return sb.ToString();
        }

        public void translate(VFixedPoint x, VFixedPoint y, VFixedPoint z)
        {
            this.max += new VInt3(x, y, z);
            this.min += new VInt3(x, y, z);
        }

        public void translate(VInt3 translation)
        {
            this.max += translation;
            this.min += translation;
        }

        public AABB getTranslated(VInt3 translation)
        {
            return new AABB(
              this.min + translation,
              this.max + translation);
        }

        public VFixedPoint getWidth()
        {
            return this.max.x - this.min.x;
        }

        public VFixedPoint getHeight()
        {
            return this.max.y - this.min.y;
        }

        public void union(AABB aabb)
        {
            this.min.x = FMath.Min(this.min.x, aabb.min.x);
            this.min.y = FMath.Min(this.min.y, aabb.min.y);
            this.min.z = FMath.Min(this.min.z, aabb.min.z);
            this.max.x = FMath.Max(this.max.x, aabb.max.x);
            this.max.y = FMath.Max(this.max.y, aabb.max.y);
            this.max.z = FMath.Max(this.max.z, aabb.max.z);
        }

        public AABB getUnion(AABB aabb)
        {
            VInt3 min = new VInt3();
            VInt3 max = new VInt3();

            min.x = FMath.Min(this.min.x, aabb.min.x);
            min.y = FMath.Min(this.min.y, aabb.min.y);
            min.z = FMath.Min(this.min.z, aabb.min.z);
            max.x = FMath.Max(this.max.x, aabb.max.x);
            max.y = FMath.Max(this.max.y, aabb.max.y);
            max.z = FMath.Max(this.max.z, aabb.max.z);

            return new AABB(min, max);
        }

        public void intersection(AABB aabb)
        {
            this.min.x = FMath.Max(this.min.x, aabb.min.x);
            this.min.y = FMath.Max(this.min.y, aabb.min.y);
            this.min.z = FMath.Max(this.min.z, aabb.min.z);
            this.max.x = FMath.Min(this.max.x, aabb.max.x);
            this.max.y = FMath.Min(this.max.y, aabb.max.y);
            this.max.z = FMath.Min(this.max.z, aabb.max.z);
            if ((this.min.x > this.max.x) || (this.min.y > this.max.y) || (this.min.z > this.max.z))
            {
                this.min.x = VFixedPoint.Zero;
                this.min.y = VFixedPoint.Zero;
                this.min.z = VFixedPoint.Zero;
                this.max.x = VFixedPoint.Zero;
                this.max.y = VFixedPoint.Zero;
                this.min.z = VFixedPoint.Zero;
            }
        }

        public AABB getIntersection(AABB aabb)
        {
            VInt3 min = new VInt3();
            VInt3 max = new VInt3();

            min.x = FMath.Max(this.min.x, aabb.min.x);
            min.y = FMath.Max(this.min.y, aabb.min.y);
            min.z = FMath.Max(this.min.z, aabb.min.z);
            max.x = FMath.Min(this.max.x, aabb.max.x);
            max.y = FMath.Min(this.max.y, aabb.max.y);
            max.z = FMath.Min(this.max.z, aabb.max.z);
            if ((min.x > max.x) || (min.y > max.y) || (min.z > max.z))
            {
                return new AABB(VInt3.zero, VInt3.zero);
            }
            return new AABB(min, max);
        }

        public void expand(VFixedPoint expansion)
        {
            if (expansion < VFixedPoint.Zero)
            {
                return;
            }
            VFixedPoint e = expansion * VFixedPoint.Half;
            this.min.x -= e;
            this.min.y -= e;
            this.min.z -= e;
            this.max.x += e;
            this.max.y += e;
            this.max.z += e;
        }

        public AABB getExpanded(VFixedPoint expansion)
        {
            if (expansion < VFixedPoint.Zero)
            {
                return new AABB(this);
            }
            VFixedPoint e = expansion * VFixedPoint.Half;
            VFixedPoint minx = this.min.x - e;
            VFixedPoint miny = this.min.y - e;
            VFixedPoint minz = this.min.z - e;
            VFixedPoint maxx = this.max.x + e;
            VFixedPoint maxy = this.max.y + e;
            VFixedPoint maxz = this.max.z + e;
            
            return new AABB(
              new VInt3(minx, miny, minz),
              new VInt3(maxx, maxy, maxz));
        }

        public bool overlaps(AABB aabb)
        {
            if ((this.min.x > aabb.max.x) || (this.max.x < aabb.min.x))
            {
                return false;
            }
            if ((this.min.y > aabb.max.y) || (this.max.y < aabb.min.y))
            {
                return false;
            }
            if ((this.min.z > aabb.max.z) || (this.max.z < aabb.min.z))
            {
                return false;
            }
            return true;
        }

        public bool contains(AABB aabb)
        {
            if ((this.min.x <= aabb.min.x) && (this.max.x >= aabb.max.x) &&
              (this.min.y <= aabb.min.y) && (this.max.y >= aabb.max.y) &&
              (this.min.z <= aabb.min.z) && (this.max.z >= aabb.max.z))
            {
                return true;
            }
            return false;
        }

        public bool contains(VInt3 point)
        {
            return contains(point.x, point.y, point.z);
        }

        public bool contains(VFixedPoint x, VFixedPoint y, VFixedPoint z)
        {
            if ((this.min.x <= x) && (this.max.x >= x) &&
              (this.min.y <= y) && (this.max.y >= y) &&
              (this.min.z <= z) && (this.max.z >= z))
            {
                return true;
            }
            return false;
        }

        public bool isDegenerate()
        {
            return (this.min.x == this.max.x) || (this.min.y == this.max.y) || (this.min.z == this.max.z);
        }

        public bool isDegenerate(VFixedPoint error)
        {
            return ((this.max.x - this.min.x).Abs() <= error) || ((this.max.y - this.min.y).Abs() <= error) || ((this.max.z - this.min.z).Abs() <= error);
        }

        public VFixedPoint getMinX()
        {
            return this.min.x;
        }

        public VFixedPoint getMaxX()
        {
            return this.max.x;
        }

        public VFixedPoint getMaxY()
        {
            return this.max.y;
        }

        public VFixedPoint getMinY()
        {
            return this.min.y;
        }

        public VFixedPoint getMaxZ()
        {
            return this.max.z;
        }

        public VFixedPoint getMinZ()
        {
            return this.min.z;
        }

        public VFixedPoint getPerimeter()
        {
            VFixedPoint length = max.x - min.x;
            VFixedPoint width = max.y - min.y;
            VFixedPoint height = max.z - min.z;
            return (length * width + width * height + width * height) * 2;
        }

    }
}


