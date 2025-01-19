using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class LeaderboardListController : GluiSimpleCollectionController
{
	private enum ELeaderboardRetrievalState
	{
		kGettingLocalPlayer = 0,
		kGettingFirstPlayer = 1,
		kGettingSurroundingPlayers = 2
	}

	private GluiBouncyScrollList mScrollList;

	private GameCenterScore[] mScores;

	private ILeaderboard mLeaderboard;

	private int mFirstIndex;

	private string mLeaderboardId = Profile.kPlayerRatingLeaderboard;

	private GameCenterLeaderboardTimeScope mTimeScope = GameCenterLeaderboardTimeScope.AllTime;

	private UserScope mUserScope;

	private ELeaderboardRetrievalState mRetrievalState;

	private GameCenterScore mLocalPlayerScore;

	public string LeaderboardId
	{
		get
		{
			return mLeaderboardId;
		}
	}

	public GameCenterLeaderboardTimeScope timeScope
	{
		get
		{
			return mTimeScope;
		}
	}

	public UserScope userScope
	{
		get
		{
			return mUserScope;
		}
	}

	private void Start()
	{
		mFirstIndex = 1;
		mLeaderboardId = Profile.kPlayerRatingLeaderboard;
		mTimeScope = GameCenterLeaderboardTimeScope.AllTime;
		mUserScope = UserScope.Global;
		GameCenterManager.scoresLoaded += OnScoresLoaded;
		GameCenterManager.scoresForPlayerIdLoaded += OnScoresLoaded;
		mScrollList = base.gameObject.GetComponent<GluiBouncyScrollList>();
		mData = new Object[0];
	}

	private void OnDestroy()
	{
		GameCenterManager.scoresLoaded -= OnScoresLoaded;
		GameCenterManager.scoresForPlayerIdLoaded -= OnScoresLoaded;
	}

	public override void ReloadData(object arg)
	{
		mData = new Object[0];
		mLocalPlayerScore = null;
		if (mUserScope == UserScope.Global)
		{
			mRetrievalState = ELeaderboardRetrievalState.kGettingLocalPlayer;
			GameCenterBinding.retrieveScoresForPlayerId(GameCenterBinding.playerIdentifier(), mLeaderboardId);
		}
		else
		{
			mRetrievalState = ELeaderboardRetrievalState.kGettingSurroundingPlayers;
			GameCenterBinding.retrieveScores(mUserScope == UserScope.FriendsOnly, mTimeScope, 1, 10, mLeaderboardId);
		}
	}

	public void SetTimeScope(GameCenterLeaderboardTimeScope scope)
	{
		if (scope != mTimeScope)
		{
			mTimeScope = scope;
			ReloadData(null);
		}
	}

	public void SetUserScope(UserScope scope)
	{
		if (scope != mUserScope)
		{
			mUserScope = scope;
			ReloadData(null);
		}
	}

	public void SetLeaderboard(string leaderboard)
	{
		if (leaderboard != mLeaderboardId)
		{
			mLeaderboardId = leaderboard;
			ReloadData(null);
		}
	}

	private void OnScoresLoaded(List<GameCenterScore> scores)
	{
		if (mRetrievalState == ELeaderboardRetrievalState.kGettingLocalPlayer)
		{
			if (scores.Count == 0)
			{
				GameCenterBinding.retrieveScores(mUserScope == UserScope.FriendsOnly, mTimeScope, 1, 10, mLeaderboardId);
			}
			else if (scores[0].rank <= 10)
			{
				GameCenterBinding.retrieveScores(mUserScope == UserScope.FriendsOnly, mTimeScope, 1, 10, mLeaderboardId);
			}
			else
			{
				mLocalPlayerScore = scores[0];
				GameCenterBinding.retrieveScores(mUserScope == UserScope.FriendsOnly, mTimeScope, 1, 9, mLeaderboardId);
			}
			mRetrievalState = ELeaderboardRetrievalState.kGettingSurroundingPlayers;
			return;
		}
		if (mLocalPlayerScore != null)
		{
			scores.Add(mLocalPlayerScore);
			mLocalPlayerScore = null;
		}
		mScores = scores.ToArray();
		mData = mScores;
		if (mScrollList != null)
		{
			mScrollList.ForceRedrawList();
		}
	}

	public override string GetCellPrefabForDataIndex(int dataIndex)
	{
		if (dataIndex == 0 && mFirstIndex == 1)
		{
			return "UI/Prefabs/Global/Card_Leaderboard_FirstPlace";
		}
		return base.GetCellPrefabForDataIndex(dataIndex);
	}
}
