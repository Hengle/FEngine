using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public interface Rotatble
    {
        void rotate(VFixedPoint paramDouble, VInt3 paramVector2);
        void rotate(VFixedPoint paramDouble1, VFixedPoint paramDouble2, VFixedPoint paramDouble3);
    }
}
