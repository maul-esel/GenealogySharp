using System;
using System.Linq;

using Genealogy.Chronicle;

namespace Genealogy.TestApplication
{
	public class Chronicle
	{
		public Event[] Events {
			get;
			private set;
		}

		public Chronicle(IEventProvider source) {
			Events =
				(from e in source.Events
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
						select getMessage(ev)
					)
			);
		}

		private static string getMessage(Event ev)
		{
			if (ev is BirthEvent) {
				Person p = (ev as BirthEvent).Person;
				if (p.Father == null)
					return string.Format("Birth of {0} {1}", p.Firstname, p.Birthname);
				return string.Format(
					"Birth of {0} {1} to {2} {3} and {4} {5} (née {6})",
					p.Firstname,
					p.Birthname,
					p.Father.Firstname,
					p.Father.getLastname(p.YearOfBirth),
					p.Mother.Firstname,
					p.Mother.getLastname(p.YearOfBirth),
					p.Mother.Birthname
				);
				// TODO: titles of parents?
			} else if (ev is DeathEvent) {
				Person p = (ev as DeathEvent).Person;
				return string.Format(
					"Death of {0} {1}",
					p.Firstname,
					p.Lastname
				) + (p.Lastname != p.Birthname ? " (née " + p.Birthname + ")" : "");
				// TODO: titles?
			} else if (ev is EstablishingEvent) {
				Title t = (ev as EstablishingEvent).Title;
				return "New established regime: " + t.Reigns[0].ToString(t.Established);
			} else if (ev is MarriageEvent) {
				Marriage m = (ev as MarriageEvent).Marriage;
				return string.Format(
					"Marriage of {0} {1} to {2} {3}",
					m.Husband.Firstname,
					m.Husband.getLastname(m.Start),
					m.Wife.Firstname,
					m.Wife.getLastname(m.Start)
				);
				// TODO: titles of both; birthname for widows
			} else if (ev is SuccessionEvent) {
				Reign r = (ev as SuccessionEvent).Successor;
				Reign previous = (ev as SuccessionEvent).Predecessor;
				return r.ToString(r.Start) + " follows " + previous.ToString(r.Start);
			}
			return string.Empty;
		}
	}
}