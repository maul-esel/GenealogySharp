using System;

namespace Genealogy
{
	public class Country
	{
		public string Name {
			get;
			private set;
		}

		public Country(string name)
		{
			this.Name = name;
		}

		public override bool Equals(object obj)
		{
			if (obj is Country)
				return (obj as Country).Name.Equals(Name);
			return false;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public override string ToString ()
		{
			return Name;
		}
	}
}

