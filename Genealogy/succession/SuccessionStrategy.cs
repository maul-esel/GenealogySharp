using System;

namespace Genealogy.Succession
{
	public interface SuccessionStrategy
	{
		Person successorTo(Reign[] previousReigns);
	}
}