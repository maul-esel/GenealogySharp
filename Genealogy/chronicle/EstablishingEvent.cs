using System;

namespace Genealogy.Chronicle
{
	public class EstablishingEvent : EventBase
	{
		public EstablishingEvent(Title t)
		: base(t.Established, getMessage(t)) { }

		private static string getMessage(Title t)
		{
			return "New established regime: " + t.Reigns[0];
		}
	}
}

