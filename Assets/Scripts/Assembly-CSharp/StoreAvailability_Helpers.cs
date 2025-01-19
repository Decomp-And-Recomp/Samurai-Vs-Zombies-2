using System.Collections.Generic;

public class StoreAvailability_Helpers
{
	public static void Get(List<StoreData.Item> items)
	{
		HelperSchema[] allHelpers = Singleton<HelpersDatabase>.Instance.AllHelpers;
		foreach (HelperSchema helper in allHelpers)
		{
			Get(helper, false, items);
		}
	}

	public static void Get(string helperID, bool force, List<StoreData.Item> items)
	{
		Get(Singleton<HelpersDatabase>.Instance[helperID], force, items);
	}

	public static void Get(HelperSchema helper, bool force, List<StoreData.Item> items)
	{
		if (helper.hideInStore && !force)
		{
			return;
		}
		string id = helper.id;
		bool flag = false;
		int num = Singleton<Profile>.Instance.GetHelperLevel(id);
		int num2 = helper.Levels.Length;
		if (helper.Locked)
		{
			flag = true;
		}
		else
		{
			num = Singleton<HelpersDatabase>.Instance.EnsureProperInitialHelperLevel(id);
		}
		int num3 = ((num >= num2) ? num : (num + 1));
		HelperLevelSchema curLevel = helper.CurLevel;
		HelperLevelSchema nextLevel = helper.NextLevel;
		string text = nextLevel.health.ToString();
		string text2 = ((nextLevel.meleeDamage <= nextLevel.bowDamage) ? nextLevel.bowDamage.ToString() : nextLevel.meleeDamage.ToString());
		float salePercentage = SaleItemSchema.FindActiveSaleForItem(id);
		Cost itemCost = new Cost(nextLevel.cost, salePercentage);
		string delegateArg = id;
		bool isLastUpgrade = num3 == num2;
		StoreData.Item item = new StoreData.Item(delegate
		{
			LevelUpHelper(itemCost, delegateArg, isLastUpgrade);
		});
		item.cost = itemCost;
		bool flag2 = string.Equals(id, "Mount_Balanced");
		if (num3 == 1)
		{
			item.details.AddStat("health_stats", text, text);
			item.details.AddStat("strength_stats", (!flag2) ? text2 : (text2 + "%"), (!flag2) ? text2 : (text2 + "%"));
			item.details.SetColumns(num3, num3);
		}
		else
		{
			string text3 = ((curLevel.meleeDamage <= curLevel.bowDamage) ? curLevel.bowDamage.ToString() : curLevel.meleeDamage.ToString());
			item.details.AddStat("health_stats", curLevel.health.ToString(), text);
			item.details.AddStat("strength_stats", (!flag2) ? text3 : (text3 + "%"), (!flag2) ? text2 : (text2 + "%"));
			item.details.SetColumns(num, num3);
		}
		item.details.MaxLevel = num2;
		item.id = id;
		if (flag && !string.IsNullOrEmpty(helper.LockedIconPath))
		{
			item.LoadIcon(helper.LockedIconPath);
		}
		else if (Singleton<Profile>.Instance.GetHelperLevel(helper.id) >= Helper.kPlatinumLevel && !string.IsNullOrEmpty(helper.PlatinumIconPath))
		{
			item.LoadIcon(helper.PlatinumIconPath);
		}
		else
		{
			item.LoadIcon(helper.IconPath);
		}
		item.locked = flag;
		item.unlockAtWave = helper.waveToUnlock;
		item.availableAtWave = helper.availableAtWave;
		item.isNew = Singleton<Profile>.Instance.highestUnlockedWave == helper.waveToUnlock && (!SingletonMonoBehaviour<StoreMenuImpl>.Exists || !SingletonMonoBehaviour<StoreMenuImpl>.Instance.HasViewedNewItem(item.id));
		if (Singleton<Profile>.Instance.GetGoldenHelperUnlocked(id))
		{
			item.secondIcon = helper.championIcon;
		}
		string stringFromStringRef = StringUtils.GetStringFromStringRef(helper.displayName);
		if (flag)
		{
			if (!DataBundleRecordKey.IsNullOrEmpty(nextLevel.specialUnlockText))
			{
				item.unlockCondition = StringUtils.GetStringFromStringRef(nextLevel.specialUnlockText);
			}
			else
			{
				item.unlockCondition = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_unlockatwave"), item.unlockAtWave);
			}
		}
		if (num3 == 1)
		{
			item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_format_recruit"), stringFromStringRef);
		}
		else if (num3 == num)
		{
			item.maxlevel = true;
			item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_upgrades_complete"), stringFromStringRef);
		}
		else
		{
			item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_format_levelup"), stringFromStringRef, num3);
		}
		item.unlockTitle = stringFromStringRef;
		item.details.AddSmallDescription(StringUtils.GetStringFromStringRef(helper.desc));
		item.details.Name = stringFromStringRef;
		item.analyticsEvent = "UpgradePurchased";
		item.analyticsParams = new Dictionary<string, object>();
		item.analyticsParams["ItemName"] = "Helper_" + item.id;
		item.analyticsParams["UpgradeLevel"] = num3;
		items.Add(item);
	}

