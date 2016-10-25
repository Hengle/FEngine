using MobaGame.FixedMath;
using System.Text;

namespace MobaGame.Collision
{
    public class Separation
    {
        public VInt3 normal;
        public VFixedPoint distance;
        public VInt3 point1;
        public VInt3 point2;

        public Separation() { }

        public Separation(VInt3 normal, VFixedPoint distance, VInt3 point1, VInt3 point2)
        {
            this.normal = normal;
            this.distance = distance;
            this.point1 = point1;
            this.point2 = point2;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Separation[Point1=").Append(this.point1)
                .Append("|Point2=").Append(this.point2)
                .Append("|Normal=").Append(this.normal)
                .Append("|Distance=").Append(this.distance)
                .Append("]");
            return sb.ToString();
        }

        public VInt3 getNormal()
        {
            return this.normal;
        }

        public VFixedPoint getDistance()
        {
            return this.distance;
        }

        public VInt3 getPoint1()
        {
            return this.point1;
        }

        public VInt3 getPoint2()
        {
            return this.point2;
        }

        public void setNormal(VInt3 normal)
        {
            this.normal = normal;
        }

        public void setDistance(VFixedPoint distance)
        {
            this.distance = distance;
        }

        public void setPoint1(VInt3 point1)
        {
            this.point1 = point1;
        }

        public void setPoint2(VInt3 point2)
        {
            this.point2 = point2;
        }
    }
}