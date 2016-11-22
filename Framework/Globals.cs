using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class Globals
    {
        public static readonly VFixedPoint EPS = VFixedPoint.FromBinary(256);
        public static readonly VFixedPoint EPS2 = VFixedPoint.FromBinary(16);
        public static readonly VFixedPoint CONVEX_DISTANCE_MARGIN = VFixedPoint.Create(4) / VFixedPoint.Create(10);

        public static VFixedPoint getContactBreakingThreshold()
        {
            return VFixedPoint.Create(4) / VFixedPoint.Create(100);
        }
    }
}
