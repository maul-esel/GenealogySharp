using System;
using System.Collections.Generic;

namespace TGC
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

		private bool visible = true;
		public virtual bool Visible {
			get { return visible; }
			protected set {
				visible = value;
				OnVisibilityChanged();
			}
		}

		public virtual event EventHandler DescendantsChanged;
		public virtual event EventHandler VisibilityChanged;
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
			AddChildNodes(children);
		}

		protected TreeNodeBase()
		{
		}
		#endregion

		#region child nodes
		protected virtual void AddChildNode(ITreeNode child)
		{
			childNodes.Add(child);
			child.DescendantsChanged += OnDescendantsChanged;
			child.VisibilityChanged += OnDescendantsChanged;
			OnDescendantsChanged();
		}

		protected virtual void AddChildNodes(IEnumerable<ITreeNode> children)
		{
			childNodes.AddRange(children);
			foreach (ITreeNode child in children) {
				child.DescendantsChanged += OnDescendantsChanged;
				child.VisibilityChanged += OnDescendantsChanged;
			}
			OnDescendantsChanged();
		}

		protected virtual void ReplaceChildNodes(IEnumerable<ITreeNode> children)
		{
			RemoveAllChildNodes();
			AddChildNodes(children);
			OnDescendantsChanged();
		}

		protected virtual void RemoveChildNode(ITreeNode child)
		{
			childNodes.Remove(child);
			child.DescendantsChanged -= OnDescendantsChanged;
			child.VisibilityChanged -= OnDescendantsChanged;
			OnDescendantsChanged();
		}

		protected virtual void RemoveAllChildNodes()
		{
			foreach (ITreeNode child in childNodes) {
				child.DescendantsChanged -= OnDescendantsChanged;
				child.VisibilityChanged -= OnDescendantsChanged;
			}
			childNodes.Clear();
			OnDescendantsChanged();
		}

		protected virtual void OnDescendantsChanged()
		{
			if (DescendantsChanged != null)
				DescendantsChanged(this, new EventArgs());
		}

		protected virtual void OnDescendantsChanged(object sender, EventArgs e)
		{
			OnDescendantsChanged();
		}

		protected virtual void OnVisibilityChanged()
		{
			if (VisibilityChanged != null)
				VisibilityChanged(this, new EventArgs());
		}
		#endregion
	}
}