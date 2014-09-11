using System;
using System.Collections.Generic;
using System.Linq;

using Genealogy.Chronicle;

namespace Genealogy
{
	public class Reign : IEventProvider
	{
		public Title Title {
			get;
			private set;
		}

		public Person Ruler {
			get;
			private set;
		}

		public int Start {
			get;
			private set;
		}

		internal Reign(Title title, Person ruler, int start)
		{
			ruler.assertAlive(start);

			this.Title = title;
			this.Ruler = ruler;
			this.Start = start;

			ruler.AddTitle(this);
		}

		public IEnumerable<Event> Events {
			get {
				if (SuccessionIndex == 1)
					return new Event[] { };
				return new Event[] { new SuccessionEvent(this) };
			}
		}

		public int End {
			get { return Ruler.YearOfDeath; }
		}

		public uint SuccessionIndex {
			get { return (uint)Title.Reigns.ToList().IndexOf(this) + 1; }
		}

		public uint NameIndex {
			get { return (uint)Title.Reigns.Take((int)SuccessionIndex).Where(r => r.Ruler.Firstname == Ruler.Firstname).Count(); }
		}

		public override string ToString()
		{
			return ToString(End);
		}

		public string ToString(int year)
		{
			return string.Format(
				"{0} {1}. {2}, {3}. {4} of {5} {6}",
				Ruler.Firstname,
				RomanNumerals.ToRomanNumeral(NameIndex),
				Ruler.getLastname(year),
				SuccessionIndex,
				Title.Rank,
				joinRealmNames(Title.Realms),
				(year >= End) ? (Start + " - " + End) : ("since " + Start)
			);
		}

		private static string joinRealmNames(IEnumerable<Realm> realms)
		{
			if (realms.Count() == 1)
				return realms.First().Name;
			return string.Join(", ", realms.Take(realms.Count() - 1).Select(r => r.Name)) + " and " + realms.Last().Name;
		}
	}
}