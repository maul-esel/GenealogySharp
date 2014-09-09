using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TGC
{
	public class TreeGraphControl : Control
	{
		public TreeGraphControl()
		{
			SelectedTreeNodeBackground = TreeNodeBackground = new SolidBrush(Color.White);
			TreeNodeBorderPen = new Pen(Color.Blue, 2);
			SelectedTreeNodeBorderPen = new Pen(Color.DarkBlue, 3);
			SelectedTreeNodeTextBrush = TreeNodeTextBrush = new SolidBrush(Color.Blue);
			ConnectionLinePen = new Pen(Color.Red, 2);
		}

		public TreeGraphControl(ITreeNode root)
			: this()
		{
			rootNode = root;
		}

		private ITreeNode rootNode;
		public virtual ITreeNode RootNode {
			get { return rootNode; }
			set {
				if (rootNode != null) {
					rootNode.DescendantsChanged -= onRootDescendantsChanged;
					rootNode.VisibilityChanged -= onRootDescendantsChanged;
				}

				rootNode = value;
				if (rootNode != null) {
					rootNode.DescendantsChanged += onRootDescendantsChanged;
					rootNode.VisibilityChanged += onRootDescendantsChanged;
				}

				SelectedNode = null;
				InvalidateLayout();
			}
		}

		private bool selectable = true;
		public bool Selectable {
			get { return selectable; }
			set {
				if (!value)
					SelectedNode = null;
				selectable = value;
			}
		}

		private ITreeNode selectedNode;
		public virtual ITreeNode SelectedNode {
			get { return selectedNode; }
			set {
				if (selectedNode != value) {
					selectedNode = value;
					Refresh();
					OnSelectedNodeChanged();
				}
			}
		}

		public virtual event EventHandler SelectedNodeChanged;

		#region styling attributes
		public Brush TreeNodeBackground {
			get;
			set;
		}

		public Color TreeNodeBackgroundColor {
			get { return (TreeNodeBackground is SolidBrush) ? (TreeNodeBackground as SolidBrush).Color : Color.Empty; }
			set { TreeNodeBackground = new SolidBrush(value); }
		}

		public Brush SelectedTreeNodeBackground {
			get;
			set;
		}

		public Color SelectedTreeNodeBackgroundColor {
			get { return (SelectedTreeNodeBackground is SolidBrush) ? (SelectedTreeNodeBackground as SolidBrush).Color : Color.Empty; }
			set { SelectedTreeNodeBackground = new SolidBrush(value); }
		}

		public Pen TreeNodeBorderPen {
			get;
			set;
		}

		public Color TreeNodeBorderColor {
			get { return TreeNodeBorderPen.Color; }
			set { TreeNodeBorderPen.Color = value; }
		}

		public float TreeNodeBorderWidth {
			get { return TreeNodeBorderPen.Width; }
			set { TreeNodeBorderPen.Width = value; }
		}

		public Pen SelectedTreeNodeBorderPen {
			get;
			set;
		}

		public Color SelectedTreeNodeBorderColor {
			get { return SelectedTreeNodeBorderPen.Color; }
			set { SelectedTreeNodeBorderPen.Color = value; }
		}

		public float SelectedTreeNodeBorderWidth {
			get { return SelectedTreeNodeBorderPen.Width; }
			set { SelectedTreeNodeBorderPen.Width = value; }
		}

		public Brush TreeNodeTextBrush {
			get;
			set;
		}

		public Color TreeNodeColor {
			get { return (TreeNodeTextBrush is SolidBrush) ? (TreeNodeTextBrush as SolidBrush).Color : Color.Empty; }
			set { TreeNodeTextBrush = new SolidBrush(value); }
		}

		public Brush SelectedTreeNodeTextBrush {
			get;
			set;
		}

		public Color SelectedTreeNodeColor {
			get { return (SelectedTreeNodeTextBrush is SolidBrush) ? (SelectedTreeNodeTextBrush as SolidBrush).Color : Color.Empty; }
			set { SelectedTreeNodeTextBrush = new SolidBrush(value); }
		}

		public Pen ConnectionLinePen {
			get;
			set;
		}

		public Color ConnectionLineColor {
			get { return ConnectionLinePen.Color; }
			set { ConnectionLinePen.Color = value; }
		}

		public float ConnectionLineWidth {
			get { return ConnectionLinePen.Width; }
			set { ConnectionLinePen.Width = value; }
		}
		#endregion

		protected bool isLayoutValid = false;

		protected DisplayGrid grid = new DisplayGrid();

		protected readonly Dictionary<ITreeNode, DisplayGrid.Cell> positions = new Dictionary<ITreeNode, DisplayGrid.Cell>();

		protected virtual void onRootDescendantsChanged(object sender, EventArgs e)
		{
			InvalidateLayout();
			Refresh();
		}

		protected virtual void OnSelectedNodeChanged()
		{
			if (SelectedNodeChanged != null)
				SelectedNodeChanged(this, new EventArgs());
		}

		#region layout
		public virtual void DoLayout()
		{
			if (rootNode != null)
				PositionNode(rootNode, grid, 1, 1);
			isLayoutValid = true;
		}

		public virtual void InvalidateLayout()
		{
			isLayoutValid = false;
			grid = new DisplayGrid();
			positions.Clear();
		}

		protected virtual void PositionNode(ITreeNode node, DisplayGrid grid, int line, int minCol)
		{
			if (!node.Visible)
				return;

			int column;
			var visibleChildren = node.ChildNodes.Where(child => child.Visible);
			if (visibleChildren.Count() > 0) {
				int minColumn = Math.Max(grid.MaxColumnInLine(line) + 1, minCol);
				int i = 0;
				foreach (ITreeNode child in visibleChildren)
					PositionNode(child, grid, line + 1, minColumn - (visibleChildren.Count() / 2) + i++);
				var childCols = visibleChildren.Select(child => positions[child].Column);
				column = Math.Max((childCols.Min() + childCols.Max()) / 2, minColumn);
			} else
				column = new[] { grid.MaxColumnInLine(line) + 1, minCol }.Max();

			positions.Add(node, grid.Reserve(column, line, node));
		}
		#endregion

		#region painting
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (rootNode == null)
				return;

			if (!isLayoutValid)
				DoLayout();

			calculateDimensions(grid);
			foreach (KeyValuePair<ITreeNode, DisplayGrid.Cell> pair in positions)
				paintTreeNode(e.Graphics, pair.Key, pair.Value);
		}

		protected virtual void paintTreeNode(Graphics g, ITreeNode node, DisplayGrid.Cell cell)
		{
			StringFormat format = new StringFormat();
			format.Alignment = format.LineAlignment = StringAlignment.Center;

			RectangleF rect = getCell(cell);
			g.FillRectangle(
				node == SelectedNode ? SelectedTreeNodeBackground : TreeNodeBackground,
				rect
			);
			g.DrawRectangle(
				node == SelectedNode ? SelectedTreeNodeBorderPen : TreeNodeBorderPen,
				rect.X, rect.Y, rect.Width, rect.Height
			);
			g.DrawString(
				node.Text, Font,
				node == SelectedNode ? SelectedTreeNodeTextBrush : TreeNodeTextBrush,
				rect, format
			);

			var visibleChildren = node.ChildNodes.Where(child => child.Visible);
			if (visibleChildren.Count() > 0) {
				PointF start = new PointF(rect.X + rect.Width / 2, rect.Bottom);
				PointF second = new PointF(start.X, rect.Bottom + lineMargin / 2);
				g.DrawLine(ConnectionLinePen, start, second);

				foreach (ITreeNode child in visibleChildren) {
					PointF childPosition = getCell(positions[child]).Location;
					PointF third = new PointF(childPosition.X + columnWidth / 2, second.Y);
					PointF last = new PointF(third.X, childPosition.Y);

					g.DrawLine(ConnectionLinePen, second, third);
					g.DrawLine(ConnectionLinePen, third, last);
				}
			}
		}

		protected virtual RectangleF getCell(DisplayGrid.Cell cell)
		{
			return new RectangleF(
				5 + (cell.Column - 1) * (columnWidth + columnMargin),
				5 + (cell.Line - 1) * (lineHeight + lineMargin),
				columnWidth,
				lineHeight
			);
		}

		#region dimensions
		protected static readonly float marginColRatio = 5;
		protected static readonly float marginLineRatio = 2;

		protected float columnWidth;
		protected float lineHeight;

		protected float columnMargin;
		protected float lineMargin;

		protected void calculateDimensions(DisplayGrid grid)
		{
			// formula: width = (maxCol * colWidth) + (maxCol - 1) * colMargin
			columnWidth = (ClientSize.Width - 10) / (grid.MaxColumn + ((grid.MaxColumn - 1) * 1 / marginColRatio));
			lineHeight = (ClientSize.Height - 10) / (grid.MaxLine + ((grid.MaxLine - 1) * 1 / marginLineRatio));

			columnMargin = columnWidth / marginColRatio;
			lineMargin = lineHeight / marginLineRatio;
		}
		#endregion
		#endregion

		public ITreeNode HitTest(PointF point)
		{
			return positions.Where(pair => getCell(pair.Value).Contains(point)).Select(pair => pair.Key).FirstOrDefault();
		}

		protected override void OnResize(System.EventArgs e)
		{
			base.OnResize(e);
			Refresh();
		}

		protected override void OnMouseClick(MouseEventArgs e)
		{
			ITreeNode node = HitTest(new PointF(e.Location.X, e.Location.Y));
			if (node != null) {
				if (e.Button == MouseButtons.Left && Selectable)
					SelectedNode = node;
			} else
				SelectedNode = null;
			base.OnMouseClick(e);
		}

		private static readonly Keys[] directionKeys = new Keys[] { Keys.Up, Keys.Down, Keys.Left, Keys.Right };
		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (SelectedNode != null)
				if (directionKeys.Contains(e.KeyCode)) {
					ITreeNode parent = positions.Keys.FirstOrDefault(node => node.ChildNodes.Contains(SelectedNode));
					switch (e.KeyCode) {
						case Keys.Up:
							if (parent != null)
								SelectedNode = parent;
							break;
						case Keys.Down:
							if (SelectedNode.ChildNodes.Where(child => child.Visible).Count() > 0)
								SelectedNode = SelectedNode.ChildNodes.First(child => child.Visible);
							break;
						case Keys.Left:
							if (parent != null && parent.ChildNodes.Where(child => child.Visible).Count() > 1) {
								ITreeNode sibling = parent.ChildNodes.Take(parent.ChildNodes.ToList().IndexOf(SelectedNode)).LastOrDefault(node => node.Visible);
								if (sibling != null)
									SelectedNode = sibling;
							}
							break;
						case Keys.Right:
							if (parent != null && parent.ChildNodes.Where(child => child.Visible).Count() > 1) {
								ITreeNode sibling = parent.ChildNodes.Skip(parent.ChildNodes.ToList().IndexOf(SelectedNode) + 1).FirstOrDefault(node => node.Visible);
								if (sibling != null)
									SelectedNode = sibling;
							}
							break;
					}
				}
			base.OnKeyUp(e);
		}

		protected override bool IsInputKey(Keys keyData)
		{
			return directionKeys.Contains(keyData) || base.IsInputKey(keyData);
		}
	}
}