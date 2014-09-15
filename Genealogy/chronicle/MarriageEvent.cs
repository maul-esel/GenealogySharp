using System;

namespace Genealogy.Chronicle
{
	public class MarriageEvent : EventBase
	{
		public MarriageEvent(Marriage m)
		: base(m.Start, getMessage(m))
		{
			Marriage = m;
		}

		public Marriage Marriage {
			get;
			private set;
		}

		private static string getMessage(Marriage m)
		{
			return string.Format (
				"Marriage of {0} {1} to {2} {3}",
				m.Husband.Firstname,
				m.Husband.getLastname(m.Start),
				m.Wife.Firstname,
				m.Wife.getLastname(m.Start)
			);
			// TODO: titles of both; birthname for widows
		}
	}
}

