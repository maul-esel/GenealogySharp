using System;

namespace Genealogy.Chronicle
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
