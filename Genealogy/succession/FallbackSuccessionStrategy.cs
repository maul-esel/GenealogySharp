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

		public Person successorTo(Reign[] previousReigns)
		{
			foreach (SuccessionStrategy strategy in strategies) {
				Person successor = strategy.successorTo(previousReigns);
				if (successor != null)
					return successor;
			}
			return null;
		}
	}
}