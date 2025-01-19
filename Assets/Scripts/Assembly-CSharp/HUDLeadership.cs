using System.Collections.Generic;
using UnityEngine;

public class HUDLeadership : UIHandlerComponent
{
	private const string kUpgradeCommand = "UPGRADE_LEADERSHIP";

	private GameObject mRootObject;

	private GameObject mUpgradeIndicator;

	private Vector3 mUpgradeIndicatorOriginalPosition;

	private GluiText mLeadershipLevel;

	private int mPreviousLeadershipLevel = -1;

	private GluiText mLeadershipCount;

	private int mPreviousLeadershipCount = -1;

	private GluiStandardButtonContainer mUpgradeButton;

	private List<GluiMeter> mMeters = new List<GluiMeter>(4);

	private GluiMeter mActiveMeter;

	private Transform[] mUpgradeFXLocators;

	private bool mEnabled = true;

	public bool enabled
	{
		get
		{
			return mEnabled;
		}
		set
		{
			if (value != mEnabled)
			{
				mEnabled = value;
				mRootObject.SetActive(mEnabled);
			}
		}
	}

	public HUDLeadership(GameObject uiParent)
	{
		mRootObject = uiParent.FindChild("HUD_Leadership");
		mUpgradeIndicator = uiParent.FindChild("UpgradeIndicator");
		mUpgradeIndicatorOriginalPosition = mUpgradeIndicator.transform.localPosition;
		mLeadershipLevel = mRootObject.FindChildComponent<GluiText>("Swap_Text_Level");
		mLeadershipCount = mRootObject.FindChildComponent<GluiText>("Swap_Text_Counter");
		GameObject parent = mRootObject.FindChild("Enabler_Upgrade");
		mUpgradeButton = parent.FindChildComponent<GluiStandardButtonContainer>("Button_Upgrade");
		int num = 0;
		while (true)
		{
			string name = string.Format("Meter_Leadership_{0:D2}", num);
			Transform transform = ObjectUtils.FindTransformInChildren(mRootObject.transform, name);
			if (transform == null)
			{
				break;
			}
			mMeters.Add(transform.gameObject.GetComponent<GluiMeter>());
			num++;
		}
		mUpgradeFXLocators = new Transform[3];
		for (int i = 0; i < 3; i++)
		{
			string name2 = string.Format("Locator_FX_Upgrade{0:D2}", i + 1);
			mUpgradeFXLocators[i] = ObjectUtils.FindTransformInChildren(mRootObject.transform, name2);
		}
		WeakGlobalMonoBehavior<HUD>.Instance.RegisterOnReleaseEvent(mUpgradeButton, "UPGRADE_LEADERSHIP");
		UpdateVisuals(true);
		RefreshLevelUpIndicators();
	}

	public void Update(bool updateExpensiveVisuals)
	{
		if (mEnabled)
		{
			UpdateVisuals(updateExpensiveVisuals);
		}
	}

	public bool OnUIEvent(string eventID)
	{
		if (eventID == "UPGRADE_LEADERSHIP")
		{
			if (WeakGlobalInstance<Leadership>.Instance.isUpgradable)
			{
				WeakGlobalInstance<Leadership>.Instance.LevelUp();
				UpdateVisuals(true);
				RefreshLevelUpIndicators();
			}
			return true;
		}
		return false;
	}

	public void OnPause(bool pause)
	{
	}

	private void ActivateMeter(int meterLevel)
	{
		mActiveMeter = null;
		for (int i = 0; i < mMeters.Count; i++)
		{
			bool flag = i == meterLevel;
			mMeters[i].gameObject.SetActive(flag);
			if (flag)
			{
				mActiveMeter = mMeters[i];
			}
		}
		Transform transform = mUpgradeFXLocators[Mathf.Max(0, meterLevel - 1)];
		SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/UI/Prefabs/HUD/FX_LeadershipUpgrade.prefab", 1);
		if (transform != null && cachedResource != null && cachedResource.Resource != null)
		{
			GameObjectPool.DefaultObjectPool.Acquire(cachedResource.Resource as GameObject, transform.position, Quaternion.identity);
		}
	}

	private void UpdateVisuals(bool updateExpensiveVisuals)
	{
		if (mUpgradeButton.gameObject.activeSelf != WeakGlobalInstance<Leadership>.Instance.isUpgradable)
		{
			mUpgradeButton.gameObject.SetActive(WeakGlobalInstance<Leadership>.Instance.isUpgradable);
		}
		int level = WeakGlobalInstance<Leadership>.Instance.level;
		if (level != mPreviousLeadershipLevel)
		{
			mPreviousLeadershipLevel = level;
			ActivateMeter(mPreviousLeadershipLevel);
			mLeadershipLevel.Text = string.Format(StringUtils.GetStringFromStringRef("MenuFixedStrings", "stat_level"), level + 1);
		}
		int num = (int)WeakGlobalInstance<Leadership>.Instance.resources;
		if (num != mPreviousLeadershipCount)
		{
			mPreviousLeadershipCount = num;
			mLeadershipCount.Text = num.ToString();
		}
		mActiveMeter.Value = WeakGlobalInstance<Leadership>.Instance.resources / WeakGlobalInstance<Leadership>.Instance.maxResources;
	}

	private void RefreshLevelUpIndicators()
	{
		if (WeakGlobalInstance<Leadership>.Instance.level < WeakGlobalInstance<Leadership>.Instance.maxLevel)
		{
			float x = mActiveMeter.Size.x;
			float num = x * (WeakGlobalInstance<Leadership>.Instance.levelUpThreshold / WeakGlobalInstance<Leadership>.Instance.maxResources);
			mUpgradeIndicator.transform.localPosition = new Vector3(mUpgradeIndicatorOriginalPosition.x + num, mUpgradeIndicatorOriginalPosition.y, mUpgradeIndicatorOriginalPosition.z);
		}
		else
		{
			mUpgradeIndicator.SetActive(false);
		}
	}
}
