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

		private float x;
		public float X {
			get { return x; }
			set {
				if (value < 0f)
					throw new ArgumentOutOfRangeException("value", value, "VisualNode.X >= 0");
				x = value;
			}
		}

		private float y;
		public float Y {
			get { return y; }
			set {
				if (value < 0f)
					throw new ArgumentOutOfRangeException("value", value, "VisualNode.X >= 0");
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