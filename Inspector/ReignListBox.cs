using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Genealogy.Inspector
{
	public class ReignListBox : ListBox
	{
		public ReignListBox()
			: base()
		{
			ItemHeight = (int)(3 * Font.GetHeight() + 16);
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			if (Items.Count > 0) {
				Reign reign = Items[e.Index] as Reign;
				if (reign == null)
					base.OnDrawItem(e);
				else {
					if (e.State.HasFlag(DrawItemState.Selected))
						e.Graphics.FillRectangle(Brushes.Beige, e.Bounds);
					else
						e.Graphics.FillRectangle(Brushes.White, e.Bounds);

					e.Graphics.DrawLine(Pens.Gray, e.Bounds.Left + e.Bounds.Width/3, e.Bounds.Bottom - 2, e.Bounds.Right - e.Bounds.Width/3, e.Bounds.Bottom - 2);

					e.Graphics.DrawImage(Crown.GetCrown(reign.Title.Rank), new RectangleF(e.Bounds.X + 5, e.Bounds.Y + 5, 30, 30));

					string name = string.Format("{0} {1}. {2}",
					                             reign.Ruler.Firstname,
					                             RomanNumerals.ToRomanNumeral(reign.NameIndex),
					                             reign.Ruler.Lastname);
					e.Graphics.DrawString(name, new Font(Font, FontStyle.Bold), Brushes.Black, new RectangleF(e.Bounds.X + 40, e.Bounds.Y + 5, e.Bounds.Width - 45, Font.GetHeight() + 2));
					string title = string.Format("{0} of {1}",
					                             reign.Title.Rank,
					                             formatRealms(reign.Title.Realms));
					e.Graphics.DrawString(title, new Font(Font, FontStyle.Italic), Brushes.Black, new RectangleF(e.Bounds.X + 40, e.Bounds.Y + 8 + Font.GetHeight(), e.Bounds.Width - 45, Font.GetHeight() + 2));
					string duration = reign.Start + " - " + reign.End;
					e.Graphics.DrawString(duration, Font, Brushes.Black, new RectangleF(e.Bounds.X + 40, e.Bounds.Y + 11 + Font.GetHeight() * 2, e.Bounds.Width - 45, Font.GetHeight() + 2));

					e.DrawFocusRectangle();
				}
			}
		}

		private static string formatRealms(Realm[] realms)
		{
			if (realms.Length == 1)
				return realms[0].Name;
			return string.Join(", ", realms.Take(realms.Length - 1).Select(r => r.Name)) + " and " + realms[realms.Length - 1].Name;
		}
	}
}