using System;

namespace Genealogy.Chronicle
{
	public abstract class EventBase : Event
	{
		private readonly string message;
		private readonly int year;

		public override int Year {
			get { return year; }
		}

		protected EventBase(int year, string message)
		{
			this.year = year;
			this.message = message;
		}

		public override string ToString ()
		{
			return message;
		}
	}
}

