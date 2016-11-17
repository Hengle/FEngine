using System;
using MobaGame.FixedMath;
using System.Collections.Generic;

namespace MobaGame.Collision
{
    public class Dbvt
    {
        public static readonly int SIMPLE_STACKSIZE = 64;
        public static readonly int DOUBLE_STACKSIZE = SIMPLE_STACKSIZE * 2;

        public Node root = null;
        public Node free = null;
        public int lkhd = -1;
        public int leaves = 0;
        public int opath = 0;

        public Dbvt()
        {
        }

        public void clear()
        {
            if (root != null)
            {
                recursedeletenode(this, root);
            }
            //btAlignedFree(m_free);
            free = null;
        }

        public bool empty()
        {
            return (root == null);
        }

        public void optimizeIncremental(int passes)
        {
            if (passes < 0)
            {
                passes = leaves;
            }

            if (root != null && (passes > 0))
            {
                do
                {
                    Node node = root;
                    int bit = 0;
                    while (node.isinternal())
                    {
                        node = sort(node, ref root).childs[(opath >> bit) & 1];

                        bit = (bit + 1) & (/*sizeof(unsigned)*/4 * 8 - 1);
                    }
                    update(node);
                    ++opath;
                }
                while (--passes != 0);
            }
        }

        public Node insert(DbvtAabbMm box, object data)
        {
            Node leaf = createnode(this, null, box, data);
            insertleaf(this, root, leaf);
            leaves++;
            return leaf;
        }

        public void update(Node leaf)
        {
            update(leaf, -1);
        }

        public void update(Node leaf, int lookahead)
        {
            Node root = removeleaf(this, leaf);
            if (root != null)
            {
                if (lookahead >= 0)
                {
                    for (int i = 0; (i < lookahead) && root.parent != null; i++)
                    {
                        root = root.parent;
                    }
                }
                else {
                    root = this.root;
                }
            }
            insertleaf(this, root, leaf);
        }

        public void update(Node leaf, DbvtAabbMm volume)
        {
            Node root = removeleaf(this, leaf);
            if (root != null)
            {
                if (lkhd >= 0)
                {
                    for (int i = 0; (i < lkhd) && root.parent != null; i++)
                    {
                        root = root.parent;
                    }
                }
                else {
                    root = this.root;
                }
            }
            leaf.volume.set(volume);
            insertleaf(this, root, leaf);
        }

        public bool update(Node leaf, DbvtAabbMm volume, VInt3 velocity, VFixedPoint margin)
        {
            if (leaf.volume.Contain(volume))
            {
                return false;
            }
            VInt3 tmp = new VInt3(margin, margin, margin);
            volume.Expand(tmp);
            volume.SignedExpand(velocity);

            update(leaf, volume);
            return true;
        }

        public bool update(Node leaf, DbvtAabbMm volume, VInt3 velocity)
        {
            if (leaf.volume.Contain(volume))
            {
                return false;
            }
            volume.SignedExpand(velocity);
            update(leaf, volume);
            return true;
        }

        public bool update(Node leaf, DbvtAabbMm volume, VFixedPoint margin)
        {
            if (leaf.volume.Contain(volume))
            {
                return false;
            }
            VInt3 tmp = new VInt3(margin, margin, margin);
            volume.Expand(tmp);

            update(leaf, volume);
            return true;
        }

        public void remove(Node leaf)
        {
            removeleaf(this, leaf);
            deletenode(this, leaf);
            leaves--;
        }

        public static void collideTT(Node root0, Node root1, ICollide policy)
        {
            //DBVT_CHECKTYPE
            if (root0 != null && root1 != null)
            {
                List<sStkNN> stack = new List<sStkNN>(DOUBLE_STACKSIZE);
                stack.Add(new sStkNN(root0, root1));
                do
                {
                    sStkNN p = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                    if (p.a == p.b)
                    {
                        if (p.a.isinternal())
                        {
                            stack.Add(new sStkNN(p.a.childs[0], p.a.childs[0]));
                            stack.Add(new sStkNN(p.a.childs[1], p.a.childs[1]));
                            stack.Add(new sStkNN(p.a.childs[0], p.a.childs[1]));
                        }
                    }
                    else if (DbvtAabbMm.Intersect(p.a.volume, p.b.volume))
                    {
                        if (p.a.isinternal())
                        {
                            if (p.b.isinternal())
                            {
                                stack.Add(new sStkNN(p.a.childs[0], p.b.childs[0]));
                                stack.Add(new sStkNN(p.a.childs[1], p.b.childs[0]));
                                stack.Add(new sStkNN(p.a.childs[0], p.b.childs[1]));
                                stack.Add(new sStkNN(p.a.childs[1], p.b.childs[1]));
                            }
                            else {
                                stack.Add(new sStkNN(p.a.childs[0], p.b));
                                stack.Add(new sStkNN(p.a.childs[1], p.b));
                            }
                        }
                        else {
                            if (p.b.isinternal())
                            {
                                stack.Add(new sStkNN(p.a, p.b.childs[0]));
                                stack.Add(new sStkNN(p.a, p.b.childs[1]));
                            }
                            else {
                                policy.Process(p.a, p.b);
                            }
                        }
                    }
                }
                while (stack.Count > 0);
            }
        }

        private static int indexof(Node node)
        {
            return (node.parent.childs[1] == node) ? 1 : 0;
        }

        private static DbvtAabbMm merge(DbvtAabbMm a, DbvtAabbMm b, DbvtAabbMm output)
        {
            DbvtAabbMm.Merge(a, b, output);
            return output;
        }

        private static void deletenode(Dbvt pdbvt, Node node)
        {
            //btAlignedFree(pdbvt->m_free);
            pdbvt.free = node;
        }

