using System;

namespace Genealogy.TestApplication
{
	class Program
	{
		public static void Main()
		{
			var s = new Storage("testData.xml");
			foreach (var c in s.Realms)
				Console.WriteLine("Realm: " + c.Name);
			foreach (var p in s.Persons)
				Console.WriteLine("Person: " + p);
			foreach (var t in s.Titles)
				foreach (var r in t.Reigns)
					Console.WriteLine(r);
			var chronicle = new Genealogy.Chronicle.Chronicle(s);
			Console.WriteLine("\n\nCHRONICLE:\n------------------\n");
			Console.WriteLine(chronicle.ToString());
		}
	}
}
