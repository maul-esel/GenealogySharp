using System;

namespace Genealogy.Succession
{
	public class FallbackSuccessionStrategy : SuccessionStrategy
	{
		private readonly SuccessionStrategy[] strategies;

		public FallbackSuccessionStrategy(SuccessionStrategy[] strategies)
		{
			this.strategies = strategies;
		}

		public Person successorTo(Person previousRuler, Person firstRuler)
		{
			foreach (SuccessionStrategy strategy in strategies) {
				Person successor = strategy.successorTo(previousRuler, firstRuler);
				if (successor != null)
					return successor;
			}
			return null;
		}
	}
}

