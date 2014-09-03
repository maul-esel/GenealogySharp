using System;

namespace Genealogy.Succession
{
	public class CognaticLineality : Lineality
	{
		#region singleton
		private static CognaticLineality instance;

		protected CognaticLineality() { }

		public static CognaticLineality Instance {
			get {
				if (instance == null)
					instance = new CognaticLineality();
				return instance;
			}
		}
		#endregion

		public bool considerChildren(Person parent)
		{
			return true;
		}
	}
}

