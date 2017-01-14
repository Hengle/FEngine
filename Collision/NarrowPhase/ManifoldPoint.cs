using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class ManifoldPoint
    {
        public VInt3 normalWorldOnB;

        public VFixedPoint distance1;

        // BP mod, store contact triangles.
        public int partId0;
        public int partId1;
        public int index0;
		public int index1;

		public ManifoldPoint()
		{
		}

        public ManifoldPoint(VInt3 normal, VFixedPoint distance)
        {
            init(normal, distance);
        }

        public void init(VInt3 normal, VFixedPoint distance)
        {

            this.normalWorldOnB = normal;
            this.distance1 = distance;
        }

        public VFixedPoint getDistance()
        {
            return distance1;
        }

        public void set(ManifoldPoint p)
        {
            normalWorldOnB = p.normalWorldOnB;
            distance1 = p.distance1;
            partId0 = p.partId0;
            partId1 = p.partId1;
            index0 = p.index0;
            index1 = p.index1;
        }

        public void setDistance(VFixedPoint dist)
        {
            distance1 = dist;
        }
    }
}
