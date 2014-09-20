using System;
using System.Collections.Generic;
using System.Linq;

namespace TGC
{
	public class CompactTreeLayout : ITreeLayout
	{
		public void Layout(VisualTreeNode root)
		{
			PositionNode(root, 0, 0);
			maxColumns.Clear();
		}

		private readonly Dictionary<int, float> maxColumns = new Dictionary<int, float>();
		protected virtual float maxColumn(int y)
		{
			if (maxColumns.ContainsKey(y))
				return maxColumns[y];
			return -1f;
		}

		protected virtual bool isLeafNode(VisualTreeNode node)
		{
			return node.Children.Count(child => child.Node.Visible) == 0;
		}

		protected virtual void PositionNode(VisualTreeNode node, int depth, float minCol)
		{
			if (!node.Node.Visible)
				return;

			node.Y = depth;
			float minX = Math.Max(maxColumn(depth) + 1f, minCol);

			if (isLeafNode(node))
				node.X = minX;
			else {
				var visibleChildren = node.Children.Where(child => child.Node.Visible);
				int i = 0;
				foreach (VisualTreeNode child in visibleChildren)
					PositionNode(child, depth + 1, minX - (visibleChildren.Count() / 2f) + i++);

				node.X = Math.Max(
					(visibleChildren.Min(child => child.X) + visibleChildren.Max(child => child.X)) / 2f,
					minX
				);
			}

			if (maxColumn(depth) < node.X)
				maxColumns[depth] = node.X;
		}
	}
}