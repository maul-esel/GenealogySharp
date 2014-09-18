using System;

namespace Genealogy.Chronicle
{
	public class DeathEvent : Event
	{
		public DeathEvent(Person p)
		: base(p.YearOfDeath)
		{
			Person = p;
		}

		public Person Person {
			get;
			private set;
		}
	}
}
