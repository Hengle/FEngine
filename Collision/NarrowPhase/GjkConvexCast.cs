using System;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class GjkConvexCast : ConvexCast
    {
        private SimplexSolverInterface simplexSolver;
        private ConvexShape convexA;
        private ConvexShape convexB;

        public GjkConvexCast(ConvexShape convexA, ConvexShape convexB, SimplexSolverInterface simplexSolver)
        {
            this.simplexSolver = simplexSolver;
            this.convexA = convexA;
            this.convexB = convexB;
        }

        public override bool calcTimeOfImpact(VIntTransform fromA, VIntTransform toA, VIntTransform fromB, VIntTransform toB, CastResult result)
        {
            VInt3 linvelA = toA.position - fromA.position;
            VInt3 linvelB = toB.position - fromB.position;

            VInt3 r = linvelB - linvelA;

            return true;
        }
    }
}
