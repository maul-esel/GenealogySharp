using System;

namespace Genealogy.Succession
{
	public interface SuccessionStrategy
	{
		Person successorTo(Person ruler, Person firstRuler);
	}
}

