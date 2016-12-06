namespace MobaGame.Collision
{
    public abstract class ContactSolverFunc
    {
        public abstract float resolveContact(RigidBody body1, RigidBody body2, ManifoldPoint contactPoint, ContactSolverInfo info);
    }
}