using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class Triangle
    {
        public VInt3[] verts = new VInt3[3];

        public Triangle() { }

        public Triangle(VInt3 p0, VInt3 p1, VInt3 p2)
        {
            verts[0] = p0;
            verts[1] = p1;
            verts[2] = p2;
        }

        public Triangle(Triangle other)
        {
            verts[0] = other.verts[0];
            verts[1] = other.verts[1];
            verts[2] = other.verts[2];
        }

        public VInt3 normal
        {
            get
            {
                return VInt3.Cross(verts[1] - verts[0], verts[2] - verts[0]).Normalize();
            }
        }

        public VInt3 denormalizedNormal
        {
            get
            {
                return VInt3.Cross(verts[1] - verts[0], verts[2] - verts[0]);
            }
        }

        public VInt3 pointFromUV(VFixedPoint u, VFixedPoint v)
        {
            return verts[0] * (VFixedPoint.One - u - v) + verts[1] * u + verts[2] * v;
        }
    }
}
