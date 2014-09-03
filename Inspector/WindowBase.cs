using System.Drawing;
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
	}
}

