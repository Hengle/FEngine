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
                        node = node.childs[(opath >> bit) & 1];
                        bit = (bit + 1) & (/*sizeof(unsigned)*/4 * 8 - 1);
                    }
                    update(node);
                    ++opath;
                }
                while (--passes != 0);
            }
        }

        public Node insert(DbvtAabbMm box, DbvtProxy data)
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

        public static void collideTT(Node root0, Node root1, Dispatcher dispatcher, ICollide policy)
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
                                if (!dispatcher.needsCollision(p.a.data.collisionFilterGroup, p.a.data.collisionFilterMask, p.b.data.collisionFilterGroup, p.b.data.collisionFilterMask))
                                    continue;
                                policy.Process(p.a, p.b);
                            }
                        }
                    }
                }
                while (stack.Count > 0);
            }
        }

        public void collideTV(Node root, Dispatcher dispatcher, DbvtAabbMm volume, short collisionFilterGroup, short collisionFilterMask, ICollide policy)
        {
            if (root != null)
            {
                List <Node> stack = new List<Node>(SIMPLE_STACKSIZE);
                stack.Add(root);
                do
                {
                    Node n = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                    if (DbvtAabbMm.Intersect(n.volume, volume))
                    {
                        if (n.isinternal())
                        {
                            stack.Add(n.childs[0]);
                            stack.Add(n.childs[1]);
                        }
                        else
                        {
                            if (!dispatcher.needsCollision(collisionFilterGroup, collisionFilterMask, n.data.collisionFilterGroup, n.data.collisionFilterMask))
                                continue;
                            policy.Process(n);
                        }
                    }
                } while (stack.Count > 0);
            }
        }

        public void rayTestInternal(Node root, Dispatcher dispatcher, VInt3 rayFrom, VInt3 rayTo, VInt3 rayDirectionInverse, uint[] signs, VFixedPoint lambdaMax, VInt3 aabbMin, VInt3 aabbMax, short collisionFilterGroup, short collisionFilterMask, ICollide policy)
        {
            if (root != null)
            {

                List<Node> stack = new List<Node>(DOUBLE_STACKSIZE);
                stack[0] = root;
                VInt3[] bounds = new VInt3[2];
                do
                {
                    Node node = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                    bounds[0] = node.volume.Mins() - aabbMax;
                    bounds[1] = node.volume.Maxs() - aabbMin;
                    VFixedPoint tmin = VFixedPoint.One, lambdaMin = VFixedPoint.Zero;
                    bool result1 = AabbUtils.RayAabb2(rayFrom, rayDirectionInverse, signs, bounds, ref tmin, lambdaMin, lambdaMax);
                    if (result1)
                    {
                        if (node.isinternal())
                        {
                            stack.Add(node.childs[0]);
                            stack.Add(node.childs[1]);
                        }
                        else
                        {
                            if (!dispatcher.needsCollision(collisionFilterGroup, collisionFilterMask, node.data.collisionFilterGroup, node.data.collisionFilterMask))
                                continue;
                            policy.Process(node);
                        }
                    }
                } while (stack.Count > 0);
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

        private static Node createnode(Dbvt pdbvt, Node parent, DbvtAabbMm volume, DbvtProxy data)
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

        public class Node
        {
            private static int NextID;
            public DbvtAabbMm volume = new DbvtAabbMm();
            public Node parent;
            public Node[] childs = new Node[2];
            public DbvtProxy data;

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
            public virtual void Process(Node n1, Node n2)
            {
            }

            public virtual void Process(Node n)
            {
            }

            public void Process(Node n, VFixedPoint f)
            {
                Process(n);
            }

            public virtual bool Descent(Node n)
            {
                return true;
            }

            public virtual bool AllLeaves(Node n)
            {
                return true;
            }
        }
    }
}
