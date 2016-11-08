using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class BroadphaseProxy
    {
        public object clientObject;

        // TODO: mask
        public short collisionFilterGroup;
        public short collisionFilterMask;

        public UUID uniqueId; // uniqueId is introduced for paircache. could get rid of this, by calculating the address offset etc.

        public BroadphaseProxy(short collisionFilterGroup, short collisionFilterMask)
        {
            uniqueId = UUID.GetNextUUID();
            this.collisionFilterGroup = collisionFilterGroup;
            this.collisionFilterMask = collisionFilterMask;
        }

        public UUID getUid()
        {
            return uniqueId;
        }
    }
}
