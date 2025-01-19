using System;

namespace Glu.Plugins.AMiscUtils
{
	public class ExceptionEventArgs : EventArgs
	{
		public Exception Exception { get; private set; }

		public ExceptionStatus Status { get; private set; }

		public ExceptionEventArgs(Exception ex, ExceptionStatus status)
		{
			Exception = ex;
			Status = status;
		}
	}
}
