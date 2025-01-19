using System.Collections.Generic;
using UnityEngine;

public class EquipPageHeroes : EquipPage, UIHandlerComponent
{
	private class Card
	{
		private GameObject mSlot;

		private GluiStandardButtonContainer mButton;

		private string mID = string.Empty;

		private HeroSchema mData;

		public string id
		{
			get
			{
				return mID;
			}
		}

		public HeroSchema data
		{
			get
			{
				return mData;
			}
		}

		public GameObject gameObject
		{
			get
			{
				return mSlot;
			}
		}

		public Card(Transform uiSlot, int index, string heroCmd, string infoCmd)
		{
			mSlot = Object.Instantiate(ResourceCache.GetCachedResource("UI/Prefabs/SelectSuite/Card_Item_Available", 1).Resource) as GameObject;
			mSlot.transform.parent = uiSlot;
			mSlot.transform.localPosition = Vector3.zero;
			mSlot.transform.localScale = new Vector3(1f, 1f, 1f);
			mSlot.transform.localRotation = Quaternion.identity;
			mButton = mSlot.FindChildComponent<GluiStandardButtonContainer>("Button_Item");
			mSlot.FindChild("SwapText_Leadership").SetActive(false);
			mSlot.FindChild("Text_Qty").SetActive(false);
			WeakGlobalMonoBehavior<EquipMenuImpl>.Instance.RegisterOnReleaseEvent(mButton, heroCmd + index);
		}

		public void DrawData(HeroSchema data)
		{
			mData = data;
			mID = data.id;
			mSlot.GetComponent<GluiElement_Equip_Ally>().adaptor.SetData(data);
		}
	}

	private const string kHeroCmd = "HERO:";

	private const string kHeroInfoCmd = "HEROINFO:";

	private List<Transform> mLocatorsList = new List<Transform>();

	private List<Card> mCards = new List<Card>();

	private Transform mHeroModelLocator;

	private Hero mHeroModel;

	private int mSelectedHero = -1;

	private int mRequiredSelection = -1;

	public EquipPageHeroes(GameObject uiParent)
	{
		if (WeakGlobalInstance<EnemiesShowCase>.Instance != null)
		{
			WeakGlobalInstance<EnemiesShowCase>.Instance.highlight = false;
		}
		int num = 1;
		while (true)
		{
			string id = string.Format("Locator_Slot{0:D2}", num);
			GameObject gameObject = uiParent.FindChild(id);
			if (gameObject == null)
			{
				break;
			}
			mLocatorsList.Add(gameObject.transform);
			num++;
		}
		mHeroModelLocator = uiParent.FindChild("Locator_ModelHero").transform;
		if (Singleton<Profile>.Instance.inVSMultiplayerWave && Singleton<PlayModesManager>.Instance.Attacking)
		{
			GameObject gameObject2 = uiParent.FindChild("Locator_ModelHero_Right");
			if ((bool)gameObject2)
			{
				mHeroModelLocator = gameObject2.transform;
			}
		}
		int num2 = FindNumberOfHeroes();
		while (num2 < mLocatorsList.Count)
		{
			mLocatorsList.RemoveAt(mLocatorsList.Count - 1);
			if (num2 < mLocatorsList.Count)
			{
				mLocatorsList.RemoveAt(0);
			}
		}
		int num3 = 0;
		foreach (Transform mLocators in mLocatorsList)
		{
			mCards.Add(new Card(mLocators, num3, "HERO:", "HEROINFO:"));
			num3++;
		}
		SetupHeroesList();
		WaveSchema waveData = WaveManager.GetWaveData(Singleton<Profile>.Instance.waveToPlay, Singleton<Profile>.Instance.waveTypeToPlay);
		if (Singleton<Profile>.Instance.inDailyChallenge)
		{
			mRequiredSelection = FindIndexFromID(Singleton<Profile>.Instance.dailyChallengeHeroSchema.id);
			SelectHero(mRequiredSelection, false);
		}
		else if (waveData.recommendedHeroIsRequired && !Singleton<Profile>.Instance.inMultiplayerWave && !Singleton<Profile>.Instance.ChangingDefenseLoadout)
		{
			mRequiredSelection = FindIndexFromID(waveData.recommendedHero.Key);
			SelectHero(mRequiredSelection, false);
			Singleton<Profile>.Instance.heroID = waveData.recommendedHero.Key;
		}
		else
		{
			SelectHero(FindIndexFromID(Singleton<Profile>.Instance.heroID), false);
		}
		Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep12_HeroSelect");
	}

