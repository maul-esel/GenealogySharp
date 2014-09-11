using System;
using System.Collections.Generic;

namespace TGC
{
	public static class TreeSearch
	{
		public static T FindFirst<T>(T root, Func<T, IEnumerable<T>> getChildNodes, Predicate<T> criteria)
		{
			T result = default(T);
			Traverse(root, getChildNodes, node => {
				bool isMatch = criteria(node);
				if (isMatch)
					result = node;
				return !isMatch;
			});
			return result;
		}

		public static ITreeNode FindFirst(ITreeNode root, Predicate<ITreeNode> criteria)
		{
			return FindFirst(root, node => node.ChildNodes, criteria);
		}

		public static VisualTreeNode FindFirst(VisualTreeNode root, Predicate<VisualTreeNode> criteria)
		{
			return FindFirst(root, node => node.Children, criteria);
		}

		/// <summary>
		/// Does a deep-search traversal of a tree.
		/// </summary>
		/// <param name="root">The tree's root node</param>
		/// <param name="getChildNodes">A delegate that returns the relevant child nodes for a given node</param>
		/// <param name="action">An action to be performed on each visited node. If this returns false, the traversal is aborted.</param>
		/// <typeparam name="T">The node type</typeparam>
		/// <returns>true if all nodes were traversed, false if aborted</returns>
		public static bool Traverse<T>(T root, Func<T, IEnumerable<T>> getChildNodes, Predicate<T> action)
		{
			if (!action(root))
				return false;
			foreach (T child in getChildNodes(root))
				if (!Traverse(child, getChildNodes, action))
					return false;
			return true;
		}

		public static bool Traverse(ITreeNode root, Predicate<ITreeNode> action)
		{
			return Traverse(root, node => node.ChildNodes, action);
		}

		public static bool Traverse(VisualTreeNode root, Predicate<VisualTreeNode> action)
		{
			return Traverse(root, node => node.Children, action);
		}
	}
}