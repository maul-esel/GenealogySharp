using System;

namespace Genealogy.Events
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