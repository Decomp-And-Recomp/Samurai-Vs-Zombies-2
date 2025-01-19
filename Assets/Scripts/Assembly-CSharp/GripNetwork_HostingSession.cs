using System;
using System.Collections.Generic;
using Gamespy.Matchmaking;

public class GripNetwork_HostingSession : DisposableMonoBehaviour
{
	public enum UpdateType
	{
		ErrorOccurred = 0,
		MessageRecieved = 1
	}

	private Action<UpdateType, string> mUpdateHandler;

	private MatchmakingSession hostingSession;

	private HostRequestState hostRequestState;

	public List<Key> Keys { get; private set; }

	public void HostingSession(List<Key> keys, Action<UpdateType, string> updateHandler)
	{
		Keys = keys;
		mUpdateHandler = updateHandler;
		hostingSession = new MatchmakingSession(GripNetwork.GameSpyAccountManager.SecurityToken);
	}

	private void Update()
	{
		hostRequestState = hostingSession.Host(Keys);
		if (hostRequestState == HostRequestState.ClientMessageReceived)
		{
			GenericUtils.TryInvoke(mUpdateHandler, UpdateType.MessageRecieved, hostingSession.Host_ClientMessage);
		}
		if (hostRequestState == HostRequestState.Complete && hostingSession.Result != 0)
		{
			GenericUtils.TryInvoke(mUpdateHandler, UpdateType.ErrorOccurred, hostingSession.ResultMessage);
		}
	}

	~GripNetwork_HostingSession()
	{
		Dispose(false);
	}

	protected override void OnDispose(bool isDisposing)
	{
		if (hostingSession != null)
		{
			hostingSession.StopHosting();
		}
		base.OnDispose(isDisposing);
	}
}
