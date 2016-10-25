using MobaGame.FixedMath;
using System.Text;

namespace MobaGame.Collision
{
    public class Penetration
    {
        public VInt3 normal;
        public VFixedPoint depth;

        public Penetration(VInt3 normal, VFixedPoint depth)
        {
            this.normal = normal;
            this.depth = depth;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Penetration[Normal=").Append(this.normal)
              .Append("|Depth=").Append(this.depth)
              .Append("]");
            return sb.ToString();
        }

        public VInt3 getNormal()
        {
            return this.normal;
        }

        public VFixedPoint getDepth()
        {
            return this.depth;
        }

        public void setNormal(VInt3 normal)
        {
            this.normal = normal;
        }

        public void setDepth(VFixedPoint depth)
        {
            this.depth = depth;
        }
    }
}
