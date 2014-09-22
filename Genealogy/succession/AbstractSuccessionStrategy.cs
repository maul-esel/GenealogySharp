using System;
using System.Collections.Generic;
using System.Linq;

namespace Genealogy.Succession
{
	public abstract partial class AbstractSuccessionStrategy : ISuccessionStrategy
	{
		public abstract Person successorTo(Reign[] previousReigns);

		private Title title;
		public Title Title {
			get { return title; }
			set {
				if (title != null)
					throw new InvalidOperationException();
				title = value;
				comparers = preferenceFilters.Select(pref => new PreferenceFilterComparer(pref, value));
			}
		}

		private IEnumerable<IComparer<Person>> comparers;

		protected readonly IPreferenceFilter[] preferenceFilters;
		protected readonly Lineage lineage;

		protected AbstractSuccessionStrategy(IPreferenceFilter[] preferenceFilters, Lineage lineage)
		{
			this.preferenceFilters = preferenceFilters;
			this.lineage = lineage;
		}

		protected bool isValidSuccessor(Person p, int yearOfSuccession)
		{
			return preferenceFilters.All(filter => filter.ShouldConsider(p, Title)) && p.isAlive(yearOfSuccession);
		}

		protected virtual IOrderedEnumerable<Person> sort(IEnumerable<Person> persons)
		{
			if (preferenceFilters.Length == 0)
				return persons.OrderBy(p => 0);

			IOrderedEnumerable<Person> orderedPersons = persons.OrderByDescending(p => p, comparers.First());
			foreach (IComparer<Person> preference in comparers.Skip(1))
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
				throw new Exception();
			return directConnection;
		}
	}
}