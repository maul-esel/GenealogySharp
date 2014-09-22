namespace Genealogy.Succession
{
	public interface IPreferenceFilter
	{
		int Compare(Person x, Person y, Title title);
		bool ShouldConsider(Person p, Title title);
	}
}