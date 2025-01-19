using System;
using System.Collections.Generic;
using Gamespy.Matchmaking;

public class GripNetwork_SubscribeHosts : DisposableMonoBehaviour
{
	private string stackTrace;

	private MatchmakingSession mServerBrowsingSession;

	private SearchRequestState mCurrentState;

	private List<string> mKeyNames;

	private string mFilter;

	private Action<GripNetwork.Result, List<GameHost>> mSubscribeHostsCallback;

	public void SubscribeHosts(string[] keyNames, string filter, Action<GripNetwork.Result, List<GameHost>> searchHostsCallback)
	{
		mSubscribeHostsCallback = searchHostsCallback;
		stackTrace = GenericUtils.StackTrace();
		try
		{
			if (!GripNetwork.Ready)
			{
				WhenDone(GripNetwork.Result.Failed, null, true);
				ObjectUtils.DestroyImmediate(base.gameObject);
			}
			else
			{
				mKeyNames = new List<string>(keyNames ?? new string[0]);
				mFilter = filter ?? string.Empty;
				mServerBrowsingSession = new MatchmakingSession(GripNetwork.GameSpyAccountManager.SecurityToken);
			}
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed, null, true);
			ObjectUtils.DestroyImmediate(base.gameObject);
		}
	}

	private void Update()
	{
		try
		{
			mCurrentState = mServerBrowsingSession.Subscribe(mKeyNames, mFilter);
			if (mCurrentState == SearchRequestState.Complete)
			{
				GripNetwork.Result result = GripNetwork.Result.Success;
				if (mServerBrowsingSession.Result != 0)
				{
					result = GripNetwork.Result.Failed;
				}
				WhenDone(result, mServerBrowsingSession.Search_GameHosts, true);
				return;
			}
			if (mCurrentState == SearchRequestState.UpdateReceived)
			{
				WhenDone(GripNetwork.Result.Success, mServerBrowsingSession.Search_GameHosts, false);
			}
			if (mServerBrowsingSession.Result != 0)
			{
				WhenDone(GripNetwork.Result.Failed, null, true);
			}
			else if (mCurrentState == SearchRequestState.Complete)
			{
				WhenDone(GripNetwork.Result.Success, mServerBrowsingSession.Search_GameHosts, true);
			}
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed, null, true);
		}
	}

	private void WhenDone(GripNetwork.Result result, List<GameHost> hosts, bool actionComplete)
	{
		if (!(this == null))
		{
			if (stackTrace == null)
			{
				stackTrace = string.Empty;
			}
			if (result != 0)
			{
			}
			if (mSubscribeHostsCallback != null)
			{
				mSubscribeHostsCallback(result, hosts);
			}
			if (actionComplete)
			{
				ObjectUtils.DestroyImmediate(base.gameObject);
			}
		}
	}
}
