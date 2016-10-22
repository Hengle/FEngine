using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public interface MinkowskiPenetrationSolver
    {
        void getPenetration(MinkowskiSumPoint[], MinkowskiSum paramMinkowskiSum, Penetration paramPenetration);
    }
}
