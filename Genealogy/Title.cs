using System;
using System.Collections.Generic;
using System.Linq;

using Genealogy.Chronicle;
using Genealogy.Succession;

namespace Genealogy
{
	public class Title : IEventProvider
	{
		#region attributes
		private readonly SuccessionStrategy strategy;
		private readonly List<Reign> reigns = new List<Reign>();

		public Rank Rank {
			get;
			private set;
		}

		public Country RuledTerritory {
			get;
			private set;
		}

		public int Established {
			get;
			private set;
		}
		#endregion

		public Title(Person firstRuler, int established, SuccessionStrategy strategy, Country ruledTerritory, Rank rank)
		{
			firstRuler.assertAlive(established);

			this.Established = established;
			this.strategy = strategy;
			this.RuledTerritory = ruledTerritory;
			this.Rank = rank;

			reigns.Add(new Reign (this, firstRuler, established));
			calculateReigns();
		}

		public IEnumerable<Event> Events {
			get {
				return
					(from r in Reigns
					from e in r.Events
					select e)
				.Concat(new Event[] { new EstablishingEvent(this) });
			}
		}

		#region reigns
		public Reign getReign(int year)
		{
			return Reigns.FirstOrDefault(reign => reign.Start <= year && reign.End > year);
		}

		public Reign[] Reigns
		{
			get { return reigns.ToArray(); }
		}

		private void calculateReigns()
		{
			Person next;
			for (Person last = reigns.Last().Ruler; last != null; last = next) {
				next = strategy.successorTo(last, reigns[0].Ruler);
				if (next != null)
					reigns.Add(new Reign(this, next, last.YearOfDeath));
			}
		}
		#endregion
	}
}

