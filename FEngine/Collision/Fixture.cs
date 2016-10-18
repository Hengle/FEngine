using MobaGame.FixedMath;
using System.Collections.Generic;
using System.Text;


namespace MobaGame.Collision
{
    public class Fixture 
    {
        protected readonly UUID id;
        protected Convex shape;
        protected Filter filter;
        protected bool sensor;

        public Fixture(Convex shape)
        {
            this.id = UUID.GetNextUUID();
            this.shape = shape;
            this.filter = new DEFAULT_FILTER();
            this.sensor = false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Fixture[Id=").Append(this.id)
              .Append("|Shape=").Append(this.shape)
              .Append("|Filter=").Append(this.filter)
              .Append("|IsSensor=").Append(this.sensor)
              .Append("]");
            return sb.ToString();
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
            if ((obj is Fixture)) {
                return this.id.Equals(((Fixture)obj).id);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public UUID getId()
        {
            return this.id;
        }

        public Convex getShape()
        {
            return this.shape;
        }

        public Filter getFilter()
        {
            return this.filter;
        }

        public void setFilter(Filter filter)
        {
            this.filter = filter;
        }

        public bool isSensor()
        {
            return this.sensor;
        }

        public void setSensor(bool flag)
        {
            this.sensor = flag;
        }
    }
}