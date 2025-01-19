using System;

namespace HTTP
{
	public class HTTPException : Exception
	{
		public HTTPException(string message)
			: base(message)
		{
		}
	}
}
