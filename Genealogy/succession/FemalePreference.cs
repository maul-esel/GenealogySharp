using System;
using System.Linq;

namespace Genealogy.Succession
{
	/* prefer females over males, within gender sort by age */
	public class FemalePreference : GenderPreference
	{
		#region singleton
		private static FemalePreference instance;

		protected FemalePreference() { }

		public static FemalePreference Instance {
			get {
				if (instance == null)
					instance = new FemalePreference();
				return instance;
			}
		}
		#endregion

		public Person firstChild(Person self)
		{
			var children = self.Children.OrderBy(c => c.YearOfBirth);
			return children.FirstOrDefault(c => c.Gender == Gender.Female) // oldest daughter
				?? children.FirstOrDefault(); // else oldest son
		}

		public Person nextSibling(Person self, Person parent)
		{
			var youngerSiblings = parent.Children
				.Where(c => c.YearOfBirth > self.YearOfBirth)
				.OrderBy(c => c.YearOfBirth);
			return youngerSiblings.FirstOrDefault(c => c.Gender == Gender.Female)
				?? youngerSiblings.FirstOrDefault();
		}

		public Person nextUncleOrAunt(Person parent, Person grandparent)
		{
			var unclesAndAunts = grandparent.Children
				.Where (c => c.YearOfBirth > parent.YearOfBirth)
				.OrderBy (c => c.YearOfBirth);
			return unclesAndAunts.FirstOrDefault(c => c.Gender == Gender.Female)
				?? unclesAndAunts.FirstOrDefault();
		}
	}
}

