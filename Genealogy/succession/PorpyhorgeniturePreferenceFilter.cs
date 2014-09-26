using System.Linq;

namespace Genealogy.Succession
{
	public class PorpyhorgeniturePreferenceFilter : IPreferenceFilter
	{
		public enum FilterKind {
			all,
			bornToRuler,
			bornWhileRuling
		}

		public enum SortingKind {
			none,
			bornToRuler,
			bornWhileRuling,
			bornWhileRuling_bornToRuler
		}

		private readonly FilterKind filter;
		private readonly SortingKind sorting;

		public PorpyhorgeniturePreferenceFilter(FilterKind filter, SortingKind sorting)
		{
			this.filter = filter;
			this.sorting = sorting;
		}

		public int Compare(Person x, Person y, Title title)
		{
			switch (sorting) {
				case SortingKind.bornToRuler:
					if (bornToRuler(x, title) == bornToRuler(y, title))
						return 0;
					else if (bornToRuler(x, title))
						return 1;
					return -1;
				case SortingKind.bornWhileRuling:
					if (bornWhileRuling(x, title) == bornWhileRuling(y, title))
						return 0;
					else if (bornWhileRuling(x, title))
						return 1;
					return -1;
				case SortingKind.bornWhileRuling_bornToRuler:
					if (bornWhileRuling(x, title) && bornWhileRuling(y, title))
						return 0;
					else if (bornWhileRuling(x, title))
						return 1;
					else if (bornWhileRuling(y, title))
						return -1;
					goto case SortingKind.bornToRuler;
				case SortingKind.none:
				default:
					return 0;
			}
		}

		public bool ShouldConsider(Person p, Title title)
		{
			switch (filter) {
				case FilterKind.bornWhileRuling:
					return bornWhileRuling(p, title);
				case FilterKind.bornToRuler:
					return bornToRuler(p, title);
				case FilterKind.all:
				default:
					return true;
			}
		}

		private bool bornToRuler(Person p, Title title)
		{
			return title.CalculatedReigns.Any(r => r.Ruler == p.Father || r.Ruler == p.Mother);
		}

		private bool bornWhileRuling(Person p, Title title)
		{
			return title.CalculatedReigns.Any(r => (r.Ruler == p.Father || r.Ruler == p.Mother) && (r.Start <= p.YearOfBirth && p.YearOfBirth <= r.End));
		}
	}
}