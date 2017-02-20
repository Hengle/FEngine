using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class CollisionObject
    {
        protected VIntTransform worldTransform = VIntTransform.Identity;

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

        public CollisionShape getCollisionShape()
        {
            return collisionShape;
        }

        public void setCollisionShape(CollisionShape collisionShape)
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
