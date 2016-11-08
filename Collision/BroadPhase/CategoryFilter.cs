using System.Text;

namespace MobaGame.Collision
{
    class CategoryFilter : Filter
    {
        public readonly long category;
        public readonly long mask;

        public CategoryFilter()
        {
            category = 1L;
            mask = long.MaxValue;
        }

        public CategoryFilter(long category, long mask)
        {
            this.category = category;
            this.mask = mask;
        }

        public bool isAllowed(Filter filter)
        {
            if (filter == null)
            {
                return true;
            }
            if ((filter is CategoryFilter))
            {
                CategoryFilter cf = (CategoryFilter)filter;

                return ((this.category & cf.mask) > 0L) && ((cf.category & this.mask) > 0L);
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (int)(this.category >> 32 ^ this.category);
            hash = hash * 31 + (int)(this.mask >> 32 ^ this.mask);
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj == this)
            {
                return true;
            }
            if ((obj is CategoryFilter))
            {
                CategoryFilter filter = (CategoryFilter)obj;
                return (filter.category == this.category) && (filter.mask == this.mask);
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CategoryFilter[Category=").Append(this.category)
              .Append("|Mask=").Append(this.mask)
              .Append("]");
            return sb.ToString();
        }
    }
}
