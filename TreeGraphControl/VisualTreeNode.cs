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

		public float X {
			get;
			set;
		}

		public float Y {
			get;
			set;
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