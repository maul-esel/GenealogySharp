using System;
using System.Collections.Generic;
using System.Linq;

namespace Genealogy
{
	public class Realm
	{
		private readonly List<Realm> fiefdoms = new List<Realm>();

		public string Name {
			get;
			private set;
		}

		public Realm ContainingRealm {
			get;
			private set;
		}

		public Realm[] Fiefdoms {
			get { return fiefdoms.ToArray(); }
		}

		public Title Ruler {
			get;
			private set;
		}

		public Realm(string name, Realm container, Title ruler)
		: this(name, ruler)
		{
			this.ContainingRealm = container;
			container.AddFiefdom(this);
		}

		public Realm(string name, Title ruler)
		{
			this.Name = name;
			this.Ruler = ruler;
			ruler.AddRealm(this);
		}

		private void AddFiefdom(Realm fief)
		{
			if (!fiefdoms.Contains(fief))
				fiefdoms.Add(fief);
		}

		public static string JoinRealmNames(IEnumerable<Realm> realms)
		{
			if (realms.Count() == 1)
				return realms.First().Name;
			return string.Join(", ", realms.Take(realms.Count() - 1).Select(r => r.Name)) + " and " + realms.Last().Name;
		}
	}
}