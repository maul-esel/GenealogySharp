using System;
using System.Collections.Generic;
using System.Linq;

namespace Genealogy.Succession
{
	public partial class Seniority : AbstractSuccessionStrategy
	{
		private readonly Sorting sorting;
		private readonly Dictionary<Person, int> ancestorSortNumber = new Dictionary<Person, int>();

		public Seniority(IPreferenceFilter[] preferenceFilters, Lineage lineage, Sorting sorting)
			: base(preferenceFilters, lineage)
		{
			this.sorting = sorting;
		}

		public override Person successorTo(Reign[] previousReigns)
		{
			ancestorSortNumber.Clear();

			int yearOfSuccession = previousReigns[previousReigns.Length - 1].End;
			Person previousRuler = previousReigns[previousReigns.Length - 1].Ruler;
			Person firstRuler = previousReigns[0].Ruler;

			ancestorSortNumber[firstRuler] = 0;

			Person[] directConnection = findAncestorPath(firstRuler, previousRuler);

			// get generation of previous ruler
			IEnumerable<Person> generation = new[] { previousReigns[0].Ruler };
			for (int i = 1; i < directConnection.Length; ++i)
				generation = nextGeneration(generation);

			// handle generation of previous ruler
			Person successor = generation
				.SkipWhile(p => p != previousRuler)
				.FirstOrDefault(p => isValidSuccessor(p, yearOfSuccession));
			if (successor != null)
				return successor;

			// handle future generations
			generation = nextGeneration(generation);
			while (generation.Any()) {
				successor = generation.FirstOrDefault(p => isValidSuccessor(p, yearOfSuccession));
				if (successor != null)
					return successor;
				generation = nextGeneration(generation);
			};
			return null;
		}

		private IEnumerable<Person> nextGeneration(IEnumerable<Person> current)
		{
			return sort(
				current
				.Where(p => shouldConsiderDescendants(p))
				.SelectMany(p => {
					if (sorting == Sorting.AncestorBased) {
						int i = 0;
						foreach (Person child in p.Children.OrderBy(c => c.YearOfBirth))
							ancestorSortNumber[child] = ancestorSortNumber[p] * 10 + i++;
					}
					return p.Children;
				})
			);
		}

		protected override IOrderedEnumerable<Person> sort(IEnumerable<Person> persons)
		{
			switch (sorting) {
				case Sorting.AncestorBased:
					return base.sort(persons).ThenBy(p => ancestorSortNumber[p]);
				case Sorting.AgeBased:
				default:
					return base.sort(persons).ThenBy(p => p.YearOfBirth);
			}
		}
	}
}