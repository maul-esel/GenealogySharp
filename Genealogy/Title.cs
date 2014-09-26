using System;
using System.Collections.Generic;
using System.Linq;

using Genealogy.Events;
using Genealogy.Succession;

namespace Genealogy
{
	public class Title : IEventProvider
	{
		#region attributes
		private readonly List<ISuccessionStrategy> strategies = new List<ISuccessionStrategy>();
		private readonly List<Reign> reigns = new List<Reign>();
		private readonly List<Realm> realms = new List<Realm>();

		public uint ID {
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

		public Title(uint id, Person firstRuler, int established, Rank rank)
		{
			firstRuler.assertAlive(established);

			this.ID = id;
			this.Established = established;
			this.Rank = rank;

			reigns.Add(new Reign(this, firstRuler, established));
		}

		internal void AddRealm(Realm r)
		{
			if (!realms.Contains(r))
				realms.Add(r);
		}

		internal void AddSuccessionStrategy(ISuccessionStrategy strategy)
		{
			strategies.Add(strategy);
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
		private bool hasCalculatedReigns = false;

		public Reign getReign(int year)
		{
			return Reigns.FirstOrDefault(reign => reign.Start <= year && reign.End > year);
		}

		public Reign[] Reigns
		{
			get {
				if (!hasCalculatedReigns)
					calculateReigns();
				return reigns.ToArray();
			}
		}

		private void calculateReigns()
		{
			for (Person next = findSuccessor();
			     next != null;
			     next = findSuccessor()
			) {
				reigns.Add(new Reign(this, next, reigns.Last().End));
			}
			hasCalculatedReigns = true;
		}

		private Person findSuccessor()
		{
			Reign[] previousReigns = reigns.ToArray();
			foreach (ISuccessionStrategy strategy in strategies) {
				Person successor = strategy.successorTo(previousReigns);
				if (successor != null)
					return successor;
			}
			return null;
		}
		#endregion
	}
}