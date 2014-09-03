using System;
using System.Linq;

namespace Genealogy.Succession
{
	public class FemaleOnlyPreference : GenderPreference
	{
		#region singleton
		private static FemaleOnlyPreference instance;

		protected FemaleOnlyPreference() { }

		public static FemaleOnlyPreference Instance {
			get {
				if (instance == null)
					instance = new FemaleOnlyPreference();
				return instance;
			}
		}
		#endregion

		public Person firstChild(Person self)
		{
			return self.Children
				.Where(c => c.Gender == Gender.Female)
				.OrderBy(c => c.YearOfBirth)
				.FirstOrDefault();
		}

		public Person nextSibling(Person self, Person parent)
		{
			return parent.Children
				.Where(c => c.Gender == Gender.Female)
				.Where(c => c.YearOfBirth > self.YearOfBirth)
				.OrderBy(c => c.YearOfBirth)
				.FirstOrDefault();
		}

		public Person nextUncleOrAunt(Person parent, Person grandparent)
		{
			return grandparent.Children
				.Where(c => c.Gender == Gender.Female)
				.Where(c => c.YearOfBirth > parent.YearOfBirth)
				.OrderBy(c => c.YearOfBirth)
				.FirstOrDefault();
		}
	}
}

