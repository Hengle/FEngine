using MobaGame.FixedMath;
using System.Collections.Generic;
using System.Text;

namespace MobaGame.Collision
{
    class DynamicAABBTreeNode
    {
        public DynamicAABBTreeNode left;
        public DynamicAABBTreeNode right;
        public DynamicAABBTreeNode parent;
        public int height;
        public AABB aabb;

        public bool isLeaf()
        {
            return this.left == null;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("DynamicAABBTreeNode[AABB=").Append(this.aabb.ToString())
              .Append("|Height=").Append(this.height)
              .Append("]");
            return sb.ToString();
        }
    }
}
