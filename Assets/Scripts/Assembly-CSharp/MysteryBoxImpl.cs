using System;
using System.Collections.Generic;
using UnityEngine;

public class MysteryBoxImpl : MonoBehaviour, IGluiActionHandler
{
	private class OddsEntry
	{
		public string id;

		public string packId;

		public int num;

		public bool isGoldenHelper;

		public int weight;
	}

	public GluiStandardButtonContainer button;

	public GluiStandardButtonContainer closeButton;

	public GluiStandardButtonContainer buyMoreButton;

	public GluiStandardButtonContainer FacebookButton;

	public GluiText tutorialText;

	public GluiSprite presentIcon;

	public float delayBeforeSlowDown = 2f;

	public float firstSpinDelay = 0.1f;

	public float lastSpinDelay = 0.6f;

	public float spinDelaySlowingIncrement = 0.05f;

	private string soundOnPop = "UI_PrizeStart";

	public string soundOnSpin = "UI_PrizeClick";

	public string soundOnOver = "UI_Prize_Win";

	public static string BoxID;

	public Cost cost;

	public static Cost MysteryBoxCost;

	private static Texture2D smPresentOverrideTexture;

	private GluiElement_ResultsLoot mContentDisplay;

	private PlayStatistics.Data.LootEntry mPrize;

	private int mPrizeIndex = -1;

	private bool mPackageOpened;

	private List<OddsEntry> mOddsTable;

	private int mGoldenHelperIndex = -1;

	private string mGoldenHelperID;

	private int mTotalUnlockedGoldenHelpers;

	private float mSpinDelay;

	private float mSpinTimer;

	private float mBeforeSlowDownTimer;

	private int mLastSpinIndex = -1;

	private void Awake()
	{
		cost = MysteryBoxCost;
		MysteryBoxCost = new Cost(0, Cost.Currency.Soft, 0f);
	}

	private void Start()
	{
		mTotalUnlockedGoldenHelpers = Singleton<HelpersDatabase>.Instance.GetTotalGoldenHelperUnlocks();
		mGoldenHelperID = Singleton<HelpersDatabase>.Instance.GetRandomAvailableGoldenHelper();
		mOddsTable = GetRandomTable();
		mPrize = RandomizeContent();
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["Reward"] = mPrize.id;
		dictionary["PlayerLevel"] = Singleton<Profile>.Instance.playerLevel;
		dictionary["WaveNumber"] = Singleton<Profile>.Instance.wave_SinglePlayerGame;
		dictionary["MPWavesWon"] = Singleton<Profile>.Instance.mpWavesWon;
		Singleton<Analytics>.Instance.LogEvent("MysteryBox", dictionary);
		if (string.IsNullOrEmpty(BoxID))
		{
			BoxID = "MysteryBox";
		}
		Singleton<Analytics>.Instance.KontagentEvent(mPrize.id, "MysteryBox", BoxID, Singleton<Profile>.Instance.wave_SinglePlayerGame, mPrize.num, Analytics.KParam("PlayerLevel", Singleton<Profile>.Instance.playerLevel.ToString()), Analytics.KParam("MPWavesWon", Singleton<Profile>.Instance.mpWavesWon.ToString()));
		if (closeButton != null)
		{
			closeButton.Locked = true;
			closeButton.Enabled = false;
		}
		mContentDisplay = base.gameObject.GetComponent<GluiElement_ResultsLoot>();
		if (FacebookButton != null)
		{
			FacebookButton.gameObject.SetActive(false);
		}
		if (buyMoreButton != null)
		{
			buyMoreButton.gameObject.SetActive(false);
		}
		if (tutorialText != null && !WeakGlobalMonoBehavior<ResultsMenuImpl>.Exists)
		{
			tutorialText.gameObject.SetActive(false);
		}
		if (smPresentOverrideTexture != null && presentIcon != null)
		{
			presentIcon.Texture = smPresentOverrideTexture;
		}
	}

