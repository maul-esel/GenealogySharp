using System;
using System.Collections.Generic;
using System.Linq;

namespace TGC
{
	public class CompactTreeLayout : ITreeLayout
	{
		public void Layout(VisualTreeNode root)
		{
			maxColumns.Clear();
			PositionNode(root, 0, 0);
		}

		private readonly Dictionary<int, int> maxColumns = new Dictionary<int, int>();
		protected virtual int maxColumn(int y)
		{
			if (maxColumns.ContainsKey(y))
				return maxColumns[y];
			return -1;
		}

		protected virtual bool isLeafNode(VisualTreeNode node)
		{
			return node.Children.Count(child => child.Node.Visible) > 0;
		}

		protected virtual void PositionNode(VisualTreeNode node, int depth, int minCol)
		{
			if (!node.Node.Visible)
				return;

			node.Y = depth;
			int minX = Math.Max(maxColumn(depth), minCol);

			if (isLeafNode(node))
				node.X = minX;
			else {
				var visibleChildren = node.Children.Where(child => child.Node.Visible);
				int i = 0;
				foreach (VisualTreeNode child in visibleChildren)
					PositionNode(child, depth + 1, minX - (visibleChildren.Count() / 2) + i++);

				node.X = Math.Max(
					(visibleChildren.Min(child => child.X) + visibleChildren.Max(child => child.X)) / 2,
					minX
				);
			}

			if (maxColumn(depth) < node.X)
				maxColumns[depth] = node.X;
		}
	}
}