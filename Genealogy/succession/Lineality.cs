using System;

namespace Genealogy.Succession
{
	public interface Lineality
	{
		bool considerChildren(Person parent);
	}
}

