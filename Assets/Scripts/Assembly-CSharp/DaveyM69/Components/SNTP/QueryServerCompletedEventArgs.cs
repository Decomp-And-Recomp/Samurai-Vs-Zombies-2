using System;

namespace DaveyM69.Components.SNTP
{
	public class QueryServerCompletedEventArgs : EventArgs
	{
		public SNTPData Data { get; internal set; }

		public ErrorData ErrorData { get; internal set; }

		public bool LocalDateTimeUpdated { get; internal set; }

		public bool Succeeded { get; internal set; }

		internal QueryServerCompletedEventArgs()
		{
			ErrorData = new ErrorData();
		}
	}
}
