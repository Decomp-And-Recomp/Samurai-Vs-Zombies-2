using System;
using System.Collections.Generic;
using UnityEngine;

public class StoreAvailability
{
	private delegate void PostSortProcessing(List<StoreData.Item> items);

	public const string kDealPackTable = "DealPacks";

	public static List<StoreData.Item> GetList(string g)
	{
		List<StoreData.Item> list = new List<StoreData.Item>();
		PostSortProcessing postSortProcessing = null;
		if (!Singleton<Profile>.Exists || !Singleton<Profile>.Instance.Initialized)
		{
			return list;
		}
		if (g == StringUtils.GetStringFromStringRef("MenuFixedStrings", "store_items_global"))
		{
			GetMysteryBox(list);
			GetUpgrades(list, true);
			EndSortedGroup(list, 1);
			GetGateUpgrade(list);
			GetVillageArchersUpgrade(list);
			GetBellUpgrade(list);
			GetPitUpgrade(list);
			EndSortedGroup(list, 2);
			StoreAvailability_Abilities.GetGlobalAbilities(list);
			EndSortedGroup(list, 3);
		}
		else if (string.Compare(g, "Helpers", true) == 0 || string.Compare(g, "Allies", true) == 0)
		{
			GetMysteryBox(list);
			StoreAvailability_Helpers.Get(list);
		}
		else if (string.Compare(g, "Champions", true) == 0)
		{
			GetMysteryBox(list);
			StoreAvailability_Helpers.GetChampions(list);
			postSortProcessing = delegate(List<StoreData.Item> all)
			{
				foreach (StoreData.Item item in all)
				{
					item.locked = false;
					item.unlockAtWave = 0;
				}
			};
		}
		else if (string.Compare(g, "Consumables", true) == 0 || string.Compare(g, StringUtils.GetStringFromStringRef("MenuFixedStrings", "Boost_Consumables"), true) == 0)
		{
			GetMysteryBox(list);
			GetDealPacks(list);
			GetPotions(list);
		}
		else if (string.Compare(g, "Upgrades", true) == 0 || string.Compare(g, StringUtils.GetStringFromStringRef("MenuFixedStrings", "Boost_Upgrades"), true) == 0)
		{
			GetUpgrades(list, false);
		}
		else if (string.Compare(g, "Charms", true) == 0 || string.Compare(g, StringUtils.GetStringFromStringRef("MenuFixedStrings", "Boost_Charms"), true) == 0)
		{
			GetCharms(list);
		}
		else
		{
			if (Array.Find(Singleton<HeroesDatabase>.Instance.AllIDs, (string s) => string.Equals(s, g)) == null)
			{
				return null;
			}
			HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[g];
			if (heroSchema != null && !heroSchema.Locked)
			{
				GetHeroMysteryBox(g, list);
			}
			GetHero(g, list);
		}
		int num = 0;
		foreach (StoreData.Item item2 in list)
		{
			item2._originalSortIndex = num++;
		}
		list.Sort(new StoreData.ItemsListSorter());
		if (postSortProcessing != null)
		{
			postSortProcessing(list);
		}
		return list;
	}

	private static void EndSortedGroup(List<StoreData.Item> items, int sortIndex)
	{
		foreach (StoreData.Item item in items)
		{
			if (item._sortGroup == 0)
			{
				item._sortGroup = sortIndex;
			}
		}
	}

	private static void GetHero(string heroID, List<StoreData.Item> items)
	{
		HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[heroID];
		int heroLevel = Singleton<Profile>.Instance.GetHeroLevel(heroID);
		int num = heroLevel + 1;
		int num2 = heroSchema.Levels.Length;
		bool isLastUpgrade = num == num2;
		StoreData.Item item = new StoreData.Item(delegate
		{
			LevelUpHero(heroID, isLastUpgrade);
		});
		bool purchased = heroSchema.Purchased;
		item.details.SetColumns(heroLevel, num);
		item.details.AddStat("health_stats", Mathf.RoundToInt(heroSchema.Extrapolate(heroLevel, (HeroLevelSchema ls) => ls.health, (HeroSchema s) => s.infiniteUpgradeHealth)).ToString(), Mathf.RoundToInt(heroSchema.Extrapolate(num, (HeroLevelSchema ls) => ls.health, (HeroSchema s) => s.infiniteUpgradeHealth)).ToString());
		float salePercentage = SaleItemSchema.FindActiveSaleForItem(heroID + ".Level");
		if (num <= num2)
		{
			item.cost = new Cost(heroSchema.Levels[num - 1].cost, salePercentage);
		}
		else
		{
			item.cost = new Cost(heroSchema.infiniteUpgradeCost, salePercentage);
		}
		item.id = heroID;
		item.LoadIcon(heroSchema.IconPath);
		item.title = string.Format(StringUtils.GetStringFromStringRef(heroSchema.store_levelup), num);
		item.details.Name = item.title;
		item.details.AddSmallDescription(StringUtils.GetStringFromStringRef(heroSchema.desc));
		item.analyticsEvent = "UpgradePurchased";
		item.analyticsParams = new Dictionary<string, object>();
		item.analyticsParams["ItemName"] = heroID + ".HeroLevel";
		item.analyticsParams["UpgradeLevel"] = num;
		items.Add(item);
		GetLeadership(heroID, items);
		GetWeapon(heroSchema.MeleeWeapon, Singleton<Profile>.Instance.GetMeleeWeaponLevel(heroID), heroID, items);
		if (heroSchema.ArmorLevels != null)
		{
			GetArmor(heroSchema.ArmorLevels, Singleton<Profile>.Instance.GetArmorLevel(heroID), heroID, items);
		}
		else if (!DataBundleRecordKey.IsNullOrEmpty(heroSchema.rangedWeapon))
		{
			GetWeapon(heroSchema.RangedWeapon, Singleton<Profile>.Instance.GetRangedWeaponLevel(heroID), heroID, items);
		}
		if (heroID.Equals("HeroBalanced"))
		{
			StoreAvailability_Helpers.Get("Mount_Balanced", true, items);
		}
		StoreAvailability_Abilities.GetHeroAbilities(heroSchema, items);
		foreach (StoreData.Item item2 in items)
		{
			if (heroSchema.Locked)
			{
				item2.locked = true;
				if (purchased)
				{
					item2.unlockAtWave = heroSchema.waveToUnlock;
					item2.unlockCondition = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_unlockatwave"), heroSchema.waveToUnlock);
				}
				else
				{
					item2.unlockAtWave = 0;
					item2.unlockCondition = StringUtils.GetStringFromStringRef("LocalizedStrings", "store_purchase_with_iap");
					item2.customButtonAction = delegate
					{
						SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("IAP_TAB", "LocalizedStrings.iap_special_tab");
						GluiActionSender.SendGluiAction("POPUP_IAP", null, null);
					};
				}
				item2.unlockTitle = item2.details.Name;
			}
			else if (heroSchema.waveToUnlock == Singleton<Profile>.Instance.highestUnlockedWave)
			{
				item2.isNew = !SingletonMonoBehaviour<StoreMenuImpl>.Exists || !SingletonMonoBehaviour<StoreMenuImpl>.Instance.HasViewedNewItem(item2.id);
			}
		}
	}

