using System;

namespace Genealogy.Inspector
{
	public class TreeWindow : WindowBase
	{
		public TreeWindow(Person root)
		{
			Text = "Genealogy Inspector - Family Tree";
			Width = 1000;
			Height = 500;

			GenealogyTreeControl tree = new GenealogyTreeControl();
			tree.RootNode = new PersonNode(root, tree);
			tree.Size = ClientSize;
			Controls.Add(tree);

			Resize += (s, e) => tree.Size = ClientSize;
		}
	}
}

