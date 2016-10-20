using System;
using System.Collections.Generic;

namespace FEngine.Framework
{
    public class Heap<T> where T : IComparable<T>
    {
        int HeapSize;
        List<T> Data;

        public Heap()
        {
            HeapSize = 0;
            Data = new List<T>();
        }

        public bool IsEmpty()
        {
            return HeapSize == 0;
        }

        public void Clear()
        {
            HeapSize = 0;
        }

        public void Push(T value)
        {
            int newIndex;
            int parentIndex = parent(HeapSize);
            if(HeapSize >= Data.Capacity)
            {
                Data.Capacity *= 2;
            }

            for (newIndex = HeapSize; newIndex > 0 && value.CompareTo(Data[parentIndex]) < 0; newIndex = parentIndex, parentIndex = parent(newIndex))
            {
                Data[newIndex] = Data[parentIndex];
            }
            Data[newIndex] = value;
            HeapSize++;
        }

        public T pop()
        {
            int i, child;
            //try to avoid LHS
            int tempHs = HeapSize - 1;
            HeapSize = tempHs;
            T min = Data[0];
            T last = Data[tempHs];

            for (i = 0; (child = left(i)) < tempHs; i = child)
            {
                /* Find highest priority child */
                int rightChild = child + 1;

                child += (rightChild < tempHs) & Data[rightChild.CompareTo(Data[child]) < 0 ? 1 : 0;

                if (compare(last, mData[child]))
                    break;

                PX_ASSERT(i < Capacity);
                mData[i] = mData[child];
            }
            PX_ASSERT(i < Capacity);
            Data[i] = last;
            return min;
        }
    }
}
