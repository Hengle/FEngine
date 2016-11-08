using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class TransformUtil
    {
        public static void calculateVelocity(VIntTransform transform0, VIntTransform transform1, VFixedPoint timeStep, ref VInt3 linVel, ref VInt3 angVel)
        {
            linVel = (transform1.position - transform0.position) / timeStep;
            VIntQuaternion rotation = VIntQuaternion.FromToRotation(transform0.forward, transform1.forward);
            angVel.x = rotation.x;
            angVel.y = rotation.y;
            angVel.z = rotation.z;
            angVel = angVel.Normalize() * rotation.w / timeStep;
	    }
    }
}
