using System;
using System.Linq;
using TGC;

namespace Genealogy.Inspector
{
	public class PersonNode : TreeNodeBase
	{
		public Person Person {
			get;
			private set;
		}

		private readonly GenealogyTreeControl control;

		public PersonNode(Person person, GenealogyTreeControl control)
			: base(formatText(person), person.Children.Select(child => new PersonNode(child, control)))
		{
			Person = person;

			control.LinealityChanged += adjustVisibility;

			this.control = control;
		}

		private void adjustVisibility(object sender, EventArgs e)
		{
			foreach (PersonNode child in ChildNodes) {
				if (control.RootNode == this
				    || (control.Lineality == Lineality.Cognatic)
				    || (control.Lineality == Lineality.Agnatic && Person.Gender == Gender.Male)
				    || (control.Lineality == Lineality.Uterine && Person.Gender == Gender.Female))
					child.Show();
				else
					child.Hide();
			}
		}

		public void Hide()
		{
			Visible = false;
		}

		public void Show()
		{
			Visible = true;
		}

		public void Destroy()
		{
			control.LinealityChanged -= adjustVisibility;
			foreach (PersonNode child in ChildNodes)
				child.Destroy();
		}

		private static string formatText(Person p)
		{
			return string.Format("{0} {1}\n(*{2}, ✝ {3})",
			                     p.Firstname,
			                     p.Birthname,
			                     p.YearOfBirth,
			                     p.YearOfDeath
			);
		}
	}
}