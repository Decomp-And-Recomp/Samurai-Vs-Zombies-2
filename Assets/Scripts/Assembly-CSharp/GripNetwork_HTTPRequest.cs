using System;
using System.Text;
using HTTP;

public class GripNetwork_HTTPRequest : DisposableMonoBehaviour
{
	private string stackTrace;

	private Action<GripNetwork.Result, string> mWhenDone;

	private Request _createRequest;

	public void HTTPRequest(string method, string uri, byte[] bytes, string username, string password, Action<GripNetwork.Result, string> whenDone)
	{
		mWhenDone = whenDone;
		stackTrace = GenericUtils.StackTrace();
		try
		{
			_createRequest = new Request(method, uri);
			string empty = string.Empty;
			empty += username;
			empty += ":";
			empty += password;
			string empty2 = string.Empty;
			empty2 += "Basic ";
			empty2 += Convert.ToBase64String(Encoding.Default.GetBytes(empty));
			_createRequest.AddHeader("Authorization", empty2);
			_createRequest.AddHeader("Content-Type", "application/json");
			_createRequest.bytes = bytes;
			_createRequest.Send();
		}
		catch (Exception)
		{
			WhenDone(false);
		}
	}

	private void Update()
	{
		try
		{
			if (_createRequest.state == RequestState.Done)
			{
				if (_createRequest.exception != null)
				{
					WhenDone(false);
				}
				else if (_createRequest.response.status != 200)
				{
					WhenDone(false);
				}
				else
				{
					WhenDone(true);
				}
			}
		}
		catch (Exception)
		{
			WhenDone(false);
		}
	}

	private void WhenDone(bool success)
	{
		string arg = string.Empty;
		try
		{
			arg = ((_createRequest == null || _createRequest.response == null) ? "Failed to generate a response" : _createRequest.response.Text);
		}
		catch (Exception)
		{
		}
		Action<GripNetwork.Result, string> action = mWhenDone;
		mWhenDone = null;
		if (!success)
		{
		}
		if (action != null)
		{
			action((!success) ? GripNetwork.Result.Failed : GripNetwork.Result.Success, arg);
		}
		ObjectUtils.DestroyImmediate(base.gameObject);
	}
}
