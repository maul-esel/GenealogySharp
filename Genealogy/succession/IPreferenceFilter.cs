using System.Collections.Generic;

namespace Genealogy.Succession
{
	public interface IPreferenceFilter : IComparer<Person>
	{
		bool ShouldConsider(Person p);
	}
}