using System;

namespace Genealogy.Succession
{
	public class UterineLineality : Lineality
	{
		#region singleton
		private static UterineLineality instance;

		protected UterineLineality() { }

		public static UterineLineality Instance {
			get {
				if (instance == null)
					instance = new UterineLineality();
				return instance;
			}
		}
		#endregion

		public bool considerChildren(Person parent)
		{
			return parent.Gender == Gender.Female;
		}
	}
}

