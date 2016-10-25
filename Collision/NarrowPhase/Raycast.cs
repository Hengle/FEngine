using MobaGame.FixedMath;
using System.Text;

namespace MobaGame.Collision
{
    public class Raycast
    {
        public VInt3 point;
        public VInt3 normal;
        public VFixedPoint distance;

        public Raycast(VInt3 point, VInt3 normal, VFixedPoint distance)
        {
            this.point = point;
            this.normal = normal;
            this.distance = distance;
        }

        public string toString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Raycast[Point=").Append(this.point)
              .Append("|Normal=").Append(this.normal)
              .Append("|Distance=").Append(this.distance)
              .Append("]");
            return sb.ToString();
        }

        public VInt3 getPoint()
        {
            return this.point;
        }

        public void setPoint(VInt3 point)
        {
            this.point = point;
        }

        public VInt3 getNormal()
        {
            return this.normal;
        }

        public void setNormal(VInt3 normal)
        {
            this.normal = normal;
        }

        public VFixedPoint getDistance()
        {
            return this.distance;
        }

        public void setDistance(VFixedPoint distance)
        {
            this.distance = distance;
        }
    }

}