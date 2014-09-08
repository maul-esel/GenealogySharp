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
			TreeNodeBackground = new SolidBrush(Color.White);
			TreeNodeBorderPen = new Pen(Color.Blue, 2);
			TreeNodeTextBrush = new SolidBrush(Color.Blue);
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
				if (rootNode != null)
					rootNode.DescendantsChanged -= onRootDescendantsChanged;

				rootNode = value;
				if (rootNode != null)
					rootNode.DescendantsChanged += onRootDescendantsChanged;

				InvalidateLayout();
			}
		}

		#region styling attributes
		public Brush TreeNodeBackground {
			get;
			set;
		}

		public Color TreeNodeBackgroundColor {
			get { return (TreeNodeBackground is SolidBrush) ? (TreeNodeBackground as SolidBrush).Color : Color.Empty; }
			set { TreeNodeBackground = new SolidBrush(value); }
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

		public Brush TreeNodeTextBrush {
			get;
			set;
		}

		public Color TreeNodeColor {
			get { return (TreeNodeTextBrush is SolidBrush) ? (TreeNodeTextBrush as SolidBrush).Color : Color.Empty; }
			set { TreeNodeTextBrush = new SolidBrush(value); }
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
			g.FillRectangle(TreeNodeBackground, rect);
			g.DrawRectangle(TreeNodeBorderPen, rect.X, rect.Y, rect.Width, rect.Height);
			g.DrawString(node.Text, Font, TreeNodeTextBrush, rect, format);

			if (node.ChildNodes.Length > 0) {
				PointF start = new PointF(rect.X + rect.Width / 2, rect.Bottom);
				PointF second = new PointF(start.X, rect.Bottom + lineMargin / 2);
				g.DrawLine(ConnectionLinePen, start, second);

				foreach (ITreeNode child in node.ChildNodes) {
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
	}
}