        private static void recursedeletenode(Dbvt pdbvt, Node node)
        {
            if (!node.isleaf())
            {
                recursedeletenode(pdbvt, node.childs[0]);
                recursedeletenode(pdbvt, node.childs[1]);
            }
            if (node == pdbvt.root)
            {
                pdbvt.root = null;
            }
            deletenode(pdbvt, node);
        }

        private static Node createnode(Dbvt pdbvt, Node parent, DbvtAabbMm volume, object data)
        {
            Node node;
            if (pdbvt.free != null)
            {
                node = pdbvt.free;
                pdbvt.free = null;
            }
            else {
                node = new Node();
            }
            node.parent = parent;
            node.volume.set(volume);
            node.data = data;
            node.childs[1] = null;
            return node;
        }

        private static void insertleaf(Dbvt pdbvt, Node root, Node leaf)
        {
            if (pdbvt.root == null)
            {
                pdbvt.root = leaf;
                leaf.parent = null;
            }
            else {
                if (!root.isleaf())
                {
                    do
                    {
                        if (DbvtAabbMm.Proximity(root.childs[0].volume, leaf.volume) <
                            DbvtAabbMm.Proximity(root.childs[1].volume, leaf.volume))
                        {
                            root = root.childs[0];
                        }
                        else {
                            root = root.childs[1];
                        }
                    }
                    while (!root.isleaf());
                }
                Node prev = root.parent;
                Node node = createnode(pdbvt, prev, merge(leaf.volume, root.volume, new DbvtAabbMm()), null);
                if (prev != null)
                {
                    prev.childs[indexof(root)] = node;
                    node.childs[0] = root;
                    root.parent = node;
                    node.childs[1] = leaf;
                    leaf.parent = node;
                    do
                    {
                        if (!prev.volume.Contain(node.volume))
                        {
                            DbvtAabbMm.Merge(prev.childs[0].volume, prev.childs[1].volume, prev.volume);
                        }
                        else {
                            break;
                        }
                        node = prev;
                    }
                    while (null != (prev = node.parent));
                }
                else {
                    node.childs[0] = root;
                    root.parent = node;
                    node.childs[1] = leaf;
                    leaf.parent = node;
                    pdbvt.root = node;
                }
            }
        }

        private static Node removeleaf(Dbvt pdbvt, Node leaf)
        {
            if (leaf == pdbvt.root)
            {
                pdbvt.root = null;
                return null;
            }
            else {
                Node parent = leaf.parent;
                Node prev = parent.parent;
                Node sibling = parent.childs[1 - indexof(leaf)];
                if (prev != null)
                {
                    prev.childs[indexof(parent)] = sibling;
                    sibling.parent = prev;
                    deletenode(pdbvt, parent);
                    while (prev != null)
                    {
                        DbvtAabbMm pb = prev.volume;
                        DbvtAabbMm.Merge(prev.childs[0].volume, prev.childs[1].volume, prev.volume);
                        if (pb != prev.volume)
                        {
                            prev = prev.parent;
                        }
                        else {
                            break;
                        }
                    }
                    return (prev != null ? prev : pdbvt.root);
                }
                else {
                    pdbvt.root = sibling;
                    sibling.parent = null;
                    deletenode(pdbvt, parent);
                    return pdbvt.root;
                }
            }
        }

        private static Node sort(Node n, ref Node r)
        {
            Node p = n.parent;
            if (p != null && p.id > n.id)
            {
                int i = indexof(n);
                int j = 1 - i;
                Node s = p.childs[j];
                Node q = p.parent;
                if (q != null)
                {
                    q.childs[indexof(p)] = n;
                }
                else {
                    r = n;
                }
                s.parent = n;
                p.parent = n;
                n.parent = q;
                p.childs[0] = n.childs[0];
                p.childs[1] = n.childs[1];
                n.childs[0].parent = p;
                n.childs[1].parent = p;
                n.childs[i] = p;
                n.childs[j] = s;

                DbvtAabbMm.swap(p.volume, n.volume);
                return p;
            }
            return n;
        }

        public class Node
        {
            private static int NextID;
            public DbvtAabbMm volume = new DbvtAabbMm();
            public Node parent;
            public Node[] childs = new Node[2];
            public object data;

            private int _id = NextID++;
            public int id
            {
                get { return _id; }
            }

            public bool isleaf()
            {
                return childs[1] == null;
            }

            public bool isinternal()
            {
                return !isleaf();
            }
        }

        /** Stack element */
        public class sStkNN
        {
            public Node a;
            public Node b;

            public sStkNN(Node na, Node nb)
            {
                a = na;
                b = nb;
            }
        }

        public class sStkNP
        {
            public Node node;
            public int mask;

            public sStkNP(Node n, int m)
            {
                node = n;
                mask = m;
            }
        }

        public class sStkNPS
        {
            public Node node;
            public int mask;
            public float value;

            public sStkNPS()
            {
            }

            public sStkNPS(Node n, int m, float v)
            {
                node = n;
                mask = m;
                value = v;
            }

            public void set(sStkNPS o)
            {
                node = o.node;
                mask = o.mask;
                value = o.value;
            }
        }

        public class sStkCLN
        {
            public Node node;
            public Node parent;

            public sStkCLN(Node n, Node p)
            {
                node = n;
                parent = p;
            }
        }

        public class ICollide
        {
            public void Process(Node n1, Node n2)
            {
            }

            public void Process(Node n)
            {
            }

            public void Process(Node n, float f)
            {
                Process(n);
            }

            public bool Descent(Node n)
            {
                return true;
            }

            public bool AllLeaves(Node n)
            {
                return true;
            }
        }
    }
}
