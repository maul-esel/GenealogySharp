using System;
using System.Linq;
using System.Windows.Forms;

namespace Genealogy.Inspector
{
	public class PersonWindow : WindowBase
	{
		private Gender treeLineage = Gender.Male;

		private Person treeRoot;
		private Person detailsSubject;

		private TreeView tree = new TreeView();
		private Label firstname = new Label();
		private Label lastname = new Label();
		private Label birthname = new Label();
		private Label gender = new Label();
		private Label born = new Label();
		private Label died = new Label();
		private LinkLabel father = new LinkLabel();
		private LinkLabel mother = new LinkLabel();
		private ListView titleList = new ListView();
		private ListView childrenList = new ListView();
		private Button makeRoot = new Button();

		public PersonWindow(Person subject)
		{
			Text = "Genealogy Inspector - View Person";
			treeRoot = detailsSubject = subject;

			Resize += onResize;

			SplitContainer panel = new SplitContainer();
			panel.Size = ClientSize;
			Controls.Add(panel);

			setupTree();
			panel.Panel1.Controls.Add(tree);

			TableLayoutPanel right = new TableLayoutPanel();
			right.Dock = DockStyle.Right;
			right.Size = panel.Panel2.ClientSize;
			panel.Panel2.Controls.Add(right);

			TableLayoutPanel table = new TableLayoutPanel();
			table.Dock = DockStyle.Fill;
			right.Controls.Add(table, 0, 0);

			table.Controls.Add(createBoldLabel("First Name"), 0, 0);
			table.Controls.Add(createBoldLabel("Last Name"), 0, 1);
			table.Controls.Add(createBoldLabel("Birth Name"), 0, 2);
			table.Controls.Add(createBoldLabel("Gender"), 0, 3);
			table.Controls.Add(createBoldLabel("Born"), 0, 4);
			table.Controls.Add(createBoldLabel("Died"), 0, 5);
			table.Controls.Add(createBoldLabel("Father"), 0, 6);
			table.Controls.Add(createBoldLabel("Mother"), 0, 7);

			table.Controls.Add(firstname, 1, 0);
			table.Controls.Add(lastname, 1, 1);
			table.Controls.Add(birthname, 1, 2);
			table.Controls.Add(gender, 1, 3);
			table.Controls.Add(born, 1, 4);
			table.Controls.Add(died, 1, 5);
			table.Controls.Add(father, 1, 6);
			table.Controls.Add(mother, 1, 7);

			father.LinkClicked += (s, e) => {
				if (detailsSubject.Father != null) {
					detailsSubject = detailsSubject.Father;
					loadDetails();
				}
			};
			mother.LinkClicked += (s, e) => {
				if (detailsSubject.Mother != null) {
					detailsSubject = detailsSubject.Mother;
					loadDetails();
				}
			};

			right.Controls.Add(createBoldLabel("Titles"), 0, 1);
			
			titleList.Columns.Add("#");
			titleList.Columns.Add("Name");
			titleList.Columns.Add("Title");
			titleList.Columns.Add("Realm");
			titleList.Columns.Add("Start");
			titleList.Columns.Add("End");
			titleList.Dock = DockStyle.Fill;
			titleList.View = View.Details;
			titleList.FullRowSelect = true;
			right.Controls.Add(titleList, 0, 2);

			right.Controls.Add(createBoldLabel("Children"), 0, 3);
			
			childrenList.Columns.Add("Name");
			childrenList.Columns.Add("Gender");
			childrenList.Columns.Add("Born");
			childrenList.Columns.Add("Died");
			childrenList.ShowGroups = true;
			childrenList.Dock = DockStyle.Fill;
			childrenList.View = View.Details;
			childrenList.FullRowSelect = true;
			right.Controls.Add(childrenList, 0, 4);

			makeRoot.Dock = DockStyle.Bottom;
			makeRoot.Text = "View Family Tree";
			makeRoot.Click += (s, e) => {
				treeRoot = detailsSubject;
				loadTree();
			};
			right.Controls.Add(makeRoot, 0, 5);

			ComboBox lineageCombo = new ComboBox();
			lineageCombo.Items.AddRange(new object[] { Gender.Male, Gender.Female });
			lineageCombo.SelectedItem = treeLineage;
			lineageCombo.DropDownStyle = ComboBoxStyle.DropDownList;
			lineageCombo.SelectedValueChanged += (s, e) => {
				treeLineage = (Gender)lineageCombo.SelectedItem;
				loadTree();
			};
			right.Controls.Add(lineageCombo, 0, 6);

			loadDetails();
			loadTree();

			PerformLayout();
		}

