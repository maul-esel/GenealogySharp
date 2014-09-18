using System;

namespace Genealogy.Chronicle
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

