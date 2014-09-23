using System;

namespace Genealogy
{
	public static class Assertions
	{
		public static void assertMale(this Person p) {
			if (p.Gender != Gender.Male)
				throw new Exception(string.Format("Expected person '{0} {1}, born {2}' to be male", p.Firstname, p.Birthname, p.YearOfBirth));
		}

		public static void assertFemale(this Person p) {
			if (p.Gender != Gender.Female)
				throw new Exception(string.Format("Expected person '{0} {1}, born {2}' to be female", p.Firstname, p.Birthname, p.YearOfBirth));
		}

		public static void assertAlive(this Person p, int year) {
			if (!p.isAlive(year))
				throw new Exception(string.Format("Expected person '{0} {1}, alive {2} - {3}' to be alive in {4}",
				                                  p.Firstname,
				                                  p.Birthname,
				                                  p.YearOfBirth,
				                                  p.YearOfDeath,
				                                  year));
		}

		public static void assertBefore(this int i, int max) {
			if (i > max)
				throw new Exception("Expected year " + i + " to be before "+ max);
		}

		public static void assertBetween(this int i, int min, int max) {
			if (i < min || i > max)
				throw new Exception("Expected " + i + " to be between " + min + " and " + max);
		}
	}
}

