using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class Globals
    {
        public static readonly VFixedPoint EPS = VFixedPoint.FromBinary(256);
        public static readonly VFixedPoint EPS2 = VFixedPoint.FromBinary(16);
        public static readonly VFixedPoint CONVEX_DISTANCE_MARGIN = VFixedPoint.Create(4) / VFixedPoint.Create(100);
        //For impulse method
        public static readonly VFixedPoint ALLOWD_PENETRATION = VFixedPoint.One / VFixedPoint.Create(100);
        public static readonly VFixedPoint BIAS_FACTOR = VFixedPoint.Two / VFixedPoint.Create(100);
        public static readonly VFixedPoint FRICTION = VFixedPoint.Half;
        //gravity
        public static readonly VInt3 g = new VInt3(VFixedPoint.Zero, -VFixedPoint.Create(10), VFixedPoint.Zero);

        public static VFixedPoint getContactBreakingThreshold()
        {
            return VFixedPoint.Create(4) / VFixedPoint.Create(100);
        }
    }
}
