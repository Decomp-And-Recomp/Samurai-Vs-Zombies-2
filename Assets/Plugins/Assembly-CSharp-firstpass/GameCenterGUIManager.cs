using System.Collections.Generic;
using UnityEngine;

public class GameCenterGUIManager : MonoBehaviour
{
	private List<GameCenterLeaderboard> leaderboards;

	private List<GameCenterAchievementMetadata> achievementMetadata;

	private void Start()
	{
		GameCenterManager.categoriesLoaded += delegate(List<GameCenterLeaderboard> leaderboards)
		{
			this.leaderboards = leaderboards;
		};
		GameCenterManager.achievementMetadataLoaded += delegate(List<GameCenterAchievementMetadata> achievementMetadata)
		{
			this.achievementMetadata = achievementMetadata;
		};
	}

	private void OnGUI()
	{
		float num = 5f;
		float left = 5f;
		float num2 = ((Screen.width < 960 && Screen.height < 960) ? 160 : 320);
		float num3 = ((Screen.width < 960 && Screen.height < 960) ? 40 : 80);
		float num4 = num3 + 10f;
		if (GUI.Button(new Rect(left, num, num2, num3), "Authenticate"))
		{
			GameCenterBinding.authenticateLocalPlayer();
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Load Achievement Metadata"))
		{
			GameCenterBinding.retrieveAchievementMetadata();
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Get Raw Achievements"))
		{
			GameCenterBinding.getAchievements();
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Post Achievement") && achievementMetadata != null && achievementMetadata.Count > 0)
		{
			int num5 = Random.Range(2, 60);
			Debug.Log("sending percentComplete: " + num5);
			GameCenterBinding.reportAchievement(achievementMetadata[0].identifier, num5);
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Show Achievements"))
		{
			GameCenterBinding.showAchievements();
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Reset Achievements"))
		{
			GameCenterBinding.resetAchievements();
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Multiplayer Scene"))
		{
			Application.LoadLevel("GameCenterMultiplayerTestScene");
		}
		left = (float)Screen.width - num2 - 5f;
		num = 5f;
		if (GUI.Button(new Rect(left, num, num2, num3), "Get Player Alias"))
		{
			string text = GameCenterBinding.playerAlias();
			Debug.Log("Player alias: " + text);
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Load Leaderboard Data"))
		{
			GameCenterBinding.loadLeaderboardTitles();
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Post Score") && leaderboards != null && leaderboards.Count > 0)
		{
			Debug.Log("about to report a random score for leaderboard: " + leaderboards[0].leaderboardId);
			GameCenterBinding.reportScore(Random.Range(1, 99999), leaderboards[0].leaderboardId);
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Show Leaderboards"))
		{
			GameCenterBinding.showLeaderboardWithTimeScope(GameCenterLeaderboardTimeScope.AllTime);
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Get Raw Score Data"))
		{
			if (leaderboards != null && leaderboards.Count > 0)
			{
				GameCenterBinding.retrieveScores(false, GameCenterLeaderboardTimeScope.AllTime, 1, 25, leaderboards[0].leaderboardId);
			}
			else
			{
				Debug.Log("Load leaderboard data before attempting to retrieve scores");
			}
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Get Scores for Me"))
		{
			if (leaderboards != null && leaderboards.Count > 0)
			{
				GameCenterBinding.retrieveScoresForPlayerId(GameCenterBinding.playerIdentifier(), leaderboards[0].leaderboardId);
			}
			else
			{
				Debug.Log("Load leaderboard data before attempting to retrieve scores");
			}
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Retrieve Friends"))
		{
			GameCenterBinding.retrieveFriends();
		}
	}
}