	private static void GetWeapon(WeaponSchema mainData, int curLevel, string heroID, List<StoreData.Item> items)
	{
		int num = curLevel + 1;
		int num2 = mainData.Levels.Length;
		bool isLastUpgrade = num == num2;
		bool isRanged = mainData.isRanged;
		StoreData.Item item = new StoreData.Item(delegate
		{
			if (isRanged)
			{
				LevelUpBow(heroID, isLastUpgrade);
			}
			else
			{
				LevelUpSword(heroID, isLastUpgrade);
			}
		});
		item.details.SetColumns(curLevel, num);
		item.details.AddStat("strength_stats", Mathf.RoundToInt(mainData.Extrapolate(curLevel, (WeaponLevelSchema ls) => ls.damage, (WeaponSchema s) => s.infiniteUpgradeDamage)).ToString(), Mathf.RoundToInt(mainData.Extrapolate(num, (WeaponLevelSchema ls) => ls.damage, (WeaponSchema s) => s.infiniteUpgradeDamage)).ToString());
		WeaponLevelSchema weaponLevelSchema = mainData.Levels[Mathf.Clamp(num - 1, 0, num2 - 1)];
		string iconPath = weaponLevelSchema.IconPath;
		item.LoadIcon(iconPath);
		item.details.AddSmallDescription(StringUtils.GetStringFromStringRef(weaponLevelSchema.desc));
		item.title = string.Format(StringUtils.GetStringFromStringRef(weaponLevelSchema.title), num);
		item.details.Name = item.title;
		float salePercentage = SaleItemSchema.FindActiveSaleForItem(heroID + ((!isRanged) ? ".MeleeWeapon" : ".RangedWeapon"));
		if (num <= num2)
		{
			item.cost = new Cost(weaponLevelSchema.cost, salePercentage);
		}
		else
		{
			item.cost = new Cost(mainData.infiniteUpgradeCost, salePercentage);
		}
		item.id = mainData.id;
		item.analyticsEvent = "UpgradePurchased";
		item.analyticsParams = new Dictionary<string, object>();
		item.analyticsParams["ItemName"] = heroID + ((!isRanged) ? ".MeleeWeapon" : ".RangedWeapon");
		item.analyticsParams["UpgradeLevel"] = num;
		items.Add(item);
	}

	private static void GetArmor(ArmorLevelSchema[] levelData, int curLevel, string heroID, List<StoreData.Item> items)
	{
		curLevel--;
		int num = levelData.Length - 1;
		curLevel = Mathf.Clamp(curLevel, 0, num);
		int num2 = ((curLevel >= num) ? curLevel : (curLevel + 1));
		bool isLastUpgrade = num2 == num;
		StoreData.Item item = new StoreData.Item(delegate
		{
			LevelUpArmor(heroID, isLastUpgrade);
		});
		item.details.SetColumns(curLevel + 1, num2 + 1);
		if (levelData[curLevel].meleeDamageModifier != levelData[num2].meleeDamageModifier)
		{
			item.details.AddStat("armor_meleeDamageModifier", ArmorLevelSchema.ModifierString(levelData[curLevel].meleeDamageModifier, true), ArmorLevelSchema.ModifierString(levelData[num2].meleeDamageModifier, true));
		}
		if (levelData[curLevel].meleeBlockRatio != levelData[num2].meleeBlockRatio)
		{
			item.details.AddStat("armor_meleeBlockRatio", ArmorLevelSchema.ModifierString(levelData[curLevel].meleeBlockRatio, false), ArmorLevelSchema.ModifierString(levelData[num2].meleeBlockRatio, false));
		}
		if (levelData[curLevel].reflectDamageRatio != levelData[num2].reflectDamageRatio)
		{
			item.details.AddStat("armor_reflectDamageRatio", ArmorLevelSchema.ModifierString(levelData[curLevel].reflectDamageRatio, false), ArmorLevelSchema.ModifierString(levelData[num2].reflectDamageRatio, false));
		}
		item.LoadIcon(levelData[num2].IconPath);
		string stringFromStringRef = StringUtils.GetStringFromStringRef(levelData[num2].title);
		item.details.AddSmallDescription(StringUtils.GetStringFromStringRef(levelData[num2].desc));
		if (curLevel == num2)
		{
			item.maxlevel = true;
			item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_upgrades_complete"), stringFromStringRef);
		}
		else
		{
			item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_format_upgrade_tolevel"), stringFromStringRef, num2 + 1);
			float salePercentage = SaleItemSchema.FindActiveSaleForItem(heroID + ".Armor");
			item.cost = new Cost(levelData[num2].costCoins + "," + levelData[num2].costGems, salePercentage);
		}
		item.id = levelData[num2].level.ToString();
		item.details.Name = stringFromStringRef;
		item.analyticsEvent = "UpgradePurchased";
		item.analyticsParams = new Dictionary<string, object>();
		item.analyticsParams["ItemName"] = heroID + ".Armor";
		item.analyticsParams["UpgradeLevel"] = num2;
		items.Add(item);
	}

	private static void GetLeadership(string heroID, List<StoreData.Item> items)
	{
		LeadershipSchema leadershipSchema = DataBundleUtils.InitializeRecord<LeadershipSchema>(new DataBundleRecordKey(Leadership.UdamanTable, heroID));
		int leadershipLevel = Singleton<Profile>.Instance.GetLeadershipLevel(heroID);
		int num = leadershipLevel + 1;
		int maxLevel = leadershipSchema.maxLevel;
		bool flag = leadershipLevel >= leadershipSchema.hideInStoreLevel;
		bool isLastUpgrade = num == maxLevel - 1;
		StoreData.Item item = new StoreData.Item(delegate
		{
			LevelUpLeadership(heroID, isLastUpgrade);
		});
		item.id = "Leadership";
		item.LoadIcon(Singleton<PlayModesManager>.Instance.selectedModeData.IconPath);
		string stringFromStringRef = StringUtils.GetStringFromStringRef("MenuFixedStrings", "Menu_Leadership");
		if (leadershipLevel == maxLevel || flag)
		{
			item.maxlevel = true;
			item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_upgrades_complete"), stringFromStringRef);
		}
		else
		{
			item.title = string.Format(StringUtils.GetStringFromStringRef(leadershipSchema.purchaseText), num + 1);
			float salePercentage = SaleItemSchema.FindActiveSaleForItem(heroID + ".Leadership");
			switch (num)
			{
			case 0:
				item.cost = new Cost(leadershipSchema.storeCost0, salePercentage);
				break;
			case 1:
				item.cost = new Cost(leadershipSchema.storeCost1, salePercentage);
				break;
			case 2:
				item.cost = new Cost(leadershipSchema.storeCost2, salePercentage);
				break;
			case 3:
				item.cost = new Cost(leadershipSchema.storeCost3, salePercentage);
				break;
			}
		}
		item.details.AddSmallDescription(StringUtils.GetStringFromStringRef(leadershipSchema.descText));
		item.details.Name = stringFromStringRef;
		item.analyticsEvent = "UpgradePurchased";
		item.analyticsParams = new Dictionary<string, object>();
		item.analyticsParams["ItemName"] = heroID + "." + item.id;
		item.analyticsParams["UpgradeLevel"] = num;
		items.Add(item);
	}

