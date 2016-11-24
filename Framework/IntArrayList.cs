using System;
using System.Collections.Generic;

namespace MobaGame.Framework
{ 
    public class IntArrayList
    {
        private int[] array = new int[16];
        private int size;

        public void add(int value)
        {
            if (size == array.Length)
            {
                expand();
            }

            array[size++] = value;
        }

        private void expand()
        {
            int[] newArray = new int[array.Length << 1];
            Array.Copy(array, 0, newArray, 0, array.Length);
            array = newArray;
        }

        public int remove(int index)
        {
            if (index >= size) throw new IndexOutOfRangeException();
            int old = array[index];
            Array.Copy(array, index + 1, array, index, size - index - 1);
            size--;
            return old;
        }

        public int get(int index)
        {
            if (index >= size) throw new IndexOutOfRangeException();
            return array[index];
        }

        public void set(int index, int value)
        {
            if (index >= size) throw new IndexOutOfRangeException();
            array[index] = value;
        }

        public int Size()
        {
            return size;
        }

        public void clear()
        {
            size = 0;
        }

        public void resize(int size, int value)
        {
            while(this.size < size)
            {
                add(value);
            }

            while(this.size > size)
            {
                remove(this.size - 1);
            }
        }

    }
}
