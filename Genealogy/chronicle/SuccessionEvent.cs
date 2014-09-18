using System;

namespace Genealogy.Events
{
	public class SuccessionEvent : Event
	{
		public SuccessionEvent(Reign r)
		: base(r.Start)
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
	}
}
