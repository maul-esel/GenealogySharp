using System;

namespace TGC
{
	public interface ITreeNode
	{
		string Text { get; }
		ITreeNode[] ChildNodes { get; }
		event EventHandler DescendantsChanged;
	}
}

