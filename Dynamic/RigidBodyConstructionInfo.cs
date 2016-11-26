using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class RigidBodyConstructionInfo
    {
        public VFixedPoint mass;

        /**
         * When a motionState is provided, the rigid body will initialize its world transform
         * from the motion state. In this case, startWorldTransform is ignored.
         */
        public MotionState motionState;
        public VIntTransform startWorldTransform = VIntTransform.Identity;

        public CollisionShape collisionShape;
        public VInt3 localInertia;
        public VFixedPoint linearDamping;
        public VFixedPoint angularDamping;

        /** Best simulation results when friction is non-zero. */
        public VFixedPoint friction = VFixedPoint.Create(0.5f);
        /** Best simulation results using zero restitution. */
        public VFixedPoint restitution = VFixedPoint.Zero;

        public VFixedPoint linearSleepingThreshold = VFixedPoint.Create(0.8f);
        public VFixedPoint angularSleepingThreshold = VFixedPoint.One;

        /**
         * Additional damping can help avoiding lowpass jitter motion, help stability for ragdolls etc.
         * Such damping is undesirable, so once the overall simulation quality of the rigid body dynamics
         * system has improved, this should become obsolete.
         */
        public bool additionalDamping = false;
        public VFixedPoint additionalDampingFactor = VFixedPoint.Create(0.005f);
        public VFixedPoint additionalLinearDampingThresholdSqr = VFixedPoint.Create(0.01f);
        public VFixedPoint additionalAngularDampingThresholdSqr = VFixedPoint.Create(0.01f);
        public VFixedPoint additionalAngularDampingFactor = VFixedPoint.Create(0.01f);

        public RigidBodyConstructionInfo(VFixedPoint mass, MotionState motionState, CollisionShape collisionShape):
            this(mass, motionState, collisionShape, VInt3.zero)
        {

        }

        public RigidBodyConstructionInfo(VFixedPoint mass, MotionState motionState, CollisionShape collisionShape, VInt3 localInertia) {
            this.mass = mass;
            this.motionState = motionState;
            this.collisionShape = collisionShape;
            this.localInertia = localInertia;
        }
    }
}