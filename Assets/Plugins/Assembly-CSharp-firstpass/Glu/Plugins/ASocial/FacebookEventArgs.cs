using System;

namespace Glu.Plugins.ASocial
{
	public class FacebookEventArgs : EventArgs
	{
		public enum FaceBookLoginStatus
		{
			Invalid = 0,
			Success = 1,
			Cancelled = 2,
			Failed = 3
		}

		private FaceBookLoginStatus status { get; set; }

		private string message { get; set; }

		public FacebookEventArgs(FaceBookLoginStatus _status, string _message = "")
		{
			status = _status;
			message = _message;
		}

		public FaceBookLoginStatus GetStatus()
		{
			return status;
		}

		public string GetMessage()
		{
			return message;
		}
	}
}
