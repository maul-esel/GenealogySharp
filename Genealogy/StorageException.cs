using System;

namespace Genealogy
{
	public class StorageException : Exception
	{
		public StorageException(string msg)
			: base(msg)
		{
		}

		public StorageException(string msg, Exception inner)
			: base(msg, inner)
		{
		}
	}
}