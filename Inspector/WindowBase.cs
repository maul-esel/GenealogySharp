using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Genealogy.Inspector
{
	public abstract class WindowBase : Form
	{
		protected Label createBoldLabel(string text)
		{
			Label label = createLabel(text);
			label.Font = new Font(label.Font, FontStyle.Bold);
			return label;
		}

		protected Label createLabel(string text)
		{
			Label label = new Label();
			label.Text = text;
			return label;
		}

		protected static string joinRealmNames(Realm[] realms)
		{
			if (realms.Count() == 1)
				return realms.First().Name;
			return string.Join(", ", realms.Take(realms.Count() - 1).Select(r => r.Name)) + " and " + realms.Last().Name;
		}
	}
}

