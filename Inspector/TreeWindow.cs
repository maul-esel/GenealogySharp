using System;
using System.Linq;
using System.Windows.Forms;

namespace Genealogy.Inspector
{
	public class TreeWindow : WindowBase
	{
		private readonly GenealogyTreeControl tree = new GenealogyTreeControl();
		private readonly Label nameLabel = new Label();
		private readonly Label bornLabel = new Label();
		private readonly Label diedLabel = new Label();
		private readonly Label fatherLink = new LinkLabel();
		private readonly Label motherLink = new LinkLabel();
		private readonly ListBox titleList = new ListBox();
		private readonly ListView childrenList = new ListView();

		private Person TreeRoot {
			get { return tree.RootNode == null ? null : (tree.RootNode as PersonNode).Person; }
			set {
				PersonNode oldRoot = tree.RootNode as PersonNode;
				tree.RootNode = new PersonNode(value, tree);
				if (oldRoot != null)
					oldRoot.Destroy();
			}
		}

		public TreeWindow(Person person)
		{
			SuspendLayout();

			Text = "Genealogy Inspector";
			AnchorStyles AllAnchors = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

			SplitContainer splitter = new SplitContainer();
			splitter.FixedPanel = FixedPanel.Panel2;
			Controls.Add(splitter);
			splitter.Dock = DockStyle.Fill;

			TreeRoot = person;
			splitter.Panel1.Controls.Add(tree);
			tree.Dock = DockStyle.Fill;

			tree.SelectedNode = tree.RootNode;
			tree.SelectedNodeChanged += (s, e) => {
				if (tree.SelectedNode == null)
					tree.SelectedNode = tree.RootNode;
			};

			FlowLayoutPanel top = new FlowLayoutPanel();
			splitter.Panel1.Controls.Add(top);
			top.AutoSize = true;
			top.Dock = DockStyle.Top;

			top.Controls.Add(createBoldLabel("Lineality:"));

			ComboBox lineality = new ComboBox();
			lineality.DropDownStyle = ComboBoxStyle.DropDownList;
			lineality.Items.AddRange(new object[] { Lineality.Agnatic, Lineality.Cognatic, Lineality.Uterine });
			lineality.SelectedItem = Lineality.Cognatic;
			lineality.SelectedValueChanged += (s, e) => tree.Lineality = (Lineality)lineality.SelectedItem;
			top.Controls.Add(lineality);
			lineality.Dock = DockStyle.Fill;

			TableLayoutPanel details = new TableLayoutPanel();
			details.Margin = new Padding(5);
			details.BorderStyle = BorderStyle.FixedSingle;
			splitter.Panel2.Controls.Add(details);
			details.Dock = DockStyle.Fill;
			details.Anchor = AllAnchors;

			details.Controls.Add(nameLabel, 0, 0);
			details.SetColumnSpan(nameLabel, 2);
			details.Controls.Add(createBoldLabel("Born:"), 0, 1);
			details.Controls.Add(createBoldLabel("Died:"), 0, 2);
			details.Controls.Add(createBoldLabel("Father:"), 0, 3);
			details.Controls.Add(createBoldLabel("Mother:"), 0, 4);
			details.Controls.Add(bornLabel, 1, 1);
			details.Controls.Add(diedLabel, 1, 2);
			details.Controls.Add(fatherLink, 1, 3);
			details.Controls.Add(motherLink, 1, 4);

			fatherLink.Click += (s, e) => openParent(tree.SelectedPerson.Father);
			motherLink.Click += (s, e) => openParent(tree.SelectedPerson.Mother);

			Label titleLabel = createBoldLabel("Titles");
			details.Controls.Add(titleLabel, 0, 5);
			details.SetColumnSpan(titleLabel, 2);

			details.Controls.Add(titleList, 0, 6);
			details.SetColumnSpan(titleList, 2);
			titleList.Dock = DockStyle.Fill;
			titleList.DoubleClick += (s, e) => {
				if (titleList.SelectedItem != null)
					new TitleDetailsWindow((titleList.SelectedItem as Reign).Title).Show(Owner);
			};

			childrenList.View = View.Details;
			childrenList.Columns.Add("Children");
			childrenList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
			details.Controls.Add(childrenList, 0, 7);
			details.SetColumnSpan(childrenList, 2);
			childrenList.Dock = DockStyle.Fill;
			childrenList.DoubleClick += (s, e) => {
				if (childrenList.SelectedItems.Count > 0) {
					Person child = childrenList.SelectedItems[0].Tag as Person;

					if (tree.Lineality == Lineality.Cognatic
					    || (tree.SelectedPerson.Gender == Gender.Male && tree.Lineality == Lineality.Agnatic)
					    || (tree.SelectedPerson.Gender == Gender.Female && tree.Lineality == Lineality.Uterine)) {
						TGC.ITreeNode childNode = tree.SelectedNode.ChildNodes.FirstOrDefault(node => (node as PersonNode).Person == child);
						if (childNode != null)
							tree.SelectedNode = childNode;
					}

					if (tree.SelectedPerson != child)
						new TreeWindow(child).Show(Owner);
				}
			};

			Button makeRoot = new Button();
			makeRoot.Text = "View Family Tree";
			makeRoot.Click += (s, e) => TreeRoot = tree.SelectedPerson;
			details.Controls.Add(makeRoot, 0, 8);
			details.SetColumnSpan(makeRoot, 2);
			makeRoot.Dock = DockStyle.Bottom;

			Button openWindow = new Button();
			openWindow.Text = "Open Tree in New Window";
			openWindow.Click += (s, e) => new TreeWindow(tree.SelectedPerson).Show(Owner);
			details.Controls.Add(openWindow, 0, 9);
			details.SetColumnSpan(openWindow, 2);
			openWindow.Dock = DockStyle.Bottom;

			loadDetails(person);
			tree.SelectedNodeChanged += (s, e) => loadDetails(tree.SelectedPerson);

			ResumeLayout();
			PerformLayout();
		}

