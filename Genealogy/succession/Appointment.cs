using System;

namespace Genealogy.Succession
{
	public class Appointment : ISuccessionStrategy
	{
		private readonly Title title;
		public Title Title {
			get { return title; }
		}

		private readonly Person[] successors;

		public Appointment(Title title, Person[] successors)
		{
			this.title = title;
			this.successors = successors;

			title.AddSuccessionStrategy(this);
		}

		public Person getSuccessor()
		{
			if (successors.Length > 0) {
				if (Title.CalculatedReigns.Length == 1) // only first ruler so far
					return successors[0];

				Person lastRuler = Title.CalculatedReigns[Title.CalculatedReigns.Length - 1].Ruler;
				for (int i = 1; i < successors.Length; ++i)
					if (successors[i - 1] == lastRuler)
						return successors[i];
			}
			return null;
		}
	}
}