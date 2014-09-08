using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TreeGraphControl
{
	public class TreeGraphControl : Control
	{
		public TreeGraphControl()
		{
		}

		public TreeGraphControl(ITreeNode root)
		{
			rootNode = root;
		}

		private ITreeNode rootNode;
		public virtual ITreeNode RootNode {
			get { return rootNode; }
			set {
				rootNode = value;
				if (rootNode != null)
					rootNode.DescendantsChanged += onRootDescendantsChanged;
				InvalidateLayout();
			}
		}

		protected bool isLayoutValid = false;

		protected DisplayGrid grid = new DisplayGrid();

		protected readonly Dictionary<ITreeNode, DisplayGrid.Cell> positions = new Dictionary<ITreeNode, DisplayGrid.Cell>();

		protected virtual void onRootDescendantsChanged(object sender, EventArgs e)
		{
			InvalidateLayout();
		}

		#region layout
		public virtual void DoLayout()
		{
			if (rootNode != null)
				PositionNode(rootNode, grid, 1);
			isLayoutValid = true;
		}

		public virtual void InvalidateLayout()
		{
			isLayoutValid = false;
			grid = new DisplayGrid();
			positions.Clear();
		}

		protected virtual void PositionNode(ITreeNode node, DisplayGrid grid, int line)
		{
			int column;

			if (node.ChildNodes.Length > 0) {
				foreach (ITreeNode child in node.ChildNodes)
					PositionNode(child, grid, line + 1);
				var childCols = node.ChildNodes.Select(child => positions[child].Column);
				column = (childCols.Min() + childCols.Max()) / 2;
			} else
				column = grid.MaxColumn + 1;

			positions.Add(node, grid.Reserve(column, line, node));
		}
		#endregion
	}
}