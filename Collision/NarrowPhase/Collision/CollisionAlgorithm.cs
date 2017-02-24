using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public delegate void CollisionAlgorithm(CollisionObject body0, CollisionObject body1, DispatcherInfo dispatchInfo, ManifoldResult resultOut);
}