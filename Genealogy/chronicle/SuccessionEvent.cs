using System;

namespace Genealogy.Chronicle
{
	public class SuccessionEvent : EventBase
	{
		public SuccessionEvent(Reign r)
		: base(r.Start, getMessage(r)) { }

		private static string getMessage(Reign r)
		{
			var previous = r.Title.Reigns [r.SuccessionIndex - 2].Ruler;
			return r + " follows " + previous.Firstname + " " + previous.getLastname();
		}
	}
}