	private void Update()
	{
		if (mSpinDelay != 0f)
		{
			UpdateSpinning();
		}
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		switch (action)
		{
		case "BUTTON_GIFT_TOUCHED":
			if (data is bool)
			{
				OnGiftTouched((bool)data);
			}
			else
			{
				OnGiftTouched();
			}
			return true;
		case "BUY_MORE_MYSTERY_BOX":
			if (WeakGlobalMonoBehavior<ResultsMenuImpl>.Exists)
			{
				List<StoreData.Item> list = new List<StoreData.Item>();
				StoreAvailability.GetMysteryBox(list);
				GluiActionSender.SendGluiAction("POPUP_CONFIRMPURCHASE", base.gameObject, list[0]);
			}
			else
			{
				GluiActionSender.SendGluiAction("POPUP_POP", base.gameObject, null);
			}
			return true;
		case "FACEBOOK_SHARE_PRIZE":
		{
			HelperSchema helperSchema = Singleton<HelpersDatabase>.Instance[mPrize.id];
			string empty = string.Empty;
			string description = string.Empty;
			if (helperSchema != null)
			{
				empty = StringUtils.GetStringFromStringRef("LocalizedText", helperSchema.displayName);
				description = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookMysteryBoxGoldenMessage"), empty);
			}
			SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.AndroidFacebookFeed(StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookMysteryBoxTitle"), description, FacebookButton.gameObject, string.Empty, string.Empty);
			return true;
		}
		default:
			return false;
		}
	}

	private void onFeedPost(string postId)
	{
		if (!string.IsNullOrEmpty(postId))
		{
			FacebookButton.gameObject.SetActive(false);
		}
	}

	public static void SetOverrideTexture(Texture2D texture)
	{
		smPresentOverrideTexture = texture;
	}

	public static Texture2D GetOverrideTexture()
	{
		return smPresentOverrideTexture;
	}

	private void OnGiftTouched(bool playPopSound = true)
	{
		if (!mPackageOpened)
		{
			mPackageOpened = true;
			if (playPopSound)
			{
				PlayPopSound();
			}
			if (button != null)
			{
				button.Selected = true;
				button.Enabled = false;
			}
			if (tutorialText != null)
			{
				tutorialText.gameObject.SetActive(false);
			}
			mSpinDelay = firstSpinDelay;
			mSpinTimer = 0f;
			ShowSpinningRandomContent();
		}
	}

	private void OnSpinDone()
	{
		mSpinDelay = 0f;
		mContentDisplay.adaptor.SetData(mPrize);
		PlayOverSound();
		CashInContent();
		Singleton<Profile>.Instance.SetMysteryBoxPurchased(true);
		Singleton<Profile>.Instance.Save();
		if (closeButton != null)
		{
			closeButton.Locked = false;
			closeButton.Enabled = true;
		}
		if (buyMoreButton != null)
		{
			buyMoreButton.gameObject.SetActive(true);
		}
	}

	private void CashInContent()
	{
		if (mPrize == null)
		{
			return;
		}
		if (mPrize.id == "dealpack")
		{
			StoreAvailability.CashInDealPackFromString(mPrize.packId, 1);
		}
		else
		{
			CashIn.From(mPrize.id, mPrize.num, "Mystery Box");
		}
		cost.Spend();
		cost.gwalletSpend(cost, "DEBIT_IN_APP_PURCHASE", "Mystery Box");
		if (FacebookButton != null)
		{
			CashIn.ItemType type = CashIn.GetType(mPrize.id);
			if (type == CashIn.ItemType.GoldenHelper)
			{
				FacebookButton.gameObject.SetActive(true);
			}
		}
	}

	private void PlayPopSound()
	{
		GluiSoundSender.SendGluiSound(soundOnPop, base.gameObject);
	}

	private void PlaySpinSound()
	{
		GluiSoundSender.SendGluiSound(soundOnSpin, base.gameObject);
	}

	private void PlayOverSound()
	{
		GluiSoundSender.SendGluiSound(soundOnOver, base.gameObject);
	}

	private void ShowSpinningRandomContent()
	{
		mContentDisplay.adaptor.SetData(GetSpinningRandomContent());
		PlaySpinSound();
	}

	private void UpdateSpinning()
	{
		if (mBeforeSlowDownTimer < delayBeforeSlowDown)
		{
			mBeforeSlowDownTimer += Time.deltaTime;
		}
		mSpinTimer += Time.deltaTime;
		if (mSpinTimer < mSpinDelay)
		{
			return;
		}
		if (mSpinDelay >= lastSpinDelay)
		{
			OnSpinDone();
			return;
		}
		mSpinTimer -= mSpinDelay;
		if (mBeforeSlowDownTimer >= delayBeforeSlowDown)
		{
			mSpinDelay += spinDelaySlowingIncrement;
		}
		ShowSpinningRandomContent();
	}

	private PlayStatistics.Data.LootEntry GetSpinningRandomContent()
	{
		int num = -1;
		string text = null;
		if (smPresentOverrideTexture == null && mLastSpinIndex != mGoldenHelperIndex && UnityEngine.Random.Range(0, 100) < 25)
		{
			num = mGoldenHelperIndex;
			text = Singleton<HelpersDatabase>.Instance.GetRandomAvailableGoldenHelper();
		}
		if (string.IsNullOrEmpty(text))
		{
			bool flag;
			do
			{
				num = UnityEngine.Random.Range(0, mOddsTable.Count - 1);
				flag = true;
				if (string.Compare(mOddsTable[num].id, mOddsTable[mPrizeIndex].id, StringComparison.OrdinalIgnoreCase) == 0)
				{
					flag = false;
				}
				else if (mLastSpinIndex >= 0 && string.Compare(mOddsTable[num].id, mOddsTable[mLastSpinIndex].id, StringComparison.OrdinalIgnoreCase) == 0)
				{
					flag = false;
				}
			}
			while (!flag);
		}
		mLastSpinIndex = num;
		OddsEntry oddsEntry;
		if (num == mGoldenHelperIndex)
		{
			oddsEntry = new OddsEntry();
			oddsEntry.id = text;
			oddsEntry.num = 1;
		}
		else
		{
			oddsEntry = mOddsTable[num];
		}
		PlayStatistics.Data.LootEntry lootEntry = new PlayStatistics.Data.LootEntry();
		lootEntry.id = oddsEntry.id;
		lootEntry.num = oddsEntry.num;
		lootEntry.packId = oddsEntry.packId;
		if (string.IsNullOrEmpty(lootEntry.id))
		{
			return GetSpinningRandomContent();
		}
		return lootEntry;
	}

	private PlayStatistics.Data.LootEntry RandomizeContent()
	{
		if (!Singleton<Profile>.Instance.GetMysteryBoxPurchased() && (!WeakGlobalInstance<MultipleMysteryBoxContents>.Exists || !WeakGlobalInstance<MultipleMysteryBoxContents>.Instance.initialReviveGiven))
		{
			if (string.IsNullOrEmpty(BoxID))
			{
				BoxID = "MysteryBox";
			}
			TextDBSchema[] data = DataBundleUtils.InitializeRecords<TextDBSchema>(BoxID);
			string @string = data.GetString("firstTime");
			if (!string.IsNullOrEmpty(@string))
			{
				if (WeakGlobalInstance<MultipleMysteryBoxContents>.Exists)
				{
					WeakGlobalInstance<MultipleMysteryBoxContents>.Instance.initialReviveGiven = true;
				}
				OddsEntry oddsEntry = ParseOddsEntry(@string, string.Empty);
				mPrizeIndex = mOddsTable.Count;
				mOddsTable.Add(oddsEntry);
				PlayStatistics.Data.LootEntry lootEntry = new PlayStatistics.Data.LootEntry();
				lootEntry.id = oddsEntry.id;
				lootEntry.num = oddsEntry.num;
				lootEntry.packId = oddsEntry.packId;
				return lootEntry;
			}
		}
		int totalWeight = GetTotalWeight(mOddsTable);
		for (int i = 0; i < 20; i++)
		{
			int num = UnityEngine.Random.Range(0, totalWeight - 1);
			mPrizeIndex = 0;
			foreach (OddsEntry item in mOddsTable)
			{
				num -= item.weight;
				if (num < 0)
				{
					PlayStatistics.Data.LootEntry lootEntry2 = new PlayStatistics.Data.LootEntry();
					lootEntry2.id = item.id;
					lootEntry2.num = item.num;
					lootEntry2.packId = item.packId;
					if (lootEntry2.id.Contains("Golden") && WeakGlobalInstance<MultipleMysteryBoxContents>.Exists)
					{
						if (WeakGlobalInstance<MultipleMysteryBoxContents>.Instance.ContainsHelper(lootEntry2.id))
						{
							continue;
						}
						WeakGlobalInstance<MultipleMysteryBoxContents>.Instance.RegisterGoldenHelper(lootEntry2.id);
					}
					return lootEntry2;
				}
				mPrizeIndex++;
			}
		}
		throw new Exception("Could not find a valid MysteryBox prize.");
	}

	private OddsEntry ParseOddsEntry(string key, string weight)
	{
		if (key.Length > 0 && key[0] == '+')
		{
			string[] array = key.Substring(1).Trim().Split(' ');
			if (array.Length >= 2)
			{
				OddsEntry oddsEntry = new OddsEntry();
				oddsEntry.id = array[1];
				if (oddsEntry.id == "dealpack" && array.Length >= 3)
				{
					oddsEntry.packId = array[2];
				}
				else if (oddsEntry.id != "goldenhelper")
				{
					for (int i = 2; i < array.Length; i++)
					{
						oddsEntry.id += ".";
						oddsEntry.id += array[i];
					}
				}
				if (int.TryParse(array[0], out oddsEntry.num) && oddsEntry.num > 0)
				{
					if (int.TryParse(weight, out oddsEntry.weight) && oddsEntry.weight > 0 && oddsEntry.id.Equals("goldenhelper", StringComparison.OrdinalIgnoreCase))
					{
						int result;
						if (!string.IsNullOrEmpty(mGoldenHelperID) && mGoldenHelperIndex == -1 && int.TryParse(array[2], out result) && mTotalUnlockedGoldenHelpers <= result)
						{
							oddsEntry.id = mGoldenHelperID;
							oddsEntry.isGoldenHelper = true;
							return oddsEntry;
						}
						return null;
					}
					return oddsEntry;
				}
			}
		}
		return null;
	}

	private List<OddsEntry> GetRandomTable()
	{
		List<OddsEntry> list = new List<OddsEntry>();
		string text = BoxID;
		if (string.IsNullOrEmpty(text))
		{
			text = "MysteryBox";
		}
		TextDBSchema[] array = DataBundleUtils.InitializeRecords<TextDBSchema>(text);
		TextDBSchema[] array2 = array;
		foreach (TextDBSchema textDBSchema in array2)
		{
			OddsEntry oddsEntry = ParseOddsEntry(textDBSchema.key, textDBSchema.value);
			if (oddsEntry != null)
			{
				list.Add(oddsEntry);
				if (oddsEntry.isGoldenHelper)
				{
					mGoldenHelperIndex = list.Count - 1;
				}
			}
		}
		return list;
	}

	private int GetTotalWeight(List<OddsEntry> lst)
	{
		int num = 0;
		foreach (OddsEntry item in lst)
		{
			num += item.weight;
		}
		return num;
	}
}
