using MobaGame.FixedMath;
using System.Text;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class Ray
    {
        protected VInt3 start;
        protected VInt3 direction;
        public Ray(VInt3 start, VInt3 direction)
        {
            if (direction == VInt3.zero)
            {
                direction = VInt3.forward;
            }
            this.start = start;
            this.direction = direction;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Ray[Start=").Append(this.start)
              .Append("|Direction=").Append(getDirectionVector())
              .Append("]");
            return sb.ToString();
        }

        public VInt3 getStart()
        {
            return this.start;
        }

        public void setStart(VInt3 start)
        {
            this.start = start;
        }

        public void setDirection(VInt3 direction)
        {
            if (direction == VInt3.zero)
            {
                direction = VInt3.forward;
            }
            this.direction = direction;
        }

        public VInt3 getDirectionVector()
        {
            return this.direction;
        }
    }

}