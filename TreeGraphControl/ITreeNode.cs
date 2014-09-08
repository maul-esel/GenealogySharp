using System;

namespace TGC
{
	public interface ITreeNode
	{
		string Text { get; }
		ITreeNode[] ChildNodes { get; }
		bool Visible { get; }
		event EventHandler DescendantsChanged;
		event EventHandler VisibilityChanged;
	}
}