	private static void FillInfinityPath(HelperSchema helper, List<StoreData.Item> items)
	{
	}

	public static bool IsAnyChampionForSale()
	{
		HelperSchema[] allHelpers = Singleton<HelpersDatabase>.Instance.AllHelpers;
		foreach (HelperSchema helper in allHelpers)
		{
			if (ChampionAvailableForSale(helper))
			{
				return true;
			}
		}
		return false;
	}

	public static void GetChampions(List<StoreData.Item> items)
	{
		HelperSchema[] allHelpers = Singleton<HelpersDatabase>.Instance.AllHelpers;
		foreach (HelperSchema helperSchema in allHelpers)
		{
			if (ChampionAvailableForSale(helperSchema))
			{
				string helperID = helperSchema.id;
				StoreData.Item item = new StoreData.Item(delegate
				{
					UnlockChampion(helperID);
				});
				item.cost = new Cost(helperSchema.goldenHelperCostToUnlock, SaleItemSchema.FindActiveSaleForItem(helperID + "_Champion"));
				item.icon = helperSchema.TryGetChampionIcon();
				item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "UnlockChampion_Title"), StringUtils.GetStringFromStringRef(helperSchema.displayName));
				item.details.AddDescription(string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "UnlockChampion_Desc"), StringUtils.GetStringFromStringRef(helperSchema.displayName)));
				item.locked = helperSchema.Locked;
				item.unlockAtWave = helperSchema.waveToUnlock;
				item.analyticsEvent = "ChampionPurchased";
				item.analyticsParams = new Dictionary<string, object>();
				item.analyticsParams["ItemName"] = "Golden_" + helperID;
				items.Add(item);
			}
		}
	}

	private static bool ChampionAvailableForSale(HelperSchema helper)
	{
		return !helper.hideInStore && !string.IsNullOrEmpty(helper.goldenHelperCostToUnlock) && !Singleton<Profile>.Instance.GetGoldenHelperUnlocked(helper.id);
	}

	private static void LevelUpHelper(Cost cost, string helperID, bool isLastUpgrade)
	{
		int val = Singleton<Profile>.Instance.GetHelperLevel(helperID) + 1;
		Singleton<Profile>.Instance.SetHelperLevel(helperID, val);
		if (string.Equals(helperID, "Mount_Balanced"))
		{
			SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", "HeroBalanced", GluiAgentBase.Order.Redraw, null, null);
		}
		else
		{
			SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", "Allies", GluiAgentBase.Order.Redraw, null, null);
		}
		HelperSchema helperSchema = Singleton<HelpersDatabase>.Instance[helperID];
		if (helperSchema != null)
		{
			Singleton<Analytics>.Instance.LogEvent("AbilityUpgradePurchased", Analytics.Param("ItemName", helperID), Analytics.Param("UpgradeLevel", Singleton<Profile>.Instance.GetHelperLevel(helperID)), Analytics.Param("Cost", cost.price), Analytics.Param("Currency", cost.currencyAnalyticCode), Analytics.Param("PlayerLevel", Singleton<Profile>.Instance.playerLevel));
			if (isLastUpgrade && helperSchema.upgradeAchievement != null && !string.IsNullOrEmpty(helperSchema.upgradeAchievement.Key))
			{
				Singleton<Achievements>.Instance.SetAchievementCompletionCount(helperSchema.upgradeAchievement.Key, 1);
				Singleton<Achievements>.Instance.CheckMetaAchievement("AllUpgrades");
			}
		}
		Singleton<Profile>.Instance.Save();
	}

	private static void UnlockChampion(string helperID)
	{
		Singleton<Profile>.Instance.SetGoldenHelperUnlocked(helperID, true);
		if (Singleton<Profile>.Instance.GetHelperLevel(helperID) == 0)
		{
			Singleton<Profile>.Instance.SetHelperLevel(helperID, 1);
		}
		SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", "Champions", GluiAgentBase.Order.Redraw, null, null);
		SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", "Allies", GluiAgentBase.Order.Redraw, null, null);
		Singleton<Profile>.Instance.Save();
	}
}
