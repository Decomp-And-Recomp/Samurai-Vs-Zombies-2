using UnityEngine;
using UnityEngine.SocialPlatforms;

public class LeaderboardHandler : MonoBehaviour, IGluiActionHandler
{
	public LeaderboardListController LeaderboardList;

	public GluiStandardButtonContainer FriendToggleButton;

	public GluiText StatHeader;

	public GluiText TimeScopeText;

	public void Start()
	{
		if (FriendToggleButton != null)
		{
			FriendToggleButton.Selected = LeaderboardList.userScope == UserScope.Global;
		}
		UpdateStatHeader();
		UpdateTimeScopeText();
	}

	private void UpdateStatHeader()
	{
		if (StatHeader != null)
		{
			if (LeaderboardList.LeaderboardId == Profile.kPlayerRatingLeaderboard)
			{
				StatHeader.TaggedStringReference = "MenuFixedStrings.Menu_AttackRatingA";
			}
			else
			{
				StatHeader.TaggedStringReference = "MenuFixedStrings.PowerRating_TempExplain2";
			}
		}
	}

	private void UpdateTimeScopeText()
	{
		if (TimeScopeText != null)
		{
			if (LeaderboardList.timeScope == GameCenterLeaderboardTimeScope.AllTime)
			{
				TimeScopeText.TaggedStringReference = "MenuFixedStrings.Leaderboard_AllTime";
			}
			else if (LeaderboardList.timeScope == GameCenterLeaderboardTimeScope.Today)
			{
				TimeScopeText.TaggedStringReference = "MenuFixedStrings.Leaderboard_Daily";
			}
			else if (LeaderboardList.timeScope == GameCenterLeaderboardTimeScope.Week)
			{
				TimeScopeText.TaggedStringReference = "MenuFixedStrings.Leaderboard_Week";
			}
		}
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		switch (action)
		{
		case "LEADERBOARD_DAILYCHALLENGE":
			LeaderboardList.SetLeaderboard(Profile.kDailyChallengeLeaderboard);
			UpdateStatHeader();
			return true;
		case "LEADERBOARD_MULTIPLAYER":
			LeaderboardList.SetLeaderboard(Profile.kMultiplayerLeaderboard);
			UpdateStatHeader();
			return true;
		case "LEADERBOARD_POWER":
			LeaderboardList.SetLeaderboard(Profile.kPlayerRatingLeaderboard);
			UpdateStatHeader();
			return true;
		case "LEADERBOARD_ALLTIME":
			LeaderboardList.SetTimeScope(GameCenterLeaderboardTimeScope.AllTime);
			UpdateTimeScopeText();
			return true;
		case "LEADERBOARD_DAILY":
			LeaderboardList.SetTimeScope(GameCenterLeaderboardTimeScope.Today);
			UpdateTimeScopeText();
			return true;
		case "LEADERBOARD_WEEKLY":
			LeaderboardList.SetTimeScope(GameCenterLeaderboardTimeScope.Week);
			UpdateTimeScopeText();
			return true;
		case "FRIEND_TOGGLE":
			if (FriendToggleButton != null)
			{
				if (LeaderboardList.userScope == UserScope.Global)
				{
					LeaderboardList.SetUserScope(UserScope.FriendsOnly);
					FriendToggleButton.Selected = false;
				}
				else
				{
					LeaderboardList.SetUserScope(UserScope.Global);
					FriendToggleButton.Selected = true;
				}
			}
			return true;
		default:
			return false;
		}
	}
}
