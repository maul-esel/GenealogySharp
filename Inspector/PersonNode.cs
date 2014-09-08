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
			switch (control.Lineality) {
				case Lineality.Agnatic:
					Visible = Person.Gender == Gender.Male;
					break;
				case Lineality.Uterine:
					Visible = Person.Gender == Gender.Female;
					break;
				case Lineality.Cognatic:
					Visible = true;
					break;
			}
		}

		public void Hide()
		{
			Visible = false;
		}

		public void Destroy()
		{
			control.LinealityChanged -= adjustVisibility;
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