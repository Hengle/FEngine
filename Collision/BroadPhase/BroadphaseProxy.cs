using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class BroadphaseProxy
    {
        public CollisionObject clientObject;

        // TODO: mask
        public short collisionFilterGroup;
        public short collisionFilterMask;

        public UUID uniqueId; // uniqueId is introduced for paircache. could get rid of this, by calculating the address offset etc.

        public BroadphaseProxy(CollisionObject collisionObject, short collisionFilterGroup, short collisionFilterMask)
        {
            uniqueId = UUID.GetNextUUID();
            this.collisionFilterGroup = collisionFilterGroup;
            this.collisionFilterMask = collisionFilterMask;
            clientObject = collisionObject;
        }

        public UUID getUid()
        {
            return uniqueId;
        }
    }
}
