using System;
using UnityEngine;

public class HUDReviveDialog : MonoBehaviour, IGluiActionHandler
{
	private struct BundleInfo
	{
		public int quantity;

		public Cost cost;
	}

	private const string kRevivePotionID = "revivePotion";

	private int numRevivesPurchased;

	private BundleInfo[] mBundles = new BundleInfo[2];

	private bool mClosed;

	private GameObject mPurchaseModule;

	private GameObject mUseModule;

	private GameObject mLootListParent;

	private int numRevivePotions
	{
		get
		{
			return Singleton<Profile>.Instance.GetNumPotions("revivePotion");
		}
	}

	private void Start()
	{
		numRevivesPurchased = 0;
		base.gameObject.transform.position = new Vector3(0f, 0f, 0f);
		if (WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			WeakGlobalMonoBehavior<InGameImpl>.Instance.gamePaused = true;
		}
		PotionSchema potionSchema = Singleton<PotionsDatabase>.Instance["revivePotion"];
		if (potionSchema != null)
		{
			float salePercentage = SaleItemSchema.FindActiveSaleForItem("revivePotion");
			mBundles[0].quantity = 1;
			mBundles[0].cost = new Cost(potionSchema.cost, salePercentage);
			mBundles[1].quantity = potionSchema.storePack;
			mBundles[1].cost = new Cost(potionSchema.storePackCost, salePercentage);
		}
		try
		{
			InitLootList(base.gameObject.FindChild("LootList"));
		}
		catch (Exception)
		{
		}
		try
		{
			GluiSprite gluiSprite = base.gameObject.FindChildComponent<GluiSprite>("Art_PortraitDead");
			HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[Singleton<Profile>.Instance.heroID];
			gluiSprite.Texture = heroSchema.icon;
		}
		catch (Exception)
		{
		}
		mPurchaseModule = base.gameObject.FindChild("Swap_BuyRevive");
		mUseModule = base.gameObject.FindChild("Swap_UseRevive");
		if (numRevivePotions > 0)
		{
			SetOptionAsUse();
		}
		else
		{
			SetOptionAsPurchase();
		}
	}

	private void Update()
	{
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		if (!mClosed)
		{
			switch (action)
			{
			case "BUTTON_REVIVE_RESUME":
				Dismiss();
				return true;
			case "BUTTON_REVIVE_BUY_A":
				Buy(0);
				return true;
			case "BUTTON_REVIVE_BUY_B":
				Buy(1);
				return true;
			case "BUTTON_REVIVE_USE":
				Use();
				return true;
			}
		}
		return false;
	}

	private void InitLootList(GameObject lootParent)
	{
		mLootListParent = lootParent;
		if (Singleton<PlayStatistics>.Instance.data.loot.Count == 0)
		{
			mLootListParent.FindChild("Text_LootAtRisk").SetActive(false);
		}
	}

	private void SetOptionAsUse()
	{
		mPurchaseModule.SetActive(false);
		GluiText gluiText = mUseModule.FindChildComponent<GluiText>("SwapText_UseRevive");
		gluiText.Text = string.Format(StringUtils.GetStringFromStringRef(gluiText.TaggedStringReference), numRevivePotions);
	}

	private void SetOptionAsPurchase()
	{
		mUseModule.SetActive(false);
		DrawPurchaseBundle("BundleA", mBundles[0]);
		DrawPurchaseBundle("BundleB", mBundles[1]);
	}

	private void DrawPurchaseBundle(string parentName, BundleInfo info)
	{
		GameObject parent = mPurchaseModule.FindChild(parentName);
		GluiText gluiText = parent.FindChildComponent<GluiText>("Swap_Text_Qty");
		gluiText.Text = string.Format(StringUtils.GetStringFromStringRef(gluiText.TaggedStringReference), info.quantity);
		WidgetPriceSpawner widgetPriceSpawner = parent.FindChildComponent<WidgetPriceSpawner>("Locator_Widget_Price");
		if (widgetPriceSpawner != null)
		{
			widgetPriceSpawner.SetCost(info.cost);
		}
		parent.FindChild("Sale_Badge_Sale").SetActive(false);
	}

	private void Close()
	{
		mClosed = true;
		GluiActionSender.SendGluiAction("POPUP_EMPTY", base.gameObject, null);
		WeakGlobalMonoBehavior<InGameImpl>.Instance.gamePaused = false;
	}

	private void Dismiss()
	{
		ReportReviveMetrics(false);
		WeakGlobalMonoBehavior<InGameImpl>.Instance.OnReviveDialog_Dismissed();
		Close();
	}

	private void Buy(int bundleIndex)
	{
		if (mBundles[bundleIndex].cost.canAfford)
		{
			numRevivesPurchased += mBundles[bundleIndex].quantity;
			mBundles[bundleIndex].cost.Spend();
			mBundles[bundleIndex].cost.gwalletSpend(mBundles[bundleIndex].cost, "DEBIT_IN_APP_PURCHASE", "Revive");
			Singleton<Profile>.Instance.SetNumPotions("revivePotion", numRevivePotions + mBundles[bundleIndex].quantity);
			Use();
		}
		else
		{
			GluiActionSender.SendGluiAction("POPUP_IAP", base.gameObject, null);
		}
	}

	private void ReportReviveMetrics(bool reviveUsed)
	{
		try
		{
			if (Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive())
			{
				return;
			}
			int num = Flurry_Session.CurrentSPWave();
			int waveAttemptCount = Singleton<Profile>.Instance.GetWaveAttemptCount(num);
			int num2 = (int)((float)WeakGlobalInstance<WaveManager>.Instance.enemiesKilledSoFar / (float)WeakGlobalInstance<WaveManager>.Instance.totalEnemies * 100f);
			int num3 = 0;
			int num4 = 0;
			string obj = string.Empty;
			foreach (PlayStatistics.Data.LootEntry item in Singleton<PlayStatistics>.Instance.data.loot)
			{
				int valueInCoins = CashIn.GetValueInCoins(item.id, item.num);
				int num5 = valueInCoins / item.num;
				if (num5 > num4)
				{
					num4 = num5;
					obj = item.id;
				}
				num3 += valueInCoins;
			}
			Singleton<Analytics>.Instance.LogEvent((!reviveUsed) ? "ReviveNotUsed" : "ReviveUsed", Analytics.Param("WaveNumber", num), Analytics.Param("AttemptNumber", waveAttemptCount), Analytics.Param("RevivesInInventory", numRevivePotions), Analytics.Param("RevivesPurchased", numRevivesPurchased), Analytics.Param("PercentComplete", num2), Analytics.Param("LootValue", num3), Analytics.Param("BestLootItem", obj));
		}
		catch (Exception)
		{
		}
	}

	private void Use()
	{
		ReportReviveMetrics(true);
		Singleton<Profile>.Instance.SetNumPotions("revivePotion", numRevivePotions - 1);
		Singleton<PlayStatistics>.Instance.data.revivesUsed++;
		WeakGlobalMonoBehavior<InGameImpl>.Instance.OnReviveDialog_Accept();
		Close();
	}
}
