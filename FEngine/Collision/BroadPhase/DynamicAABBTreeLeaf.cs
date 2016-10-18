using System.Text;

namespace MobaGame.Collision
{
    class DynamicAABBTreeLeaf<E, T>:DynamicAABBTreeNode where E :Collidable<T> where T :Fixture
    {
        public readonly E collidable;
        public readonly T fixture;
        public bool tested = false;

        public DynamicAABBTreeLeaf(E collidable, T fixture)
        {
            this.collidable = collidable;
            this.fixture = fixture;
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
            if ((obj is DynamicAABBTreeLeaf<E, T>))
            {
                DynamicAABBTreeLeaf <E, T> leaf = (DynamicAABBTreeLeaf<E, T>)obj;
                return (leaf.collidable.Equals(this.collidable)) &&
                  (leaf.fixture.Equals(this.fixture));
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + this.collidable.GetHashCode();
            hash = hash * 31 + this.fixture.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("DynamicAABBTreeLeaf[Collidable=").Append(this.collidable.getId())
              .Append("|Fixture=").Append(this.fixture.getId())
              .Append("|AABB=").Append(this.aabb.ToString())
              .Append("|Height=").Append(this.height)
              .Append("|Tested=").Append(this.tested)
              .Append("]");
            return sb.ToString();
        }
    }
}
