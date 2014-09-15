using System;

namespace Genealogy.Chronicle
{
	public class SuccessionEvent : EventBase
	{
		public SuccessionEvent(Reign r)
		: base(r.Start, getMessage(r))
		{
			Successor = r;
		}

		public Reign Successor {
			get;
			private set;
		}

		public Reign Predecessor {
			get { return Successor.Title.Reigns[Successor.SuccessionIndex - 2]; }
		}

		private static string getMessage(Reign r)
		{
			var previous = r.Title.Reigns[r.SuccessionIndex - 2];
			return r.ToString(r.Start) + " follows " + previous.ToString(r.Start);
		}
	}
}
