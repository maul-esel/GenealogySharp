using System;
using System.Drawing;
using System.Windows.Forms;

namespace Genealogy.Inspector
{
	public class TitleDetailsWindow : WindowBase
	{
		private readonly Title subject;

		private ReignListBox reignList = new ReignListBox();

		public TitleDetailsWindow(Title title)
		{
			Text = "Genealogy Inspector - View Title";
			this.subject = title;

			TableLayoutPanel panel = new TableLayoutPanel();
			Controls.Add(panel);
			panel.Dock = DockStyle.Fill;
			panel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

			TableLayoutPanel table = new TableLayoutPanel();
			panel.Controls.Add(table, 0, 0);
			panel.Dock = DockStyle.Fill;

			table.Controls.Add(createBoldLabel("Title:"), 0, 0);
			table.Controls.Add(createBoldLabel("Realm:"), 0, 1);
			table.Controls.Add(createBoldLabel("Established in:"), 0, 2);

			table.Controls.Add(createLabel(subject.Rank.ToString()), 1, 0);
			table.Controls.Add(createLabel(Realm.JoinRealmNames(subject.Realms)), 1, 1);
			table.Controls.Add(createLabel(subject.Established.ToString()), 1, 2);

			reignList.Margin = new Padding(5);
			panel.Controls.Add(reignList, 0, 1);
			reignList.Dock = DockStyle.Fill;
			reignList.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;

			reignList.Items.AddRange(subject.Reigns);

			reignList.DoubleClick += onPersonDetails;
			reignList.KeyUp += onPersonEnter;

			PerformLayout();
		}

		private void onPersonDetails(object s, EventArgs e)
		{
			if (reignList.SelectedItem != null)
				new TreeWindow((reignList.SelectedItem as Reign).Ruler).Show(Owner);
		}

		private void onPersonEnter(object s, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				onPersonDetails(s, e);
			e.Handled = true;
		}
	}
}