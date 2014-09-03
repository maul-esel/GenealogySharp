using System;
using System.Collections.Generic;

namespace Genealogy.Chronicle
{
	public interface IEventProvider
	{
		IEnumerable<Event> Events { get; }
	}
}

