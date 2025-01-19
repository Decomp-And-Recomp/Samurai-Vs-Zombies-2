using System;

namespace Glu.Plugins.ASocial
{
	public class AmazonEventArgs : EventArgs
	{
		public enum Status
		{
			Invalid = 0,
			Success = 1,
			Failed = 2
		}

		private Status status { get; set; }

		private string message { get; set; }

		private string playerAlias { get; set; }

		public AmazonEventArgs(Status _status, string _message = "")
		{
			status = _status;
			message = _message;
		}

		public Status GetStatus()
		{
			return status;
		}

		public string GetMessage()
		{
			return message;
		}

		internal void SetPlayerAlias(string _alias)
		{
			playerAlias = _alias;
		}

		public string GetPlayerName()
		{
			return playerAlias;
		}
	}
}
