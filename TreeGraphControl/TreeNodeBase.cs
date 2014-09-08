using System;
using System.Collections.Generic;

namespace TreeGraphControl
{
	public abstract class TreeNodeBase : ITreeNode
	{
		#region ITreeNode
		private string text = null;

		public virtual string Text {
			get { return text; }
			protected set { text = value; }
		}

		private readonly List<ITreeNode> childNodes = new List<ITreeNode>();

		public virtual ITreeNode[] ChildNodes {
			get { return childNodes.ToArray(); }
		}

		public virtual event EventHandler DescendantsChanged;
		#endregion

		#region constructors
		protected TreeNodeBase(string text, IEnumerable<ITreeNode> children)
			: this(children)
		{
			this.text = text;
		}

		protected TreeNodeBase(string text)
			: this()
		{
			this.text = text;
		}

		protected TreeNodeBase(IEnumerable<ITreeNode> children)
			: this()
		{
			childNodes.AddRange(children);
		}

		protected TreeNodeBase()
		{
		}
		#endregion

		#region child nodes
		protected virtual void AddChildNode(ITreeNode child)
		{
			childNodes.Add(child);
			OnDescendantsChanged();
		}

		protected virtual void AddChildNodes(IEnumerable<ITreeNode> children)
		{
			childNodes.AddRange(children);
			OnDescendantsChanged();
		}

		protected virtual void ReplaceChildNodes(IEnumerable<ITreeNode> children)
		{
			childNodes.Clear();
			childNodes.AddRange(children);
			OnDescendantsChanged();
		}

		protected virtual void RemoveChildNode(ITreeNode child)
		{
			childNodes.Remove(child);
			OnDescendantsChanged();
		}

		protected virtual void RemoveAllChildNodes()
		{
			childNodes.Clear();
			OnDescendantsChanged();
		}

		protected virtual void OnDescendantsChanged()
		{
			if (DescendantsChanged != null)
				DescendantsChanged(this, new EventArgs());
		}
		#endregion
	}
}