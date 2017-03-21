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
        public int leaves = 0;

        public void clear()
        {
            if (root != null)
            {
                recursedeletenode(this, root);
            }
        }

        public bool empty()
        {
            return (root == null);
        }

        public Node insert(DbvtAabbMm box, DbvtProxy data)
        {
            Node leaf = createnode(this, null, box, data);
            insertleaf(this, leaf);
            leaves++;
            return leaf;
        }

        public void remove(Node leaf)
        {
            removeleaf(this, leaf);
            deletenode(this, leaf);
        }

        public bool update(Node leaf, DbvtAabbMm volume)
        {
            if (leaf.volume.Contain(volume))
            {
                return false;
            }
            removeleaf(this, leaf);
            leaf.volume = volume;
            insertleaf(this, leaf);
            return true;
        }

        public static void collideTT(Node root0, Node root1, Dispatcher dispatcher, ICollide policy)
        {
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

        public void rayTestInternal(Node root, Dispatcher dispatcher, VInt3 rayFrom, VInt3 rayTo, VInt3 aabbMin, VInt3 aabbMax, short collisionFilterGroup, short collisionFilterMask, ICollide policy)
        {
            if (root != null)
            {

                List<Node> stack = new List<Node>(DOUBLE_STACKSIZE);
                stack.Add(root);
                VInt3[] bounds = new VInt3[2];
                do
                {
                    Node node = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                    VFixedPoint tmin = VFixedPoint.One, tmax = VFixedPoint.One;
                    bool result1 = AabbUtils.RayAabb2(rayFrom, rayTo, node.volume.Mins() - aabbMax, node.volume.Maxs() - aabbMin, ref tmin, ref tmax);
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
            node.childs[0] = null;
            node.childs[1] = null;
            node.data = null;
            node.parent = null;
            node.volume = null;
            node.height = -1;
            pdbvt.leaves--;
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
            Node node = new Node();
            node.parent = parent;
            node.volume = volume;
            node.data = data;
            node.height = 0;
            return node;
        }

        private static void insertleaf(Dbvt pdbvt, Node leaf)
        {
            if (pdbvt.root == null)
            {
                pdbvt.root = leaf;
                leaf.parent = null;
            }
            else
            {
                Node node = pdbvt.root;
                while (!node.isleaf())
                {
                    if (DbvtAabbMm.Proximity(node.childs[0].volume, leaf.volume) <
                        DbvtAabbMm.Proximity(node.childs[1].volume, leaf.volume))
                    {
                        node = node.childs[0];
                    }
                    else
                    {
                        node = node.childs[1];
                    }
                }
                Node sibling = node;
                Node oldParent = node.parent;
                Node newParent = createnode(pdbvt, oldParent, merge(leaf.volume, sibling.volume, new DbvtAabbMm()), null);
                if (oldParent != null)
                {
                    oldParent.childs[indexof(sibling)] = newParent;
                    newParent.childs[0] = sibling;
                    newParent.childs[1] = leaf;
                    sibling.parent = newParent;
                    leaf.parent = newParent;
                }
                else
                {
                    newParent.childs[0] = sibling;
                    newParent.childs[1] = leaf;
                    sibling.parent = newParent;
                    leaf.parent = newParent;
                    pdbvt.root = newParent;
                }

                node = leaf.parent;
                while(node != null)
                {
                    node = Balance(pdbvt, node);
                    Node child0 = node.childs[0];
                    Node child1 = node.childs[1];
                    node.height = Math.Max(child0.height, child1.height) + 1;
                    node.volume = merge(child0.volume, child1.volume, new DbvtAabbMm()); 
                    node = node.parent;
                }
            }
        }

        private static void removeleaf(Dbvt pdbvt, Node leaf)
        {
            if (leaf == pdbvt.root)
            {
                pdbvt.root = null;
                return;
            }
            else
            {
                Node parent = leaf.parent;
                Node grandParent = parent != null ? parent.parent : null;
                Node sibling = parent.childs[1 - indexof(leaf)];
                if(grandParent != null)
                {
                    grandParent.childs[indexof(parent)] = sibling;
                    sibling.parent = grandParent;
                    deletenode(pdbvt, parent);

                    Node node = grandParent;
                    while(node != null)
                    {
                        node = Balance(pdbvt, node);
                        Node child0 = node.childs[0];
                        Node child1 = node.childs[1];
                        node.height = Math.Max(child0.height, child1.height) + 1;
                        node.volume = merge(child0.volume, child1.volume, new DbvtAabbMm());
                        node = node.parent;
                    }
                }
                else
                {
                    pdbvt.root = sibling;
                    sibling.parent = null;
                    deletenode(pdbvt, parent);
                }
            }
        }

        private static Node Balance(Dbvt pdbvt, Node A)
        {
            if(A.isleaf() || A.height < 2)
            {
                return A;
            }

            Node B = A.childs[0];
            Node C = A.childs[1];

            int balance = C.height - B.height;
            
            if(balance > 1)
            {
                Node F = C.childs[0];
                Node G = C.childs[1];

                // Swap A and C
                C.childs[0] = A;
                C.parent = A.parent;
                // A's old parent should point to C
                if (A.parent != null)
                {
                    A.parent.childs[indexof(A)] = C;
                }
                else
                {
                    pdbvt.root = C;
                }
                A.parent = C;

                // Rotate
                if (F.height > G.height)
                {
                    C.childs[1] = F;
                    A.childs[1] = G;
                    G.parent = A;
                    A.volume = merge(B.volume, G.volume, new DbvtAabbMm());
                    C.volume = merge(A.volume, F.volume, new DbvtAabbMm());

                    A.height = 1 + Math.Max(B.height, G.height);
                    C.height = 1 + Math.Max(A.height, F.height);
                }
                else
                {
                    C.childs[1] = G;
                    A.childs[1] = F;
                    F.parent = A;
                    A.volume = merge(B.volume, F.volume, new DbvtAabbMm());
                    C.volume = merge(A.volume, G.volume, new DbvtAabbMm());

                    A.height = 1 + Math.Max(B.height, F.height);
                    C.height = 1 + Math.Max(A.height, G.height);
                }

                return C; 
            }

            if (balance < -1)
            {
                Node D = B.childs[0];
                Node E = B.childs[1];

                // Swap A andB 
                B.childs[0] = A;
                B.parent = A.parent;
                // A's old parent should point to B
                if (A.parent != null)
                {
                    A.parent.childs[indexof(A)] = B;
                }
                else
                {
                    pdbvt.root = B;
                }
                A.parent = B;

                // Rotate
                if (D.height > E.height)
                {
                    B.childs[1] = D;
                    A.childs[0] = E;
                    E.parent = A;
                    A.volume = merge(C.volume, E.volume, new DbvtAabbMm());
                    B.volume = merge(A.volume, D.volume, new DbvtAabbMm());

                    A.height = 1 + Math.Max(C.height, E.height);
                    B.height = 1 + Math.Max(A.height, D.height);
                }
                else
                {
                    B.childs[1] = E;
                    A.childs[0] = D;
                    D.parent = A;
                    A.volume = merge(C.volume, D.volume, new DbvtAabbMm());
                    B.volume = merge(A.volume, E.volume, new DbvtAabbMm());

                    A.height = 1 + Math.Max(C.height, D.height);
                    B.height = 1 + Math.Max(A.height, E.height);
                }

                return B;
            }

            return A;
        }

        public class Node
        {
            private static int NextID;
            public DbvtAabbMm volume = new DbvtAabbMm();
            public Node parent;
            public Node[] childs = new Node[2];
            public DbvtProxy data;
            public int height = 0;

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
