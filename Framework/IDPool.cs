using System.Collections.Generic;

namespace MobaGame.Framework
{
    public class IDPoolBase
    {
        protected int mCurrentID = 0;
        protected int[] mFreeID;

        protected int capacity;
        protected int size;

        public IDPoolBase(int capacity)
        {
            mFreeID = new int[capacity];
            this.capacity = capacity;
            size = 0;
        }

        public void freeID(int id)
        {
            if(id == mCurrentID - 1)
            {
                --mCurrentID;
            }
            else
            {
                mFreeID[size++] = id;
            }
        }

        public virtual void freeAll()
        {
            mCurrentID = 0;
            mFreeID = new int[capacity];
        }

        public int getNewID()
        {
            if(size > 0)
            {
                return mFreeID[--size];
            }
            else
            {
                return mCurrentID++;
            }
        }

        public int getNumUsedID()
        {
            return mCurrentID - size;
        }

        public int getNumRemainingIDs()
        {
            return capacity - size;
        }
    }

    public class DeferredIDPoolBase: IDPoolBase
    {
        private List<int> mDeferredFreeIDs = new List<int>();

        public DeferredIDPoolBase(int capacity): base(capacity)
        { }

        public void deferredFreeID(int id)
        {
            mDeferredFreeIDs.Add(id);
        }

        public void processDeferredIDs()
        {
            int deferredFreeIDCount = mDeferredFreeIDs.Count;
            for(int a = 0; a < deferredFreeIDCount; a++)
            {
                mFreeID[size++] = mDeferredFreeIDs[a];
            }
            mDeferredFreeIDs.Clear();
        }

        public override void freeAll()
        {
            mDeferredFreeIDs.Clear();
            base.freeAll();
        } 
    }
}
