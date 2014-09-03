using System;
using System.Collections.Generic;
using System.Linq;

namespace Genealogy
{
	public static class FamilyTreeUtil
	{
		public static Person[][] FindConnections(Person root, Person goal) {
			if (root == goal)
				return new Person[][] { new Person[] { goal } };

			return (
				from child in root.Children
				from path in FindConnections(child, goal)
				select new[] { root }.Concat(path).ToArray()
			).ToArray();
		}

		public static Person[] FindShortestConnection(Person root, Person goal) {
			return FindConnections(root, goal).OrderBy(path => path.Length).FirstOrDefault();
		}
	}
}