	public void Update(bool updateExpensiveVisuals)
	{
	}

	public bool OnUIEvent(string eventID)
	{
		if (eventID.Length > "HERO:".Length && eventID.Substring(0, "HERO:".Length) == "HERO:")
		{
			SelectHero(int.Parse(eventID.Substring("HERO:".Length)), true);
			return true;
		}
		return false;
	}

	public void OnPause(bool pause)
	{
	}

	public void Save()
	{
		if (mHeroModel != null)
		{
			mHeroModel.Destroy();
		}
	}

	private int FindNumberOfHeroes()
	{
		int num = 0;
		foreach (DataBundleRecordHandle<HeroSchema> allHero in Singleton<HeroesDatabase>.Instance.AllHeroes)
		{
			if (allHero.Data != null && !allHero.Data.disabled)
			{
				num++;
			}
		}
		return num;
	}

	private void SetupHeroesList()
	{
		int num = 0;
		foreach (DataBundleRecordHandle<HeroSchema> allHero in Singleton<HeroesDatabase>.Instance.AllHeroes)
		{
			if (num < mCards.Count && !allHero.Data.disabled)
			{
				Card card = mCards[num++];
				card.DrawData(allHero.Data);
			}
		}
	}

	private void SelectHero(int index, bool updateLoadout)
	{
		if (mCards[index].data.Locked && mCards[index].data.purchaseToUnlock)
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("IAP_TAB", "LocalizedStrings.iap_special_tab");
			GluiActionSender.SendGluiAction("POPUP_IAP", mCards[index].gameObject, null);
		}
		else if ((mRequiredSelection == -1 || index == mRequiredSelection || (mCards[index].data.overrideRequirements && !Singleton<Profile>.Instance.inDailyChallenge)) && (!mCards[index].data.Locked || Singleton<Profile>.Instance.inDailyChallenge) && index != mSelectedHero)
		{
			mSelectedHero = index;
			Card card = mCards[index];
			WeakGlobalMonoBehavior<EquipMenuImpl>.Instance.commonInfoDisplay = card.data;
			Singleton<Profile>.Instance.heroID = card.id;
			if (mHeroModel != null)
			{
				Object.Destroy(mHeroModel.controlledObject);
			}
			mHeroModel = new Hero(mHeroModelLocator, 0);
			mHeroModel.rootObject.transform.parent = mHeroModelLocator;
			mHeroModel.rootObject.transform.localPosition = Vector3.zero;
			ObjectUtils.SetLayerRecursively(mHeroModel.rootObject, LayerMask.NameToLayer("GLUI"));
			mHeroModel.rootObject.transform.localScale = new Vector3(1f, 1f, 1f);
			mHeroModel.rootObject.transform.localRotation = Quaternion.identity;
			if (Singleton<Profile>.Instance.inVSMultiplayerWave && Singleton<PlayModesManager>.Instance.Attacking)
			{
				mHeroModel.rootObject.transform.localRotation = Quaternion.AngleAxis(180f, Vector3.up);
			}
			if (Singleton<Profile>.Instance.ChangingDefenseLoadout && updateLoadout)
			{
				Singleton<Profile>.Instance.ChangingDefenseLoadout = false;
				List<string> selectedHelpers = Singleton<Profile>.Instance.GetSelectedHelpers();
				List<string> selectedAbilities = Singleton<Profile>.Instance.GetSelectedAbilities();
				Singleton<Profile>.Instance.ChangingDefenseLoadout = true;
				Singleton<Profile>.Instance.SetSelectedDefendHelpers(selectedHelpers);
				Singleton<Profile>.Instance.SetSelectedDefendAbilities(selectedAbilities);
				Singleton<Profile>.Instance.MultiplayerData.LocalPlayerLoadout.UpdateLocalProfile();
			}
		}
	}

	private int FindIndexFromID(string id)
	{
		int num = 0;
		foreach (Card mCard in mCards)
		{
			if (mCard.id == id)
			{
				return num;
			}
			num++;
		}
		return -1;
	}
}
