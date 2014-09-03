using System;
using System.Windows.Forms;

namespace Genealogy.Inspector
{
	public class Program
	{
		public static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new InspectorWindow());
		}
	}
}