	private static void GetPitUpgrade(List<StoreData.Item> items)
	{
		PitSchema[] array = DataBundleUtils.InitializeRecords<PitSchema>("Pit");
		int pitLevel = Singleton<Profile>.Instance.pitLevel;
		int num = array.Length;
		int num2 = ((pitLevel >= num) ? pitLevel : (pitLevel + 1));
		PitSchema pitSchema = ((pitLevel != 0) ? array[pitLevel - 1] : null);
		PitSchema pitSchema2 = array[num2 - 1];
		int unlocksAtLevel = array[0].unlocksAtLevel;
		bool flag = Singleton<Profile>.Instance.highestUnlockedWave < unlocksAtLevel;
		bool isLastUpgrade = num2 == num;
		StoreData.Item item = new StoreData.Item(delegate
		{
			LevelUpPit(isLastUpgrade);
		});
		item.details.Name = StringUtils.GetStringFromStringRef("MenuFixedStrings", "Menu_Pit");
		item.unlockTitle = item.details.Name;
		item.unlockAtWave = unlocksAtLevel;
		item.isNew = Singleton<Profile>.Instance.highestUnlockedWave == unlocksAtLevel && (!SingletonMonoBehaviour<StoreMenuImpl>.Exists || !SingletonMonoBehaviour<StoreMenuImpl>.Instance.HasViewedNewItem(item.id));
		item.locked = flag;
		if (flag)
		{
			item.unlockCondition = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_unlockatwave"), item.unlockAtWave);
		}
		float salePercentage = SaleItemSchema.FindActiveSaleForItem("Pit");
		if (pitLevel == num2)
		{
			item.maxlevel = true;
			item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_upgrades_complete"), item.details.Name);
		}
		else
		{
			item.title = string.Format(StringUtils.GetStringFromStringRef(pitSchema2.displayName), num2);
			item.cost = new Cost(pitSchema2.cost, salePercentage);
		}
		if (pitSchema != null)
		{
			item.details.AddStat("strength_stats", (int)pitSchema.chanceToEnact + "%", (int)pitSchema2.chanceToEnact + "%");
			item.details.SetColumns(pitLevel, num2);
		}
		else
		{
			item.details.AddStat("strength_stats", (int)pitSchema2.chanceToEnact + "%", (int)pitSchema2.chanceToEnact + "%");
			item.details.SetColumns(num2, num2);
		}
		item.details.MaxLevel = num;
		item.id = "Pit";
		item.LoadIcon(DataBundleRuntime.Instance.GetValue<string>(typeof(PitSchema), "Pit", pitSchema2.level.ToString(), "icon", true));
		item.details.AddSmallDescription(StringUtils.GetStringFromStringRef(pitSchema2.upgradeDescription));
		item.analyticsEvent = "UpgradePurchased";
		item.analyticsParams = new Dictionary<string, object>();
		item.analyticsParams["ItemName"] = item.id;
		item.analyticsParams["UpgradeLevel"] = num2;
		items.Add(item);
	}

	private static void GetGateUpgrade(List<StoreData.Item> items)
	{
		TextDBSchema[] data = DataBundleUtils.InitializeRecords<TextDBSchema>("Gate");
		int baseLevel = Singleton<Profile>.Instance.baseLevel;
		int maxLevels = data.GetMaxLevels();
		int num = ((baseLevel >= maxLevels) ? baseLevel : (baseLevel + 1));
		bool isLastUpgrade = num == maxLevels;
		StoreData.Item item = new StoreData.Item(delegate
		{
			LevelUpGate(isLastUpgrade);
		});
		item.details.Name = StringUtils.GetStringFromStringRef("MenuFixedStrings", "Menu_Gate");
		float salePercentage = SaleItemSchema.FindActiveSaleForItem("Gate");
		if (baseLevel == num)
		{
			item.maxlevel = true;
			item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_upgrades_complete"), item.details.Name);
		}
		else
		{
			item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", data.GetString("storeTitle")), num);
			item.cost = new Cost(data.GetString(TextDBSchema.LevelKey("cost", num)), salePercentage);
		}
		item.details.AddStat("health_stats", data.GetString(TextDBSchema.LevelKey("health", baseLevel)), data.GetString(TextDBSchema.LevelKey("health", num)));
		item.details.SetColumns(baseLevel, num);
		item.details.MaxLevel = maxLevels;
		item.id = "Gate";
		string @string = data.GetString(TextDBSchema.LevelKey("icon", num));
		item.LoadIcon(@string);
		item.details.AddSmallDescription(StringUtils.GetStringFromStringRef("LocalizedStrings", data.GetString("desc")));
		item.analyticsEvent = "UpgradePurchased";
		item.analyticsParams = new Dictionary<string, object>();
		item.analyticsParams["ItemName"] = item.id;
		item.analyticsParams["UpgradeLevel"] = num;
		items.Add(item);
	}

