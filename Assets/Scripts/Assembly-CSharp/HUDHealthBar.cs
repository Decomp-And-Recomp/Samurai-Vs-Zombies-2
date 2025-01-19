using UnityEngine;

public class HUDHealthBar : UIHandlerComponent
{
	private Color mStartupColor;

	private GluiSprite mPortrait;

	private GluiMeter mMeter;

	private Hero mObservedChar;

	private bool mPortraitSet;

	public HUDHealthBar(GameObject uiParent, Hero charToObserve)
	{
		mMeter = uiParent.FindChildComponent<GluiMeter>("Meter_Life");
		mStartupColor = mMeter.Color;
		mObservedChar = charToObserve;
		mPortrait = uiParent.FindChildComponent<GluiSprite>("Swap_HeroPortrait");
	}

	public void Update(bool updateExpensiveVisuals)
	{
		TrySetPortrait();
		float mountedHealth = mObservedChar.mountedHealth;
		if (mountedHealth > 0f)
		{
			mMeter.Value = mountedHealth / mObservedChar.mountedHealthMax;
			mMeter.Color = Color.cyan;
		}
		else
		{
			UpdateHealthThreshold();
		}
	}

	public bool OnUIEvent(string eventID)
	{
		return false;
	}

	public void OnPause(bool pause)
	{
	}

	private void TrySetPortrait()
	{
		if (!mPortraitSet && WeakGlobalMonoBehavior<HUDSharedHeroPortrait>.Instance != null)
		{
			Texture2D texture = WeakGlobalMonoBehavior<HUDSharedHeroPortrait>.Instance.Texture;
			if (texture != null)
			{
				mPortrait.Texture = texture;
				mPortraitSet = true;
			}
		}
	}

	private void UpdateHealthThreshold()
	{
		float health = mObservedChar.health;
		float value = health / mObservedChar.maxHealth;
		mMeter.Value = value;
		float num = mObservedChar.maxHealth / 3f;
		if (health <= num)
		{
			mMeter.Color = Color.red;
		}
		else if (health <= num * 2f)
		{
			mMeter.Color = Color.yellow;
		}
		else
		{
			mMeter.Color = mStartupColor;
		}
	}
}
