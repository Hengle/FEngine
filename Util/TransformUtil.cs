using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public static class TransformUtil
    {
        public static readonly VFixedPoint ANGULAR_MOTION_THRESHOLD = FMath.Pi * VFixedPoint.Half * VFixedPoint.Half;

        public static void calculateVelocity(VIntTransform transform0, VIntTransform transform1, VFixedPoint timeStep, ref VInt3 linVel, ref VInt3 angVel)
        {
            linVel = (transform1.position - transform0.position) / timeStep;
            VIntQuaternion rotation = VIntQuaternion.FromToRotation(transform0.forward, transform1.forward);
            angVel.x = rotation.x;
            angVel.y = rotation.y;
            angVel.z = rotation.z;
            angVel = angVel.Normalize() * rotation.w / timeStep;
	    }

        public static VIntTransform integrateTransform(VIntTransform curTrans, VInt3 linvel, VInt3 angvel, VFixedPoint timeStep)
        {
            VIntTransform predictedTransform = VIntTransform.Identity;
            predictedTransform.position += linvel * timeStep;
            VInt3 axis;
            VFixedPoint fAngle = angvel.magnitude;

            // limit the angular motion
            if (fAngle * timeStep > ANGULAR_MOTION_THRESHOLD) {
                fAngle = ANGULAR_MOTION_THRESHOLD / timeStep;
            }

            VIntQuaternion dorn = VIntQuaternion.AngleAxis(fAngle * timeStep, angvel);
            VIntQuaternion predictedOrn = dorn * curTrans.rotation;
            predictedTransform.rotation = predictedOrn;
            return predictedTransform;
        }
    }
}
