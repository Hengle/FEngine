using MobaGame.FixedMath;
using System.Collections.Generic;
using System;

namespace MobaGame.Collision
{
    class DynamicAABBTree<E, T>: AbstractBroadphaseDetector<E, T>, BroadphaseDetector<E, T> where E :Collidable<T> where T :Fixture
    {
        DynamicAABBTreeNode root;
        readonly Dictionary<BroadphaseKey, DynamicAABBTreeLeaf<E, T>> map;

        public DynamicAABBTree()
        {
            map = new Dictionary<BroadphaseKey, DynamicAABBTreeLeaf<E, T>>();
        }

        public override void add(E collidable, T fixture)
        {
            BroadphaseKey key = BroadphaseKey.get(collidable, fixture);

            DynamicAABBTreeLeaf<E, T> node = (DynamicAABBTreeLeaf<E, T>)this.map[key];
            if (node != null)
            {
                update(key, node, collidable, fixture);
            }
            else
            {
                add(key, collidable, fixture);
            }
        }

        void add(BroadphaseKey key, E collidable, T fixture)
        {
            VIntTransform tx = collidable.getTransform();
            AABB aabb = fixture.getShape().createAABB(tx);

            aabb.expand(this.expansion);

            DynamicAABBTreeLeaf<E, T> node = new DynamicAABBTreeLeaf<E, T>(collidable, fixture);
            node.aabb = aabb;

            map[key] = node;

            insert(node);
        }

        public override bool remove(E collidable, T fixture)
        {
            BroadphaseKey key = BroadphaseKey.get(collidable, fixture);

            DynamicAABBTreeLeaf<E, T> node = null;
            map.TryGetValue(key, out node);
            if (node != null)
            {
                map.Remove(key);
                remove(node);
                return true;
            }
            return false;
        }

        public override void update(E collidable, T fixture)
        {
            BroadphaseKey key = BroadphaseKey.get(collidable, fixture);

            DynamicAABBTreeLeaf<E, T> node = null;
            map.TryGetValue(key, out node);
            if (node != null)
            {
                update(key, node, collidable, fixture);
            }
            else
            {
                add(key, collidable, fixture);
            }
        }

        void update(BroadphaseKey key, DynamicAABBTreeLeaf<E, T> node, E collidable, T fixture)
        {
            VIntTransform tx = collidable.getTransform();

            AABB aabb = fixture.getShape().createAABB(tx);
            if (node.aabb.contains(aabb))
            {
                return;
            }
            aabb.expand(this.expansion);

            remove(node);

            node.aabb = aabb;

            insert(node);
        }

        public override AABB getAABB(E collidable, T fixture)
        {
            BroadphaseKey key = BroadphaseKey.get(collidable, fixture);
            DynamicAABBTreeLeaf<E, T> node = null;
            map.TryGetValue(key, out node);
            if (node != null)
            {
                return node.aabb;
            }
            return fixture.getShape().createAABB(collidable.getTransform());
        }

        public override bool contains(E collidable)
        {
            int size = collidable.getFixtureCount();
            bool result = true;
            for (int i = 0; i < size; i++)
            {
                T fixture = collidable.getFixture(i);
                BroadphaseKey key = BroadphaseKey.get(collidable, fixture);
                result &= this.map.ContainsKey(key);
            }
            return result;
        }

        public override bool contains(E collidable, T fixture)
        {
            BroadphaseKey key = BroadphaseKey.get(collidable, fixture);
            return this.map.ContainsKey(key);
        }

        public override void clear()
        {
            this.map.Clear();
            this.root = null;
        }

        public override int size()
        {
            return this.map.Count;
        }

        public override List<BroadphasePair<E, T>> detect(BroadphaseFilter<E, T> filter)
        {
            int size = this.map.Count;
            foreach (DynamicAABBTreeLeaf<E, T> node in map.Values)
            {
                node.tested = false;
            }
            int eSize = Collisions.getEstimatedCollisionPairs(size);
            List<BroadphasePair<E, T>> pairs = new List<BroadphasePair<E, T>>(eSize);
            foreach (DynamicAABBTreeLeaf<E, T> node in map.Values)
            {
                detectNonRecursive(node, this.root, filter, pairs);

                node.tested = true;
            }
            return pairs;
        }

        public override List<BroadphaseItem<E, T>> detect(AABB aabb, BroadphaseFilter<E, T> filter)
        {
            return detectNonRecursive(aabb, this.root, filter);
        }

