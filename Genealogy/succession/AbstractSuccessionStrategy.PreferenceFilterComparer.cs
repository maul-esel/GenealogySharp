using System.Collections.Generic;

namespace Genealogy.Succession
{
	partial class AbstractSuccessionStrategy
	{
		private class PreferenceFilterComparer : IComparer<Person>
		{
			private readonly IPreferenceFilter preference;
			private readonly Title title;

			public PreferenceFilterComparer(IPreferenceFilter preference, Title title)
			{
				this.preference = preference;
				this.title = title;
			}

			public int Compare(Person x, Person y)
			{
				return preference.Compare(x, y, title);
			}
		}
	}
}