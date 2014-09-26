using System;
using System.Collections.Generic;
using System.Linq;

namespace Genealogy.Succession
{
	public class Primogeniture : AbstractSuccessionStrategy
	{
		public Primogeniture(Title title, IPreferenceFilter[] preferenceFilters, Lineage lineage)
			: base(title, preferenceFilters, lineage)
		{
		}

		public override Person successorTo(Reign[] previousReigns)
		{
			Person[] directConnection = findAncestorPath(previousReigns[0].Ruler, previousReigns[previousReigns.Length - 1].Ruler);

			int yearOfSuccession = previousReigns[previousReigns.Length - 1].End;
			List<Person> traversed = new List<Person>();

			foreach (Person ancestor in directConnection.Reverse()) {
				Person successor = searchDescendants(ancestor, yearOfSuccession, traversed);
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

				foreach (Person child in sort(self.Children)) {
					if (isValidSuccessor(child, year))
						return child;

					Person successor = searchDescendants(child, year, traversed);
					if (successor != null)
						return successor;
				}
			}
			return null;
		}

		protected override IOrderedEnumerable<Person> sort(IEnumerable<Person> persons)
		{
			return base.sort(persons).ThenBy(p => p.YearOfBirth);
		}
	}
}