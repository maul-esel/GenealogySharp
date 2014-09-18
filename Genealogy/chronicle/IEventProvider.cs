using System;
using System.Collections.Generic;

namespace Genealogy.Events
{
	public interface IEventProvider
	{
		IEnumerable<Event> Events { get; }
	}
}