	private static void GetBellUpgrade(List<StoreData.Item> items)
	{
		if (Singleton<PlayModesManager>.Instance.selectedModeData.useBell)
		{
			bool flag = false;
			TextDBSchema[] data = DataBundleUtils.InitializeRecords<TextDBSchema>("Bell");
			int bellLevel = Singleton<Profile>.Instance.bellLevel;
			int maxLevels = data.GetMaxLevels();
			int num = ((bellLevel >= maxLevels) ? bellLevel : (bellLevel + 1));
			int @int = data.GetInt("waveToUnlock");
			if (Singleton<Profile>.Instance.highestUnlockedWave < @int)
			{
				flag = true;
			}
			bool isLastUpgrade = num == maxLevels;
			StoreData.Item item = new StoreData.Item(delegate
			{
				LevelUpBell(isLastUpgrade);
			});
			if (num == 1)
			{
				item.details.AddStat("strength_stats", data.GetString(TextDBSchema.LevelKey("damage", num)), data.GetString(TextDBSchema.LevelKey("damage", num)));
				item.details.SetColumns(num, num);
			}
			else
			{
				item.details.AddStat("strength_stats", data.GetString(TextDBSchema.LevelKey("damage", bellLevel)), data.GetString(TextDBSchema.LevelKey("damage", num)));
				item.details.SetColumns(bellLevel, num);
			}
			item.details.MaxLevel = maxLevels;
			item.id = "Bell";
			item.locked = flag;
			string @string = data.GetString(TextDBSchema.LevelKey("icon", num));
			item.LoadIcon(@string);
			item.unlockAtWave = @int;
			item.isNew = Singleton<Profile>.Instance.highestUnlockedWave == @int && (!SingletonMonoBehaviour<StoreMenuImpl>.Exists || !SingletonMonoBehaviour<StoreMenuImpl>.Instance.HasViewedNewItem(item.id));
			if (flag)
			{
				item.unlockCondition = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_unlockatwave"), item.unlockAtWave);
			}
			float salePercentage = SaleItemSchema.FindActiveSaleForItem("Bell");
			item.cost = new Cost(data.GetString(TextDBSchema.LevelKey("cost", num)), salePercentage);
			string stringFromStringRef = StringUtils.GetStringFromStringRef("LocalizedStrings", data.GetString("displayName"));
			if (bellLevel == 0)
			{
				item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_format_purchase_atlevel"), stringFromStringRef, num);
			}
			else if (bellLevel == num)
			{
				item.maxlevel = true;
				item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_upgrades_complete"), stringFromStringRef);
			}
			else
			{
				item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_format_upgrade_tolevel"), stringFromStringRef, num);
			}
			item.unlockTitle = stringFromStringRef;
			item.details.Name = stringFromStringRef;
			item.details.AddSmallDescription(StringUtils.GetStringFromStringRef("LocalizedStrings", data.GetString("desc")));
			item.analyticsEvent = "UpgradePurchased";
			item.analyticsParams = new Dictionary<string, object>();
			item.analyticsParams["ItemName"] = item.id;
			item.analyticsParams["UpgradeLevel"] = num;
			items.Add(item);
		}
	}

	private static void GetVillageArchersUpgrade(List<StoreData.Item> items)
	{
		if (!Singleton<PlayModesManager>.Instance.selectedModeData.useVillageArchers)
		{
			return;
		}
		bool flag = false;
		int num = 7;
		if (Singleton<Profile>.Instance.highestUnlockedWave < num)
		{
			flag = true;
		}
		int archerLevel = Singleton<Profile>.Instance.archerLevel;
		int recordTableLength = DataBundleRuntime.Instance.GetRecordTableLength(typeof(VillageArcherSchema), "VillageArchers");
		int num2 = ((archerLevel >= recordTableLength) ? archerLevel : (archerLevel + 1));
		bool isLastUpgrade = num2 == recordTableLength;
		StoreData.Item item = new StoreData.Item(delegate
		{
			LevelUpVillageArchers(isLastUpgrade);
		});
		DataBundleRecordHandle<VillageArcherSchema> dataBundleRecordHandle = new DataBundleRecordHandle<VillageArcherSchema>("VillageArchers", archerLevel.ToString());
		DataBundleRecordHandle<VillageArcherSchema> dataBundleRecordHandle2 = new DataBundleRecordHandle<VillageArcherSchema>("VillageArchers", num2.ToString());
		VillageArcherSchema data = dataBundleRecordHandle.Data;
		VillageArcherSchema data2 = dataBundleRecordHandle2.Data;
		if (data2.bowDamage_2 == 0f)
		{
			if (data == null || data.bowDamage_1 == 0f)
			{
				item.details.AddStat("strength_stats", string.Empty, data2.bowDamage_1.ToString());
			}
			else
			{
				item.details.AddStat("strength_stats", data.bowDamage_1.ToString(), data2.bowDamage_1.ToString());
			}
			item.details.SetColumns(num2, num2);
		}
		else
		{
			string val = data2.bowDamage_1 + "/" + data2.bowDamage_2;
			string val2 = data.bowDamage_1 + "/" + data.bowDamage_2;
			item.details.AddStat("strength_stats", val2, val);
			item.details.SetColumns(archerLevel, num2);
		}
		item.details.MaxLevel = recordTableLength;
		item.id = "VillageArchers";
		string value = DataBundleRuntime.Instance.GetValue<string>(typeof(VillageArcherSchema), "VillageArchers", num2.ToString(), "icon", true);
		item.LoadIcon(value);
		item.locked = flag;
		item.isNew = Singleton<Profile>.Instance.highestUnlockedWave == num && (!SingletonMonoBehaviour<StoreMenuImpl>.Exists || !SingletonMonoBehaviour<StoreMenuImpl>.Instance.HasViewedNewItem(item.id));
		item.unlockAtWave = num;
		if (flag)
		{
			item.unlockCondition = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_unlockatwave"), item.unlockAtWave);
		}
		float salePercentage = SaleItemSchema.FindActiveSaleForItem("VillageArchers");
		int costCoins = data2.costCoins;
		if (costCoins > 0)
		{
			item.cost = new Cost(data2.costCoins, Cost.Currency.Soft, salePercentage);
		}
		else
		{
			item.cost = new Cost(data2.costGems, Cost.Currency.Hard, salePercentage);
		}
		string stringFromStringRef = StringUtils.GetStringFromStringRef("LocalizedStrings", "village_archers_name");
		if (Singleton<Profile>.Instance.archerLevel == 0)
		{
			item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_format_purchase_atlevel"), stringFromStringRef, num2);
		}
		else if (archerLevel == num2)
		{
			item.maxlevel = true;
			item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_upgrades_complete"), stringFromStringRef);
		}
		else
		{
			item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_format_upgrade_tolevel"), stringFromStringRef, num2);
		}
		item.unlockTitle = stringFromStringRef;
		item.details.AddSmallDescription(StringUtils.GetStringFromStringRef(data2.upgradeDescription));
		item.details.Name = stringFromStringRef;
		item.analyticsEvent = "UpgradePurchased";
		item.analyticsParams = new Dictionary<string, object>();
		item.analyticsParams["ItemName"] = item.id;
		item.analyticsParams["UpgradeLevel"] = num2;
		items.Add(item);
	}

	public static void GetPotions(List<StoreData.Item> items)
	{
		foreach (string item in Singleton<PotionsDatabase>.Instance.GetIDsForPlayMode("classic"))
		{
			items.Add(GetPotion(item));
		}
	}

