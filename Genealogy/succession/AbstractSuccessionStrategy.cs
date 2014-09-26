using System;
using System.Collections.Generic;
using System.Linq;

namespace Genealogy.Succession
{
	public abstract partial class AbstractSuccessionStrategy : ISuccessionStrategy
	{
		public abstract Person getSuccessor();

		private readonly Title title;
		public Title Title {
			get { return title; }
		}

		protected readonly IPreferenceFilter[] preferenceFilters;
		protected readonly Lineage lineage;

		protected AbstractSuccessionStrategy(Title title, IPreferenceFilter[] preferenceFilters, Lineage lineage)
		{
			this.title = title;
			this.preferenceFilters = preferenceFilters;
			this.lineage = lineage;

			title.AddSuccessionStrategy(this);
		}

		protected bool isValidSuccessor(Person p, int yearOfSuccession)
		{
			return preferenceFilters.All(filter => filter.ShouldConsider(p)) && p.isAlive(yearOfSuccession);
		}

		protected virtual IOrderedEnumerable<Person> sort(IEnumerable<Person> persons)
		{
			if (preferenceFilters.Length == 0)
				return persons.OrderBy(p => 0);

			IOrderedEnumerable<Person> orderedPersons = persons.OrderByDescending(p => p, preferenceFilters.First());
			foreach (IComparer<Person> preference in preferenceFilters.Skip(1))
				orderedPersons = orderedPersons.ThenByDescending(p => p, preference);
			return orderedPersons;
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
				throw new Exception(string.Format("Could not find ancestor path from '{0} {1} ({2} - {3})' down to '{4} {5} ({6} - {7})'",
				                    ancestor.Firstname, ancestor.Birthname, ancestor.YearOfBirth, ancestor.YearOfDeath,
				                    descendant.Firstname, descendant.Birthname, descendant.YearOfBirth, descendant.YearOfDeath));
			return directConnection;
		}
	}
}