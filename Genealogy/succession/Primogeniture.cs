using System;

namespace Genealogy.Succession
{
	public class Primogeniture : SuccessionStrategy
	{
		private readonly IPreferenceFilter preferenceFilter;
		private readonly Lineality lineality;

		public Primogeniture(IPreferenceFilter preferenceFilter, Lineality lineality)
		{
			this.preferenceFilter = preferenceFilter;
			this.lineality = lineality;
		}

		public Person successorTo(Person previousRuler, Person firstRuler)
		{
			// 1. call on oldest child
			Person successor = successorViaOldestChild(previousRuler, previousRuler.YearOfDeath);
			if (successor != null)
				return successor;

			// first ruler has no known siblings, uncles, aunts
			if (previousRuler == firstRuler)
				return null;

			// for siblings and uncles/aunts, need to know from which side of the family
			Person[] directConnection = DijkstraAlgorithm<Person>.FindShortestLink(firstRuler, previousRuler, person => person.Children);
			if (directConnection == null)
				throw new Exception();
			Person relevantParent = directConnection[directConnection.Length - 2]; // NOTE: always at least two elements in array: firstRuler and previousRuler

			return successorViaNextSibling(previousRuler, relevantParent, previousRuler.YearOfDeath) // 2. call on younger siblings
				?? successorViaUncleOrAunt(previousRuler, firstRuler, previousRuler.YearOfDeath); // 3. uncles / aunts
		}

		private Person findSuccessor(Person relevantParent, Person root, int yearOfSuccession) {
			if (root.isAlive(yearOfSuccession))
				return root;

			return successorViaOldestChild(root, yearOfSuccession)
				?? successorViaNextSibling(root, relevantParent, yearOfSuccession);
		}

		private Person successorViaOldestChild(Person self, int yearOfSuccession) {
			if (!shouldConsiderChildren(self))
				return null;

			var firstChild = genderPreference.firstChild(self);
			if (firstChild != null)
				return findSuccessor(self, firstChild, yearOfSuccession);
			return null;
		}

		private bool shouldConsiderChildren(Person p)
		{
			switch (lineality) {
				case Lineality.agnatic:
					return p.Gender == Gender.Male;
				case Lineality.uterine:
					return p.Gender == Gender.Female;
				default:
					return true;
			}
		}

		private Person successorViaNextSibling(Person self, Person parent, int yearOfSuccession) {
			Person nextSibling = genderPreference.nextSibling(self, parent);
			if (nextSibling != null)
				return findSuccessor(parent, nextSibling, yearOfSuccession);
			return null;
		}

		private Person successorViaUncleOrAunt(Person self, Person firstRuler, int yearOfSuccession) {
			if (self == firstRuler)
				return null;

			Person[] directConnection = DijkstraAlgorithm<Person>.FindShortestLink(firstRuler, self, person => person.Children);
			if (directConnection == null)
				throw new Exception();

			if (directConnection.Length >= 3) {
				Person parent = directConnection[directConnection.Length - 2];
				Person grandparent = directConnection[directConnection.Length - 3];

				Person nextUncleOrAunt = genderPreference.nextUncleOrAunt(parent, grandparent);
				if (nextUncleOrAunt != null)
					return findSuccessor(grandparent, nextUncleOrAunt, yearOfSuccession)
						?? successorViaUncleOrAunt(nextUncleOrAunt, firstRuler, yearOfSuccession);
			}

			return null;
		}
	}
}