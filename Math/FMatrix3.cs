namespace MobaGame.FixedMath
{
    public class FMatrix3
    {
        VFixedPoint[,] matrix = new VFixedPoint[3, 3];

        public FMatrix3()
        {

        }

        public FMatrix3(VFixedPoint m00, VFixedPoint m01, VFixedPoint m02,
            VFixedPoint m10, VFixedPoint m11, VFixedPoint m12,
            VFixedPoint m20, VFixedPoint m21, VFixedPoint m22
        )
        {
            matrix[0, 0] = m00;
            matrix[0, 1] = m01;
            matrix[0, 2] = m02;
            matrix[1, 0] = m10;
            matrix[1, 1] = m11;
            matrix[1, 2] = m12;
            matrix[2, 0] = m20;
            matrix[2, 1] = m21;
            matrix[2, 2] = m22;
        }

        public FMatrix3(FMatrix3 other)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    matrix[i, j] = other.matrix[i, j];
                }
            }

        }

        public FMatrix3(VIntTransform trans):this(
            trans.right.x, trans.up.x, trans.forward.x,
            trans.right.y, trans.up.y, trans.forward.y,
            trans.right.z, trans.up.z, trans.forward.z
        )
        {
        }

        public FMatrix3 Transpose()
        {
            FMatrix3 tmatrix = new FMatrix3();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    tmatrix.matrix[i, j] = matrix[j, i];
                }
            }
            return tmatrix;
        }

        public VInt3 Transform(VInt3 input)
        {
            VFixedPoint x = VInt3.Dot(new VInt3(matrix[0, 0], matrix[0, 1], matrix[0, 2]), input);
            VFixedPoint y = VInt3.Dot(new VInt3(matrix[1, 0], matrix[1, 1], matrix[1, 2]), input);
            VFixedPoint z = VInt3.Dot(new VInt3(matrix[2, 0], matrix[2, 1], matrix[2, 2]), input);
            return new VInt3(x, y, z);
        }

        public VFixedPoint this[int i, int j]
        {
            get { return matrix[i,j]; }
            set { matrix[i, j] = value; }
        }

        public static FMatrix3 operator *(FMatrix3 lhs, FMatrix3 rhs)
        {
            VInt3 a = lhs.Transform(new VInt3(rhs[0, 0], rhs[1, 0], rhs[2, 0]));
            VInt3 b = lhs.Transform(new VInt3(rhs[0, 1], rhs[1, 1], rhs[2, 1]));
            VInt3 c = lhs.Transform(new VInt3(rhs[0, 2], rhs[1, 2], rhs[2, 2]));
            return new FMatrix3(a.x, b.x, c.x,
                a.y, b.y, c.y,
                a.z, b.z, c.z);
        }

        public static VInt3 operator *(FMatrix3 lhs, VInt3 rhs)
        {
            return lhs.Transform(rhs);
        }

    }
}