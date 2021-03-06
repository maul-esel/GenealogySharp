using System;

namespace Genealogy.Events
{
	public class BirthEvent : Event
	{
		public BirthEvent(Person p)
		: base(p.YearOfBirth)
		{
			Person = p;
		}

		public Person Person {
			get;
			private set;
		}
	}
}