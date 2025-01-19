using System;

[Serializable]
public class DataAdaptor_Achievement : DataAdaptorBase
{
	public GluiText text_displayName;

	public GluiText text_description;

	public GluiSprite sprite_icon;

	public GluiStandardButtonContainer iconButton;

	public GluiStandardButtonContainer facebookButton;

	private AchievementTracker achievement;

	public override void SetData(object data)
	{
		achievement = data as AchievementTracker;
		if (achievement == null)
		{
			return;
		}
		if (text_displayName != null)
		{
			text_displayName.Text = StringUtils.GetStringFromStringRef(achievement.achievement.Data.displayName);
		}
		if (text_description != null)
		{
			text_description.Text = StringUtils.GetStringFromStringRef(achievement.achievement.Data.description);
		}
		if (sprite_icon != null && achievement.completedCount >= achievement.achievement.Data.CompletionCount)
		{
			sprite_icon.Texture = achievement.achievement.Data.Icon;
		}
		if (iconButton != null)
		{
			iconButton.Locked = achievement.completedCount < achievement.achievement.Data.CompletionCount;
		}
		if (facebookButton != null)
		{
			if (achievement.completedCount < achievement.achievement.Data.CompletionCount || achievement.shared)
			{
				facebookButton.gameObject.SetActive(false);
				return;
			}
			facebookButton.gameObject.SetActive(true);
			string text = "FACEBOOK_SHARE_" + achievement.achievement.Data.id;
			facebookButton.onReleaseActions = new string[1] { text };
		}
	}
}
