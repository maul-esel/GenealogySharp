using System;

namespace Genealogy.Events
{
	public abstract class Event
	{
		public int Year {
			get;
			private set;
		}

		protected Event(int year)
		{
			Year = year;
		}
	}
}

