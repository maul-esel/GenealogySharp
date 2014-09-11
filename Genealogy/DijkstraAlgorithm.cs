using System;
using System.Collections.Generic;
using System.Linq;

namespace Genealogy
{
	/// <summary>
	/// Implements a slightly modified Dijkstra algorithm:
	/// * all edges have weight 1
	/// * set of nodes does not need to be known beforehand
	/// </summary>
	public class DijkstraAlgorithm<T>
	{
		private static HashSet<T> knownNodes = new HashSet<T>();
		private static HashSet<T> visitedNodes = new HashSet<T>();
		private static Dictionary<T, int> distances = new Dictionary<T, int>();
		private static Dictionary<T, T> previous = new Dictionary<T, T>();

		public static T[] FindShortestLink(T start, T end, Func<T, IEnumerable<T>> neighbours)
		{
			if (start.Equals(end))
				return new T[] { start };

			distances[start] = 0;
			knownNodes.Add(start);

			addNeighbours(start, neighbours);

			while (visitedNodes.Count < knownNodes.Count) {
				T minDistNode = knownNodes.Except(visitedNodes).OrderBy(node => distance(node)).First();
				if (minDistNode.Equals(end)) {
					T[] path = extractPath(end, start);
					cleanup();
					return path;
				}

				visitedNodes.Add(minDistNode);
				addNeighbours(minDistNode, neighbours);

				var unvisitedNeighbours = neighbours(minDistNode).Except(visitedNodes);
				foreach (T neighbour in unvisitedNeighbours) {
					int neighbourDist = distance(minDistNode) + 1;
					if (distance(neighbour) > neighbourDist) {
						distances[neighbour] = neighbourDist;
						previous[neighbour] = minDistNode;
					}
				}
			}

			cleanup();
			return null;
		}

		private static void cleanup()
		{
			knownNodes.Clear();
			visitedNodes.Clear();
			distances.Clear();
			previous.Clear();
		}

		private static int distance(T node)
		{
			if (distances.ContainsKey(node))
				return distances[node];
			return int.MaxValue;
		}

		private static void addNeighbours(T node, Func<T, IEnumerable<T>> neighbours)
		{
			foreach (T neighbour in neighbours(node))
				knownNodes.Add(neighbour);
		}

		private static T[] extractPath(T end, T start)
		{
			List<T> path = new List<T>();
			for (T current = end; previous.ContainsKey(current); current = previous[current])
				path.Add(current);
			path.Add(start);
			path.Reverse();
			return path.ToArray();
		}
	}
}