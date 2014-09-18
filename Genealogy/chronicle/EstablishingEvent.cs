using System;

namespace Genealogy.Events
{
	public class EstablishingEvent : Event
	{
		public EstablishingEvent(Title t)
		: base(t.Established)
		{
			Title = t;
		}

		public Title Title {
			get;
			private set;
		}
	}
}