		private void openParent(Person parent)
		{
			if (parent == null)
				return;

			PersonNode parentNode = tree.GetParentNode(tree.SelectedNode) as PersonNode;
			if (parentNode != null && parentNode.Person == parent)
				tree.SelectedNode = parentNode;
			else if (tree.SelectedNode == tree.RootNode)
				TreeRoot = parent;
			else
				new TreeWindow(parent).Show(Owner);
		}

		private void loadDetails(Person subject)
		{
			if (subject == null)
				return;

			nameLabel.Text = formatName(subject);
			bornLabel.Text = subject.YearOfBirth.ToString();
			diedLabel.Text = subject.YearOfDeath.ToString();
			fatherLink.Text = formatName(subject.Father);
			motherLink.Text = formatName(subject.Mother);

			titleList.Items.Clear();
			foreach (Reign reign in subject.Titles)
				titleList.Items.Add(reign);
			// TODO: nicer display for titles

			childrenList.Items.Clear();
			childrenList.Groups.Clear();
			var groups = subject.Children
				.Select(c => subject.Gender == Gender.Male ? c.Mother : c.Father)
				.Distinct()
				.ToDictionary(
						parent => parent,
						parent => new ListViewGroup(
							string.Format(
								"with {0} {1} ({2} - {3}) (married in {4})",
								parent.Firstname,
								parent.Birthname,
								parent.YearOfBirth,
								parent.YearOfDeath,
								parent.Marriages.Where(m => subject == m.Husband || subject == m.Wife).First().Start
							)
						)
				);
			childrenList.Groups.AddRange(groups.Values.ToArray());
			foreach (Person child in subject.Children) {
				ListViewItem item = new ListViewItem(
					string.Format(
						"{0} {1} ({2} - {3})",
						child.Firstname,
						child.Lastname,
						child.YearOfBirth,
						child.YearOfDeath
					),
					groups[subject.Gender == Gender.Male ? child.Mother : child.Father]
				);
				item.Tag = child;
				childrenList.Items.Add(item);
			}
			childrenList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		private string formatName(Person p)
		{
			if (p == null)
				return "(unknown)";
			return p.Firstname + " " + p.Lastname
				+ (p.Lastname != p.Birthname ? " (née " + p.Birthname + ")" : "");
		}
	}
}