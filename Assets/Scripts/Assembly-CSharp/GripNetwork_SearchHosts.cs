using System;
using System.Collections.Generic;
using Gamespy.Matchmaking;

public class GripNetwork_SearchHosts : DisposableMonoBehaviour
{
	private string stackTrace;

	private MatchmakingSession mServerBrowsingSession;

	private SearchRequestState mCurrentState;

	private List<string> mKeyNames;

	private int mCount;

	private string mFilter;

	private Action<GripNetwork.Result, List<GameHost>> mSearchHostsCallback;

	public void SearchHosts(string[] keyNames, int count, string filter, Action<GripNetwork.Result, List<GameHost>> searchHostsCallback)
	{
		mSearchHostsCallback = searchHostsCallback;
		stackTrace = GenericUtils.StackTrace();
		try
		{
			if (!GripNetwork.Ready)
			{
				WhenDone(GripNetwork.Result.Failed, null);
				return;
			}
			mKeyNames = new List<string>(keyNames ?? new string[0]);
			mCount = count;
			mFilter = filter ?? string.Empty;
			if (GripNetwork.GameSpyAccountManager != null && GripNetwork.GameSpyAccountManager.SecurityToken != null)
			{
				mServerBrowsingSession = new MatchmakingSession(GripNetwork.GameSpyAccountManager.SecurityToken);
			}
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed, null);
		}
	}

	private void Update()
	{
		try
		{
			if (mCurrentState != SearchRequestState.Complete)
			{
				mCurrentState = mServerBrowsingSession.Search(mKeyNames, mCount, mFilter);
			}
			if (mServerBrowsingSession.Result != 0)
			{
				WhenDone(GripNetwork.Result.Failed, null);
			}
			else if (mCurrentState == SearchRequestState.Complete)
			{
				WhenDone(GripNetwork.Result.Success, mServerBrowsingSession.Search_GameHosts);
			}
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed, null);
		}
	}

	private void WhenDone(GripNetwork.Result result, List<GameHost> hosts)
	{
		if (stackTrace == null)
		{
			stackTrace = string.Empty;
		}
		if (result != 0)
		{
		}
		if (mSearchHostsCallback != null)
		{
			mSearchHostsCallback(result, hosts);
		}
		ObjectUtils.DestroyImmediate(base.gameObject);
	}
}
