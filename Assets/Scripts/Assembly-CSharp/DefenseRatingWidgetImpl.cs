using UnityEngine;

public class DefenseRatingWidgetImpl : MonoBehaviour
{
	public GluiSprite IconSprite;

	public GluiSprite GoldenIconSprite;

	public GluiText LevelText;

	public void SetupWidget(Texture2D icon, Texture2D goldIcon, int level)
	{
		if (IconSprite != null)
		{
			IconSprite.Texture = icon;
		}
		if (GoldenIconSprite != null)
		{
			if (goldIcon != null)
			{
				GoldenIconSprite.Texture = goldIcon;
			}
			else
			{
				GoldenIconSprite.Visible = false;
			}
		}
		if ((bool)LevelText)
		{
			level = Mathf.Max(level, 1);
			LevelText.Text = level.ToString();
		}
		base.transform.localScale = Vector3.one;
	}

	public void SetupWidget(string iconPath, int level)
	{
		SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource(iconPath, 1);
		SetupWidget(cachedResource.Resource as Texture2D, null, level);
	}
}
