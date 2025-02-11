using Glu.Plugins.ASocial;
using UnityEngine;

public class PowerRatingImpl : MonoBehaviour, IGluiActionHandler
{
	public GluiText[] RatingValueText = new GluiText[14];

	public GluiText AttackRatingText;

	public GluiText PlayerNameText;

	public static int[] RatingsToDisplay;

	public static int AttackRatingToDisplay;

	public static string PlayerNameToDisplay;

	private GameObject _leaderboardsButton;

	private GameObject _achievementsButton;

	private void Start()
	{
		if (GameObject.Find("PopUp_Prestige(Clone)") != null)
		{
			_leaderboardsButton = GameObject.Find("Button_Leaderboards");
			_achievementsButton = GameObject.Find("Button_Achievements");
			_leaderboardsButton.gameObject.SetActive(false);
			_achievementsButton.gameObject.SetActive(false);
			GameObject.Find("Button_GameCenter").gameObject.SetActive(false);
			if (!AJavaTools.Properties.IsBuildAmazon())
			{
				GameObject.Find("10_AchievementsEarned").FindChild("Text_Stat").SetActive(false);
				GameObject.Find("10_AchievementsEarned").FindChild("SwapText_Points").SetActive(false);
				Vector3 localPosition = GameObject.Find("11_DailyRewards").transform.localPosition;
				GameObject.Find("11_DailyRewards").transform.localPosition = new Vector3(localPosition.x, localPosition.y + 47f, localPosition.z);
			}
		}
		if (RatingsToDisplay == null)
		{
			RatingsToDisplay = Singleton<Profile>.Instance.AttackRatingCategory;
			AttackRatingToDisplay = Singleton<Profile>.Instance.playerAttackRating;
			PlayerNameToDisplay = Singleton<Profile>.Instance.MultiplayerData.UserName;
		}
		if (RatingsToDisplay != null)
		{
			for (int i = 0; i < 14; i++)
			{
				if (i < RatingsToDisplay.Length && RatingValueText[i] != null)
				{
					RatingValueText[i].Text = RatingsToDisplay[i].ToString();
				}
			}
		}
		if (AttackRatingText != null)
		{
			AttackRatingText.Text = string.Format(StringUtils.GetStringFromStringRef(AttackRatingText.TaggedStringReference), AttackRatingToDisplay);
		}
		if (PlayerNameText != null)
		{
			PlayerNameText.Text = PlayerNameToDisplay;
		}
		RatingsToDisplay = null;
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		switch (action)
		{
		case "GAMECENTER_LEADERBOARD":
			return true;
		case "INGAME_ACHIEVEMENTS":
			return true;
		case "LEADERBOARDS":
			return true;
		default:
			return false;
		}
	}
}
