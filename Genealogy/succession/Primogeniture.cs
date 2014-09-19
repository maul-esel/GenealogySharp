using System;
using System.Collections.Generic;
using System.Linq;

namespace Genealogy.Succession
{
	public class Primogeniture : SuccessionStrategy
	{
		private readonly IPreferenceFilter preferenceFilter;
		private readonly Lineality lineality;

		public Primogeniture(IPreferenceFilter preferenceFilter, Lineality lineality)
		{
			this.preferenceFilter = preferenceFilter;
			this.lineality = lineality;
		}

		public Person successorTo(Person previousRuler, Person firstRuler)
		{
			Person[] directConnection = DijkstraAlgorithm<Person>.FindShortestLink(firstRuler, previousRuler, person => person.Children);
			if (directConnection == null)
				throw new Exception();

			List<Person> traversed = new List<Person>();
			foreach (Person ancestor in directConnection.Reverse()) {
				Person successor = searchDescendants(ancestor, previousRuler.YearOfDeath, traversed);
				if (successor != null)
					return successor;
			}

			return null;
		}

		/// <summary>
		/// Searches for the successor within the descendants of <paramref name="self"/>.
		/// </summary>
		/// <returns>the preferable successor</returns>
		/// <param name="self">the person whose descendants are searched</param>
		/// <param name="year">the year of succession</param>
		/// <param name="traversed">a list of already traversed persons, to avoid traversing them multiple times</param>
		private Person searchDescendants(Person self, int year, List<Person> traversed) {
			if (!traversed.Contains(self) && shouldConsiderDescendants(self)) {
				traversed.Add(self);

				var children = self.Children
					.OrderByDescending(c => c, preferenceFilter)
					.ThenBy(c => c.YearOfBirth);

				foreach (Person child in children) {
					if (preferenceFilter.ShouldConsider(child) && child.isAlive(year))
						return child;

					Person successor = searchDescendants(child, year, traversed);
					if (successor != null)
						return successor;
				}
			}
			return null;
		}

		private bool shouldConsiderDescendants(Person p)
		{
			switch (lineality) {
				case Lineality.agnatic:
					return p.Gender == Gender.Male;
				case Lineality.uterine:
					return p.Gender == Gender.Female;
				default:
					return true;
			}
		}
	}
}