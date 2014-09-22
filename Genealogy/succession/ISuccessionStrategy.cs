using System;

namespace Genealogy.Succession
{
	public interface ISuccessionStrategy
	{
		Person successorTo(Reign[] previousReigns);
		Title Title {
			get;
			set;
		}
	}
}