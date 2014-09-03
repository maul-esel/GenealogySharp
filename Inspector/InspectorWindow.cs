using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Genealogy.Inspector
{
	public class InspectorWindow : WindowBase
	{
		public Storage DataStorage {
			get;
			private set;
		}

		private Label noDataLoaded = new Label();
		private ListView titleList = new ListView();

		public InspectorWindow()
		{
			SuspendLayout();

			Text = "Genealogy Inspector";
			//WindowState = FormWindowState.Maximized;

			Menu = new MainMenu();

			var menuFile = new MenuItem("File");
			Menu.MenuItems.Add(menuFile);

			menuFile.MenuItems.Add(new MenuItem("Open", onFileOpen));

			noDataLoaded.Text = "Currently no data available. Use 'File -> Open' to load data.";
			noDataLoaded.Dock = DockStyle.Fill;
			noDataLoaded.TextAlign = ContentAlignment.MiddleCenter;
			noDataLoaded.BorderStyle = BorderStyle.FixedSingle;
			Controls.Add(noDataLoaded);

			Label info = createBoldLabel("Double-click any title to get further information");
			info.Dock = DockStyle.Bottom;
			info.TextAlign = ContentAlignment.MiddleCenter;
			Controls.Add(info);

			setupTitleList();
			Controls.Add(titleList);

			ResumeLayout ();
			PerformLayout();
		}

		private void onFileOpen(object sender, EventArgs e)
		{
			FileDialog dialog = new OpenFileDialog();
			if (dialog.ShowDialog () == DialogResult.OK) {
				closeStorage();
				openStorage(dialog.FileName);
			}
		}

		private void onTitleOpen(object sender, EventArgs e)
		{
			if (titleList.SelectedItems.Count > 0)
				new TitleDetailsWindow(titleList.SelectedItems[0].Tag as Title).Show(this);
		}

		private void onTitleEnter(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				onTitleOpen(sender, e);
			e.Handled = true;
		}

		private void closeStorage()
		{
			DataStorage = null;
		}

		private void openStorage(string filename)
		{
			try {
				DataStorage = new Storage(filename);
			} catch (Exception e) {
				MessageBox.Show(this, "Failed to open file: " + e.Message, "Error", MessageBoxButtons.OK);
			}

			noDataLoaded.Visible = false;
			titleList.Visible = true;

			titleList.Items.Clear();
			foreach (Title title in DataStorage.Titles) {
				ListViewItem item = new ListViewItem(title.Rank.ToString());
				item.SubItems.AddRange(new ListViewItem.ListViewSubItem[] {
					new ListViewItem.ListViewSubItem(item, joinRealmNames(title.Realms)),
					new ListViewItem.ListViewSubItem(item, title.Established.ToString()),
					new ListViewItem.ListViewSubItem(item, title.Reigns.First().ToString()),
					new ListViewItem.ListViewSubItem(item, title.Reigns.Last().ToString())
				});
				item.Tag = title;
				titleList.Items.Add(item);
			}
			titleList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		private void setupTitleList()
		{
			titleList.Dock = DockStyle.Fill;

			titleList.View = View.Details;
			titleList.FullRowSelect = true;
			titleList.GridLines = true;

			titleList.DoubleClick += onTitleOpen;
			titleList.KeyUp += onTitleEnter;

			titleList.Columns.Add("Rank");
			titleList.Columns.Add("Territory");
			titleList.Columns.Add("Established");
			titleList.Columns.Add("First Ruler");
			titleList.Columns.Add("Last Ruler");

			titleList.Visible = false;
		}
	}
}