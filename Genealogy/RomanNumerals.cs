using System;
using System.Linq;

namespace Genealogy
{
	// source: http://stackoverflow.com/questions/7040289/converting-integers-to-roman-numerals/7445709#7445709
	public static class RomanNumerals
	{
		private static readonly string[][] numerals = new string[][] {
			new string[] { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" },
			new string[] { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" },
			new string[] { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" },
			new string[] { "", "M", "MM", "MMM" }
		};

		public static string ToRomanNumeral(uint n)
		{
			if (n <= 0 || n > 3999)
				throw new ArgumentOutOfRangeException("n", n, "n must be between 1 and 3999");

			return string.Join(
				"",
			    n.ToString()
					.Reverse()
					.Select((d, i) => numerals[i][int.Parse(d.ToString())])
			);
		}
	}
}

