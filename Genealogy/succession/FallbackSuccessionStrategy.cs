using System;

namespace Genealogy.Succession
{
	public class FallbackSuccessionStrategy : ISuccessionStrategy
	{
		private readonly ISuccessionStrategy[] strategies;

		private Title title;
		public Title Title {
			get { return title; }
			set {
				if (title != null)
					throw new InvalidOperationException();
				foreach (ISuccessionStrategy strategy in strategies)
					strategy.Title = value;
				title = value;
			}
		}

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