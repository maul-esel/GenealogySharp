using System;
using System.Linq;

namespace Genealogy.Succession
{
	/* prefer males over females, within gender sort by age */
	public class MalePreference : GenderPreference
	{
		#region singleton
		private static MalePreference instance;

		protected MalePreference() { }

		public static MalePreference Instance {
			get {
				if (instance == null)
					instance = new MalePreference();
				return instance;
			}
		}
		#endregion

		public Person firstChild(Person self)
		{
			var children = self.Children.OrderBy(c => c.YearOfBirth);
			return children.FirstOrDefault(c => c.Gender == Gender.Male) // oldest son
				?? children.FirstOrDefault(); // else oldest daughter
		}

		public Person nextSibling(Person self, Person parent)
		{
			var youngerSiblings = parent.Children
				.Where(c => c.YearOfBirth > self.YearOfBirth)
				.OrderBy(c => c.YearOfBirth);
			return youngerSiblings.FirstOrDefault(c => c.Gender == Gender.Male)
				?? youngerSiblings.FirstOrDefault();
		}

		public Person nextUncleOrAunt(Person parent, Person grandparent)
		{
			var unclesAndAunts = grandparent.Children
				.Where (c => c.YearOfBirth > parent.YearOfBirth)
				.OrderBy (c => c.YearOfBirth);
			return unclesAndAunts.FirstOrDefault(c => c.Gender == Gender.Male)
				?? unclesAndAunts.FirstOrDefault();
		}
	}
}

