using System;

namespace Genealogy.Chronicle
{
	public class DeathEvent : EventBase
	{
		public DeathEvent(Person p)
		: base(p.YearOfDeath, getMessage(p))
		{
			Person = p;
		}

		public Person Person {
			get;
			private set;
		}

		private static string getMessage(Person p) {
			return string.Format (
				"Death of {0} {1}",
				p.Firstname,
				p.Lastname
			) + (p.Lastname != p.Birthname ? " (née " + p.Birthname + ")" : "");
			// TODO: titles?
		}
	}
}
