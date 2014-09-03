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
		private readonly List<Realm> realms = new List<Realm>();

		public int ID {
			get;
			private set;
		}

		public Rank Rank {
			get;
			private set;
		}

		public Realm[] Realms {
			get { return realms.ToArray(); }
		}

		public int Established {
			get;
			private set;
		}
		#endregion

		public Title(int id, Person firstRuler, int established, SuccessionStrategy strategy, Rank rank)
		{
			firstRuler.assertAlive(established);

			this.ID = id;
			this.Established = established;
			this.strategy = strategy;
			this.Rank = rank;

			reigns.Add(new Reign (this, firstRuler, established));
			calculateReigns();
		}

		internal void AddRealm(Realm r)
		{
			if (!realms.Contains(r))
				realms.Add(r);
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

