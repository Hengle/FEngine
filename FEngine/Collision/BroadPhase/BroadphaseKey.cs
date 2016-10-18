using MobaGame.FixedMath;
using System.Collections.Generic;
using System.Text;

namespace MobaGame.Collision
{
    class BroadphaseKey
    {
        readonly UUID collidable;
        readonly UUID fixture;
        private readonly int hashCode;

        public BroadphaseKey(UUID collidable, UUID fixture)
        {
            this.collidable = collidable;
            this.fixture = fixture;

            hashCode = computeHashCode();
        }

        public static BroadphaseKey get<T>(Collidable<T> collidable, Fixture fixture) where T :Fixture
        {
            return new BroadphaseKey(collidable.getId(), fixture.getId());
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj == this)
            {
                return false;
            }
            if ((obj is BroadphaseKey))
            {
                BroadphaseKey key = (BroadphaseKey)obj;
                return (key.collidable.Equals(this.collidable)) &&
                  (key.fixture.Equals(this.fixture));
            }
            return false;
        }

        protected int computeHashCode()
        {
            int hash = 17;
            hash = hash * 31 + this.collidable.GetHashCode();
            hash = hash * 31 + this.fixture.GetHashCode();
            return hash;
        }

        public override int GetHashCode()
        {
            return this.hashCode;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("BroadphaseKey[CollidableId=").Append(this.collidable)
              .Append("|FixtureId=").Append(this.fixture)
              .Append("]");
            return sb.ToString();
        }
    }
}
