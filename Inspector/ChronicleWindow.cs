using System.Linq;
using System.Windows.Forms;

using Genealogy.Chronicle;

namespace Genealogy.Inspector
{
	public class ChronicleWindow : WindowBase
	{
		public ChronicleWindow(IEventProvider provider)
		{
			Text = "Genealogy Inspector - Chronicle";
			SuspendLayout();

			ListBox eventList = new EventListBox();
			eventList.Items.AddRange(provider.Events.OrderBy(e => e.Year).ToArray());
			Controls.Add(eventList);
			eventList.Size = ClientSize;
			eventList.Dock = DockStyle.Fill;
			eventList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

			ResumeLayout();
			PerformLayout();
		}
	}
}