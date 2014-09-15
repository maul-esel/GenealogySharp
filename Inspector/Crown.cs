using System.Drawing;

namespace Genealogy.Inspector
{
	public static class Crown
	{
		private static readonly Bitmap grid = new Bitmap(System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream("Genealogy.Inspector.resources.crowns.jpg"));

		private static Rectangle Crown1 = new Rectangle(70, 35, 80, 80);
		private static Rectangle Crown2 = new Rectangle(200, 35, 100, 100);
		private static Rectangle Crown3 = new Rectangle(330, 35, 100, 100);
		private static Rectangle Crown7 = new Rectangle(330, 150, 100, 100);
		private static Rectangle Crown8 = new Rectangle(460, 150, 100, 100);
		private static Rectangle Crown10 = new Rectangle(200, 265, 100, 100);
		private static Rectangle Crown12 = new Rectangle(200, 380, 100, 100);
		private static Rectangle Crown17 = new Rectangle(70, 490, 100, 100);

		public static Bitmap GetCrown(Rank rank)
		{
			grid.MakeTransparent(Color.White);
			return grid.Clone(getRect(rank), grid.PixelFormat);
		}

		private static Rectangle getRect(Rank rank)
		{
			switch (rank) {
				case Rank.GodEmperor:
					return Crown2;
				case Rank.Pharaoh:
				case Rank.Emperor:
				case Rank.Caesar:
				case Rank.Tsar:
				case Rank.HighKing:
					return Crown17;
				case Rank.King:
				case Rank.Khan:
				case Rank.Shah:
				case Rank.Sultan:
					return Crown7;
				case Rank.Prince:
					return Crown3;
				case Rank.Archduke:
				case Rank.GrandDuke:
					return Crown10;
				case Rank.Duke:
				case Rank.Emir:
				case Rank.Patrician:
					return Crown12;
				case Rank.Markgrave:
				case Rank.Landgrave:
				case Rank.Count:
				case Rank.Earl:
				case Rank.Viscount:
				case Rank.Freiherr:
				case Rank.Baron:
				case Rank.Bey:
				case Rank.Knight:
					return Crown8;
				default:
					return Crown1;
			}
		}
	}
}

