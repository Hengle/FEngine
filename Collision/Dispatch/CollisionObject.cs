using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class CollisionObject
    {
        public static readonly int ACTIVE_TAG = 1;
        public static readonly int ISLAND_SLEEPING = 2;
        public static readonly int WANTS_DEACTIVATION = 3;
        public static readonly int DISABLE_DEACTIVATION = 4;
        public static readonly int DISABLE_SIMULATION = 5;
        protected VIntTransform worldTransform = VIntTransform.Identity;

        protected VIntTransform interpolationWorldTransform = VIntTransform.Identity;

        protected VInt3 interpolationLinearVelocity = VInt3.zero;
        protected VInt3 interpolationAngularVelocity = VInt3.zero;

        protected BroadphaseProxy broadphaseHandle;
        protected CollisionShape collisionShape;
        protected CollisionShape rootCollisionShape;

        protected int collisionFlags;
        protected int islandTag1;
        protected int companionId;
        protected int activationState1;
        protected VFixedPoint deactivationTime;
        protected VFixedPoint friction;
        protected VFixedPoint restitution;

        protected CollisionObjectType internalType = CollisionObjectType.COLLISION_OBJECT;

        ///time of impact calculation
        protected VFixedPoint hitFraction;
        ///Swept sphere radius (0.0 by default), see btConvexConvexAlgorithm::
        protected VFixedPoint ccdSweptSphereRadius;
        /// Don't do continuous collision detection if the motion (in one step) is less then ccdMotionThreshold
        protected VFixedPoint ccdMotionThreshold = VFixedPoint.Zero;
        /// If some object should have elaborate collision filtering by sub-classes
        protected bool checkCollideWith;

        public CollisionObject()
        {
            this.collisionFlags = CollisionFlags.STATIC_OBJECT;
            this.islandTag1 = -1;
            this.companionId = -1;
            this.activationState1 = 1;
            this.friction = VFixedPoint.Create(0.5f);
            this.hitFraction = VFixedPoint.One;
        }

        public virtual bool checkCollideWithOverride(CollisionObject co)
        {
            return true;
        }

        public bool mergesSimulationIslands()
        {
            ///static objects, kinematic and object without contact response don't merge islands
            return ((collisionFlags & (CollisionFlags.STATIC_OBJECT | CollisionFlags.KINEMATIC_OBJECT | CollisionFlags.NO_CONTACT_RESPONSE)) == 0);
        }

        public bool isStaticObject()
        {
            return (collisionFlags & CollisionFlags.STATIC_OBJECT) != 0;
        }

        public bool isKinematicObject()
        {
            return (collisionFlags & CollisionFlags.KINEMATIC_OBJECT) != 0;
        }

        public bool isStaticOrKinematicObject()
        {
            return (collisionFlags & (CollisionFlags.KINEMATIC_OBJECT | CollisionFlags.STATIC_OBJECT)) != 0;
        }

        public bool hasContactResponse()
        {
            return (collisionFlags & CollisionFlags.NO_CONTACT_RESPONSE) == 0;
        }

        public CollisionShape getCollisionShape()
        {
            return collisionShape;
        }

        public void setCollisionShape(CollisionShape collisionShape)
        {
            this.collisionShape = collisionShape;
            this.rootCollisionShape = collisionShape;
        }

        public CollisionShape getRootCollisionShape()
        {
            return rootCollisionShape;
        }

        /**
         * Avoid using this internal API call.
         * internalSetTemporaryCollisionShape is used to temporary replace the actual collision shape by a child collision shape.
         */
        public void internalSetTemporaryCollisionShape(CollisionShape collisionShape)
        {
            this.collisionShape = collisionShape;
        }

        public int getActivationState()
        {
            return activationState1;
        }

        public void setActivationState(int newState)
        {
            if ((activationState1 != DISABLE_DEACTIVATION) && (activationState1 != DISABLE_SIMULATION))
            {
                this.activationState1 = newState;
            }
        }

        public VFixedPoint getDeactivationTime()
        {
            return deactivationTime;
        }

        public void setDeactivationTime(VFixedPoint deactivationTime)
        {
            this.deactivationTime = deactivationTime;
        }

        public void forceActivationState(int newState)
        {
            this.activationState1 = newState;
        }

        public void activate()
        {
            activate(false);
        }

        public void activate(bool forceActivation)
        {
            if (forceActivation || (collisionFlags & (CollisionFlags.STATIC_OBJECT | CollisionFlags.KINEMATIC_OBJECT)) == 0)
            {
                setActivationState(ACTIVE_TAG);
                deactivationTime = VFixedPoint.Zero;
            }
        }

        public bool isActive()
        {
            return ((getActivationState() != ISLAND_SLEEPING) && (getActivationState() != DISABLE_SIMULATION));
        }

        public VFixedPoint getRestitution()
        {
            return restitution;
        }

        public void setRestitution(VFixedPoint restitution)
        {
            this.restitution = restitution;
        }

        public VFixedPoint getFriction()
        {
            return friction;
        }

        public void setFriction(VFixedPoint friction)
        {
            this.friction = friction;
        }

        // reserved for Bullet internal usage
        public CollisionObjectType getInternalType()
        {
            return internalType;
        }

        public VIntTransform getWorldTransform()
        {
            return worldTransform;
        }

        public void setWorldTransform(VIntTransform worldTransform)
        {
            this.worldTransform = worldTransform;
        }

        public BroadphaseProxy getBroadphaseHandle()
        {
            return broadphaseHandle;
        }

        public void setBroadphaseHandle(BroadphaseProxy broadphaseHandle)
        {
            this.broadphaseHandle = broadphaseHandle;
        }

        public VIntTransform getInterpolationWorldTransform()
        {
            return interpolationWorldTransform;
        }

        public void setInterpolationWorldTransform(VIntTransform interpolationWorldTransform)
        {
            this.interpolationWorldTransform = interpolationWorldTransform;
        }

        public void setInterpolationLinearVelocity(VInt3 linvel)
        {
            interpolationLinearVelocity = linvel;
        }

        public void setInterpolationAngularVelocity(VInt3 angvel)
        {
            interpolationAngularVelocity = angvel;
        }

        public VInt3 getInterpolationLinearVelocity(VInt3 returnParam)
        {
            returnParam = interpolationLinearVelocity;
            return returnParam;
        }

        public VInt3 getInterpolationAngularVelocity(VInt3 returnParam)
        {
            returnParam = interpolationAngularVelocity;
            return returnParam;
        }

        public int getIslandTag()
        {
            return islandTag1;
        }

        public void setIslandTag(int islandTag)
        {
            this.islandTag1 = islandTag;
        }

        public int getCompanionId()
        {
            return companionId;
        }

        public void setCompanionId(int companionId)
        {
            this.companionId = companionId;
        }

        public VFixedPoint getHitFraction()
        {
            return hitFraction;
        }

        public void setHitFraction(VFixedPoint hitFraction)
        {
            this.hitFraction = hitFraction;
        }

        public int getCollisionFlags()
        {
            return collisionFlags;
        }

        public void setCollisionFlags(int collisionFlags)
        {
            this.collisionFlags = collisionFlags;
        }

        // Swept sphere radius (0.0 by default), see btConvexConvexAlgorithm::
        public VFixedPoint getCcdSweptSphereRadius()
        {
            return ccdSweptSphereRadius;
        }

        // Swept sphere radius (0.0 by default), see btConvexConvexAlgorithm::
        public void setCcdSweptSphereRadius(VFixedPoint ccdSweptSphereRadius)
        {
            this.ccdSweptSphereRadius = ccdSweptSphereRadius;
        }

        public VFixedPoint getCcdMotionThreshold()
        {
            return ccdMotionThreshold;
        }

        public VFixedPoint getCcdSquareMotionThreshold()
        {
            return ccdMotionThreshold * ccdMotionThreshold;
        }

        // Don't do continuous collision detection if the motion (in one step) is less then ccdMotionThreshold
        public void setCcdMotionThreshold(VFixedPoint ccdMotionThreshold)
        {
            // JAVA NOTE: fixed bug with usage of ccdMotionThreshold*ccdMotionThreshold
            this.ccdMotionThreshold = ccdMotionThreshold;
        }

        public bool CheckCollideWith(CollisionObject co)
        {
            if (checkCollideWith)
            {
                return checkCollideWithOverride(co);
            }

            return true;
        }
    }
}
