using System;

public class FacebookFeedEventArgs : EventArgs
{
	public enum FacebookFeedStatus
	{
		Invalid = 0,
		Success = 1,
		Cancelled = 2,
		Failed = 3
	}

	private FacebookFeedStatus status { get; set; }

	private string message { get; set; }

	public FacebookFeedEventArgs(FacebookFeedStatus _status, string _message = "")
	{
		status = _status;
		message = _message;
	}

	public FacebookFeedStatus GetStatus()
	{
		return status;
	}

	public string GetMessage()
	{
		return message;
	}
}
