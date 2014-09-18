using System;
using System.Linq;

namespace Genealogy.TestApplication
{
	class Program
	{
		public static void Main()
		{
			var s = new Storage("testData.xml");

			Console.WriteLine("Average lifespan: {0}\n\tmale: {1}\n\tfemale: {2}",
			                  s.Persons.Average(p => p.YearOfDeath - p.YearOfBirth),
			                  s.Persons.Where(p => p.Gender == Gender.Male).Average(p => p.YearOfDeath - p.YearOfBirth),
			                  s.Persons.Where(p => p.Gender == Gender.Female).Average(p => p.YearOfDeath - p.YearOfBirth)
			);
			Console.WriteLine("Average number of children: {0}\n\tmen: {1}\n\twomen: {2}",
			                  s.Persons.Average(p => p.Children.Length),
			                  s.Persons.Where(p => p.Gender == Gender.Male).Average(p => p.Children.Length),
			                  s.Persons.Where(p => p.Gender == Gender.Female).Average(p => p.Children.Length)
			);
			Console.WriteLine("Persons: {0}\n\tmale: {1} ({2} %)\n\tfemale: {3} ({4} %)",
			                  s.Persons.Length,
			                  s.Persons.Where(p => p.Gender == Gender.Male).Count(),
			                  s.Persons.Where(p => p.Gender == Gender.Male).Count() / (double)s.Persons.Length * 100,
			                  s.Persons.Where(p => p.Gender == Gender.Female).Count(),
			                  s.Persons.Where(p => p.Gender == Gender.Female).Count() / (double)s.Persons.Length * 100
			);
			var reigns = (from t in s.Titles from r in t.Reigns select r);
			Console.WriteLine("Rulers: {0}\n\tmale: {1} ({2} %)\n\tfemale: {3} ({4} %)",
			                  reigns.Count(),
			                  reigns.Where(r => r.Ruler.Gender == Gender.Male).Count(),
			                  reigns.Where(r => r.Ruler.Gender == Gender.Male).Count() / (double)reigns.Count() * 100,
			                  reigns.Where(r => r.Ruler.Gender == Gender.Female).Count(),
			                  reigns.Where(r => r.Ruler.Gender == Gender.Female).Count() / (double)reigns.Count() * 100
			);
			Console.WriteLine("Average reign length: {0}\n\tmale: {1} ({2} %)\n\tfemale: {3} ({4} %)",
			                  reigns.Average(r => r.End - r.Start),
			                  reigns.Where(r => r.Ruler.Gender == Gender.Male).Average(r => r.End - r.Start),
			                  reigns.Where(r => r.Ruler.Gender == Gender.Male).Average(r => r.End - r.Start) / (double)reigns.Average(r => r.End - r.Start) * 100,
			                  reigns.Where(r => r.Ruler.Gender == Gender.Female).Average(r => r.End - r.Start),
			                  reigns.Where(r => r.Ruler.Gender == Gender.Female).Average(r => r.End - r.Start) / (double)reigns.Average(r => r.End - r.Start) * 100
			);


			foreach (var c in s.Realms)
				Console.WriteLine("Realm: " + c.Name);
			foreach (var p in s.Persons)
				Console.WriteLine("Person: " + p);
			foreach (var t in s.Titles)
				foreach (var r in t.Reigns)
					Console.WriteLine(r);
			var chronicle = new Chronicle(s);
			Console.WriteLine("\n\nCHRONICLE:\n------------------\n");
			Console.WriteLine(chronicle.ToString());

			var unmarried = from p in s.Persons
				where p.Marriages.Length == 0
					orderby p.YearOfBirth
					group p by p.Gender into genderGroup
					select genderGroup;
			Console.WriteLine("Unmarried:\n\tmale:\n\t\t" + string.Join("\n\t\t", unmarried.First(g => g.Key == Gender.Male).Select(p => p.Firstname + " " + p.Birthname + " " + p.YearOfBirth + " - " + p.YearOfDeath + " " + p.ID)));
			Console.WriteLine("\n\tfemale:\n\t\t" + string.Join("\n\t\t", unmarried.First(g => g.Key == Gender.Female).Select(p => p.Firstname + " " + p.Birthname + " " + p.YearOfBirth + " - " + p.YearOfDeath + " " + p.ID)));
		}
	}
}