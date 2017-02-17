using System.Collections.Generic;
using MobaGame.FixedMath;

namespace MobaGame.Collision
{
    public class GhostObject: CollisionObject
    {
        protected List<CollisionObject> overlappingObjects = new List<CollisionObject>();

        public GhostObject()
        {
            this.internalType = CollisionObjectType.GHOST_OBJECT;
        }

        public void addOverlappingObjectInternal(BroadphaseProxy otherProxy, BroadphaseProxy thisProxy)
        {
            CollisionObject otherObject = otherProxy.clientObject;

            // if this linearSearch becomes too slow (too many overlapping objects) we should add a more appropriate data structure
            int index = overlappingObjects.IndexOf(otherObject);
            if (index == -1)
            {
                // not found
                overlappingObjects.Add(otherObject);
            }
        }

        /**
         * This method is mainly for expert/internal use only.
         */
        public void removeOverlappingObjectInternal(BroadphaseProxy otherProxy)
        {
            CollisionObject otherObject = otherProxy.clientObject;

            int index = overlappingObjects.IndexOf(otherObject);
            if (index != -1)
            {
                overlappingObjects[index] = overlappingObjects[overlappingObjects.Count - 1];
                overlappingObjects.RemoveAt(overlappingObjects.Count - 1);
            }
        }

        public int getNumOverlappingObjects()
        {
            return overlappingObjects.Count;
        }

        public CollisionObject getOverlappingObject(int index)
        {
            return overlappingObjects[index];
        }

        public List<CollisionObject> getOverlappingPairs()
        {
            return overlappingObjects;
        }
    }
}
