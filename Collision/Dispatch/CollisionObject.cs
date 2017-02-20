using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class CollisionObject
    {
        protected VIntTransform worldTransform = VIntTransform.Identity;

        protected VIntTransform interpolationWorldTransform = VIntTransform.Identity;

        protected VInt3 interpolationLinearVelocity = VInt3.zero;
        protected VInt3 interpolationAngularVelocity = VInt3.zero;

        protected BroadphaseProxy broadphaseHandle;
        protected CollisionShape collisionShape;

        protected int collisionFlags;

        protected CollisionObjectType internalType = CollisionObjectType.COLLISION_OBJECT;
        protected bool checkCollideWith;

        public CollisionObject()
        {
            this.collisionFlags = CollisionFlags.STATIC_OBJECT;
        }

        public virtual bool checkCollideWithOverride(CollisionObject co)
        {
            return true;
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
        }

        /**
         * Avoid using this internal API call.
         * internalSetTemporaryCollisionShape is used to temporary replace the actual collision shape by a child collision shape.
         */
        public void internalSetTemporaryCollisionShape(CollisionShape collisionShape)
        {
            this.collisionShape = collisionShape;
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

        public int getCollisionFlags()
        {
            return collisionFlags;
        }

        public void setCollisionFlags(int collisionFlags)
        {
            this.collisionFlags = collisionFlags;
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
