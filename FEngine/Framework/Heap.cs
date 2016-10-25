using System;
using System.Collections.Generic;

namespace MobaGame.Framework
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

        public T this[int key]
        {
            get
            {
                return Data[key];
            }
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

        public T peak()
        {
            return Data[0];
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

                child += (rightChild < tempHs) && Data[rightChild].CompareTo(Data[child]) < 0 ? 1 : 0;

                if (last.CompareTo(Data[child]) < 0)
                    break;
                Data[i] = Data[child];
            }
            Data[i] = last;
            return min;
        }

        public int size()
		{
			return HeapSize;
		}

        static int left(int nodeIndex)
        {
            return (nodeIndex << 1) + 1;
        }

        static int parent(int nodeIndex)
        {
            return (nodeIndex - 1) >> 1;
        }

    }
}
