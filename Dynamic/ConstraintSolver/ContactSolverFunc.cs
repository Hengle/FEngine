using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public abstract class ContactSolverFunc
    {
        public abstract VFixedPoint resolveContact(RigidBody body1, RigidBody body2, ManifoldPoint contactPoint, ContactSolverInfo info);
    }
}