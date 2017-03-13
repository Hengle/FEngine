using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class CollisionObject
    {
        protected VIntTransform worldTransform = VIntTransform.Identity;
        public VInt3 LinearVel
        {
            set; get;
        }

        VFixedPoint _InvMass;
        public VFixedPoint InvMass
        {
            set
            {
                _InvMass = value;
            }

            get
            {
                if(isStaticOrKinematicObject())
                {
                    return VFixedPoint.Zero;
                }
                else
                {
                    return _InvMass;
                }
            }
        }

        protected BroadphaseProxy broadphaseHandle;
        protected CollisionShape collisionShape;

        protected int collisionFlags;
        protected bool checkCollideWith;

        public CollisionObject()
        {
            this.collisionFlags = CollisionFlags.NORMAL_OBJECT;
            InvMass = VFixedPoint.One;
            LinearVel = VInt3.zero;
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

        public void ApplyImpulse(VInt3 impulse)
        {
            if (!isStaticOrKinematicObject())
            {
                LinearVel += impulse * InvMass;
            }
        }

        public void IntegrateVelocity(VFixedPoint dt)
        {
            worldTransform.position = worldTransform.position + LinearVel * dt;
        }
    }
}
