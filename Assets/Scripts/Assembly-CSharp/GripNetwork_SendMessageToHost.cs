using System;
using Gamespy.Matchmaking;

public class GripNetwork_SendMessageToHost : DisposableMonoBehaviour
{
	private string stackTrace;

	private string mIP;

	private int mPort;

	private string mMessage;

	private MatchmakingSession matchmakingSession;

	private SendMessageRequestState sendMessageRequestState;

	private Action<GripNetwork.Result> mSendMessageCallback;

	public void SendMessageToHost(string ip, int port, string message, Action<GripNetwork.Result> sendMessageCallback)
	{
		mSendMessageCallback = sendMessageCallback;
		stackTrace = GenericUtils.StackTrace();
		try
		{
			if (!GripNetwork.Ready)
			{
				WhenDone(GripNetwork.Result.Failed);
				return;
			}
			mIP = ip;
			mPort = port;
			mMessage = message;
			matchmakingSession = new MatchmakingSession(GripNetwork.GameSpyAccountManager.SecurityToken);
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed);
		}
	}

	private void Update()
	{
		try
		{
			if (sendMessageRequestState != SendMessageRequestState.Complete)
			{
				sendMessageRequestState = matchmakingSession.SendMessageToHost(mIP, mPort, mMessage);
			}
			else if (matchmakingSession.Result != 0)
			{
				WhenDone(GripNetwork.Result.Failed);
			}
			else
			{
				WhenDone(GripNetwork.Result.Success);
			}
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed);
		}
	}

	private void WhenDone(GripNetwork.Result result)
	{
		Action<GripNetwork.Result> action = mSendMessageCallback;
		mSendMessageCallback = null;
		if (result != 0)
		{
		}
		if (action != null)
		{
			action(GripNetwork.Result.Failed);
		}
		ObjectUtils.DestroyImmediate(base.transform.gameObject);
	}
}
