using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Genealogy.Inspector
{
	public class RelationshipWindow : WindowBase
	{
		public RelationshipWindow(Storage data)
		{
			Text = "Calculate relationship degree";
			SuspendLayout();

			var personItems = data.Persons.Select(p => new { Title = string.Format("{0} {1}{2}, {3} - {4}",
				                                                                       p.Firstname,
				                                                                       p.Lastname,
				                                                                       (p.Lastname != p.Birthname ? " née " + p.Birthname : ""),
				                                                                       p.YearOfBirth,
				                                                                       p.YearOfDeath), Person = p })
				.OrderBy(d => d.Title).ToList();

			TableLayoutPanel panel = new TableLayoutPanel();
			Controls.Add(panel);
			panel.Size = ClientSize;
			panel.Dock = DockStyle.Fill;
			panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

			ComboBox personsOne = new ComboBox();
			personsOne.DisplayMember = "Title";
			personsOne.ValueMember = "Person";
			personsOne.DataSource = personItems;
			panel.Controls.Add(personsOne);
			personsOne.Dock = DockStyle.Fill;
			personsOne.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

			ComboBox personsTwo = new ComboBox();
			personsTwo.DisplayMember = "Title";
			personsTwo.ValueMember = "Person";
			personsTwo.DataSource = personItems.ToList();
			panel.Controls.Add(personsTwo);
			personsTwo.Dock = DockStyle.Fill;
			personsTwo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

			Label output = new Label();
			output.Text = "Select two persons from the lists above.";
			panel.Controls.Add(output);
			output.Dock = DockStyle.Fill;
			output.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

			EventHandler calcDegree = (s, e) => {
				if (personsOne.SelectedValue != null && personsTwo.SelectedValue != null) {
					int degree = Person.RelationshipDegree(personsOne.SelectedValue as Person, personsTwo.SelectedValue as Person);
					output.Text = "Relationship degree: " + (degree == -1 ? "no relationship" : degree.ToString());
				}
			};
			personsOne.SelectedIndexChanged += calcDegree;
			personsTwo.SelectedIndexChanged += calcDegree;

			ResumeLayout();
			PerformLayout();
		}
	}
}