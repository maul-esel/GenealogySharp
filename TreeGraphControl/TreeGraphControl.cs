using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TGC
{
	public class TreeGraphControl : ScrollableControl
	{
		public TreeGraphControl()
		{
			AutoScroll = true;
			TreeLayout = new BuchheimTreeLayout();

			BackColor = Color.White;

			SelectedTreeNodeBackground = TreeNodeBackground = Brushes.Ivory; //.White;
			SelectedTreeNodeTextBrush = TreeNodeTextBrush = Brushes.Black;

			ConnectionLinePen = new Pen(Color.Firebrick, 2);
			TreeNodeBorderPen = new Pen(Color.Firebrick, 2);
			SelectedTreeNodeBorderPen = new Pen(Color.DarkRed, 3);
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
			if (SelectedNode != null && currentLayout != null) {
				// scroll into view
				VisualTreeNode visualNode = TreeSearch.FindFirst(currentLayout, node => node.Node == SelectedNode);
				if (visualNode != null) {
					RectangleF View = new RectangleF(Math.Abs(AutoScrollPosition.X),
					                                 Math.Abs(AutoScrollPosition.Y),
					                                 ClientSize.Width - SystemInformation.VerticalScrollBarWidth,
					                                 ClientSize.Height - SystemInformation.HorizontalScrollBarHeight);
					RectangleF Node = new RectangleF(visualNode.X * (columnWidth + columnMargin),
					                               visualNode.Y * (lineHeight + lineMargin),
					                               columnWidth,
					                               lineHeight);
					if (!View.Contains(Node)) {
						Console.WriteLine(View.ToString() + " !>= " + Node.ToString());
						Point newScrollPosition = new Point(Math.Abs(AutoScrollPosition.X), Math.Abs(AutoScrollPosition.Y));
						if (View.Right < Node.Right)
							newScrollPosition.X += (int)(Node.Right - View.Right);
						else if (View.Left > Node.Left)
							newScrollPosition.X += (int)(Node.Left - View.Left);

						if (View.Bottom < Node.Bottom)
							newScrollPosition.Y += (int)(Node.Bottom - View.Bottom);
						else if (View.Top > Node.Top)
							newScrollPosition.Y += (int)(Node.Top - View.Top);
						AutoScrollPosition = newScrollPosition;
						Refresh();
					}
				}
			}
			if (SelectedNodeChanged != null)
				SelectedNodeChanged(this, new EventArgs());
		}

		#region layout
		public bool layoutSuspended = false;
		private bool isLayoutValid = false;
		private VisualTreeNode currentLayout;
		private float maxColumn, maxLine, minColumn, minLine;

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
			RetrieveLayoutDimensions(currentLayout);
			calculateMinTreeSize();

			isLayoutValid = true;
		}

		public virtual void RetrieveLayoutDimensions(VisualTreeNode root)
		{
			minColumn = maxColumn = root.X;
			minLine = maxLine = root.Y;

			TreeSearch.Traverse(root, node => {
				minColumn = Math.Min(minColumn, node.X);
				maxColumn = Math.Max(maxColumn, node.X);
				minLine = Math.Min(minLine, node.Y);
				maxLine = Math.Max(maxLine, node.Y);
				return true;
			});
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

			calculateDimensions(ClientSize);
			paintTreeNode(e.Graphics, currentLayout);
		}

		public void SaveToImage(Image im)
		{
			if (!isLayoutValid)
				DoLayout();

			calculateDimensions(im.Size);
			Graphics g = Graphics.FromImage(im);
			g.FillRectangle(new SolidBrush(BackColor), new RectangleF(0, 0, im.Size.Width, im.Size.Height));
			paintTreeNode(g, currentLayout);
		}

		protected virtual void paintTreeNode(Graphics g, VisualTreeNode node)
		{
			StringFormat format = new StringFormat();
			format.Alignment = format.LineAlignment = StringAlignment.Center;

			RectangleF rect = getCell(node);
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
					PointF childPosition = getCell(child).Location;

					PointF third = new PointF(childPosition.X + columnWidth / 2, second.Y);
					PointF last = new PointF(third.X, childPosition.Y);

					g.DrawLine(ConnectionLinePen, second, third);
					g.DrawLine(ConnectionLinePen, third, last);

					paintTreeNode(g, child);
				}
			}
		}

		protected virtual RectangleF getCell(VisualTreeNode node)
		{
			RectangleF rect = new RectangleF(
				5 + (node.X - minColumn) * (columnWidth + columnMargin),
				5 + (node.Y - minLine) * (lineHeight + lineMargin),
				columnWidth,
				lineHeight
			);
			rect.Offset(AutoScrollPosition);
			return rect;
		}

		#region dimensions
		protected static readonly float marginColRatio = 5;
		protected static readonly float marginLineRatio = 2;
		protected static readonly int minColMargin = 5;
		protected static readonly int minLineMargin = 20;

		protected Size minTreeSize = new Size();
		protected void calculateMinTreeSize()
		{
			Size minNodeSize = minimalNodeSize();
			float columns = maxColumn - minColumn,
				lines = maxLine - minLine;

			AutoScrollMinSize = minTreeSize = new Size(
				(int)(columns * (minColMargin + minNodeSize.Width) + minNodeSize.Width),
				(int)(lines * (minLineMargin + minNodeSize.Height) + minNodeSize.Height)
			);
		}

		protected float columnWidth;
		protected float lineHeight;

		protected float columnMargin;
		protected float lineMargin;

		protected void calculateDimensions(Size clientSize)
		{
			Size actualTreeSize = new Size(
				Math.Max(clientSize.Width, minTreeSize.Width),
				Math.Max(clientSize.Height, minTreeSize.Height)
			);

			float columns = maxColumn - minColumn,
				lines = maxLine - minLine;

			// formula: width - padding = (maxCol + 1) * colWidth + columns * colMargin
			columnWidth = (actualTreeSize.Width - 10) / ((columns + 1) + (columns / marginColRatio));
			lineHeight = (actualTreeSize.Height - 30) / ((lines + 1) + (lines / marginLineRatio));

			columnMargin = columnWidth / marginColRatio;
			lineMargin = lineHeight / marginLineRatio;

			if (columnMargin < minColMargin) {
				columnMargin = minColMargin;
				columnWidth = (actualTreeSize.Width - 10 - maxColumn * columnMargin) / (maxColumn + 1);
			}
			if (lineMargin < minLineMargin) {
				lineMargin = minLineMargin;
				lineHeight = (actualTreeSize.Height - 10 - maxLine * lineMargin) / (maxLine + 1);
			}
		}

		protected Size minimalNodeSize()
		{
			Size minTextSize = new Size();
			TreeSearch.Traverse(currentLayout, node => {
				Size nodeTextSize = TextRenderer.MeasureText(node.Node.Text, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.WordBreak);
				minTextSize = new Size(Math.Max(minTextSize.Width, nodeTextSize.Width), Math.Max(minTextSize.Height, nodeTextSize.Height));
				return true;
			});
			return new Size(minTextSize.Width + 5, minTextSize.Height + 5);
		}

		protected Size maximalNodeSize()
		{
			Size minSize = minimalNodeSize();
			return new Size(minSize.Width * 2, minSize.Height * 2);
		}
		#endregion
		#endregion

		public ITreeNode HitTest(PointF point)
		{
			if (currentLayout != null) {
				VisualTreeNode visualNode = TreeSearch.FindFirst(currentLayout, node => getCell(node).Contains(point));
				if (visualNode != null)
					return visualNode.Node;
			}
			return null;
		}

		protected override void OnResize(System.EventArgs e)
		{
			base.OnResize(e);
			Refresh();
		}

		protected override void OnScroll(ScrollEventArgs se)
		{
			base.OnScroll(se);
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