	public static StoreData.Item GetPotion(string id)
	{
		PotionSchema s = Singleton<PotionsDatabase>.Instance[id];
		string delegateArg = s.id;
		string stringFromStringRef = StringUtils.GetStringFromStringRef(s.displayName);
		float salePercentage = SaleItemSchema.FindActiveSaleForItem(id);
		StoreData.Item item = new StoreData.Item(delegate
		{
			BuyPotions(delegateArg, s.amount);
		});
		item.id = s.id;
		item.LoadIcon(s.IconPath);
		item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_format_purchase_atlevel"), stringFromStringRef);
		item.details.AddDescription(StringUtils.GetStringFromStringRef(s.storeDesc));
		item.amount = s.amount;
		item.cost = new Cost(s.cost, salePercentage);
		item.details.Count = Singleton<Profile>.Instance.GetNumPotions(s.id);
		int packAmount = s.storePack;
		if (id.Equals("souls"))
		{
			int maxSouls = Singleton<Profile>.Instance.GetMaxSouls();
			item.details.MaxLevel = maxSouls;
			packAmount = maxSouls - Singleton<Profile>.Instance.souls;
		}
		item.packAmount = packAmount;
		item.packCost = new Cost(s.storePackCost, salePercentage);
		item.packPurchaseFunc = delegate
		{
			BuyPotions(delegateArg, packAmount);
		};
		item.isConsumable = true;
		item.details.Name = stringFromStringRef;
		item.analyticsEvent = "ConsumablePurchased";
		item.analyticsParams = new Dictionary<string, object>();
		item.analyticsParams["ItemName"] = item.id;
		return item;
	}

	public static void GetCharms(List<StoreData.Item> items)
	{
		foreach (CharmSchema item2 in Singleton<CharmsDatabase>.Instance.AllCharmsForActivePlayMode)
		{
			if (item2.store)
			{
				string delegateArg = item2.id;
				float salePercentage = SaleItemSchema.FindActiveSaleForItem(item2.id);
				StoreData.Item item = new StoreData.Item(delegate
				{
					BuyCharms(delegateArg, 1);
				});
				item.id = item2.id;
				item.LoadIcon(item2.IconPath);
				item.details.Name = StringUtils.GetStringFromStringRef(item2.displayName);
				item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_format_purchase"), item.details.Name);
				item.details.AddDescription(StringUtils.GetStringFromStringRef(item2.storeDesc));
				item.cost = new Cost(item2.cost, salePercentage);
				int packAmount = item2.storePack;
				item.packAmount = packAmount;
				item.packCost = new Cost(item2.storePackCost, salePercentage);
				item.packPurchaseFunc = delegate
				{
					BuyCharms(delegateArg, packAmount);
				};
				item.isConsumable = true;
				item.analyticsEvent = "ConsumablePurchased";
				item.analyticsParams = new Dictionary<string, object>();
				item.analyticsParams["ItemName"] = item.id;
				item.details.Count = Singleton<Profile>.Instance.GetNumCharms(item2.id);
				int i;
				for (i = 0; i < items.Count && item.cost.price < items[i].cost.price; i++)
				{
				}
				items.Insert(i, item);
			}
		}
	}

	public static void GetMPShield(List<StoreData.Item> items)
	{
		TextDBSchema[] mainData = DataBundleUtils.InitializeRecords<TextDBSchema>("MPShield");
		StoreData.Item item = new StoreData.Item(delegate
		{
			BuyMPShield(mainData.GetInt("duration"));
		});
		float salePercentage = SaleItemSchema.FindActiveSaleForItem("MPShield");
		item.id = "mpshield";
		item.cost = new Cost(mainData.GetString("cost"), salePercentage);
		item.title = StringUtils.GetStringFromStringRef("LocalizedStrings", mainData.GetString("title"));
		item.packCost = new Cost(mainData.GetString("cost2"), salePercentage);
		item.cost3 = new Cost(mainData.GetString("cost3"), salePercentage);
		item.packOverrideFunc = delegate
		{
			BuyMPShield(mainData.GetInt("duration2"));
		};
		item.packPurchaseFunc = delegate
		{
			BuyMPShield(mainData.GetInt("duration3"));
		};
		item.details.Name = item.title;
		item.details.AddDescription(StringUtils.GetStringFromStringRef("LocalizedStrings", mainData.GetString("desc")));
		item.LoadIcon("UI/Textures/DynamicIcons/Misc/MPShield");
		item.amount = mainData.GetInt("duration");
		item.isConsumable = false;
		item.analyticsParams = new Dictionary<string, object>();
		item.analyticsParams["ItemName"] = item.id;
		items.Add(item);
	}

	public static void GetUpgrades(List<StoreData.Item> items, bool globalGroup)
	{
		UpgradesSchema[] allUpgrades = Singleton<Profile>.Instance.AllUpgrades;
		foreach (UpgradesSchema upgradesSchema in allUpgrades)
		{
			if (upgradesSchema.globalStoreGroup == globalGroup)
			{
				items.Add(GetUpgrade(upgradesSchema.id));
			}
		}
	}

