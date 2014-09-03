using System;
using System.Linq;

namespace Genealogy.Succession
{
	/* both genders considered equally */
	public class AbsolutePreference : GenderPreference
	{
		#region singleton
		private static AbsolutePreference instance;

		protected AbsolutePreference() { }

		public static AbsolutePreference Instance {
			get {
				if (instance == null)
					instance = new AbsolutePreference();
				return instance;
			}
		}
		#endregion

		public Person firstChild(Person self)
		{
			return self.Children
				.OrderBy(c => c.YearOfBirth)
				.FirstOrDefault();
		}

		public Person nextSibling(Person self, Person parent)
		{
			return parent.Children
				.Where(c => c.YearOfBirth > self.YearOfBirth)
				.OrderBy(c => c.YearOfBirth)
				.FirstOrDefault();
		}

		public Person nextUncleOrAunt(Person parent, Person grandparent)
		{
			return grandparent.Children
				.Where(c => c.YearOfBirth > parent.YearOfBirth)
				.OrderBy(c => c.YearOfBirth)
				.FirstOrDefault();
		}
	}
}

