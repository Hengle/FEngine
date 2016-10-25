using MobaGame.FixedMath;
using System.Collections.Generic;
using System.Text;

namespace MobaGame.Collision
{
    public class BroadphasePair<E, T> where E :Collidable<T> where T :Fixture
    {
        readonly E collidable1;
        readonly T fixture1;
        readonly E collidable2;
        readonly T fixture2;
  
        public BroadphasePair(E collidable1, T fixture1, E collidable2, T fixture2)
        {
            this.collidable1 = collidable1;
            this.fixture1 = fixture1;
            this.collidable2 = collidable2;
            this.fixture2 = fixture2;
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
            if ((obj is BroadphasePair<E, T>))
            {
                BroadphasePair<E, T> pair = (BroadphasePair<E, T>)obj;
                return ((pair.collidable1.Equals(this.collidable1)) &&
                  (pair.fixture1.Equals(this.fixture1)) &&
                  (pair.collidable2.Equals(this.collidable2)) &&
                  (pair.fixture2.Equals(this.fixture2)));
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + this.collidable1.GetHashCode();
            hash = hash * 31 + this.fixture1.GetHashCode();
            hash = hash * 31 + this.collidable2.GetHashCode();
            hash = hash * 31 + this.fixture2.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("BroadphasePair[Collidable1=").Append(this.collidable1.getId())
              .Append("|Fixture1=").Append(this.fixture1.getId())
              .Append("|Collidable2=").Append(this.collidable2.getId())
              .Append("|Fixture2=").Append(this.fixture2.getId())
              .Append("]");
            return sb.ToString();
        }

        public E getCollidable1()
        {
            return this.collidable1;
        }

        public T getFixture1()
        {
            return this.fixture1;
        }

        public E getCollidable2()
        {
            return this.collidable2;
        }

        public T getFixture2()
        {
            return this.fixture2;
        }
    }
}
