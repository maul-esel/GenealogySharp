using System;

namespace Genealogy
{
	public static class Assertions
	{
		public static void assertMale(this Person p) {
			if (p.Gender != Gender.Male)
				throw new Exception();
		}

		public static void assertFemale(this Person p) {
			if (p.Gender != Gender.Female)
				throw new Exception();
		}

		public static void assertAlive(this Person p, int year) {
			if (p.YearOfBirth > year || p.YearOfDeath < year)
				throw new Exception();
		}

		public static void assertBefore(this int i, int max) {
			if (i > max)
				throw new Exception();
		}

		public static void assertBetween(this int i, int min, int max) {
			if (i < min || i > max)
				throw new Exception();
		}
	}
}