	public static StoreData.Item GetUpgrade(string id)
	{
		UpgradesSchema upgradeData = Singleton<Profile>.Instance.GetUpgradeData(id);
		int upgradeLevel = Singleton<Profile>.Instance.GetUpgradeLevel(upgradeData.id);
		int numUpgradeLevels = upgradeData.numUpgradeLevels;
		int num = ((upgradeLevel >= numUpgradeLevels) ? upgradeLevel : (upgradeLevel + 1));
		string delegateArg = upgradeData.id;
		StoreData.Item item = new StoreData.Item(delegate
		{
			BuyUpgrade(delegateArg);
		});
		item.id = upgradeData.id;
		item.details.Name = StringUtils.GetStringFromStringRef(upgradeData.name);
		if (numUpgradeLevels == 1)
		{
			item.details.LevelA = upgradeLevel;
			item.details.LevelB = num;
			item.details.MaxLevel = numUpgradeLevels;
			if (upgradeLevel == num)
			{
				item.maxlevel = true;
				item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_upgrades_complete"), item.details.Name);
			}
			else
			{
				item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_format_purchase"), item.details.Name);
			}
		}
		else
		{
			item.details.LevelA = upgradeLevel + 1;
			item.details.LevelB = num + 1;
			item.details.MaxLevel = numUpgradeLevels + 1;
			if (upgradeLevel == num)
			{
				item.maxlevel = true;
				item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_upgrades_complete"), item.details.Name);
			}
			else
			{
				item.title = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_format_upgrade_tolevel"), item.details.Name, num + 1);
			}
		}
		string val = string.Empty;
		string val2 = string.Empty;
		float salePercentage = SaleItemSchema.FindActiveSaleForItem(id);
		switch (num)
		{
		case 1:
			item.cost = new Cost(upgradeData.costLevel1, salePercentage);
			item.details.AddDescription(StringUtils.GetStringFromStringRef(upgradeData.descLevel1));
			item.details.Count = Mathf.RoundToInt(upgradeData.amountLevel1);
			item.LoadIcon(DataBundleRuntime.Instance.GetValue<string>(typeof(UpgradesSchema), "Upgrades", upgradeData.id, "iconLevel1", true));
			val = upgradeData.startingAmount.ToString();
			val2 = upgradeData.amountLevel1.ToString();
			break;
		case 2:
			item.cost = new Cost(upgradeData.costLevel2, salePercentage);
			item.details.AddDescription(StringUtils.GetStringFromStringRef(upgradeData.descLevel2));
			item.details.Count = Mathf.RoundToInt(upgradeData.amountLevel2);
			item.LoadIcon(DataBundleRuntime.Instance.GetValue<string>(typeof(UpgradesSchema), "Upgrades", upgradeData.id, "iconLevel2", true));
			val = upgradeData.amountLevel1.ToString();
			val2 = upgradeData.amountLevel2.ToString();
			break;
		case 3:
			item.cost = new Cost(upgradeData.costLevel3, salePercentage);
			item.details.AddDescription(StringUtils.GetStringFromStringRef(upgradeData.descLevel3));
			item.details.Count = Mathf.RoundToInt(upgradeData.amountLevel3);
			item.LoadIcon(DataBundleRuntime.Instance.GetValue<string>(typeof(UpgradesSchema), "Upgrades", upgradeData.id, "iconLevel3", true));
			val = upgradeData.amountLevel2.ToString();
			val2 = upgradeData.amountLevel3.ToString();
			break;
		case 4:
			item.cost = new Cost(upgradeData.costLevel4, salePercentage);
			item.details.AddDescription(StringUtils.GetStringFromStringRef(upgradeData.descLevel4));
			item.details.Count = Mathf.RoundToInt(upgradeData.amountLevel4);
			item.LoadIcon(DataBundleRuntime.Instance.GetValue<string>(typeof(UpgradesSchema), "Upgrades", upgradeData.id, "iconLevel4", true));
			val = upgradeData.amountLevel3.ToString();
			val2 = upgradeData.amountLevel4.ToString();
			break;
		}
		if (!item.maxlevel && id == "SoulJar")
		{
			item.details.AddStat("icon", val, val2);
			item.details.SetColumns(upgradeLevel + 1, num + 1);
		}
		item.analyticsEvent = "UpgradePurchased";
		item.analyticsParams = new Dictionary<string, object>();
		item.analyticsParams["ItemName"] = item.id;
		item.analyticsParams["UpgradeLevel"] = num;
		return item;
	}

	public static void GetMysteryBox(List<StoreData.Item> items)
	{
		TextDBSchema[] data = DataBundleUtils.InitializeRecords<TextDBSchema>("MysteryBox");
		StoreData.Item item = new StoreData.Item(delegate
		{
			MysteryBoxImpl.SetOverrideTexture(null);
			MysteryBoxImpl.BoxID = "MysteryBox";
			GenericTriggerAction("POPUP_MYSTERY_BOX");
		});
		float salePercentage = SaleItemSchema.FindActiveSaleForItem("MysteryBox");
		item.id = "mysterybox";
		item.cost = new Cost(data.GetString("cost"), salePercentage);
		item.title = StringUtils.GetStringFromStringRef("LocalizedStrings", data.GetString("title"));
		item.triggersOwnPopup = true;
		item.details.Name = item.title;
		item.details.AddDescription(StringUtils.GetStringFromStringRef("LocalizedStrings", data.GetString("desc")));
		item.LoadIcon("UI/Textures/DynamicIcons/Consumables/Consume_MysteryBox");
		item.packAmount = 5;
		float variable = SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable("MysteryBoxPackDiscount", 20f);
		int price = (int)((float)(item.cost.price * 5) * (1f - variable / 100f));
		item.packCost = new Cost(price, item.cost.currency, 0f);
		item.packShowBonus = true;
		item.packDiscount = (int)variable;
		item.packOverrideFunc = delegate
		{
			MysteryBoxImpl.SetOverrideTexture(null);
			MysteryBoxImpl.BoxID = "MysteryBox";
			GenericTriggerAction("POPUP_MYSTERY_BOX_5");
		};
		item.isConsumable = true;
		item.analyticsEvent = "MysteryBoxPurchased";
		item.analyticsParams = new Dictionary<string, object>();
		item.analyticsParams["ItemName"] = item.id;
		items.Add(item);
	}

	public static void GetHeroMysteryBox(string heroId, List<StoreData.Item> items)
	{
		if (SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable("HeroMysteryBox", false))
		{
			string table = heroId + "MysteryBox";
			TextDBSchema[] data = DataBundleUtils.InitializeRecords<TextDBSchema>(table);
			Texture2D overrideIcon = null;
			string path = "UI/Textures/DynamicIcons/Misc/Present_" + heroId;
			SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource(path, 1);
			if (cachedResource != null)
			{
				overrideIcon = cachedResource.Resource as Texture2D;
			}
			StoreData.Item item = new StoreData.Item(delegate
			{
				MysteryBoxImpl.SetOverrideTexture(overrideIcon);
				MysteryBoxImpl.BoxID = heroId + "MysteryBox";
				GenericTriggerAction("POPUP_MYSTERY_BOX");
			});
			float salePercentage = SaleItemSchema.FindActiveSaleForItem("MysteryBox");
			item.id = "mysterybox";
			item.cost = new Cost(data.GetString("cost"), salePercentage);
			item.title = StringUtils.GetStringFromStringRef("LocalizedStrings", data.GetString("title"));
			item.triggersOwnPopup = true;
			item.details.Name = item.title;
			item.details.AddDescription(StringUtils.GetStringFromStringRef("LocalizedStrings", data.GetString("desc")));
			item.LoadIcon(path);
			item.packAmount = 5;
			float variable = SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable("MysteryBoxPackDiscount", 20f);
			int price = (int)((float)(item.cost.price * 5) * (1f - variable / 100f));
			item.packCost = new Cost(price, item.cost.currency, 0f);
			item.packShowBonus = true;
			item.packDiscount = (int)variable;
			item.packOverrideFunc = delegate
			{
				MysteryBoxImpl.SetOverrideTexture(overrideIcon);
				MysteryBoxImpl.BoxID = heroId + "MysteryBox";
				GenericTriggerAction("POPUP_MYSTERY_BOX_5");
			};
			item.isConsumable = true;
			item.analyticsEvent = "MysteryBoxPurchased";
			item.analyticsParams = new Dictionary<string, object>();
			item.analyticsParams["ItemName"] = item.id + heroId;
			items.Add(item);
		}
	}

