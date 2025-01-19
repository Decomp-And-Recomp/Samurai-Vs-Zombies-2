using System;
using System.Collections.Generic;
using UnityEngine;

public class HUDAllies : UIHandlerComponent
{
	private class Card
	{
		private int mIndex;

		private GameObject mGameObject;

		private GluiStandardButtonContainer mButtonRef;

		private GluiSprite mIconRef;

		private ProgressMeterRadial mMeterRef;

		private GluiText mLeadershipCost;

		private GluiSprite mGoldStar;

		private bool mEnabled = true;

		private bool mSkipFirstUpdateState = true;

		public bool isAvailable
		{
			get
			{
				bool uniqueLimited = false;
				return mEnabled && WeakGlobalInstance<Leadership>.Instance.IsAvailable(mIndex, out uniqueLimited);
			}
			set
			{
				mEnabled = value;
				WeakGlobalInstance<Leadership>.Instance.ResetCoolDown(mIndex);
			}
		}

		public Card(int index, GameObject prefab, Transform trans)
		{
			mIndex = index;
			mGameObject = (GameObject)UnityEngine.Object.Instantiate(prefab);
			mGameObject.transform.parent = trans;
			mGameObject.transform.localPosition = Vector3.zero;
			mButtonRef = mGameObject.FindChildComponent<GluiStandardButtonContainer>("Button_Ally");
			mIconRef = mGameObject.FindChildComponent<GluiSprite>("SwapIcon_Ally");
			mMeterRef = mGameObject.FindChildComponent<ProgressMeterRadial>("Meter_CooldownOL");
			mLeadershipCost = mGameObject.FindChildComponent<GluiText>("SwapText_Leadership");
			mGoldStar = mGameObject.FindChildComponent<GluiSprite>("SwapIcon_ChallengerStar");
			if (mGoldStar != null && Singleton<Profile>.Instance.GetGoldenHelperUnlocked(WeakGlobalInstance<Leadership>.Instance.GetID(mIndex)))
			{
				mGoldStar.gameObject.SetActive(true);
				mGoldStar.Texture = WeakGlobalInstance<Leadership>.Instance.GetChampionIconFile(mIndex);
			}
			mIconRef.Texture = WeakGlobalInstance<Leadership>.Instance.GetIconFile(mIndex);
			mMeterRef.Texture = mIconRef.Texture;
			RefreshCost();
			WeakGlobalMonoBehavior<HUD>.Instance.RegisterOnPressEvent(mButtonRef, "SPAWN_ALLY:" + index);
		}

		public void Update(bool updateExpensiveVisuals)
		{
			if (updateExpensiveVisuals)
			{
				mMeterRef.Value = Mathf.Clamp(1f - WeakGlobalInstance<Leadership>.Instance.GetCoolDown(mIndex), 0f, 1f);
			}
			if (mSkipFirstUpdateState)
			{
				mSkipFirstUpdateState = false;
			}
			else
			{
				mButtonRef.Locked = !isAvailable;
			}
		}

		public void RefreshCost()
		{
			mLeadershipCost.Text = WeakGlobalInstance<Leadership>.Instance.GetCost(mIndex).ToString();
		}
	}

	private const string kSpawnCmd = "SPAWN_ALLY:";

	private const float kDisabledAlpha = 0.3f;

	private bool mEnabled = true;

	private List<Transform> mAlliesLocations = new List<Transform>(6);

	private List<Card> mCards = new List<Card>(6);

	public bool enabled
	{
		get
		{
			return mEnabled;
		}
		set
		{
			mEnabled = value;
			foreach (Card mCard in mCards)
			{
				mCard.isAvailable = value;
			}
		}
	}

	public HUDAllies(GameObject uiParent)
	{
		int num = 1;
		while (true)
		{
			string name = string.Format("Locator_Ally_{0}", num);
			Transform transform = ObjectUtils.FindTransformInChildren(uiParent.transform, name);
			if (transform == null)
			{
				break;
			}
			mAlliesLocations.Add(transform);
			num++;
		}
		string text = string.Empty;
		try
		{
			SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/UI/Prefabs/HUD/Card_Ally_HUD.prefab", 1);
			GameObject prefab = cachedResource.Resource as GameObject;
			List<string> selectedHelpers = Singleton<Profile>.Instance.GetSelectedHelpers();
			for (int i = 0; i < selectedHelpers.Count; i++)
			{
				if (Singleton<HelpersDatabase>.Instance[selectedHelpers[i]].resourcesCost == 0f)
				{
					text = text + "~" + selectedHelpers[i] + "~; ";
					continue;
				}
				text = text + selectedHelpers[i] + "; ";
				mCards.Add(new Card(i, prefab, mAlliesLocations[mCards.Count]));
			}
		}
		catch (Exception)
		{
		}
	}

	public void Update(bool updateExpensiveVisuals)
	{
		foreach (Card mCard in mCards)
		{
			mCard.Update(updateExpensiveVisuals);
		}
	}

	public bool OnUIEvent(string eventID)
	{
		if (eventID.Length > "SPAWN_ALLY:".Length && eventID.Substring(0, "SPAWN_ALLY:".Length) == "SPAWN_ALLY:")
		{
			TrySpawnAlly(int.Parse(eventID.Substring("SPAWN_ALLY:".Length)));
			return true;
		}
		return false;
	}

	public void OnPause(bool pause)
	{
	}

	public void RefreshCosts()
	{
		foreach (Card mCard in mCards)
		{
			mCard.RefreshCost();
		}
	}

	private void TrySpawnAlly(int index)
	{
		bool uniqueLimited = false;
		if (mCards[index].isAvailable && WeakGlobalInstance<Leadership>.Instance.IsAvailable(index, out uniqueLimited))
		{
			WeakGlobalInstance<Leadership>.Instance.Spawn(index);
		}
	}

	public bool IsAvailable(int index)
	{
		return mCards[index].isAvailable;
	}
}
