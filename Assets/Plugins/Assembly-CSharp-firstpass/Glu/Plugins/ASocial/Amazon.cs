using System;
using UnityEngine;

namespace Glu.Plugins.ASocial
{
	public class Amazon : MonoBehaviour
	{
		public enum GameCircleFeatures
		{
			WhisperSync = 1,
			Achievements = 2,
			Leaderboards = 4
		}

		public enum LeaderBoardFilter
		{
			FRIENDS_ALL_TIME = 0,
			GLOBAL_ALL_TIME = 1,
			GLOBAL_DAY = 2,
			GLOBAL_WEEK = 3
		}

		public enum PopUpLocation
		{
			BOTTOM_CENTER = 0,
			BOTTOM_LEFT = 1,
			BOTTOM_RIGHT = 2,
			TOP_CENTER = 3,
			TOP_LEFT = 4,
			TOP_RIGHT = 5
		}

		public const string CONFLICT_STRATEGY_PLAYER_SELECT = "PLAYER_SELECT";

		public const string CONFLICT_STRATEGY_AUTO_TO_CLOUD = "AUTO_RESOLVE_TO_CLOUD";

		public const string CONFLICT_STRATEGY_AUTO_TO_IGNORE = "AUTO_RESOLVE_TO_IGNORE";

		private const string gameObjectName = "AmazonGO";

		private static bool bIsSignedIn;

		public static EventHandler<AchievementUpdateEventArgs> AchievementUpdateHandler;

		public static EventHandler<LeaderboardUpdateArgs> LeaderboardUpdateHandler;

		public static EventHandler<AchievementsQueryArgs> AchievementsQueryPercentileCompleteHandler;

		public static EventHandler<AchievementsQueryArgs> AchievementsQueryAchievementHiddenHandler;

		public static EventHandler<AchievementsQueryArgs> AchievementsQueryAchievementUnlockedHandler;

		public static EventHandler<LeaderboardQueryArgs> LeaderboardQueryScoreHandler;

		public static EventHandler<LeaderboardQueryArgs> LeaderboardQueryRankHandler;

		public static EventHandler<AmazonEventArgs> QueryPlayerNameHandler;

		private static AndroidJavaClass _amazon;

		public static AndroidJavaClass amazon
		{
			get
			{
				if (_amazon == null)
				{
					_amazon = new AndroidJavaClass("com.glu.plugins.AmazonGameCircleGlu");
				}
				return _amazon;
			}
		}

		public static void Init(GameCircleFeatures supportedFeatures = GameCircleFeatures.WhisperSync)
		{
			GameObject gameObject = new GameObject("AmazonGO");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			gameObject.AddComponent<AmazonComponent>();
			ASocial.Init();
			API_Init((int)supportedFeatures);
		}

		public static bool IsSignedIn()
		{
			return bIsSignedIn;
		}

		public static void Sync(string description, string conflictStrategy = "")
		{
			API_Sync(description, conflictStrategy);
		}

		public static void SyncOnExit()
		{
			API_Sync(string.Empty, "AUTO_RESOLVE_TO_IGNORE");
		}

		public static void RequestRevert()
		{
			API_RequestRevert();
		}

		public static void GetPlayerName()
		{
			API_GetPlayerName();
		}

		public static void SetPopUpLocation(PopUpLocation location = PopUpLocation.BOTTOM_RIGHT)
		{
			API_SetPopUpLocation(location);
		}

		public static void UpdateProgress(string achievementID, float percentageComplete = 100f)
		{
			API_UpdateProgress(achievementID, percentageComplete);
		}

		public static void ShowAchievements()
		{
			API_ShowAchievementsOverlay();
		}

		public static void ResetAchievement(string achievementID)
		{
			API_ResetAchievement(achievementID);
		}

		public static void ResetAchievements()
		{
			API_ResetAchievements();
		}

		public static void GetAchivementPercentileComplete(string achievementID)
		{
			API_GetAchivementPercentileComplete(achievementID);
		}

		public static void IsAchievementHidden(string achievementID)
		{
			API_IsAchievementHidden(achievementID);
		}

		public static void IsAchievementUnlocked(string achievementID)
		{
			API_IsAchievementUnlocked(achievementID);
		}

		public static void SubmitScore(string leaderboardID, long score)
		{
			API_SubmitScore(leaderboardID, score);
		}

		public static void ShowLeaderBoards()
		{
			API_ShowLeaderboardsOverlay();
		}

		public static void GetLeaderboardScore(string leaderboardID, LeaderBoardFilter lbFilter)
		{
			API_GetLBScore(leaderboardID, lbFilter.ToString());
		}

		public static void GetLeaderboardRank(string leaderboardID, LeaderBoardFilter lbFilter)
		{
			API_GetLBRank(leaderboardID, lbFilter.ToString());
		}

		private void onAmazonGamesClientServiceReady(string status)
		{
			bIsSignedIn = true;
		}

		private void onUpdateProgressComplete(string result)
		{
			string[] array = result.Split('|');
			string achievementID = array[0];
			result = array[1];
			if (result == "true" && AchievementUpdateHandler != null)
			{
				AchievementUpdateEventArgs e = new AchievementUpdateEventArgs(AchievementUpdateEventArgs.UpdateStatus.Success, achievementID, string.Empty);
				AchievementUpdateHandler(this, e);
			}
			else if (result == "false" && AchievementUpdateHandler != null)
			{
				AchievementUpdateEventArgs e2 = new AchievementUpdateEventArgs(AchievementUpdateEventArgs.UpdateStatus.Failed, achievementID, string.Empty);
				AchievementUpdateHandler(this, e2);
			}
		}

