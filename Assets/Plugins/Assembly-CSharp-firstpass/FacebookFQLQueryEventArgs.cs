using System;

public class FacebookFQLQueryEventArgs : EventArgs
{
	private string mResult { get; set; }

	public FacebookFQLQueryEventArgs(string _result)
	{
		mResult = _result;
	}

	public string GetFQLQueryResult()
	{
		return mResult;
	}
}
