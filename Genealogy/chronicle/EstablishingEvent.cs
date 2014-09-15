using System;

namespace Genealogy.Chronicle
{
	public class EstablishingEvent : EventBase
	{
		public EstablishingEvent(Title t)
		: base(t.Established, getMessage(t))
		{
			Title = t;
		}

		public Title Title {
			get;
			private set;
		}
		private static string getMessage(Title t)
		{
			return "New established regime: " + t.Reigns[0].ToString(t.Established);
		}
	}
}
