using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    public class UnionFind
    {
        private readonly List<Element> elements = new List<Element>();

        public void sortIslands()
        {
            // first store the original body index, and islandId
            int numElements = elements.Count;

            for (int i = 0; i < numElements; i++)
            {
                elements[i].id = find(i);
                elements[i].sz = i;
            }

            // Sort the vector using predicate and std::sort
            elements.Sort();
        }

        public void reset(int N)
        {
            allocate(N);

            for (int i = 0; i < N; i++)
            {
                elements[i].id = i;
                elements[i].sz = 1;
            }
        }

        public int getNumElements()
        {
            return elements.Count;
        }

        public bool isRoot(int x)
        {
            return (x == elements[x].id);
        }

        public Element getElement(int index)
        {
            return elements[index];
        }

        public void allocate(int N)
        {
            elements.Capacity = N;
	    }

        public void free()
        {
            elements.Clear();
        }

        public int find(int p, int q)
        {
            return (find(p) == find(q)) ? 1 : 0;
        }

        public void unite(int p, int q)
        {
            int i = find(p), j = find(q);
            if (i == j)
            {
                return;
            }

            //#ifndef USE_PATH_COMPRESSION
            ////weighted quick union, this keeps the 'trees' balanced, and keeps performance of unite O( log(n) )
            //if (m_elements[i].m_sz < m_elements[j].m_sz)
            //{ 
            //	m_elements[i].m_id = j; m_elements[j].m_sz += m_elements[i].m_sz; 
            //}
            //else 
            //{ 
            //	m_elements[j].m_id = i; m_elements[i].m_sz += m_elements[j].m_sz; 
            //}
            //#else
            elements[i].id = j;
            elements[j].sz += elements[i].sz;
            //#endif //USE_PATH_COMPRESSION
        }

        public int find(int x)
        {
            //assert(x < m_N);
            //assert(x >= 0);

            while (x != elements[x].id)
            {
                // not really a reason not to use path compression, and it flattens the trees/improves find performance dramatically

                //#ifdef USE_PATH_COMPRESSION
                elements[x].id = elements[elements[x].id].id;
                //#endif //
                x = elements[x].id;
                //assert(x < m_N);
                //assert(x >= 0);
            }
            return x;
        }

        
    }

    public class Element: IComparable<Element>
    {
        public int id;
        public int sz;

        public int CompareTo(Element other)
        {
            return id < other.id ? -1 : 1;
        }
    }
}
