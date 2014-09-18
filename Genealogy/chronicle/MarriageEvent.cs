using System;

namespace Genealogy.Chronicle
{
	public class MarriageEvent : Event
	{
		public MarriageEvent(Marriage m)
		: base(m.Start)
		{
			Marriage = m;
		}

		public Marriage Marriage {
			get;
			private set;
		}
	}
}

