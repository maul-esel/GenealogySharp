using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using TGC;

namespace Genealogy.Inspector
{
	public class GenealogyTreeControl : TreeGraphControl
	{
		private Lineality lineality = Lineality.Cognatic;
		public Lineality Lineality {
			get { return lineality; }
			set {
 				lineality = value;
				OnLinealityChanged();
				removeDuplicates();
				selectedNode = menuNode = null;
			}
		}

		public override ITreeNode RootNode {
			get { return base.RootNode; }
			set {
				if (!(value is PersonNode))
					throw new ArgumentException();
				base.RootNode = value;
				removeDuplicates();
				selectedNode = menuNode = null;
			}
		}

		public event EventHandler LinealityChanged;

		private void OnLinealityChanged()
		{
			if (LinealityChanged != null)
				LinealityChanged(this, new EventArgs());
		}

		private ContextMenu nodeMenu = new ContextMenu();

		public GenealogyTreeControl()
		{
			nodeMenu.MenuItems.Add(new MenuItem("Make Root", (s, e) => {
				if (menuNode != null) {
					RootNode = menuNode;
					Refresh();
				}
			}));
			nodeMenu.MenuItems.Add(new MenuItem("Open Details", (s, e) => {
				if (menuNode != null)
					new PersonWindow(menuNode.Person).Show(FindForm().Owner);
			}));
			MouseClick += onClick;
			KeyUp += onKeyUp;
		}

		private void removeDuplicates()
		{
			if (RootNode == null)
				return;

			List<PersonNode> nodes = new List<PersonNode>();
			collectNodes(RootNode as PersonNode, nodes);

			var duplicatesToHide =
				from node in nodes
				group node by node.Person into nodeGroup
				where nodeGroup.Count() > 1
				where nodes.Exists(n => n.Visible && n.Person == nodeGroup.Key.Father)
					&& nodes.Exists(n => n.Visible && n.Person == nodeGroup.Key.Mother)
				select nodeGroup;
			foreach (var duplicate in duplicatesToHide) {
				var collapsibleParents = nodes.Where(node => node.Person == (Lineality == Lineality.Uterine ? duplicate.Key.Father : duplicate.Key.Mother));
				foreach (var parent in collapsibleParents)
					parent.ChildNodes.Cast<PersonNode>().First(child => child.Person == duplicate.Key).Hide();
			}
		}

		private void collectNodes(PersonNode node, List<PersonNode> list)
		{
			list.Add(node);
			foreach (PersonNode child in node.ChildNodes)
				collectNodes(child, list);
		}

		private PersonNode selectedNode = null;
		private PersonNode menuNode = null;

		private void onClick(object sender, MouseEventArgs e)
		{
			menuNode = null;
			PersonNode node = HitTest(new PointF(e.X, e.Y)) as PersonNode;
			if (node != null) {
				if (e.Button == MouseButtons.Right) {
					menuNode = node;
					nodeMenu.Show(this, e.Location);
				} else if (e.Button == MouseButtons.Left) {
					if (node != selectedNode) {
						selectedNode = node;
						Refresh();
					}
				}
			} else if (selectedNode != null) {
				selectedNode = null;
				Refresh();
			}
		}

		private void onKeyUp(object sender, KeyEventArgs e)
		{
			if (selectedNode != null) {
				List<PersonNode> nodes = new List<PersonNode>();
				collectNodes(RootNode as PersonNode, nodes);
				PersonNode parent = nodes.FirstOrDefault(node => node.ChildNodes.Contains(selectedNode));

				switch (e.KeyCode) {
					case Keys.Up:
						if (parent != null)
							selectedNode = parent;
						break;
					case Keys.Down:
						if (selectedNode.ChildNodes.Where(child => child.Visible).Count() > 0)
							selectedNode = selectedNode.ChildNodes.First(child => child.Visible) as PersonNode;
						break;
					case Keys.Left:
						if (parent != null && parent.ChildNodes.Where(child => child.Visible).Count() > 1) {
							ITreeNode sibling = parent.ChildNodes.Take(parent.ChildNodes.ToList().IndexOf(selectedNode)).LastOrDefault(node => node.Visible);
							if (sibling != null)
								selectedNode = sibling as PersonNode;
						}
						break;
					case Keys.Right:
						if (parent != null && parent.ChildNodes.Where(child => child.Visible).Count() > 1) {
							ITreeNode sibling = parent.ChildNodes.Skip(parent.ChildNodes.ToList().IndexOf(selectedNode) + 1).FirstOrDefault(node => node.Visible);
							if (sibling != null)
								selectedNode = sibling as PersonNode;
						}
						break;
					//case Keys.Enter:
				}
				Refresh();
			}
		}

		protected override void paintTreeNode(Graphics g, ITreeNode node, DisplayGrid.Cell cell)
		{
			base.paintTreeNode(g, node, cell);
			if (node == selectedNode)
				highlightNode(g, cell);
		}

		private void highlightNode(Graphics g, DisplayGrid.Cell cell)
		{
			RectangleF rect = getCell(cell);
			g.DrawRectangle(new Pen(Color.DarkBlue, 4),
			                rect.X,
			                rect.Y,
			                rect.Width,
			                rect.Height);
		}
	}
}