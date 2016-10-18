using MobaGame.FixedMath;
using System.Text;

namespace MobaGame.Collision
{
    public class MinkowskiSum
    {
        Convex convex1;
        Convex convex2;
        VIntTransform transform1;
        VIntTransform transform2;
  
        public MinkowskiSum(Convex convex1, VIntTransform transform1, Convex convex2, VIntTransform transform2)
        {
            this.convex1 = convex1;
            this.convex2 = convex2;
            this.transform1 = transform1;
            this.transform2 = transform2;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("MinkowskiSum[Convex1=").Append(this.convex1.getId())
              .Append("|Transform1=").Append(this.transform1)
              .Append("|Convex2=").Append(this.convex2.getId())
              .Append("|Transform2=").Append(this.transform2)
              .Append("]");
            return sb.ToString();
        }

        public VInt3 getSupportPoint(VInt3 direction)
        {
            VInt3 point1 = this.convex1.getFarthestPoint(direction, this.transform1);
            direction *= -1;

            VInt3 point2 = this.convex2.getFarthestPoint(direction, this.transform2);

            return point1 - point2;
        }

        public MinkowskiSumPoint getSupportPoints(VInt3 direction)
        {
            VInt3 point1 = this.convex1.getFarthestPoint(direction, this.transform1);
            direction *= -1;

            VInt3 point2 = this.convex2.getFarthestPoint(direction, this.transform2);

            return new MinkowskiSumPoint(point1, point2);
        }

        public Convex getConvex1()
        {
            return this.convex1;
        }

        public Convex getConvex2()
        {
            return this.convex2;
        }

        public VIntTransform getTransform1()
        {
            return this.transform1;
        }

        public VIntTransform getTransform2()
        {
            return this.transform2;
        }
    }

    public class MinkowskiSumPoint
    {
        public VInt3 supportPoint1;
        public VInt3 supportPoint2;
        public VInt3 point;
  
        public MinkowskiSumPoint(VInt3 supportPoint1, VInt3 supportPoint2)
        {
            this.supportPoint1 = supportPoint1;
            this.supportPoint2 = supportPoint2;
            this.point = supportPoint1 - supportPoint2;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("MinkowskiSum.Point[Point=").Append(this.point)
              .Append("|SupportPoint1=").Append(this.supportPoint1)
              .Append("|SupportPoint2=").Append(this.supportPoint2)
              .Append("]");
            return sb.ToString();
        }

        public VInt3 getSupportPoint1()
        {
            return this.supportPoint1;
        }

        public VInt3 getSupportPoint2()
        {
            return this.supportPoint2;
        }

        public VInt3 getPoint()
        {
            return this.point;
        }
    }
}

