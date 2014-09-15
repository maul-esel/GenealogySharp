using System;

namespace Genealogy.Chronicle
{
	public class BirthEvent : EventBase
	{
		public BirthEvent(Person p)
		: base(p.YearOfBirth, getMessage(p))
		{
			Person = p;
		}

		public Person Person {
			get;
			private set;
		}

		private static string getMessage(Person p) {
			if (p.Father == null)
				return string.Format("Birth of {0} {1}", p.Firstname, p.Birthname);
			return string.Format(
				"Birth of {0} {1} to {2} {3} and {4} {5} (née {6})",
				p.Firstname,
				p.Birthname,
				p.Father.Firstname,
				p.Father.getLastname(p.YearOfBirth),
				p.Mother.Firstname,
				p.Mother.getLastname(p.YearOfBirth),
				p.Mother.Birthname
			);
			// TODO: titles of parents?
		}
	}
}

