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
				SelectedNode = menuNode = null;
			}
		}

		public override ITreeNode RootNode {
			get { return base.RootNode; }
			set {
				if (!(value is PersonNode))
					throw new ArgumentException();
				base.RootNode = value;
				removeDuplicates();
			}
		}

		public Person SelectedPerson {
			get { return SelectedNode == null ? null : (SelectedNode as PersonNode).Person; }
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

		private PersonNode menuNode = null;

		private void onClick(object sender, MouseEventArgs e)
		{
			menuNode = null;
			PersonNode node = HitTest(new PointF(e.X, e.Y)) as PersonNode;
			if (node != null && e.Button == MouseButtons.Right) {
				menuNode = node;
				nodeMenu.Show(this, e.Location);
			}
		}

		private void onKeyUp(object sender, KeyEventArgs e)
		{
			if (SelectedNode != null && e.KeyCode == Keys.Enter)
				new PersonWindow(SelectedPerson).Show(FindForm().Owner);
		}
	}
}