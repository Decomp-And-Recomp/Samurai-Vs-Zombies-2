using System;

namespace HTTPS
{
	public class HTTPException : Exception
	{
		public HTTPException(string message)
			: base(message)
		{
		}
	}
}
