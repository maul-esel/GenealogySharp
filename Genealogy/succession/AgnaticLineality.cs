using System;

namespace Genealogy.Succession
{
	public class AgnaticLineality : Lineality
	{
		#region singleton
		private static AgnaticLineality instance;

		protected AgnaticLineality() { }

		public static AgnaticLineality Instance {
			get {
				if (instance == null)
					instance = new AgnaticLineality();
				return instance;
			}
		}
		#endregion

		public bool considerChildren(Person parent)
		{
			return parent.Gender == Gender.Male;
		}
	}
}