        public override List<BroadphaseItem<E, T>> raycast(Ray ray, VFixedPoint length, BroadphaseFilter<E, T> filter)
        {
            if (map.Count == 0)
            {
                return new List<BroadphaseItem<E, T>>();
            }
            VInt3 s = ray.getStart();
            VInt3 d = ray.getDirectionVector();

            VFixedPoint l = length;
            if (length <= VFixedPoint.Zero)
            {
                l = VFixedPoint.MaxValue;
            }
            VFixedPoint x1 = s.x;
            VFixedPoint x2 = s.x + d.x * l;
            VFixedPoint y1 = s.y;
            VFixedPoint y2 = s.y + d.y * l;
            VFixedPoint z1 = s.z;
            VFixedPoint z2 = s.z + d.z * l;

            VInt3 min = new VInt3(
              FMath.Min(x1, x2),
              FMath.Min(y1, y2),
              FMath.Min(z1, z2));
            VInt3 max = new VInt3(
              FMath.Max(x1, x2),
              FMath.Max(y1, y2),
              FMath.Max(z1, z2));

            AABB aabb = new AABB(min, max);

            VFixedPoint invDx = VFixedPoint.One / d.x;
            VFixedPoint invDy = VFixedPoint.One / d.y;
            VFixedPoint invDz = VFixedPoint.One / d.z;
            DynamicAABBTreeNode node = this.root;

            int eSize = Collisions.getEstimatedRaycastCollisions(this.map.Count);
            List<BroadphaseItem<E, T>> list = new List<BroadphaseItem<E, T>>(eSize);
            while (node != null)
            {
                if (aabb.overlaps(node.aabb))
                {
                    if (node.left != null)
                    {
                        node = node.left;
                        continue;
                    }
                    if (raycast(s, l, invDx, invDy, invDz, node.aabb))
                    {
                        DynamicAABBTreeLeaf<E, T> leaf = (DynamicAABBTreeLeaf<E, T>)node;
                        if (filter.isAllowed(ray, length, leaf.collidable, leaf.fixture))
                        {
                            list.Add(new BroadphaseItem<E, T>(leaf.collidable, leaf.fixture));
                        }
                    }
                }
                bool nextNodeFound = false;
                while (node.parent != null)
                {
                    if (node == node.parent.left)
                    {
                        node = node.parent.right;
                        nextNodeFound = true;
                        break;
                    }
                    node = node.parent;
                }
                if (!nextNodeFound)
                {
                    break;
                }
            }
            return list;
        }

        public override void shift(VInt3 shift)
        {
            DynamicAABBTreeNode node = this.root;
            while (node != null)
            {
                if (node.left != null)
                {
                    node = node.left;
                }
                else if (node.right != null)
                {
                    node.aabb.translate(shift);
                    node = node.right;
                }
                else
                {
                    node.aabb.translate(shift);
                    bool nextNodeFound = false;
                    while (node.parent != null)
                    {
                        if ((node == node.parent.left) &&
                          (node.parent.right != null))
                        {
                            node.parent.aabb.translate(shift);
                            node = node.parent.right;
                            nextNodeFound = true;
                            break;
                        }
                        node = node.parent;
                    }
                    if (!nextNodeFound)
                    {
                        break;
                    }
                }
            }
        }

        void detect(DynamicAABBTreeLeaf<E, T> node, DynamicAABBTreeNode root, BroadphaseFilter<E, T> filter, List<BroadphasePair<E, T>> pairs)
        {
            if (node.aabb.overlaps(root.aabb))
            {
                if (root.left == null)
                {
                    DynamicAABBTreeLeaf<E, T> leaf = (DynamicAABBTreeLeaf<E, T>)root;
                    if ((!leaf.tested) && !leaf.collidable.Equals(node.collidable))
                    {
                        if (filter.isAllowed(node.collidable, node.fixture, leaf.collidable, leaf.fixture))
                        {
                            BroadphasePair<E, T> pair = new BroadphasePair<E, T>(
                              node.collidable,
                              node.fixture,
                              leaf.collidable,
                              leaf.fixture);

                            pairs.Add(pair);
                        }
                    }
                    return;
                }
                if (root.left != null)
                {
                    detect(node, root.left, filter, pairs);
                }
                if (root.right != null)
                {
                    detect(node, root.right, filter, pairs);
                }
            }
        }

