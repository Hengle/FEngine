using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public interface MinkowskiPenetrationSolver
    {
        void getPenetration(List<VInt3> paramList, MinkowskiSum paramMinkowskiSum, Penetration paramPenetration);
    }
}
