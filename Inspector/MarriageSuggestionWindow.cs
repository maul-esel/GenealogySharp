using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Genealogy.Inspector
{
	public class MarriageSuggestionWindow : WindowBase
	{
		private int maxAgeGap = 20;
		private int minDegree = 3;

		private readonly ComboBox personBox = new ComboBox();
		private readonly CheckBox useYearCheckbox = new CheckBox();
		private readonly NumericUpDown yearCtrl = new NumericUpDown();
		private readonly ListView candidateList = new ListView();

		private Storage dataStorage;

		public MarriageSuggestionWindow(Storage data)
		{
			dataStorage = data;
			Text = "Genealogy Inspector - Marriage Proposals";
			SuspendLayout();

			TableLayoutPanel panel = new TableLayoutPanel();
			Controls.Add(panel);
			panel.Size = ClientSize;
			panel.Dock = DockStyle.Fill;
			panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

			Label maxAgeGapLabel = createBoldLabel("max. age gap");
			maxAgeGapLabel.Dock = DockStyle.Bottom;
			panel.Controls.Add(maxAgeGapLabel, 0, 0);

			NumericUpDown maxAgeGapCtrl = new NumericUpDown();
			maxAgeGapCtrl.Value = maxAgeGap;
			maxAgeGapCtrl.Minimum = 0;
			maxAgeGapCtrl.Dock = DockStyle.Bottom;
			maxAgeGapCtrl.ValueChanged += (s, e) => {
				maxAgeGap = (int)maxAgeGapCtrl.Value;
				loadCandidates();
			};
			panel.Controls.Add(maxAgeGapCtrl, 1, 0);

			Label minDegreeLabel = createBoldLabel("min. kinship degree");
			minDegreeLabel.Dock = DockStyle.Bottom;
			panel.Controls.Add(minDegreeLabel, 0, 1);

			NumericUpDown minDegreeCtrl = new NumericUpDown();
			minDegreeCtrl.Value = minDegree;
			minDegreeCtrl.Minimum = 2;
			minDegreeCtrl.Dock = DockStyle.Bottom;
			minDegreeCtrl.ValueChanged += (s, e) => {
				minDegree = (int)minDegreeCtrl.Value;
				loadCandidates();
			};
			panel.Controls.Add(minDegreeCtrl, 1, 1);

			personBox.DisplayMember = "Title";
			personBox.ValueMember = "Person";
			personBox.Dock = DockStyle.Bottom;
			personBox.SelectedIndexChanged += (s, e) => loadCandidates();
			panel.Controls.Add(personBox, 0, 2);
			panel.SetColumnSpan(personBox, 2);
			personBox.DataSource = createPersonData();

			useYearCheckbox.Text = "use year";
			useYearCheckbox.Checked = false;
			useYearCheckbox.CheckStateChanged += (s, e) => {
				loadCandidates();
				yearCtrl.Enabled = useYearCheckbox.Checked;
			};
			useYearCheckbox.Dock = DockStyle.Bottom;
			panel.Controls.Add(useYearCheckbox, 0, 3);

			yearCtrl.Dock = DockStyle.Bottom;
			yearCtrl.ValueChanged += (s, e) => loadCandidates();
			yearCtrl.Enabled = false;
			yearCtrl.Maximum = decimal.MaxValue;
			panel.Controls.Add(yearCtrl, 1, 3);

			// listview for candidates
			candidateList.View = View.Details;
			candidateList.FullRowSelect = true;
			candidateList.Columns.Add("ID");
			candidateList.Columns.Add("Firstname");
			candidateList.Columns.Add("Lastname");
			candidateList.Columns.Add("Birth");
			candidateList.Columns.Add("Death");
			candidateList.Columns.Add("Relationship Degree");
			candidateList.Columns.Add("Age Gap");
			candidateList.Dock = DockStyle.Bottom;
			panel.Controls.Add(candidateList, 0, 4);
			panel.SetColumnSpan(candidateList, 2);

			loadCandidates();

			ResumeLayout();
			PerformLayout();
		}

		private void loadCandidates()
		{
			Person selected = personBox.SelectedValue as Person;
			if (selected != null) {
				candidateList.Items.Clear();
				foreach (Person p in dataStorage.Persons.Where(p => isValidPartner(selected, p))) {
					candidateList.Items.Add(new ListViewItem(new[] {
						p.ID.ToString(),
						p.Firstname,
						p.Lastname + (p.Lastname != p.Birthname ? " (née " + p.Birthname + ")" : ""),
						p.YearOfBirth.ToString(),
						p.YearOfDeath.ToString(),
						formatDegree(Person.RelationshipDegree(selected, p)),
						(p.YearOfBirth - selected.YearOfBirth).ToString()
					}));
				}
				candidateList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
			}
		}

		private string formatDegree(int d)
		{
			if (d == -1)
				return "none";
			return d.ToString();
		}

		private bool isValidPartner(Person a, Person b)
		{
			int degree = Person.RelationshipDegree(a, b);
			bool validDegree = degree == -1 || degree >= minDegree;

			bool valid;
			if (useYearCheckbox.Checked)
				valid = b.isAlive((int)yearCtrl.Value) && !b.Marriages.Any(m => m.Start <= yearCtrl.Value && yearCtrl.Value <= m.End);
			else
				valid = Math.Abs(b.YearOfBirth - a.YearOfBirth) <= maxAgeGap && !b.Marriages.Any();

			return valid && validDegree && (a.Gender != b.Gender);
		}

		private List<object> createPersonData()
		{
			return dataStorage.Persons.Select(p => new {
				Title = string.Format("{0} {1}{2}, {3} - {4}",
				                      p.Firstname,
				                      p.Lastname,
				                      (p.Lastname != p.Birthname ? " née " + p.Birthname : ""),
				                      p.YearOfBirth,
				                      p.YearOfDeath),
				Person = p
			}).OrderBy(p => p.Title).Cast<object>().ToList();
		}
	}
}