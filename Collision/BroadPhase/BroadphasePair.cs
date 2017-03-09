using System;

namespace MobaGame.Collision
{
    public class BroadphasePair: IComparable<BroadphasePair>
    {
        public BroadphaseProxy pProxy0;
        public BroadphaseProxy pProxy1;
        public PersistentManifold manifold;

        public BroadphasePair()
        {
            
        }

        public BroadphasePair(BroadphaseProxy pProxy0, BroadphaseProxy pProxy1)
        {
            this.pProxy0 = pProxy0;
            this.pProxy1 = pProxy1;
            manifold = new PersistentManifold(pProxy0.clientObject, pProxy1.clientObject);
        }

        public BroadphasePair(BroadphasePair p)
        {
            pProxy0 = p.pProxy0;
            pProxy1 = p.pProxy1;
            manifold = new PersistentManifold(pProxy0.clientObject, pProxy1.clientObject);
        }

        public bool Equals(BroadphasePair obj)
        {
            if (obj == null)
                return false;
            return pProxy0 == obj.pProxy0 && pProxy1 == obj.pProxy1;
        }

        public int CompareTo(BroadphasePair other)
        {
            if(pProxy0.getUid() > other.pProxy0.getUid())
            {
                return -1;
            }
            else if (pProxy0.getUid() == other.pProxy0.getUid() && pProxy1.getUid() >= other.pProxy1.getUid())
            {
                return -1;
            }
            return 1;
        }

    }
}
