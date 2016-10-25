using MobaGame.FixedMath;
using System.Text;

namespace MobaGame.Collision
{
    public class Interval
    {
        protected VFixedPoint min;
        protected VFixedPoint max;

        public Interval(VFixedPoint min, VFixedPoint max)
        {
            if (min > max)
            {
                VFixedPoint tmp = max;
                max = min;
                min = tmp;
            }
            this.min = min;
            this.max = max;
        }

        public Interval(Interval interval)
        {
            this.min = interval.min;
            this.max = interval.max;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[").Append(this.min).Append(", ").Append(this.max).Append("]");
            return sb.ToString();
        }

        public VFixedPoint getMin()
        {
            return this.min;
        }

        public VFixedPoint getMax()
        {
            return this.max;
        }

        public void setMin(VFixedPoint min)
        {
            if (min > this.max)
            {
                return;
            }
            this.min = min;
        }

        public void setMax(VFixedPoint max)
        {
            if (max < this.min)
            {
                return;
            }
            this.max = max;
        }

        public bool includesInclusive(VFixedPoint value)
        {
            return (value <= this.max) && (value >= this.min);
        }

        public bool includesExclusive(VFixedPoint value)
        {
            return (value < this.max) && (value > this.min);
        }

        public bool includesInclusiveMin(VFixedPoint value)
        {
            return (value < this.max) && (value >= this.min);
        }

        public bool includesInclusiveMax(VFixedPoint value)
        {
            return (value <= this.max) && (value > this.min);
        }

        public bool overlaps(Interval interval)
        {
            return (this.min <= interval.max) && (interval.min <= this.max);
        }

        public VFixedPoint getOverlap(Interval interval)
        {
            if (overlaps(interval))
            {
                return FMath.Min(this.max, interval.max) - FMath.Max(this.min, interval.min);
            }
            return VFixedPoint.Zero;
        }

        public VFixedPoint clamp(VFixedPoint value)
        {
            return FMath.Clamp(value, this.min, this.max);
        }

        public static VFixedPoint clamp(VFixedPoint value, VFixedPoint min, VFixedPoint max)
        {
            if ((value <= max) && (value >= min))
            {
                return value;
            }
            if (max < value)
            {
                return max;
            }
            return min;
        }

        public bool isDegenerate()
        {
            return this.min == this.max;
        }

        public bool isDegenerate(VFixedPoint error)
        {
            return (this.max - this.min).Abs() <= error;
        }

        public bool contains(Interval interval)
        {
            return (interval.min > this.min) && (interval.max < this.max);
        }

        public void union(Interval interval)
        {
            this.min = FMath.Min(interval.min, this.min);
            this.max = FMath.Max(interval.max, this.max);
        }

        public Interval getUnion(Interval interval)
        {
            return new Interval(FMath.Min(interval.min, this.min), FMath.Max(interval.max, this.max));
        }

        public void intersection(Interval interval)
        {
            if (overlaps(interval))
            {
                this.min = FMath.Max(interval.min, this.min);
                this.max = FMath.Min(interval.max, this.max);
            }
            else
            {
                this.min = VFixedPoint.Zero;
                this.max = VFixedPoint.Zero;
            }
        }

        public Interval getIntersection(Interval interval)
        {
            if (overlaps(interval))
            {
                return new Interval(FMath.Max(interval.min, this.min), FMath.Min(interval.max, this.max));
            }
            return new Interval(VFixedPoint.Zero, VFixedPoint.Zero);
        }

        public VFixedPoint distance(Interval interval)
        {
            if (!overlaps(interval))
            {
                if (this.max < interval.min)
                {
                    return interval.min - this.max;
                }
                return this.min - interval.max;
            }
            return VFixedPoint.Zero;
        }

        public void expand(VFixedPoint value)
        {
            VFixedPoint e = value * VFixedPoint.Half;
            this.min -= e;
            this.max += e;
            if ((value < VFixedPoint.Zero) && (this.min > this.max))
            {
                VFixedPoint p = (this.min + this.max) * VFixedPoint.Half;
                this.min = p;
                this.max = p;
            }
        }

        public Interval getExpanded(VFixedPoint value)
        {
            VFixedPoint e = value * VFixedPoint.Half;
            VFixedPoint min = this.min - e;
            VFixedPoint max = this.max + e;
            if ((value < VFixedPoint.Zero) && (min > max))
            {
                VFixedPoint p = (min + max) * VFixedPoint.Half;
                min = p;
                max = p;
            }
            return new Interval(min, max);
        }

        public VFixedPoint getLength()
        {
            return this.max - this.min;
        }
    }
}