		private void onResize(object s, EventArgs e)
		{
			var container = Controls[0] as SplitContainer;
			container.Size = ClientSize;
			container.Panel2.Controls[0].Size = container.Panel2.ClientSize;
		}

		private void loadDetails()
		{
			firstname.Text = detailsSubject.Firstname;
			lastname.Text = detailsSubject.Lastname;
			birthname.Text = detailsSubject.Birthname;
			gender.Text = detailsSubject.Gender.ToString().ToLower();
			born.Text = detailsSubject.YearOfBirth.ToString();
			died.Text = detailsSubject.YearOfDeath.ToString();

			if (detailsSubject.Father != null) {
				father.Text = detailsSubject.Father.Firstname + " " + detailsSubject.Father.Lastname;
				mother.Text = detailsSubject.Mother.Firstname + " " + detailsSubject.Mother.Lastname;
			} else
				father.Text = mother.Text = "(unknown)";

			titleList.Items.Clear();
			foreach (Reign reign in detailsSubject.Titles) {
				ListViewItem item = new ListViewItem(new string[] {
					reign.SuccessionIndex.ToString(),
					reign.Ruler.Firstname + " " + RomanNumerals.ToRomanNumeral(reign.NameIndex) + ".",
					reign.Title.Rank.ToString(),
					reign.Title.Realm.Name,
					reign.Start.ToString(),
					reign.End.ToString()
				});
				item.Tag = reign;
				titleList.Items.Add(item);
			}

			childrenList.Items.Clear();
			childrenList.Groups.Clear();
			var groups = detailsSubject.Children
				.Select(c => detailsSubject.Gender == Gender.Male ? c.Mother : c.Father)
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
							parent.Marriages.Where(m => detailsSubject == m.Husband || detailsSubject == m.Wife).First().Start
						)
					)
				);
			childrenList.Groups.AddRange(groups.Values.ToArray());
			foreach (Person child in detailsSubject.Children) {
				ListViewItem item = new ListViewItem(new string[] {
						child.Firstname,
						child.Gender.ToString().ToLower(),
						child.YearOfBirth.ToString(),
						child.YearOfDeath.ToString()
					},
					groups[detailsSubject.Gender == Gender.Male ? child.Mother : child.Father]
				);
				item.Tag = child;
				childrenList.Items.Add(item);
			}

			makeRoot.Enabled = detailsSubject != treeRoot;
		}

		private void loadTree()
		{
			tree.Nodes.Clear();
			tree.Nodes.Add(getTreeNode(treeRoot));
			tree.ExpandAll();

			makeRoot.Enabled = detailsSubject != treeRoot;
		}

		private TreeNode getTreeNode(Person p)
		{
			TreeNode node = new TreeNode(
				p.Firstname + " " + p.Lastname,
				(from c in p.Children where p.Gender == treeLineage || p == treeRoot orderby c.YearOfBirth ascending select getTreeNode(c)).ToArray()
			);
			node.Tag = p;
			return node;
		}

		private void onLoadPerson(object sender, EventArgs e)
		{
			if (tree.SelectedNode != null) {
				detailsSubject = tree.SelectedNode.Tag as Person;
				loadDetails();
			}
		}

		private void onEnterPerson(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				onLoadPerson(sender, e);
		}

		private void setupTree()
		{
			tree.Dock = DockStyle.Fill;
			tree.ShowPlusMinus = false;
			tree.DoubleClick += onLoadPerson;
			tree.KeyUp += onEnterPerson;
			tree.BeforeCollapse += (s, e) => e.Cancel = true;
		}
	}
}