		private void onGetPlayerNameComplete(string playerName)
		{
			if (QueryPlayerNameHandler != null)
			{
				AmazonEventArgs amazonEventArgs = new AmazonEventArgs(AmazonEventArgs.Status.Success, string.Empty);
				amazonEventArgs.SetPlayerAlias(playerName);
				QueryPlayerNameHandler(this, amazonEventArgs);
			}
		}

		private void onSubmitScoreComplete(string result)
		{
			string[] array = result.Split('|');
			string leaderboardID = array[0];
			result = array[1];
			if (result == "true" && LeaderboardUpdateHandler != null)
			{
				LeaderboardUpdateArgs e = new LeaderboardUpdateArgs(LeaderboardUpdateArgs.UpdateStatus.Success, leaderboardID, string.Empty);
				LeaderboardUpdateHandler(this, e);
			}
			else if (result == "false" && LeaderboardUpdateHandler != null)
			{
				LeaderboardUpdateArgs e2 = new LeaderboardUpdateArgs(LeaderboardUpdateArgs.UpdateStatus.Failed, leaderboardID, string.Empty);
				LeaderboardUpdateHandler(this, e2);
			}
		}

		private void onGetAchivementPercentileComplete(string percenticleComplete)
		{
			if (AchievementsQueryPercentileCompleteHandler != null)
			{
				AchievementsQueryArgs achievementsQueryArgs = new AchievementsQueryArgs(AchievementsQueryArgs.QueryStatus.Success, string.Empty);
				achievementsQueryArgs.SetPercentileComplete((float)Convert.ToDouble(percenticleComplete));
				AchievementsQueryPercentileCompleteHandler(this, achievementsQueryArgs);
			}
		}

		private void onAchievementHiddenComplete(string hidden)
		{
			if (AchievementsQueryAchievementHiddenHandler != null)
			{
				AchievementsQueryArgs achievementsQueryArgs = new AchievementsQueryArgs(AchievementsQueryArgs.QueryStatus.Success, string.Empty);
				achievementsQueryArgs.SetHidden(Convert.ToBoolean(hidden));
				AchievementsQueryAchievementHiddenHandler(this, achievementsQueryArgs);
			}
		}

		private void onAchievementUnlockedComplete(string unLocked)
		{
			if (AchievementsQueryAchievementUnlockedHandler != null)
			{
				AchievementsQueryArgs achievementsQueryArgs = new AchievementsQueryArgs(AchievementsQueryArgs.QueryStatus.Success, string.Empty);
				achievementsQueryArgs.SetUnlocked(Convert.ToBoolean(unLocked));
				AchievementsQueryAchievementUnlockedHandler(this, achievementsQueryArgs);
			}
		}

		private void onGetLBScoreComplete(string score)
		{
			if (LeaderboardQueryScoreHandler != null)
			{
				LeaderboardQueryArgs leaderboardQueryArgs = new LeaderboardQueryArgs(LeaderboardQueryArgs.QueryStatus.Success, string.Empty);
				leaderboardQueryArgs.SetScore(Convert.ToInt64(score));
				LeaderboardQueryScoreHandler(this, leaderboardQueryArgs);
			}
		}

		private void onGetLBRankComplete(string rank)
		{
			if (LeaderboardQueryRankHandler != null)
			{
				LeaderboardQueryArgs leaderboardQueryArgs = new LeaderboardQueryArgs(LeaderboardQueryArgs.QueryStatus.Success, string.Empty);
				leaderboardQueryArgs.SetRank(Convert.ToInt16(rank));
				LeaderboardQueryRankHandler(this, leaderboardQueryArgs);
			}
		}

		private static void API_Init(int supportedFeatures)
		{
			amazon.CallStatic("Init", "AmazonGO", supportedFeatures);
		}

		private static void API_Sync(string description, string conflictStrategy)
		{
			amazon.CallStatic("Sync", description, conflictStrategy);
		}

		private static void API_RequestRevert()
		{
			amazon.CallStatic("RequestRevert");
		}

		private static void API_GetPlayerName()
		{
			amazon.CallStatic("GetPlayerName");
		}

		private static void API_SetPopUpLocation(PopUpLocation location)
		{
			amazon.CallStatic("SetPopUpLocation", (int)location);
		}

		private static void API_UpdateProgress(string achievementID, float percentageComplete)
		{
			amazon.CallStatic("UpdateProgress", achievementID, percentageComplete);
		}

		private static void API_ShowAchievementsOverlay()
		{
			amazon.CallStatic("ShowAchievementsOverlay");
		}

		private static void API_ResetAchievement(string achievementID)
		{
			amazon.CallStatic("ResetAchievement", achievementID);
		}

		private static void API_ResetAchievements()
		{
			amazon.CallStatic("ResetAchievements");
		}

		private static void API_GetAchivementPercentileComplete(string achievementID)
		{
			amazon.CallStatic("GetAchivementPercentileComplete", achievementID);
		}

		private static void API_IsAchievementHidden(string achievementID)
		{
			amazon.CallStatic("IsAchievementHidden", achievementID);
		}

		private static void API_IsAchievementUnlocked(string achievementID)
		{
			amazon.CallStatic("IsAchievementUnlocked", achievementID);
		}

		private static void API_SubmitScore(string leaderboardID, long score)
		{
			amazon.CallStatic("SubmitScore", leaderboardID, score);
		}

		private static void API_ShowLeaderboardsOverlay()
		{
			amazon.CallStatic("ShowLeaderboardsOverlay");
		}

		private static void API_GetLBScore(string leaderboardID, string lbFilter)
		{
			amazon.CallStatic("GetLBScore", leaderboardID, lbFilter);
		}

		private static void API_GetLBRank(string leaderboardID, string lbFilter)
		{
			amazon.CallStatic("GetLBRank", leaderboardID, lbFilter);
		}
	}
}
