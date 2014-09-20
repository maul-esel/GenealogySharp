using System;
using System.Collections.Generic;
using System.Linq;

namespace Genealogy.Succession
{
	/// <summary>
	/// Implements a "proximity of blood" succession strategy that prefers descendants
	/// over other relatives in case of same blood proximity.
	///
	/// Can be configured to prefer / filter based on genders and lineage.
	/// </summary>
	public class ProximityOfBlood : ISuccessionStrategy
	{
		private readonly IPreferenceFilter preferenceFilter;
		private readonly Lineage lineage;

		public ProximityOfBlood(IPreferenceFilter preferenceFilter, Lineage lineage)
		{
			this.preferenceFilter = preferenceFilter;
			this.lineage = lineage;
		}

		public Person successorTo(Reign[] previousReigns)
		{
			Person previousRuler = previousReigns[previousReigns.Length - 1].Ruler;

			Person[] directConnection = DijkstraAlgorithm<Person>.FindShortestLink(
				previousReigns[0].Ruler,
				previousRuler,
				person => person.Children
			);
			if (directConnection == null)
				throw new Exception();

			List<Person> traversed = new List<Person>();
			return searchRelatives(previousRuler, directConnection.Reverse().Skip(1), previousRuler.YearOfDeath, traversed);
		}

		private Person searchRelatives(Person p, IEnumerable<Person> ancestorChain, int yearOfSuccession, List<Person> traversed)
		{
			if (!traversed.Contains(p)) {
				traversed.Add(p);

				Person desc = searchDescendants(p, yearOfSuccession);
				Person anc = null;
				if (ancestorChain.Any())
					anc = searchRelatives(ancestorChain.First(), ancestorChain.Skip(1), yearOfSuccession, traversed);

				if (desc == null)
					return anc;
				else if (anc == null || Person.RelationshipDegree(p, desc) <= Person.RelationshipDegree(p, anc))
					return desc;
				return anc;
			}
			return null;
		}

		private Person searchDescendants(Person self, int yearOfSuccession)
		{
			for (var descendants = sort(self.Children); descendants.Any(); descendants = nextLevelDescendants(descendants)) {
				Person result = descendants.FirstOrDefault(d => isValidSuccessor(d, yearOfSuccession));
				if (result != null)
					return result;
			}
			return null;
		}

		private bool isValidSuccessor(Person p, int yearOfSuccession)
		{
			return preferenceFilter.ShouldConsider(p) && p.isAlive(yearOfSuccession);
		}

		private IEnumerable<Person> nextLevelDescendants(IEnumerable<Person> descendants)
		{
			return sort(descendants.Where(d => shouldConsiderChildren(d)).SelectMany(d => d.Children));
		}

		private IEnumerable<Person> sort(IEnumerable<Person> persons)
		{
			return persons
				.OrderByDescending(p => p, preferenceFilter)
				.ThenBy(p => p.YearOfBirth);
		}

		private bool shouldConsiderChildren(Person p)
		{
			switch (lineage) {
				case Lineage.Agnatic:
					return p.Gender == Gender.Male;
				case Lineage.Uterine:
					return p.Gender == Gender.Female;
				default:
					return true;
			}
		}
	}
}