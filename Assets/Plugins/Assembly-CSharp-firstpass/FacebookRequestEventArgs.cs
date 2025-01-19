using System;

public class FacebookRequestEventArgs : EventArgs
{
	public enum FacebookRequestStatus
	{
		Invalid = 0,
		Success = 1,
		Cancelled = 2,
		Failed = 3
	}

	private FacebookRequestStatus status { get; set; }

	private string message { get; set; }

	public FacebookRequestEventArgs(FacebookRequestStatus _status, string _message = "")
	{
		status = _status;
		message = _message;
	}

	public FacebookRequestStatus GetStatus()
	{
		return status;
	}

	public string GetMessage()
	{
		return message;
	}
}
