using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using TGC;
using Genealogy.Succession;

namespace Genealogy.Inspector
{
	public class GenealogyTreeControl : TreeGraphControl
	{
		private Lineage lineage = Lineage.Cognatic;
		public Lineage Lineage {
			get { return lineage; }
			set {
				lineage = value;

				SuspendLayoutAndPainting();
				OnLineageChanged();
				ResumeLayoutAndPainting();

				removeDuplicates();
				SelectedNode = null;
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

		private ToolTip tooltip = new ToolTip();

		public GenealogyTreeControl() : base()
		{
			tooltip.IsBalloon = true;
			tooltip.AutomaticDelay = 0;
			tooltip.ReshowDelay = 0;
			tooltip.InitialDelay = 0;
		}

		public event EventHandler LineageChanged;

		private void OnLineageChanged()
		{
			if (LineageChanged != null)
				LineageChanged(this, new EventArgs());
		}

		private void removeDuplicates()
		{
			if (RootNode == null)
				return;

			List<PersonNode> nodes = new List<PersonNode>();
			collectNodes(RootNode as PersonNode, nodes);

			SuspendLayoutAndPainting();

			var duplicatesToHide =
				from node in nodes
				group node by node.Person into nodeGroup
				where nodeGroup.Count() > 1
				where nodes.Exists(n => n.Visible && n.Person == nodeGroup.Key.Father)
					&& nodes.Exists(n => n.Visible && n.Person == nodeGroup.Key.Mother)
				select nodeGroup;
			foreach (var duplicate in duplicatesToHide) {
				var collapsibleParents = nodes.Where(node => node.Person == (Lineage == Lineage.Uterine ? duplicate.Key.Father : duplicate.Key.Mother));
				foreach (var parent in collapsibleParents)
					parent.ChildNodes.Cast<PersonNode>().First(child => child.Person == duplicate.Key).Hide();
			}

			ResumeLayoutAndPainting();
			InvalidateLayout();
			Refresh();
		}

		private void collectNodes(PersonNode node, List<PersonNode> list)
		{
			list.Add(node);
			foreach (PersonNode child in node.ChildNodes)
				collectNodes(child, list);
		}

		protected override void paintTreeNode(Graphics g, VisualTreeNode node)
		{
			if ((node.Node as PersonNode).Person.Titles.Length == 0)
				SelectedTreeNodeBorderPen.DashStyle = TreeNodeBorderPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
			else
				SelectedTreeNodeBorderPen.DashStyle = TreeNodeBorderPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

			Person p = (node.Node as PersonNode).Person;
			if (SelectedPerson != null && SelectedPerson.Marriages.Any(m => m.Husband == p || m.Wife == p))
				TreeNodeBackgroundColor = Color.MistyRose;
			else
				TreeNodeBackgroundColor = Color.Ivory;
			base.paintTreeNode(g, node);
		}

		private PersonNode tooltipNode = null;

		protected override void OnMouseHover(EventArgs e)
		{
			base.OnMouseHover(e);
			Point location = PointToClient(Cursor.Position);

			PersonNode node = HitTest(new PointF(location.X, location.Y)) as PersonNode;
			if (node != tooltipNode) {
				tooltip.Hide(this);
				tooltipNode = node;
				if (node != null) {
					tooltip.ToolTipTitle = node.Person.Firstname + " " + node.Person.Lastname + (node.Person.Lastname != node.Person.Birthname ? " née " + node.Person.Birthname : "");
					string text = string.Format("{0} - {1}\n\nRelationship degree with {2} {3}{4}: {5}\n\nMarriages:\n\t{6}\n\nTitles:\n\t{7}\n\n{8}",
					                           node.Person.YearOfBirth,
					                           node.Person.YearOfDeath,
					                           SelectedPerson.Firstname,
					                           SelectedPerson.Lastname,
					                           (SelectedPerson.Lastname != SelectedPerson.Birthname ? " née " + SelectedPerson.Birthname : ""),
					                           Person.RelationshipDegree(node.Person, SelectedPerson),
					                           node.Person.Marriages.Length > 0 ? "- " + string.Join("\n\t- ", node.Person.Marriages.Select(m => formatMarriage(m, node.Person))) : "(none)",
					                           node.Person.Titles.Length > 0 ? "- " + string.Join("\n\t- ", node.Person.Titles.Select(r => r.ToString())) : "(none)",
					                           formatParents(node)
					);
					tooltip.Show(text, this, location);
				}
			}
		}

		private string formatMarriage(Marriage m, Person p)
		{
			Person spouse = p == m.Husband ? m.Wife : m.Husband;
			return string.Format("{0} - {1}: {2} {3}{4} ({5} - {6}) - {7} children",
			                     m.Start,
			                     m.End,
			                     spouse.Firstname,
			                     spouse.Lastname,
			                     (spouse.Lastname != spouse.Birthname ? " née " + spouse.Birthname : ""),
			                     spouse.YearOfBirth,
			                     spouse.YearOfDeath,
			                     m.Children.Length
			);
		}

		private string formatParents(PersonNode node)
		{
			string text = "";
			PersonNode displayedParent = GetParentNode(node) as PersonNode;
			if (node.Person.Father != null && (displayedParent == null || displayedParent.Person != node.Person.Father))
				text = string.Format("Father: {0} {1} ({2} - {3})",
				                     node.Person.Father.Firstname,
				                     node.Person.Father.Lastname,
				                     node.Person.Father.YearOfBirth,
				                     node.Person.Father.YearOfDeath);
			if (node.Person.Mother != null && (displayedParent == null || displayedParent.Person != node.Person.Mother))
				text += string.Format("\nMother: {0} {1} née {2} ({3} - {4})",
				                      node.Person.Mother.Firstname,
				                      node.Person.Mother.Lastname,
				                      node.Person.Mother.Birthname,
				                      node.Person.Mother.YearOfBirth,
				                      node.Person.Mother.YearOfDeath);
			return text.TrimStart('\n');
		}
	}
}