using MobaGame.FixedMath;
using System.Collections.Generic;
using System.Text;

namespace MobaGame.Collision
{
    public class BroadphaseItem<E, T> where E :Collidable<T> where T :Fixture
    {
        readonly E collidable;
        readonly T fixture;
  
        public BroadphaseItem(E collidable, T fixture)
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
            if ((obj is BroadphaseItem<E, T>))
            {
                BroadphaseItem <E, T> pair = (BroadphaseItem<E, T>)obj;
                return (pair.collidable.Equals(this.collidable)) &&
                  (pair.fixture.Equals(this.fixture));
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
            sb.Append("BroadphaseItem[Collidable=").Append(this.collidable.getId())
              .Append("|Fixture=").Append(this.fixture.getId())
              .Append("]");
            return sb.ToString();
        }

        public E getCollidable()
        {
            return this.collidable;
        }

        public T getFixture()
        {
            return this.fixture;
        }
    }
}
