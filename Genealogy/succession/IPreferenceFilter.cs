using System.Collections.Generic;

namespace Genealogy
{
	public interface IPreferenceFilter : IComparer<Person>
	{
		bool ShouldConsider(Person p);
	}
}