using System;

namespace Genealogy
{
	public class ConflictingMarriagesException : Exception
	{
		public ConflictingMarriagesException(Marriage original, Marriage conflicting)
		{
		}
	}
}