        void detectNonRecursive(DynamicAABBTreeLeaf<E, T> node, DynamicAABBTreeNode root, BroadphaseFilter<E, T> filter, List<BroadphasePair<E, T>> pairs)
        {
            DynamicAABBTreeNode test = root;
            while (test != null)
            {
                if (test.aabb.overlaps(node.aabb))
                {
                    if (test.left != null)
                    {
                        test = test.left;
                        continue;
                    }
                    DynamicAABBTreeLeaf<E, T> leaf = (DynamicAABBTreeLeaf<E, T>)test;
                    if ((!leaf.tested) && !leaf.collidable.Equals(node.collidable))
                    {
                        if (filter.isAllowed(node.collidable, node.fixture, leaf.collidable, leaf.fixture))
                        {
                            BroadphasePair<E, T> pair = new BroadphasePair<E, T>(
                              node.collidable,
                              node.fixture,
                              leaf.collidable,
                              leaf.fixture);

                            pairs.Add(pair);
                        }
                    }
                }
                bool nextNodeFound = false;
                while (test.parent != null)
                {
                    if (test == test.parent.left)
                    {
                        test = test.parent.right;
                        nextNodeFound = true;
                        break;
                    }
                    test = test.parent;
                }
                if (!nextNodeFound)
                {
                    break;
                }
            }
        }

        void detect(AABB aabb, DynamicAABBTreeNode node, BroadphaseFilter<E, T> filter, List<BroadphaseItem<E, T>> list)
        {
            if (aabb.overlaps(node.aabb))
            {
                if (node.left == null)
                {
                    DynamicAABBTreeLeaf<E, T> leaf = (DynamicAABBTreeLeaf<E, T>)node;
                    if (filter.isAllowed(aabb, leaf.collidable, leaf.fixture))
                    {
                        list.Add(new BroadphaseItem<E, T>(leaf.collidable, leaf.fixture));
                    }
                    return;
                }
                if (node.left != null)
                {
                    detect(aabb, node.left, filter, list);
                }
                if (node.right != null)
                {
                    detect(aabb, node.right, filter, list);
                }
            }
        }

        List<BroadphaseItem<E, T>> detectNonRecursive(AABB aabb, DynamicAABBTreeNode node, BroadphaseFilter<E, T> filter)
        {
            int eSize = Collisions.getEstimatedCollisionsPerObject();
            List<BroadphaseItem<E, T>> list = new List<BroadphaseItem<E, T>>(eSize);
            while (node != null)
            {
                if (aabb.overlaps(node.aabb))
                {
                    if (node.left != null)
                    {
                        node = node.left;
                        continue;
                    }
                    DynamicAABBTreeLeaf<E, T> leaf = (DynamicAABBTreeLeaf<E, T>)node;
                    if (filter.isAllowed(aabb, leaf.collidable, leaf.fixture))
                    {
                        list.Add(new BroadphaseItem<E, T>(leaf.collidable, leaf.fixture));
                    }
                }
                bool nextNodeFound = false;
                while (node.parent != null)
                {
                    if (node == node.parent.left)
                    {
                        node = node.parent.right;
                        nextNodeFound = true;
                        break;
                    }
                    node = node.parent;
                }
                if (!nextNodeFound)
                {
                    break;
                }
            }
            return list;
        }

        void insert(DynamicAABBTreeNode item)
        {
            if (this.root == null)
            {
                this.root = item;

                return;
            }
            AABB itemAABB = item.aabb;

            DynamicAABBTreeNode node = this.root;
            while (!node.isLeaf())
            {
                AABB aabb = node.aabb;

                VFixedPoint perimeter = aabb.getPerimeter();

                AABB union = aabb.getUnion(itemAABB);

                VFixedPoint unionPerimeter = union.getPerimeter();

                VFixedPoint cost = unionPerimeter * 2;

                VFixedPoint descendCost = (unionPerimeter - perimeter) * 2;

                DynamicAABBTreeNode left = node.left;
                DynamicAABBTreeNode right = node.right;

                VFixedPoint costl = VFixedPoint.Zero;
                if (left.isLeaf())
                {
                    AABB u = left.aabb.getUnion(itemAABB);
                    costl = u.getPerimeter() + descendCost;
                }
                else
                {
                    AABB u = left.aabb.getUnion(itemAABB);
                    VFixedPoint oldPerimeter = left.aabb.getPerimeter();
                    VFixedPoint newPerimeter = u.getPerimeter();
                    costl = newPerimeter - oldPerimeter + descendCost;
                }
                VFixedPoint costr = VFixedPoint.Zero;
                if (right.isLeaf())
                {
                    AABB u = right.aabb.getUnion(itemAABB);
                    costr = u.getPerimeter() + descendCost;
                }
                else
                {
                    AABB u = right.aabb.getUnion(itemAABB);
                    VFixedPoint oldPerimeter = right.aabb.getPerimeter();
                    VFixedPoint newPerimeter = u.getPerimeter();
                    costr = newPerimeter - oldPerimeter + descendCost;
                }
                if ((cost < costl) && (cost < costr))
                {
                    break;
                }
                if (costl < costr)
                {
                    node = left;
                }
                else {
                    node = right;
                }
            }
            DynamicAABBTreeNode parent = node.parent;
            DynamicAABBTreeNode newParent = new DynamicAABBTreeNode();
            newParent.parent = node.parent;
            newParent.aabb = node.aabb.getUnion(itemAABB);
            node.height += 1;
            if (parent != null)
            {
                if (parent.left == node)
                {
                    parent.left = newParent;
                }
                else {
                    parent.right = newParent;
                }
                newParent.left = node;
                newParent.right = item;
                node.parent = newParent;
                item.parent = newParent;
            }
            else
            {
                newParent.left = node;
                newParent.right = item;
                node.parent = newParent;
                item.parent = newParent;
                this.root = newParent;
            }
            node = item.parent;
            while (node != null)
            {
                node = balance(node);

                DynamicAABBTreeNode left = node.left;
                DynamicAABBTreeNode right = node.right;

                node.height = (1 + Math.Max(left.height, right.height));
                node.aabb = left.aabb.getUnion(right.aabb);

                node = node.parent;
            }
        }

