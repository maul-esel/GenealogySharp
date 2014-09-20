using System;
using System.Collections.Generic;

namespace Genealogy.Succession
{
	public abstract class AbstractSuccessionStrategy : ISuccessionStrategy
	{
		public abstract Person successorTo(Reign[] previousReigns);

		protected readonly IPreferenceFilter preferenceFilter;
		protected readonly Lineage lineage;

		protected AbstractSuccessionStrategy(IPreferenceFilter preferenceFilter, Lineage lineage)
		{
			this.preferenceFilter = preferenceFilter;
			this.lineage = lineage;
		}

		protected bool isValidSuccessor(Person p, int yearOfSuccession)
		{
			return preferenceFilter.ShouldConsider(p) && p.isAlive(yearOfSuccession);
		}

		protected bool shouldConsiderDescendants(Person p)
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

		protected Person[] findAncestorPath(Person ancestor, Person descendant)
		{
			Person[] directConnection = DijkstraAlgorithm<Person>.FindShortestLink(
				ancestor,
				descendant,
				person => person.Children
			);
			if (directConnection == null)
				throw new Exception();
			return directConnection;
		}
	}
}