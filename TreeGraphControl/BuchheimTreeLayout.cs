using System.Collections.Generic;
using System.Linq;

namespace TGC
{
	// as presented at billmill.org/pymag-trees, adapted for C#
	public class BuchheimTreeLayout : ITreeLayout
	{
		public void Layout(VisualTreeNode root)
		{
			Node buchheimTree = new Node(root, null, 1, 0);

			firstWalk(buchheimTree);
			secondWalk(buchheimTree, 0, 0);
		}

		private const float distance = 1;

		private void firstWalk(Node v)
		{
			if (v.children.Length == 0) {
				if (v.leftSibling != null)
					v.X = v.leftSibling.X + distance;
				else
					v.X = 0f;
			} else {
				Node defaultAncestor = v.children[0];
				foreach (Node w in v.children) {
					firstWalk(w);
					defaultAncestor = apportion(w, defaultAncestor);
				}
				executeShifts(v);
				float midpoint = (v.children[0].X + v.children.Last().X) / 2f;
				if (v.leftSibling != null) {
					v.X = v.leftSibling.X + distance;
					v.mod = v.X - midpoint;
				} else
					v.X = midpoint;
			}
		}

		private void secondWalk(Node v, float m, int depth)
		{
			v.X += m;
			v.Y = depth;

			foreach (Node w in v.children)
				secondWalk(w, m + v.mod, depth + 1);
		}

		private Node apportion(Node v, Node defaultAncestor)
		{
			if (v.leftSibling != null) {
				Node vir = v,
					vor = v,
					vil = v.leftSibling,
					vol = vir.leftMostSibling;
				float sir = vir.mod,
					sor = vor.mod,
					sil = vil.mod,
					sol = vol.mod;

				while (vil.nextRight() != null && vir.nextLeft() != null) {
					vil = vil.nextRight();
					vir = vir.nextLeft();
					vol = vol.nextLeft();
					vor = vor.nextRight();

					vor.ancestor = v;

					float shift = (vil.X + sil) - (vir.X + sir) + distance;
					if (shift > 0) {
						moveSubtree(ancestor(vil, v, defaultAncestor), v, shift);
						sir += shift;
						sor += shift;
					}

					sil += vil.mod;
					sir += vir.mod;
					sol += vol.mod;
					sor += vor.mod;
				}

				if (vil.nextRight() != null && vor.nextRight() == null) {
					vor.thread = vil.nextRight();
					vor.mod += sil - sor;
				}
				if (vir.nextLeft() != null && vol.nextLeft() == null) {
					vol.thread = vir.nextLeft();
					vol.mod += sir - sol;
					defaultAncestor = v;
				}
			}
			return defaultAncestor;
		}

		private void moveSubtree(Node wl, Node wr, float shift)
		{
			int subtrees = wr.number - wl.number;
			wr.change -= shift / subtrees;
			wr.shift += shift;
			wl.change += shift / subtrees;
			wr.X += shift;
			wr.mod += shift;
		}

		private void executeShifts(Node v)
		{
			float shift = 0, change = 0;
			foreach (Node w in v.children.Reverse()) {
				w.X += shift;
				w.mod += shift;
				change += w.change;
				shift += w.shift + change;
			}
		}

		private Node ancestor(Node vil, Node v, Node defaultAncestor)
		{
			if (v.parent != null && v.parent.children.Contains(vil.ancestor))
				return vil.ancestor;
			return defaultAncestor;
		}

		protected class Node
		{
			public float mod, offset, change, shift;
			public int number, level;
			public Node thread, ancestor;

			public readonly VisualTreeNode node;
			public readonly Node parent;
			public readonly Node[] children;

			public float X {
				get { return node.X; }
				set { node.X = value; }
			}

			public float Y {
				get { return node.Y; }
				set { node.Y = value; }
			}

			public Node(VisualTreeNode node, Node parent, int number, int level)
			{
				this.node = node;
				this.parent = parent;
				this.ancestor = this;
				this.number = number;
				this.level = level;
				children = createChildren();
			}

			protected virtual Node[] createChildren()
			{
				List<Node> childMetadata = new List<Node>();
				int i = 0;
				foreach (VisualTreeNode child in node.Children.Where(c => c.Node.Visible))
					childMetadata.Add(new Node(child, this, ++i, level + 1));
				return childMetadata.ToArray();
			}

			public Node nextLeft() {
				if (children.Length > 0)
					return children[0];
				return thread;
			}

			public Node nextRight() {
				if (children.Length > 0)
					return children.Last();
				return thread;
			}

			public Node leftSibling {
				get {
					if (parent != null && number > 1)
						return parent.children[number - 2];
					return null;
				}
			}

			public Node leftMostSibling {
				get {
					if (parent != null && this != parent.children[0])
						return parent.children[0];
					return null;
				}
			}
		}
	}
}