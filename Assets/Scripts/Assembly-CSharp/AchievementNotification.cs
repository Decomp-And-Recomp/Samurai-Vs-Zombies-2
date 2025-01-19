using UnityEngine;

public class AchievementNotification : MonoBehaviour
{
	public GluiSprite IconSprite;

	public GluiText TitleText;

	public GluiText DescriptionText;

	public static string AchievementDescription = string.Empty;

	public static string AchievementTitle = string.Empty;

	public static Texture2D AchievementIcon;

	public static AchievementNotification Instance;

	private void Start()
	{
		if (DescriptionText != null)
		{
			DescriptionText.Text = AchievementDescription;
		}
		if (TitleText != null)
		{
			TitleText.Text = AchievementTitle;
		}
		if (IconSprite != null && AchievementIcon != null)
		{
			IconSprite.Texture = AchievementIcon;
		}
		Instance = this;
	}

	private void OnDestroy()
	{
		Instance = null;
	}
}