	private static void GetDealPacks(List<StoreData.Item> items)
	{
		DealPackSchema[] array = DataBundleUtils.InitializeRecords<DealPackSchema>("DealPacks");
		DealPackSchema[] array2 = array;
		foreach (DealPackSchema dealPackSchema in array2)
		{
			if (dealPackSchema.showInStore)
			{
				float salePercentage = SaleItemSchema.FindActiveSaleForItem(dealPackSchema.id);
				StoreData.Item item = new StoreData.Item(null);
				string[] array3 = dealPackSchema.items.Split(',');
				string[] array4 = array3;
				foreach (string item2 in array4)
				{
					item.bundleContent.Add(item2);
				}
				item.id = dealPackSchema.id;
				item.cost = new Cost(dealPackSchema.cost, salePercentage);
				item.title = StringUtils.GetStringFromStringRef(dealPackSchema.displayName);
				item.details.Name = item.title;
				item.details.AddDescription(StringUtils.GetStringFromStringRef(dealPackSchema.description));
				item.LoadIcon(DataBundleRuntime.Instance.GetValue<string>(typeof(DealPackSchema), "DealPacks", dealPackSchema.id, "icon", true));
				item.analyticsEvent = "DealPackPurchased";
				item.analyticsParams = new Dictionary<string, object>();
				item.analyticsParams["ItemName"] = item.id;
				item.dealPack = dealPackSchema;
				items.Add(item);
			}
		}
	}

	private static void LevelUpHero(string heroID, bool isLastUpgrade)
	{
		Singleton<Profile>.Instance.SetHeroLevel(heroID, Singleton<Profile>.Instance.GetHeroLevel(heroID) + 1);
		Singleton<Profile>.Instance.Save();
		if (isLastUpgrade)
		{
			HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[heroID];
			if (heroSchema != null && heroSchema.upgradeAchievement != null && !string.IsNullOrEmpty(heroSchema.upgradeAchievement.Key))
			{
				Singleton<Achievements>.Instance.SetAchievementCompletionCount(heroSchema.upgradeAchievement.Key, 1);
				Singleton<Achievements>.Instance.CheckMetaAchievement("AllUpgrades");
			}
		}
		SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", heroID, GluiAgentBase.Order.Redraw, null, null);
	}

	private static void LevelUpLeadership(string heroID, bool isLastUpgrade)
	{
		Singleton<Profile>.Instance.SetLeadershipLevel(heroID, Singleton<Profile>.Instance.GetLeadershipLevel(heroID) + 1);
		Singleton<Profile>.Instance.Save();
		SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", heroID, GluiAgentBase.Order.Redraw, null, null);
		if (isLastUpgrade)
		{
			HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[heroID];
			if (heroSchema != null && heroSchema.leadershipAchievement != null && !string.IsNullOrEmpty(heroSchema.leadershipAchievement.Key))
			{
				Singleton<Achievements>.Instance.SetAchievementCompletionCount(heroSchema.leadershipAchievement.Key, 1);
				Singleton<Achievements>.Instance.CheckMetaAchievement("AllUpgrades");
			}
		}
	}

	private static void LevelUpSword(string heroID, bool isLastUpgrade)
	{
		Singleton<Profile>.Instance.SetMeleeWeaponLevel(heroID, Singleton<Profile>.Instance.GetMeleeWeaponLevel(heroID) + 1);
		Singleton<Profile>.Instance.Save();
		SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", heroID, GluiAgentBase.Order.Redraw, null, null);
		if (isLastUpgrade)
		{
			HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[heroID];
			if (heroSchema != null && heroSchema.meleeAchievement != null && !string.IsNullOrEmpty(heroSchema.meleeAchievement.Key))
			{
				Singleton<Achievements>.Instance.SetAchievementCompletionCount(heroSchema.meleeAchievement.Key, 1);
				Singleton<Achievements>.Instance.CheckMetaAchievement("AllUpgrades");
			}
		}
	}

	private static void LevelUpBow(string heroID, bool isLastUpgrade)
	{
		Singleton<Profile>.Instance.SetRangedWeaponLevel(heroID, Singleton<Profile>.Instance.GetRangedWeaponLevel(heroID) + 1);
		Singleton<Profile>.Instance.Save();
		SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", heroID, GluiAgentBase.Order.Redraw, null, null);
		if (isLastUpgrade)
		{
			HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[heroID];
			if (heroSchema != null && heroSchema.rangedAchievement != null && !string.IsNullOrEmpty(heroSchema.rangedAchievement.Key))
			{
				Singleton<Achievements>.Instance.SetAchievementCompletionCount(heroSchema.rangedAchievement.Key, 1);
				Singleton<Achievements>.Instance.CheckMetaAchievement("AllUpgrades");
			}
		}
	}

	private static void LevelUpArmor(string heroID, bool isLastUpgrade)
	{
		Singleton<Profile>.Instance.SetArmorLevel(heroID, Singleton<Profile>.Instance.GetArmorLevel(heroID) + 1);
		Singleton<Profile>.Instance.Save();
		SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", heroID, GluiAgentBase.Order.Redraw, null, null);
		if (isLastUpgrade)
		{
			HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[heroID];
			if (heroSchema != null && heroSchema.rangedAchievement != null && !string.IsNullOrEmpty(heroSchema.rangedAchievement.Key))
			{
				Singleton<Achievements>.Instance.SetAchievementCompletionCount(heroSchema.rangedAchievement.Key, 1);
				Singleton<Achievements>.Instance.CheckMetaAchievement("AllUpgrades");
			}
		}
	}

	private static void LevelUpGate(bool isLastUpgrade)
	{
		Singleton<Profile>.Instance.baseLevel++;
		Singleton<Profile>.Instance.Save();
		SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", StringUtils.GetStringFromStringRef("MenuFixedStrings", "store_items_global"), GluiAgentBase.Order.Redraw, null, null);
		if (isLastUpgrade)
		{
			Singleton<Achievements>.Instance.SetAchievementCompletionCount("GateUpgrade", 1);
			Singleton<Achievements>.Instance.CheckMetaAchievement("AllUpgrades");
		}
	}

	private static void LevelUpBell(bool isLastUpgrade)
	{
		Singleton<Profile>.Instance.bellLevel++;
		Singleton<Profile>.Instance.Save();
		SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", StringUtils.GetStringFromStringRef("MenuFixedStrings", "store_items_global"), GluiAgentBase.Order.Redraw, null, null);
		if (isLastUpgrade)
		{
			Singleton<Achievements>.Instance.SetAchievementCompletionCount("BellUpgrade", 1);
			Singleton<Achievements>.Instance.CheckMetaAchievement("AllUpgrades");
		}
	}

	private static void LevelUpPit(bool isLastUpgrade)
	{
		Singleton<Profile>.Instance.pitLevel++;
		Singleton<Profile>.Instance.Save();
		SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", StringUtils.GetStringFromStringRef("MenuFixedStrings", "store_items_global"), GluiAgentBase.Order.Redraw, null, null);
		if (isLastUpgrade)
		{
			Singleton<Achievements>.Instance.SetAchievementCompletionCount("PitUpgrade", 1);
			Singleton<Achievements>.Instance.CheckMetaAchievement("AllUpgrades");
		}
	}

