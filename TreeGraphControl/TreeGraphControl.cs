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
			TreeLayout = new BuchheimTreeLayout();

			SelectedTreeNodeBackground = TreeNodeBackground = new SolidBrush(Color.White);
			TreeNodeBorderPen = new Pen(Color.Blue, 2);
			SelectedTreeNodeBorderPen = new Pen(Color.DarkBlue, 3);
			SelectedTreeNodeTextBrush = TreeNodeTextBrush = new SolidBrush(Color.Blue);
			ConnectionLinePen = new Pen(Color.Red, 2);
		}

		public TreeGraphControl(ITreeNode root)
			: this()
		{
			RootNode = root;
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
				Refresh();
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
		public bool layoutSuspended = false;
		private bool isLayoutValid = false;
		private VisualTreeNode currentLayout;

		public ITreeLayout TreeLayout {
			get;
			set;
		}

		public void SuspendLayoutAndPainting()
		{
			layoutSuspended = true;
		}

		public void ResumeLayoutAndPainting()
		{
			layoutSuspended = false;
		}

		public virtual void DoLayout()
		{
			if (layoutSuspended)
				throw new Exception("layout is suspended");

			if (RootNode == null)
				throw new Exception("root is null");

			TreeLayout.Layout(currentLayout = new VisualTreeNode(RootNode));
			isLayoutValid = true;
		}

		public virtual void InvalidateLayout()
		{
			isLayoutValid = false;
		}
		#endregion

		#region painting
		protected override void OnPaint(PaintEventArgs e)
		{
			if (layoutSuspended)
				return;

			base.OnPaint(e);
			if (RootNode == null)
				return;

			if (!isLayoutValid)
				DoLayout();

			calculateDimensions();
			paintTreeNode(e.Graphics, currentLayout);
		}

		protected virtual void paintTreeNode(Graphics g, VisualTreeNode node)
		{
			StringFormat format = new StringFormat();
			format.Alignment = format.LineAlignment = StringAlignment.Center;

			RectangleF rect = getCell(node.X, node.Y);
			g.FillRectangle(
				node.Node == SelectedNode ? SelectedTreeNodeBackground : TreeNodeBackground,
				rect
			);
			g.DrawRectangle(
				node.Node == SelectedNode ? SelectedTreeNodeBorderPen : TreeNodeBorderPen,
				rect.X, rect.Y, rect.Width, rect.Height
			);
			g.DrawString(
				node.Node.Text, Font,
				node.Node == SelectedNode ? SelectedTreeNodeTextBrush : TreeNodeTextBrush,
				rect, format
			);

			var visibleChildren = node.Children.Where(child => child.Node.Visible);
			if (visibleChildren.Count() > 0) {
				PointF start = new PointF(rect.X + rect.Width / 2, rect.Bottom);
				PointF second = new PointF(start.X, rect.Bottom + lineMargin / 2);
				g.DrawLine(ConnectionLinePen, start, second);

				foreach (VisualTreeNode child in visibleChildren) {
					PointF childPosition = getCell(child.X, child.Y).Location;
					PointF third = new PointF(childPosition.X + columnWidth / 2, second.Y);
					PointF last = new PointF(third.X, childPosition.Y);

					g.DrawLine(ConnectionLinePen, second, third);
					g.DrawLine(ConnectionLinePen, third, last);

					paintTreeNode(g, child);
				}
			}
		}

		protected virtual RectangleF getCell(int X, int Y)
		{
			return new RectangleF(
				5 + X * (columnWidth + columnMargin),
				5 + Y * (lineHeight + lineMargin),
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

		protected void calculateDimensions()
		{
			int maxColumn = 0, maxLine = 0;

			TreeSearch.Traverse(currentLayout, node => {
				if (node.X > maxColumn)
					maxColumn = node.X;
				if (node.Y > maxLine)
					maxLine = node.Y;
				return true;
			});

			// formula: width = (maxCol + 1) * colWidth + maxCol * colMargin
			columnWidth = (ClientSize.Width - 10) / ((maxColumn + 1) + (maxColumn / marginColRatio));
			lineHeight = (ClientSize.Height - 10) / ((maxLine + 1) + (maxLine / marginLineRatio));

			columnMargin = columnWidth / marginColRatio;
			lineMargin = lineHeight / marginLineRatio;
		}
		#endregion
		#endregion

		public ITreeNode HitTest(PointF point)
		{
			VisualTreeNode visualNode = TreeSearch.FindFirst(currentLayout, node => getCell(node.X, node.Y).Contains(point));
			if (visualNode != null)
				return visualNode.Node;
			return null;
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
					ITreeNode parent = GetParentNode(SelectedNode);
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

		public ITreeNode GetParentNode(ITreeNode child)
		{
			VisualTreeNode visualNode = TreeSearch.FindFirst(currentLayout, node => node.Node.ChildNodes.Contains(child));
			if (visualNode != null)
				return visualNode.Node;
			return null;
		}
	}
}