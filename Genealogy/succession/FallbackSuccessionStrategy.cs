using System;

namespace Genealogy.Succession
{
	public class FallbackSuccessionStrategy : ISuccessionStrategy
	{
		private readonly ISuccessionStrategy[] strategies;

		public FallbackSuccessionStrategy(ISuccessionStrategy[] strategies)
		{
			this.strategies = strategies;
		}

		public Person successorTo(Reign[] previousReigns)
		{
			foreach (ISuccessionStrategy strategy in strategies) {
				Person successor = strategy.successorTo(previousReigns);
				if (successor != null)
					return successor;
			}
			return null;
		}
	}
}