	private static void LevelUpUndeath(bool isLastUpgrade)
	{
		Singleton<Profile>.Instance.undeathLevel++;
		Singleton<Profile>.Instance.Save();
	}

	private static void LevelUpVillageArchers(bool isLastUpgrade)
	{
		Singleton<Profile>.Instance.archerLevel++;
		Singleton<Profile>.Instance.Save();
		SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", StringUtils.GetStringFromStringRef("MenuFixedStrings", "store_items_global"), GluiAgentBase.Order.Redraw, null, null);
		if (isLastUpgrade)
		{
			Singleton<Achievements>.Instance.SetAchievementCompletionCount("VillageArchersUpgrade", 1);
			Singleton<Achievements>.Instance.CheckMetaAchievement("AllUpgrades");
		}
	}

	private static void BuyPotions(string potionID, int amount)
	{
		if (potionID.Equals("souls"))
		{
			int souls = Singleton<Profile>.Instance.souls;
			int maxSouls = Singleton<Profile>.Instance.GetMaxSouls();
			if (amount == 0)
			{
				amount = maxSouls - souls;
			}
		}
		Singleton<Profile>.Instance.SetNumPotions(potionID, Singleton<Profile>.Instance.GetNumPotions(potionID) + amount);
		Singleton<Profile>.Instance.Save();
		SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", "Consumables", GluiAgentBase.Order.Redraw, null, null);
	}

	private static void BuyCharms(string charmID, int count)
	{
		Singleton<Profile>.Instance.SetNumCharms(charmID, Singleton<Profile>.Instance.GetNumCharms(charmID) + count);
		Singleton<Profile>.Instance.Save();
		SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", "Charms", GluiAgentBase.Order.Redraw, null, null);
	}

	private static void BuyBalls(int num)
	{
		Singleton<Profile>.Instance.pachinkoBalls += num;
		Singleton<Profile>.Instance.Save();
	}

	private static void BuyUpgrade(string upgradeID)
	{
		Singleton<Profile>.Instance.SetUpgradeLevel(upgradeID, Singleton<Profile>.Instance.GetUpgradeLevel(upgradeID) + 1);
		if (upgradeID == "AbilitySlot")
		{
			SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", StringUtils.GetStringFromStringRef("MenuFixedStrings", "store_items_global"), GluiAgentBase.Order.Redraw, null, null);
		}
		else
		{
			SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", "Consumables", GluiAgentBase.Order.Redraw, null, null);
		}
		switch (upgradeID)
		{
		case "SoulJar":
			Singleton<Profile>.Instance.souls = Singleton<Profile>.Instance.souls;
			if (Singleton<Profile>.Instance.GetUpgradeLevel(upgradeID) == Singleton<Profile>.Instance.GetUpgradeData(upgradeID).numUpgradeLevels)
			{
				Singleton<Achievements>.Instance.SetAchievementCompletionCount("UpgradeSoulJar", 1);
				Singleton<Achievements>.Instance.CheckMetaAchievement("AllUpgrades");
			}
			break;
		case "SoulCollectionRate":
			if (Singleton<Profile>.Instance.GetUpgradeLevel(upgradeID) == Singleton<Profile>.Instance.GetUpgradeData(upgradeID).numUpgradeLevels)
			{
				Singleton<Achievements>.Instance.SetAchievementCompletionCount("UpgradeSoulRate", 1);
				Singleton<Achievements>.Instance.CheckMetaAchievement("AllUpgrades");
			}
			break;
		case "DefenseTimer":
			if (Singleton<Profile>.Instance.GetUpgradeLevel(upgradeID) == Singleton<Profile>.Instance.GetUpgradeData(upgradeID).numUpgradeLevels)
			{
				Singleton<Achievements>.Instance.SetAchievementCompletionCount("UpgradeDefenseTimer", 1);
				Singleton<Achievements>.Instance.CheckMetaAchievement("AllUpgrades");
			}
			break;
		case "SoulsDouble":
			if (Singleton<Profile>.Instance.GetUpgradeLevel(upgradeID) == Singleton<Profile>.Instance.GetUpgradeData(upgradeID).numUpgradeLevels)
			{
				Singleton<Achievements>.Instance.SetAchievementCompletionCount("SoulDouble", 1);
				Singleton<Achievements>.Instance.CheckMetaAchievement("AllUpgrades");
			}
			break;
		case "CoinDouble":
			if (Singleton<Profile>.Instance.GetUpgradeLevel(upgradeID) == Singleton<Profile>.Instance.GetUpgradeData(upgradeID).numUpgradeLevels)
			{
				Singleton<Achievements>.Instance.SetAchievementCompletionCount("CoinDoubler", 1);
				Singleton<Achievements>.Instance.CheckMetaAchievement("AllUpgrades");
			}
			break;
		}
		SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", "Upgrades", GluiAgentBase.Order.Redraw, null, null);
		Singleton<Profile>.Instance.Save();
	}

	public static void BuyMPShield(int duration)
	{
		if (SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.SNTPTime.SNTPSuccessful)
		{
			DateTime dateTime = Singleton<Profile>.Instance.MultiplayerShieldExpireTime;
			DateTime universalTime = SntpTime.UniversalTime;
			if (dateTime < universalTime)
			{
				dateTime = universalTime;
			}
			TimeSpan timeSpan = TimeSpan.FromHours(duration);
			dateTime += timeSpan;
			Singleton<Profile>.Instance.SetMultiplayerShieldTime(dateTime);
			Singleton<Profile>.Instance.Save();
		}
	}

	private static void GenericTriggerAction(string action)
	{
		GluiActionSender.SendGluiAction(action, null, null);
	}

	public static void CashInDealPack(DealPackSchema dp)
	{
		string[] array = dp.items.Split(',');
		string[] array2 = array;
		foreach (string id in array2)
		{
			CashIn.From(id, 1, "DEALPACK");
		}
		Singleton<Profile>.Instance.Save();
	}

	public static void CashInDealPackFromString(string dealpack, int quantity)
	{
		DataBundleRecordKey dataBundleRecordKey = new DataBundleRecordKey("DealPacks", dealpack);
		DealPackSchema dealPackSchema = DataBundleRuntime.Instance.InitializeRecord<DealPackSchema>(dataBundleRecordKey);
		if (dealPackSchema == null)
		{
			return;
		}
		string[] array = dealPackSchema.items.Split(',');
		string[] array2 = array;
		foreach (string id in array2)
		{
			for (int j = 0; j < quantity; j++)
			{
				CashIn.From(id, 1, "DEALPACK");
			}
		}
		Singleton<Profile>.Instance.Save();
	}
}
