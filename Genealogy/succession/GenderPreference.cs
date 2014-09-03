using System;

namespace Genealogy.Succession
{
	public interface GenderPreference
	{
		Person firstChild(Person self);
		Person nextSibling(Person self, Person parent);
		Person nextUncleOrAunt(Person parent, Person grandparent);
	}
}

