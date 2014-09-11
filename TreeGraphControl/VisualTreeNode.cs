using System;
using System.Linq;

namespace TGC
{
	public class VisualTreeNode
	{
		public ITreeNode Node {
			get;
			set;
		}

		private int x;
		public int X {
			get { return x; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException();
				x = value;
			}
		}

		private int y;
		public int Y {
			get { return y; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException();
				y = value;
			}
		}

		public VisualTreeNode[] Children {
			get;
			private set;
		}

		public VisualTreeNode(ITreeNode node)
		{
			Node = node;
			Children = node.ChildNodes.Select(child => new VisualTreeNode(child)).ToArray();
		}
	}
}