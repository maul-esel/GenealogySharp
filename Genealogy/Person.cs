using System;
using System.Collections.Generic;
using System.Linq;

using Genealogy.Chronicle;

namespace Genealogy
{
	public class Person : IEventProvider
	{
		#region attributes
		/* can be null */
		private readonly Marriage parentMarriage;

		private readonly string birthname;
		private readonly List<Marriage> marriageList = new List<Marriage>();
		private readonly List<Reign> titles = new List<Reign>();

		public int ID {
			get;
			private set;
		}

		public int YearOfBirth {
			get;
			private set;
		}

		public int YearOfDeath {
			get;
			private set;
		}

		public Gender Gender {
			get;
			private set;
		}

		public string Firstname {
			get;
			private set;
		}
		#endregion

		#region constructors
		internal Person(int id, int birth, int death, Gender gender, string firstname, /* nullable */ Marriage parentMarriage)
		{
			birth.assertBefore(death);
			if (parentMarriage != null)
				birth.assertBetween(parentMarriage.Start, parentMarriage.End);

			this.ID = id;
			this.YearOfBirth = birth;
			this.YearOfDeath = death;
			this.Gender = gender;
			this.Firstname = firstname;
			this.parentMarriage = parentMarriage;

			if (Father != null)
				birthname = Father.Lastname;
		}

		public Person(int id, int birth, int death, Gender gender, string firstname, string birthname)
		: this(id, birth, death, gender, firstname, Marriage.NULL) {
			this.birthname = birthname;
		}
		#endregion

		#region derived attributes
		public Marriage[] Marriages {
			get { return marriageList.ToArray(); }
		}

		public string Birthname {
			get { return birthname; }
		}

		/* can be null */
		public Person Father {
			get { return parentMarriage == null ? null : parentMarriage.Husband; }
		}
		
		/* can be null */
		public Person Mother {
			get { return parentMarriage == null ? null : parentMarriage.Wife; }
		}

		public Person[] Children {
			get { return (from m in Marriages from c in m.Children select c).ToArray(); }
		}

		public string Lastname {
			get { return getLastname(YearOfDeath); }
		}

		public string getLastname(int year)
		{
			if (Gender == Gender.Male || Marriages.Count() == 0 || year <= Marriages.First().Start) // male or not (yet) married
				return Birthname;

			if (currentMarriage(year) == null) // no longer married (i.e. widowed)
				return Marriages.Last().Husband.Birthname;
			else // still married
				return currentMarriage(year).Husband.Birthname;
		}

		public Marriage currentMarriage(int year)
		{
			return Marriages.FirstOrDefault(marriage => marriage.Start < year && year <= marriage.End);
		}

		public bool isAlive(int year)
		{
			return YearOfBirth <= year && YearOfDeath >= year;
		}

		public Reign[] Titles {
			get { return titles.ToArray(); }
		}

		public Reign[] getTitles(int year)
		{
			return titles.Where(r => r.Start < year).ToArray();
		}

		#endregion

		public IEnumerable<Event> Events {
			get {
				Event[] events = new Event[] { new BirthEvent(this), new DeathEvent(this) };
				if (Gender == Gender.Female)
					return events;
				return events.Concat(
					from m in Marriages
					from e in m.Events
					select e
				);
			}
		}

		#region marriages
		public Marriage marryTo(Person spouse, int year)
		{
			this.assertAlive(year);
			spouse.assertAlive(year);

			Marriage marriage = new Marriage(Gender == Gender.Male ? this : spouse,
			                                 Gender == Gender.Male ? spouse : this,
			                                 year);
			addMarriage(marriage);
			try {
				spouse.addMarriage(marriage);
			} catch (ConflictingMarriagesException e) {
				marriageList.Remove(marriage); // reverse own addition to avoid inconsistent data
				throw e;
			}
			return marriage;
		}

		private void addMarriage(Marriage marriage)
		{
			var conflictingMarriages =
				from m in Marriages
				where marriage.Start < m.End && marriage.End > m.Start
				select m;
			if (conflictingMarriages.Count() > 0)
				throw new ConflictingMarriagesException(conflictingMarriages.First(), marriage);

			marriageList.Add(marriage);
		}
		#endregion

		#region titles
		internal void AddTitle(Reign r)
		{
			if (r.Ruler != this)
				throw new Exception();
			if (!titles.Contains(r))
				titles.Add(r);
		}

		internal void RemoveTitle(Reign r)
		{
			titles.Remove(r);
		}
		#endregion

		public override string ToString()
		{
			return ToString(YearOfDeath);
		}

		public string ToString(int year)
		{
			var titles = getTitles(year);
			if (titles.Length == 0)
				return Firstname + " " + getLastname(year);
			else if (titles.Length == 1)
				return string.Format(
					"{0} {1}. {2}, {3} of {4}, {5}",
					Firstname,
					RomanNumerals.ToRomanNumeral(titles[0].NameIndex),
					getLastname (year),
					titles[0].Title.Rank,
					titles[0].Title.RuledTerritory.Name,
					(year >= YearOfDeath) ? (YearOfBirth + " - " + YearOfDeath) : ("* " + YearOfBirth)
				);
			else
				return string.Format (
					"{0} {1}, {2}\n\t{3}",
					Firstname,
					getLastname (year),
					(year >= YearOfDeath) ? (YearOfBirth + " - " + YearOfDeath) : ("* " + YearOfBirth),
					string.Join("\n\t", titles.Select(r => r.ToString(year)))
				);
		}
	}
}