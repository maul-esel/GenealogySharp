using System;
using System.Collections.Generic;

using Genealogy.Events;

namespace Genealogy
{
	public class Marriage : IEventProvider
	{
		private readonly List<Person> childrenList = new List<Person>();

		public Person Husband {
			get;
			private set;
		}

		public Person Wife {
			get;
			private set;
		}

		public int Start {
			get;
			private set;
		}

		internal Marriage(Person husband, Person wife, int year)
		{
			husband.assertMale();
			husband.assertAlive(year);
			wife.assertFemale();
			wife.assertAlive(year);

			this.Husband = husband;
			this.Wife = wife;
			this.Start = year;
		}

		public Person[] Children {
			get { return childrenList.ToArray (); }
		}

		public int End {
			get { return Math.Min(Husband.YearOfDeath, Wife.YearOfDeath); }
		}

		public Person addChild(uint id, int birth, int death, Gender gender, string firstname)
		{
			birth.assertBetween(Start, End);

			Person child = new Person(id, birth, death, gender, firstname, this);
			childrenList.Add(child);
			return child;
		}

		public IEnumerable<Event> Events {
			get {
				return new Event[] { new MarriageEvent(this) };
			}
		}
	}
}