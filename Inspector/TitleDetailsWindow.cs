using System;
using System.Drawing;
using System.Windows.Forms;

namespace Genealogy.Inspector
{
	public class TitleDetailsWindow : WindowBase
	{
		private readonly Title subject;

		private ListView reignList = new ListView();

		public TitleDetailsWindow(Title title)
		{
			Text = "Genealogy Inspector - View Title";
			this.subject = title;

			Resize += onResize;

			TableLayoutPanel panel = new TableLayoutPanel();
			panel.Size = ClientSize;
			Controls.Add(panel);

			TableLayoutPanel table = new TableLayoutPanel();
			panel.Controls.Add(table);

			table.Controls.Add(createBoldLabel("Title:"), 0, 0);
			table.Controls.Add(createBoldLabel("Realm:"), 0, 1);
			table.Controls.Add(createBoldLabel("Established in:"), 0, 2);

			table.Controls.Add(createLabel(subject.Rank.ToString()), 1, 0);
			table.Controls.Add(createLabel(subject.Realm.ToString()), 1, 1);
			table.Controls.Add(createLabel(subject.Established.ToString()), 1, 2);

			setupReignList();
			panel.Controls.Add(reignList);

			PerformLayout();
			reignList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
		}

		private void onResize(object sender, EventArgs e)
		{
			Controls[0].Size = ClientSize;
			reignList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
		}

		private void onPersonDetails(object s, EventArgs e)
		{
			if (reignList.SelectedItems.Count > 0)
				new PersonWindow((reignList.SelectedItems[0].Tag as Reign).Ruler).Show();
		}

		private void onPersonEnter(object s, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				onPersonDetails(s, e);
			e.Handled = true;
		}

		private void setupReignList()
		{
			reignList.Dock = DockStyle.Fill;

			reignList.View = View.Details;
			reignList.FullRowSelect = true;
			reignList.GridLines = true;

			reignList.DoubleClick += onPersonDetails;
			reignList.KeyUp += onPersonEnter;

			reignList.Columns.Add("#");
			reignList.Columns.Add("First Name");
			reignList.Columns.Add("Last Name");
			reignList.Columns.Add("Start");
			reignList.Columns.Add("End");

			foreach (Reign reign in subject.Reigns) {
				ListViewItem item = new ListViewItem(reign.SuccessionIndex.ToString());
				item.SubItems.AddRange(new ListViewItem.ListViewSubItem[] {
					new ListViewItem.ListViewSubItem(item, reign.Ruler.Firstname + " " + RomanNumerals.ToRomanNumeral(reign.NameIndex)),
					new ListViewItem.ListViewSubItem(item, reign.Ruler.Lastname),
					new ListViewItem.ListViewSubItem(item, reign.Start.ToString()),
					new ListViewItem.ListViewSubItem(item, reign.End.ToString())
				});
				item.Tag = reign;
				reignList.Items.Add(item);
			}
		}
	}
}

