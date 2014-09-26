namespace Genealogy.Succession
{
	public interface IPreferenceFilter
	{
		int Compare(Person x, Person y);
		bool ShouldConsider(Person p);
	}
}