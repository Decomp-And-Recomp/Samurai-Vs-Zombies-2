using System;

namespace DaveyM69.Components.SNTP
{
	public class ErrorData
	{
		public bool Error { get; private set; }

		public string ErrorText { get; private set; }

		public Exception Exception { get; private set; }

		internal ErrorData(string errorText)
		{
			ErrorText = errorText;
			Error = true;
		}

		internal ErrorData(Exception exception)
		{
			ErrorText = exception.Message;
			Exception = exception;
			Error = true;
		}

		internal ErrorData()
		{
		}
	}
}
