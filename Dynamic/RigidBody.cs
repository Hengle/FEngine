using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class RigidBody: CollisionObject
    {
        private static VFixedPoint MAX_ANGVEL = FMath.Pi * VFixedPoint.Half;

        private FMatrix3 invInertiaTensorWorld;
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
        public VIntTransform predictIntegratedTransform(VFixedPoint timeStep) {
            return TransformUtil.integrateTransform(worldTransform, linearVelocity, angularVelocity, timeStep);
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

        public FMatrix3 getInvInertiaTensorWorld() {
            return invInertiaTensorWorld;
        }

        public void integrateVelocities(VFixedPoint step) {
            if (isStaticOrKinematicObject()) {
                return;
            }

            linearVelocity = totalForce * inverseMass * step + linearVelocity;
            VInt3 tmp = invInertiaTensorWorld.Transform(totalTorque);
            angularVelocity = tmp * step + angularVelocity;

            // clamp angular velocity. collision calculations will fail on higher angular velocities
            VFixedPoint angvel = angularVelocity.magnitude;
            if (angvel * step > MAX_ANGVEL) {
                angularVelocity *= MAX_ANGVEL / step / angvel;
            }
        }

        public void setCenterOfMassTransform(VIntTransform xform) {
            if (isStaticOrKinematicObject()) {
                interpolationWorldTransform = worldTransform;
            }
            else {
                interpolationWorldTransform = xform;
            }
            interpolationLinearVelocity = getLinearVelocity();
            interpolationAngularVelocity = getAngularVelocity();
            worldTransform = xform;
            updateInertiaTensor();
        }

        public void applyCentralForce(VInt3 force) {
            totalForce += force;
        }

        public VInt3 getInvInertiaDiagLocal() {
            return invInertiaLocal;
        }

        public void setInvInertiaDiagLocal(VInt3 diagInvInertia) {
            invInertiaLocal = diagInvInertia;
        }

        public void setSleepingThresholds(VFixedPoint linear, VFixedPoint angular) {
            linearSleepingThreshold = linear;
            angularSleepingThreshold = angular;
        }

        public void applyTorque(VInt3 torque) {
            totalTorque += torque;
        }

        public void applyForce(VInt3 force, VInt3 rel_pos)
        {
            applyCentralForce(force);

            VInt3 tmp = VInt3.Cross(rel_pos, force) * angularFactor;
            applyTorque(tmp);
        }

        public void applyCentralImpulse(VInt3 impulse)
        {
            linearVelocity = impulse * inverseMass + linearVelocity;
        }

        public void applyTorqueImpulse(VInt3 torque) {
            VInt3 tmp = invInertiaTensorWorld.Transform(torque);
            angularVelocity += tmp;
        }

        public void applyImpulse(VInt3 impulse, VInt3 rel_pos) {
            if (inverseMass != VFixedPoint.Zero) {
                applyCentralImpulse(impulse);
                if (angularFactor != VFixedPoint.Zero)
                {
                    VInt3 tmp = VInt3.Cross(rel_pos, impulse) * angularFactor;
                    applyTorqueImpulse(tmp);
                }
            }
        }

        /**
         * Optimization for the iterative solver: avoid calculating constant terms involving inertia, normal, relative position.
         */
        public void internalApplyImpulse(VInt3 linearComponent, VInt3 angularComponent, VFixedPoint impulseMagnitude) {
            if (inverseMass != VFixedPoint.Zero) {
                linearVelocity = linearComponent * impulseMagnitude + linearVelocity;
                if (angularFactor != VFixedPoint.Zero) {
                    angularVelocity = angularComponent * impulseMagnitude * angularFactor + angularVelocity;
                }
            }
        }

        public void clearForces() {
            totalForce = VInt3.zero;
            totalTorque = VInt3.zero;
        }

        public void updateInertiaTensor() {
            invInertiaTensorWorld = new FMatrix3(invInertiaLocal.x / (worldTransform.scale.x * worldTransform.scale.x), VFixedPoint.Zero, VFixedPoint.Zero,
                VFixedPoint.Zero, invInertiaLocal.y /(worldTransform.scale.y * worldTransform.scale.y), VFixedPoint.Zero,
                VFixedPoint.Zero, VFixedPoint.Zero, invInertiaLocal.z / (worldTransform.scale.z * worldTransform.scale.z)
            );
        }

        public VInt3 getCenterOfMassPosition() {
            return worldTransform.position;
        }

        public VIntQuaternion getOrientation() {
            return worldTransform.rotation;
        }

        public VIntTransform getCenterOfMassTransform() {
            return worldTransform;
        }

        public VInt3 getLinearVelocity() {
            return linearVelocity;
        }

        public VInt3 getAngularVelocity() {
            return angularVelocity;
        }

        public void setLinearVelocity(VInt3 lin_vel)
        {
            if (collisionFlags != CollisionFlags.STATIC_OBJECT)
                return;
            linearVelocity = lin_vel;
        }

        public void setAngularVelocity(VInt3 ang_vel)
        {
            if (collisionFlags != CollisionFlags.STATIC_OBJECT)
                return;
            angularVelocity = ang_vel;
        }

        public VInt3 getVelocityInLocalPoint(VInt3 rel_pos) {
            // we also calculate lin/ang velocity for kinematic objects
            VInt3 vec = VInt3.Cross(angularVelocity, rel_pos) + linearVelocity;
            return vec;
        }

        public void translate(VInt3 v) {
            worldTransform.position += v;
        }

        public void getAabb(out VInt3 aabbMin, out VInt3 aabbMax) {
            getCollisionShape().getAabb(worldTransform, out aabbMin, out aabbMax);
        }

        public VFixedPoint computeImpulseDenominator(VInt3 pos, VInt3 normal)
        {
            VInt3 r0 = pos - getCenterOfMassPosition();
            VInt3 c0 = VInt3.Cross(r0, normal);
            VInt3 tmp = getInvInertiaTensorWorld().Transpose().Transform(c0);
            VInt3 vec = VInt3.Cross(tmp, r0);
            return inverseMass + VInt3.Dot(normal, vec);
        }

        public VFixedPoint computeAngularImpulseDenominator(VInt3 axis) {
            VInt3 vec = getInvInertiaTensorWorld().Transpose().Transform(axis);
            return VInt3.Dot(axis, vec);
        }

        public void updateDeactivation(VFixedPoint timeStep) {
            if ((getActivationState() == ISLAND_SLEEPING) || (getActivationState() == DISABLE_DEACTIVATION)) {
                return;
            }

            if ((getLinearVelocity().sqrMagnitude < linearSleepingThreshold * linearSleepingThreshold) &&
            (getAngularVelocity().sqrMagnitude < angularSleepingThreshold * angularSleepingThreshold)) {
                deactivationTime += timeStep;
            }
            else {
                deactivationTime = VFixedPoint.Zero;
                setActivationState(0);
            }
        }

        public bool wantsSleeping() {
            if (getActivationState() == DISABLE_DEACTIVATION) {
                return false;
            }

            if ((getActivationState() == ISLAND_SLEEPING) || (getActivationState() == WANTS_DEACTIVATION)) {
                return true;
            }

            return false;
        }

        public BroadphaseProxy getBroadphaseProxy() {
            return broadphaseHandle;
        }

        public void setNewBroadphaseProxy(BroadphaseProxy broadphaseProxy) {
            this.broadphaseHandle = broadphaseProxy;
        }

        public MotionState getMotionState() {
            return optionalMotionState;
        }

        public void setMotionState(MotionState motionState) {
            this.optionalMotionState = motionState;
            if (optionalMotionState != null) {
                motionState.getWorldTransform(worldTransform);
            }
        }

        public void setAngularFactor(VFixedPoint angFac) {
            angularFactor = angFac;
        }

        public VFixedPoint getAngularFactor() {
            return angularFactor;
        }

        /**
         * Is this rigidbody added to a CollisionWorld/DynamicsWorld/Broadphase?
         */
        public bool isInWorld() {
            return (getBroadphaseProxy() != null);
        }

        public override bool checkCollideWithOverride(CollisionObject co) {
            // TODO: change to cast
            RigidBody otherRb = RigidBody.upcast(co);
            if (otherRb == null) {
                return true;
            }

            for (int i = 0; i < constraints.Count; ++i) {
                TypedConstraint c = constraints[i];
                if (c.getRigidBodyA() == otherRb || c.getRigidBodyB() == otherRb) {
                    return false;
                }
            }

            return true;
        }

        public void addConstraintRef(TypedConstraint c) {
            int index = constraints.IndexOf(c);
            if (index == -1) {
                constraints.Add(c);
            }

            checkCollideWith = true;
        }

        public void removeConstraintRef(TypedConstraint c) {
            constraints.Remove(c);
            checkCollideWith = (constraints.Count > 0);
        }

        public TypedConstraint getConstraintRef(int index) {
            return constraints[index];
        }

        public int getNumConstraintRefs() {
            return constraints.Count;
        }

    }
}