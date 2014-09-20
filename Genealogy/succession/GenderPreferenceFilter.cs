namespace Genealogy
{
	public class GenderPreferenceFilter : IPreferenceFilter
	{
		public enum Kind {
			maleOnly,
			malePreference,
			femalePreference,
			femaleOnly
		}

		private readonly Kind kind;

		public GenderPreferenceFilter(Kind kind)
		{
			this.kind = kind;
		}

		public bool ShouldConsider(Person p)
		{
			switch (kind) {
				case Kind.maleOnly:
					return p.Gender == Gender.Male;
				case Kind.femaleOnly:
					return p.Gender == Gender.Female;
				default:
					return true;
			}
		}

		public int Compare(Person x, Person y)
		{
			if (x == null)
				return -1;
			else if (y == null)
				return 1;

			if (x.Gender == y.Gender)
				return 0;

			switch (kind) {
				case Kind.maleOnly:
				case Kind.malePreference:
					return x.Gender == Gender.Male ? 1 : -1;
				case Kind.femaleOnly:
				case Kind.femalePreference:
					return x.Gender == Gender.Female ? -1 : 1;
				default:
					return 0;
			}
		}
	}
}