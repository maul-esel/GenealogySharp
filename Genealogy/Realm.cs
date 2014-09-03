using System;
using System.Collections.Generic;

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

		internal void AddFiefdom(Realm fief)
		{
			if (!fiefdoms.Contains(fief))
				fiefdoms.Add(fief);
		}
	}
}

