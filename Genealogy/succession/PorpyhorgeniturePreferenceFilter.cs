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

		private readonly Title title;
		private readonly FilterKind filter;
		private readonly SortingKind sorting;

		public PorpyhorgeniturePreferenceFilter(Title title, FilterKind filter, SortingKind sorting)
		{
			this.title = title;
			this.filter = filter;
			this.sorting = sorting;
		}

		public int Compare(Person x, Person y)
		{
			switch (sorting) {
				case SortingKind.bornToRuler:
					if (bornToRuler(x) == bornToRuler(y))
						return 0;
					else if (bornToRuler(x))
						return 1;
					return -1;
				case SortingKind.bornWhileRuling:
					if (bornWhileRuling(x) == bornWhileRuling(y))
						return 0;
					else if (bornWhileRuling(x))
						return 1;
					return -1;
				case SortingKind.bornWhileRuling_bornToRuler:
					if (bornWhileRuling(x) && bornWhileRuling(y))
						return 0;
					else if (bornWhileRuling(x))
						return 1;
					else if (bornWhileRuling(y))
						return -1;
					goto case SortingKind.bornToRuler;
				case SortingKind.none:
				default:
					return 0;
			}
		}

		public bool ShouldConsider(Person p)
		{
			switch (filter) {
				case FilterKind.bornWhileRuling:
					return bornWhileRuling(p);
				case FilterKind.bornToRuler:
					return bornToRuler(p);
				case FilterKind.all:
				default:
					return true;
			}
		}

		private bool bornToRuler(Person p)
		{
			return title.CalculatedReigns.Any(r => r.Ruler == p.Father || r.Ruler == p.Mother);
		}

		private bool bornWhileRuling(Person p)
		{
			return title.CalculatedReigns.Any(r => (r.Ruler == p.Father || r.Ruler == p.Mother) && (r.Start <= p.YearOfBirth && p.YearOfBirth <= r.End));
		}
	}
}