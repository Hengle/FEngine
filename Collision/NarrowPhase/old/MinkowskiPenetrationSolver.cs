using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public interface MinkowskiPenetrationSolver
    {
        void getPenetration(MinkowskiSumPoint[] simplex, MinkowskiSum paramMinkowskiSum, Penetration paramPenetration);
    }
}
