using System;
using System.Linq;

namespace Genealogy.Succession
{
	public class MaleOnlyPreference : GenderPreference
	{
		#region singleton
		private static MaleOnlyPreference instance;

		protected MaleOnlyPreference() { }

		public static MaleOnlyPreference Instance {
			get {
				if (instance == null)
					instance = new MaleOnlyPreference ();
				return instance;
			}
		}
		#endregion

		public Person firstChild(Person self)
		{
			return self.Children
				.Where(c => c.Gender == Gender.Male)
				.OrderBy(c => c.YearOfBirth)
				.FirstOrDefault();
		}

		public Person nextSibling(Person self, Person parent)
		{
			return parent.Children
				.Where(c => c.Gender == Gender.Male)
				.Where(c => c.YearOfBirth > self.YearOfBirth)
				.OrderBy(c => c.YearOfBirth)
				.FirstOrDefault();
		}

		public Person nextUncleOrAunt(Person parent, Person grandparent)
		{
			return grandparent.Children
				.Where(c => c.Gender == Gender.Male)
				.Where(c => c.YearOfBirth > parent.YearOfBirth)
				.OrderBy(c => c.YearOfBirth)
				.FirstOrDefault();
		}
	}
}

