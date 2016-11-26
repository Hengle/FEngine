using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class RigidBody: CollisionObject
    {
        private static VFixedPoint MAX_ANGVEL = FMath.Pi * VFixedPoint.Half;

        private Matrix3f invInertiaTensorWorld;
        private VInt3 linearVelocity;
        private VInt3 angularVelocity;
        private VFixedPoint inverseMass;
        private VFixedPoint angularFactor;

        private VInt3 gravity;
        private VInt3 invInertiaLocal;
        private VInt3 totalForce;
        private VInt3 totalTorque;

        private VFixedPoint linearDamping;
        private VFixedPoint angularDamping;

        private bool additionalDamping;
        private VFixedPoint additionalDampingFactor;
        private VFixedPoint additionalLinearDampingThresholdSqr;
        private VFixedPoint additionalAngularDampingThresholdSqr;
        private VFixedPoint additionalAngularDampingFactor;

        private VFixedPoint linearSleepingThreshold;
        private VFixedPoint angularSleepingThreshold;

        // optionalMotionState allows to automatic synchronize the world transform for active objects
        private MotionState optionalMotionState;

        // keep track of typed constraints referencing this rigid body
        private List<TypedConstraint> constraints = new List<TypedConstraint>();

        // for experimental overriding of friction/contact solver func
        public int contactSolverType;
        public int frictionSolverType;

        public RigidBody(RigidBodyConstructionInfo constructionInfo) {
            setupRigidBody(constructionInfo);
        }

        public RigidBody(VFixedPoint mass, MotionState motionState, CollisionShape collisionShape):
            this(mass, motionState, collisionShape, VInt3.zero)
        {

        }

        public RigidBody(VFixedPoint mass, MotionState motionState, CollisionShape collisionShape, VInt3 localInertia)
        {
            RigidBodyConstructionInfo cinfo = new RigidBodyConstructionInfo(mass, motionState, collisionShape, localInertia);
            setupRigidBody(cinfo);
        }

        private void setupRigidBody(RigidBodyConstructionInfo constructionInfo) {
            internalType = CollisionObjectType.RIGID_BODY;

            linearVelocity = VInt3.zero;
            angularVelocity = VInt3.zero;
            angularFactor = VFixedPoint.One;
            gravity = VInt3.zero;
            totalForce = VInt3.zero;
            totalTorque = VInt3.zero;
            linearDamping = VFixedPoint.Zero;
            angularDamping = VFixedPoint.Half;
            linearSleepingThreshold = constructionInfo.linearSleepingThreshold;
            angularSleepingThreshold = constructionInfo.angularSleepingThreshold;
            optionalMotionState = constructionInfo.motionState;
            contactSolverType = 0;
            frictionSolverType = 0;
            additionalDamping = constructionInfo.additionalDamping;
            additionalDampingFactor = constructionInfo.additionalDampingFactor;
            additionalLinearDampingThresholdSqr = constructionInfo.additionalLinearDampingThresholdSqr;
            additionalAngularDampingThresholdSqr = constructionInfo.additionalAngularDampingThresholdSqr;
            additionalAngularDampingFactor = constructionInfo.additionalAngularDampingFactor;

            if (optionalMotionState != null)
            {
                worldTransform = optionalMotionState.getWorldTransform();
            } else
            {
                worldTransform = constructionInfo.startWorldTransform;
            }

            interpolationWorldTransform = worldTransform;
            interpolationLinearVelocity = VInt3.zero;
            interpolationAngularVelocity = VInt3.zero;

            // moved to CollisionObject
            friction = constructionInfo.friction;
            restitution = constructionInfo.restitution;

            setCollisionShape(constructionInfo.collisionShape);

            setMassProps(constructionInfo.mass, constructionInfo.localInertia);
            setDamping(constructionInfo.linearDamping, constructionInfo.angularDamping);
            updateInertiaTensor();
        }

        public void destroy()
        {

        }

        public void proceedToTransform(VIntTransform newTrans)
        {
            setCenterOfMassTransform(newTrans);
        }

        /**
         * To keep collision detection and dynamics separate we don't store a rigidbody pointer,
         * but a rigidbody is derived from CollisionObject, so we can safely perform an upcast.
         */
        public static RigidBody upcast(CollisionObject colObj) {
            if (colObj.getInternalType() == CollisionObjectType.RIGID_BODY) {
                return (RigidBody)colObj;
            }
            return null;
        }

        /**
         * Continuous collision detection needs prediction.
         */
        public void predictIntegratedTransform(VFixedPoint timeStep, VIntTransform predictedTransform) {
            TransformUtil.integrateTransform(worldTransform, linearVelocity, angularVelocity, timeStep, predictedTransform);
        }

        public void saveKinematicState(VFixedPoint timeStep) {
            //todo: clamp to some (user definable) safe minimum timestep, to limit maximum angular/linear velocities
            if (timeStep != VFixedPoint.Zero) {
                //if we use motionstate to synchronize world transforms, get the new kinematic/animated world transform
                if (getMotionState() != null) {
                    worldTransform = getMotionState().getWorldTransform();
                }
                //Vector3f linVel = new Vector3f(), angVel = new Vector3f();

                TransformUtil.calculateVelocity(interpolationWorldTransform, worldTransform, timeStep, ref linearVelocity, ref angularVelocity);
                interpolationLinearVelocity = linearVelocity;
                interpolationAngularVelocity = angularVelocity;
                interpolationWorldTransform = worldTransform;
                //printf("angular = %f %f %f\n",m_angularVelocity.getX(),m_angularVelocity.getY(),m_angularVelocity.getZ());
            }
        }

        public void applyGravity() {
            if (isStaticOrKinematicObject())
                return;

            applyCentralForce(gravity);
        }

        public void setGravity(VInt3 acceleration) {
            if (inverseMass != VFixedPoint.Zero) {
                gravity = acceleration * VFixedPoint.One / inverseMass;
            }
        }

        public VInt3 getGravity() {
            return gravity;
        }

        public void setDamping(VFixedPoint lin_damping, VFixedPoint ang_damping) {
            linearDamping = FMath.Clamp(lin_damping, VFixedPoint.Zero, VFixedPoint.One);
            angularDamping = FMath.Clamp(ang_damping, VFixedPoint.Zero, VFixedPoint.One);
        }

        public VFixedPoint getLinearDamping() {
            return linearDamping;
        }

        public VFixedPoint getAngularDamping() {
            return angularDamping;
        }

        public VFixedPoint getLinearSleepingThreshold() {
            return linearSleepingThreshold;
        }

        public VFixedPoint getAngularSleepingThreshold() {
            return angularSleepingThreshold;
        }

        /**
         * Damps the velocity, using the given linearDamping and angularDamping.
         */
        public void applyDamping(VFixedPoint timeStep) {

            linearVelocity *= FMath.Clamp(VFixedPoint.One - linearDamping * timeStep, VFixedPoint.Zero, VFixedPoint.One);
            angularVelocity *= FMath.Clamp(VFixedPoint.One - angularDamping * timeStep, VFixedPoint.Zero, VFixedPoint.One);

            if (additionalDamping) {
                // Additional damping can help avoiding lowpass jitter motion, help stability for ragdolls etc.
                // Such damping is undesirable, so once the overall simulation quality of the rigid body dynamics system has improved, this should become obsolete
                if ((angularVelocity.sqrMagnitude < additionalAngularDampingThresholdSqr) &&
                    (linearVelocity.sqrMagnitude < additionalLinearDampingThresholdSqr)) {
                    angularVelocity *= additionalDampingFactor;
                    linearVelocity *= additionalDampingFactor;
                }

                VFixedPoint speed = linearVelocity.magnitude;
                if (speed < linearDamping) {
                    VFixedPoint dampVel = VFixedPoint.Create(0.005f);
                    if (speed > dampVel) {
                        linearVelocity -= linearVelocity.Normalize() * dampVel;
                    }
                    else {
                        linearVelocity = VInt3.zero;
                    }
                }

                VFixedPoint angSpeed = angularVelocity.magnitude;
                if (angSpeed < angularDamping) {
                    VFixedPoint angDampVel = VFixedPoint.Create(0.005f);
                    if (angSpeed > angDampVel) {
                        angularVelocity -= angularVelocity.Normalize() * angDampVel;
                    }
                    else {
                        angularVelocity = VInt3.zero;
                    }
                }
            }
        }

        public void setMassProps(VFixedPoint mass, VInt3 inertia) {
            if (mass == VFixedPoint.Zero) {
                collisionFlags |= CollisionFlags.STATIC_OBJECT;
                inverseMass = VFixedPoint.Zero;
            }
            else {
                collisionFlags &= (~CollisionFlags.STATIC_OBJECT);
                inverseMass = VFixedPoint.One / mass;
            }

            invInertiaLocal = new VInt3(inertia.x != VFixedPoint.Zero ? VFixedPoint.One / inertia.x : VFixedPoint.Zero,
                inertia.y != VFixedPoint.Zero ? VFixedPoint.One / inertia.y : VFixedPoint.Zero,
                inertia.z != VFixedPoint.Zero ? VFixedPoint.One / inertia.z : VFixedPoint.Zero);
        }

        public VFixedPoint getInvMass() {
            return inverseMass;
        }

        public Matrix3f getInvInertiaTensorWorld() {
            return invInertiaTensorWorld;
        }

        public void integrateVelocities(VFixedPoint step) {
            if (isStaticOrKinematicObject()) {
                return;
            }

            linearVelocity = totalForce * inverseMass * step + linearVelocity;
            VInt3 tmp = invInertiaTensorWorld;
            invInertiaTensorWorld.transform(tmp);
            angularVelocity = tmp * step + angularVelocity;

            // clamp angular velocity. collision calculations will fail on higher angular velocities
            float angvel = angularVelocity.length();
            if (angvel * step > MAX_ANGVEL) {
                angularVelocity.scale((MAX_ANGVEL / step) / angvel);
            }
        }

        public void setCenterOfMassTransform(VIntTransform xform) {
            if (isStaticOrKinematicObject()) {
                interpolationWorldTransform = worldTransform;
            }
            else {
                interpolationWorldTransform = xform;
            }
            getLinearVelocity(interpolationLinearVelocity);
            getAngularVelocity(interpolationAngularVelocity);
            worldTransform = xform;
            updateInertiaTensor();
        }

        public void applyCentralForce(VInt3 force) {
            totalForce += force;
        }

        public VInt3 getInvInertiaDiagLocal() {
            return invInertiaLocal;
        }
    }
}