using System;

namespace TreeGraphControl
{
	public interface ITreeNode
	{
		string Text { get; }
		ITreeNode[] ChildNodes { get; }
		event EventHandler DescendantsChanged;
	}
}

