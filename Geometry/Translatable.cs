using MobaGame.FixedMath;

namespace MobaGame.Collision
{

    public interface Translatable
    {
        void translate(VFixedPoint x, VFixedPoint y, VFixedPoint z);
        void translate(VInt3 paramVector2);
    }
}