        void remove(DynamicAABBTreeNode node)
        {
            if (this.root == null)
            {
                return;
            }
            if (node == this.root)
            {
                this.root = null;

                return;
            }
            DynamicAABBTreeNode parent = node.parent;
            DynamicAABBTreeNode grandparent = parent.parent;
            DynamicAABBTreeNode other = null;
            if (parent.left == node)
            {
                other = parent.right;
            }
            else {
                other = parent.left;
            }
            if (grandparent != null)
            {
                if (grandparent.left == parent)
                {
                    grandparent.left = other;
                }
                else {
                    grandparent.right = other;
                }
                other.parent = grandparent;

                DynamicAABBTreeNode n = grandparent;
                while (n != null)
                {
                    n = balance(n);

                    DynamicAABBTreeNode left = n.left;
                    DynamicAABBTreeNode right = n.right;

                    n.height = (1 + Math.Max(left.height, right.height));
                    n.aabb = left.aabb.getUnion(right.aabb);

                    n = n.parent;
                }
            }
            else
            {
                this.root = other;

                other.parent = null;
            }
        }

        DynamicAABBTreeNode balance(DynamicAABBTreeNode node)
        {
            DynamicAABBTreeNode a = node;
            if ((a.isLeaf()) || (a.height < 2))
            {
                return a;
            }
            DynamicAABBTreeNode b = a.left;
            DynamicAABBTreeNode c = a.right;

            int balance = c.height - b.height;
            if (balance > 1)
            {
                DynamicAABBTreeNode f = c.left;
                DynamicAABBTreeNode g = c.right;

                c.left = a;
                c.parent = a.parent;
                a.parent = c;
                if (c.parent != null)
                {
                    if (c.parent.left == a)
                    {
                        c.parent.left = c;
                    }
                    else {
                        c.parent.right = c;
                    }
                }
                else {
                    this.root = c;
                }
                if (f.height > g.height)
                {
                    c.right = f;
                    a.right = g;
                    g.parent = a;

                    a.aabb = b.aabb.getUnion(g.aabb);
                    c.aabb = a.aabb.getUnion(f.aabb);

                    a.height = (1 + Math.Max(b.height, g.height));
                    c.height = (1 + Math.Max(a.height, f.height));
                }
                else
                {
                    c.right = g;
                    a.right = f;
                    f.parent = a;

                    a.aabb = b.aabb.getUnion(f.aabb);
                    c.aabb = a.aabb.getUnion(g.aabb);

                    a.height = (1 + Math.Max(b.height, f.height));
                    c.height = (1 + Math.Max(a.height, g.height));
                }
                return c;
            }
            if (balance < -1)
            {
                DynamicAABBTreeNode d = b.left;
                DynamicAABBTreeNode e = b.right;

                b.left = a;
                b.parent = a.parent;
                a.parent = b;
                if (b.parent != null)
                {
                    if (b.parent.left == a)
                    {
                        b.parent.left = b;
                    }
                    else {
                        b.parent.right = b;
                    }
                }
                else {
                    this.root = b;
                }
                if (d.height > e.height)
                {
                    b.right = d;
                    a.left = e;
                    e.parent = a;

                    a.aabb = c.aabb.getUnion(e.aabb);
                    b.aabb = a.aabb.getUnion(d.aabb);

                    a.height = (1 + Math.Max(c.height, e.height));
                    b.height = (1 + Math.Max(a.height, d.height));
                }
                else
                {
                    b.right = e;
                    a.left = d;
                    d.parent = a;

                    a.aabb = c.aabb.getUnion(d.aabb);
                    b.aabb = a.aabb.getUnion(e.aabb);

                    a.height = (1 + Math.Max(c.height, d.height));
                    b.height = (1 + Math.Max(a.height, e.height));
                }
                return b;
            }
            return a;
        }
    }
}
