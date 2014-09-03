using System;
using System.Linq;

namespace Genealogy.Chronicle
{
	public class Chronicle
	{
		public Event[] Events {
			get;
			private set;
		}

		public Chronicle(IEventProvider source) {
			Events =
				(from e in source.getEvents ()
				orderby e.Year ascending
				select e)
					.ToArray();
		}

		public override string ToString()
		{
			return String.Join(
				"\n",
				from e in Events
				group e by e.Year into yearEvents
				select yearEvents.Key + ": " +
					String.Join(
						"\n\t",
						from ev in yearEvents
						select ev.ToString()
					)
			);
		}
	}
}

