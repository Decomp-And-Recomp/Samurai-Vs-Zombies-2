using System.Runtime.InteropServices;
using UnityEngine;

public class GameCenterBinding
{
	[DllImport("__Internal")]
	private static extern bool _gameCenterIsGameCenterAvailable();

	public static bool isGameCenterAvailable()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _gameCenterIsGameCenterAvailable();
		}
		return false;
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterAuthenticateLocalPlayer();

	public static void authenticateLocalPlayer()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterAuthenticateLocalPlayer();
		}
	}

	[DllImport("__Internal")]
	private static extern bool _gameCenterIsPlayerAuthenticated();

	public static bool isPlayerAuthenticated()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _gameCenterIsPlayerAuthenticated();
		}
		return false;
	}

	[DllImport("__Internal")]
	private static extern string _gameCenterPlayerAlias();

	public static string playerAlias()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _gameCenterPlayerAlias();
		}
		return string.Empty;
	}

	[DllImport("__Internal")]
	private static extern string _gameCenterPlayerIdentifier();

	public static string playerIdentifier()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _gameCenterPlayerIdentifier();
		}
		return string.Empty;
	}

	[DllImport("__Internal")]
	private static extern bool _gameCenterIsUnderage();

	public static bool isUnderage()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _gameCenterIsUnderage();
		}
		return false;
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveFriends();

	public static void retrieveFriends()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveFriends();
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterLoadPlayerData(string playerIds);

	public static void loadPlayerData(string[] playerIdArray)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterLoadPlayerData(string.Join(",", playerIdArray));
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterLoadLeaderboardLeaderboardTitles();

	public static void loadLeaderboardTitles()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterLoadLeaderboardLeaderboardTitles();
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterReportScore(long score, string leaderboardId);

	public static void reportScore(long score, string leaderboardId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterReportScore(score, leaderboardId);
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterShowLeaderboardWithTimeScope(int timeScope);

	public static void showLeaderboardWithTimeScope(GameCenterLeaderboardTimeScope timeScope)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterShowLeaderboardWithTimeScope((int)timeScope);
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterShowLeaderboardWithTimeScopeAndLeaderboardId(int timeScope, string leaderboardId);

	public static void showLeaderboardWithTimeScopeAndLeaderboard(GameCenterLeaderboardTimeScope timeScope, string leaderboardId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterShowLeaderboardWithTimeScopeAndLeaderboardId((int)timeScope, leaderboardId);
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveScores(bool friendsOnly, int timeScope, int start, int end);

	public static void retrieveScores(bool friendsOnly, GameCenterLeaderboardTimeScope timeScope, int start, int end)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveScores(friendsOnly, (int)timeScope, start, end);
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveScoresForLeaderboard(bool friendsOnly, int timeScope, int start, int end, string leaderboardId);

	public static void retrieveScores(bool friendsOnly, GameCenterLeaderboardTimeScope timeScope, int start, int end, string leaderboardId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveScoresForLeaderboard(friendsOnly, (int)timeScope, start, end, leaderboardId);
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveScoresForPlayerId(string playerId);

	public static void retrieveScoresForPlayerId(string playerId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveScoresForPlayerId(playerId);
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveScoresForPlayerIdAndLeaderboard(string playerId, string leaderboardId);

	public static void retrieveScoresForPlayerId(string playerId, string leaderboardId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveScoresForPlayerIdAndLeaderboard(playerId, leaderboardId);
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterReportAchievement(string identifier, float percent);

	public static void reportAchievement(string identifier, float percent)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterReportAchievement(identifier, percent);
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterGetAchievements();

	public static void getAchievements()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterGetAchievements();
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterResetAchievements();

	public static void resetAchievements()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterResetAchievements();
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterShowAchievements();

	public static void showAchievements()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterShowAchievements();
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveAchievementMetadata();

	public static void retrieveAchievementMetadata()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveAchievementMetadata();
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterShowCompletionBannerForAchievements();

	public static void showCompletionBannerForAchievements()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterShowCompletionBannerForAchievements();
		}
	}
}
