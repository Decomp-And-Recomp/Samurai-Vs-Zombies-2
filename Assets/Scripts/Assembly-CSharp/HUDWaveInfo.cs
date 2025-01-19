using UnityEngine;

public class HUDWaveInfo : UIHandlerComponent
{
	private GluiText mPercentCompletedLabel;

	private GluiMeter mMeter;

	private int mPreviousPercent = -1;

	public HUDWaveInfo(GameObject uiParent)
	{
		mPercentCompletedLabel = uiParent.FindChildComponent<GluiText>("Swap_Text_Enemies");
		mMeter = uiParent.FindChildComponent<GluiMeter>("Sprite_EnemyOverlayMeter");
		if (Singleton<Profile>.Instance.inVSMultiplayerWave)
		{
			GluiSprite gluiSprite = uiParent.FindChildComponent<GluiSprite>("Sprite_SoulSkull");
			if (gluiSprite != null)
			{
				gluiSprite.Visible = false;
			}
		}
	}

	public void Update(bool updateExpensiveVisuals)
	{
		float num = 0f;
		int num2 = 0;
		if (WeakGlobalInstance<WaveManager>.Instance != null)
		{
			num = Mathf.Clamp((float)WeakGlobalInstance<WaveManager>.Instance.enemiesKilledSoFar / (float)WeakGlobalInstance<WaveManager>.Instance.totalEnemies, 0f, 1f);
			num2 = 100;
			if (!Singleton<PlayStatistics>.Instance.data.victory)
			{
				num2 = (int)(num * 100f);
			}
		}
		else if (WeakGlobalMonoBehavior<InGameImpl>.Instance.GetGate(1) != null)
		{
			num = 1f - WeakGlobalMonoBehavior<InGameImpl>.Instance.GetGate(1).health / WeakGlobalMonoBehavior<InGameImpl>.Instance.GetGate(1).maxHealth;
			num2 = (int)(num * 100f);
		}
		else if (WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(1) != null)
		{
			num = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(1).GetPercentDoneWithWave();
			num2 = 100;
			if (!Singleton<PlayStatistics>.Instance.data.victory)
			{
				num2 = (int)(num * 100f);
			}
		}
		if (num2 != mPreviousPercent)
		{
			mPreviousPercent = num2;
			mPercentCompletedLabel.Text = string.Format(StringUtils.GetStringFromStringRef("MenuFixedStrings", "percent"), num2);
			mMeter.Value = num;
		}
	}

	public bool OnUIEvent(string eventID)
	{
		return false;
	}

	public void OnPause(bool pause)
	{
	}
}
