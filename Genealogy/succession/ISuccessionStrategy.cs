using System;

namespace Genealogy.Succession
{
	public interface ISuccessionStrategy
	{
		Person getSuccessor();
		Title Title { get; }
	}
}