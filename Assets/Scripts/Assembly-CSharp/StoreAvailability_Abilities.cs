using System;
using System.Collections.Generic;
using UnityEngine;

public class StoreAvailability_Abilities
{
	public static void GetGlobalAbilities(List<StoreData.Item> items)
	{
		foreach (string globalAbilityID in Singleton<AbilitiesDatabase>.Instance.GlobalAbilityIDs)
		{
			Singleton<AbilitiesDatabase>.Instance.BuildStoreData(globalAbilityID, items);
		}
	}

	public static void GetHeroAbilities(HeroSchema hero, List<StoreData.Item> items)
	{
		AbilitySchema[] abilities = hero.Abilities;
		foreach (AbilitySchema abilitySchema in abilities)
		{
			if (!Singleton<AbilitiesDatabase>.Instance.GlobalAbilityIDs.Contains(abilitySchema.id))
			{
				Singleton<AbilitiesDatabase>.Instance.BuildStoreData(abilitySchema.id, items);
			}
		}
	}

	private static StoreData.Item GetAbilityUpgrade(string abilityID, out AbilitySchema mainData)
	{
		bool flag = false;
		mainData = Singleton<AbilitiesDatabase>.Instance[abilityID];
		if ((float)Singleton<Profile>.Instance.highestUnlockedWave < mainData.levelToUnlock)
		{
			flag = true;
		}
		else
		{
			EnsureProperInitialLevel(abilityID);
		}
		int abilityLevel = Singleton<Profile>.Instance.GetAbilityLevel(abilityID);
		int num = abilityLevel + 1;
		int num2 = mainData.levelData.Length;
		DataBundleRecordKey dataBundleRecordKey = null;
		float salePercentage = SaleItemSchema.FindActiveSaleForItem(abilityID);
		bool flag2 = false;
		Cost costForItem;
		if (num > num2)
		{
			if (mainData.infiniteUpgradeCostCoins == 0f && mainData.infiniteUpgradeCostGems == 0f)
			{
				flag2 = true;
				num = num2;
			}
			costForItem = new Cost((int)mainData.infiniteUpgradeCostCoins + "," + (int)mainData.infiniteUpgradeCostGems, salePercentage);
		}
		else
		{
			costForItem = new Cost(mainData.levelData[num - 1].costCoins + "," + mainData.levelData[num - 1].costGems, salePercentage);
			dataBundleRecordKey = mainData.levelData[num - 1].upgradeDescription;
		}
		bool isLastUpgrade = num == num2;
		StoreData.Item item = new StoreData.Item(delegate
		{
			LevelUpAbility(costForItem, abilityID, isLastUpgrade);
		});
		item.cost = costForItem;
		item.id = abilityID;
		string iconPath = mainData.IconPath;
		item.LoadIcon(iconPath);
		item.locked = flag;
		item.unlockAtWave = (int)mainData.levelToUnlock;
		item.isNew = (float)Singleton<Profile>.Instance.highestUnlockedWave == mainData.levelToUnlock && (!SingletonMonoBehaviour<StoreMenuImpl>.Exists || !SingletonMonoBehaviour<StoreMenuImpl>.Instance.HasViewedNewItem(item.id));
		if (flag)
		{
			item.unlockCondition = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "store_unlockatwave"), item.unlockAtWave);
		}
		string stringFromStringRef = StringUtils.GetStringFromStringRef(mainData.displayName);
		if (flag2)
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
		string text = StringUtils.GetStringFromStringRef(mainData.description);
		if (!DataBundleRecordKey.IsNullOrEmpty(dataBundleRecordKey))
		{
			text = string.Format("{0} {1}", text, StringUtils.GetStringFromStringRef(dataBundleRecordKey));
		}
		item.details.AddSmallDescription(text);
		item.details.Count = abilityLevel;
		item.analyticsEvent = "UpgradePurchased";
		item.analyticsParams = new Dictionary<string, object>();
		item.analyticsParams["ItemName"] = "Ability_" + item.id;
		item.analyticsParams["UpgradeLevel"] = num;
		return item;
	}

	public static void GetAbilityUpgrade_DamageOnly(string abilityID, List<StoreData.Item> items)
	{
		AbilitySchema mainData;
		StoreData.Item abilityUpgrade = GetAbilityUpgrade(abilityID, out mainData);
		int abilityLevel = Singleton<Profile>.Instance.GetAbilityLevel(abilityID);
		int num = abilityLevel + 1;
		if (num > 1)
		{
			abilityUpgrade.details.SetColumns(abilityLevel, num);
			abilityUpgrade.details.AddStat("strength_stats", Mathf.RoundToInt(mainData.Extrapolate(abilityLevel, (AbilityLevelSchema als) => als.damage)).ToString(), Mathf.RoundToInt(mainData.Extrapolate(num, (AbilityLevelSchema als) => als.damage)).ToString());
		}
		items.Add(abilityUpgrade);
	}

	public static void GetAbilityUpgrade_DivineIntervention(string abilityID, List<StoreData.Item> items)
	{
		AbilitySchema mainData;
		StoreData.Item abilityUpgrade = GetAbilityUpgrade(abilityID, out mainData);
		int abilityLevel = Singleton<Profile>.Instance.GetAbilityLevel(abilityID);
		int num = abilityLevel + 1;
		if (num > 1)
		{
			float num2 = mainData.Extrapolate(abilityLevel, (AbilityLevelSchema als) => als.effectModifier);
			float num3 = mainData.Extrapolate(num, (AbilityLevelSchema als) => als.effectModifier);
			abilityUpgrade.details.AddStat("summon_ally", num2.ToString(), num3.ToString());
			abilityUpgrade.details.SetColumns(num - 1, num);
		}
		items.Add(abilityUpgrade);
	}

	public static void GetAbilityUpgrade_Lethargy(string abilityID, List<StoreData.Item> items)
	{
		AbilitySchema mainData;
		StoreData.Item abilityUpgrade = GetAbilityUpgrade(abilityID, out mainData);
		int abilityLevel = Singleton<Profile>.Instance.GetAbilityLevel(abilityID);
		int num = abilityLevel + 1;
		if (num > 1)
		{
			float secs = mainData.Extrapolate(abilityLevel, (AbilityLevelSchema als) => als.effectDuration);
			float secs2 = mainData.Extrapolate(num, (AbilityLevelSchema als) => als.effectDuration);
			abilityUpgrade.details.AddStat("duration", GetSecondsString(secs), GetSecondsString(secs2));
			abilityUpgrade.details.SetColumns(abilityLevel, num);
		}
		items.Add(abilityUpgrade);
	}

	public static void GetAbilityUpgrade_SummonLightning(string abilityID, List<StoreData.Item> items)
	{
		AbilitySchema mainData;
		StoreData.Item abilityUpgrade = GetAbilityUpgrade(abilityID, out mainData);
		int abilityLevel = Singleton<Profile>.Instance.GetAbilityLevel(abilityID);
		int num = abilityLevel + 1;
		if (num > 1)
		{
			float damage = mainData.Extrapolate(abilityLevel, (AbilityLevelSchema als) => als.damage);
			float dOTDamage = mainData.Extrapolate(abilityLevel, (AbilityLevelSchema als) => als.DOTDamage);
			float secs = mainData.Extrapolate(abilityLevel, (AbilityLevelSchema als) => als.DOTDuration);
			float damage2 = mainData.Extrapolate(num, (AbilityLevelSchema als) => als.damage);
			float dOTDamage2 = mainData.Extrapolate(num, (AbilityLevelSchema als) => als.DOTDamage);
			float secs2 = mainData.Extrapolate(num, (AbilityLevelSchema als) => als.DOTDuration);
			abilityUpgrade.details.AddStat("strength_stats", CombineLightningDamage(damage, dOTDamage), CombineLightningDamage(damage2, dOTDamage2));
			abilityUpgrade.details.AddStat("duration", GetSecondsString(secs), GetSecondsString(secs2));
			abilityUpgrade.details.SetColumns(abilityLevel, num);
		}
		items.Add(abilityUpgrade);
	}

	public static void GetAbilityUpgrade_NightOfTheDead(string abilityID, List<StoreData.Item> items)
	{
		AbilitySchema mainData;
		StoreData.Item abilityUpgrade = GetAbilityUpgrade(abilityID, out mainData);
		int abilityLevel = Singleton<Profile>.Instance.GetAbilityLevel(abilityID);
		int num = abilityLevel + 1;
		if (num > 1)
		{
			float secs = mainData.Extrapolate(abilityLevel, (AbilityLevelSchema als) => als.duration);
			float secs2 = mainData.Extrapolate(num, (AbilityLevelSchema als) => als.duration);
			abilityUpgrade.details.AddStat("duration", GetSecondsString(secs), GetSecondsString(secs2));
			abilityUpgrade.details.SetColumns(abilityLevel, num);
		}
		abilityUpgrade.analyticsEvent = "UpgradePurchased";
		abilityUpgrade.analyticsParams = new Dictionary<string, object>();
		abilityUpgrade.analyticsParams["ItemName"] = "Ability_" + abilityUpgrade.id;
		abilityUpgrade.analyticsParams["UpgradeLevel"] = num;
		items.Add(abilityUpgrade);
	}

	public static void GetAbilityUpgrade_FlashBomb(string abilityID, List<StoreData.Item> items)
	{
		AbilitySchema mainData;
		StoreData.Item abilityUpgrade = GetAbilityUpgrade(abilityID, out mainData);
		int abilityLevel = Singleton<Profile>.Instance.GetAbilityLevel(abilityID);
		int num = abilityLevel + 1;
		if (num > 1)
		{
			float secs = mainData.Extrapolate(abilityLevel, (AbilityLevelSchema als) => als.effectDuration);
			float secs2 = mainData.Extrapolate(num, (AbilityLevelSchema als) => als.effectDuration);
			abilityUpgrade.details.AddStat("duration", GetSecondsString(secs), GetSecondsString(secs2));
			abilityUpgrade.details.AddStat("strength_stats", Mathf.RoundToInt(mainData.Extrapolate(abilityLevel, (AbilityLevelSchema als) => als.damage)).ToString(), Mathf.RoundToInt(mainData.Extrapolate(num, (AbilityLevelSchema als) => als.damage)).ToString());
			abilityUpgrade.details.SetColumns(abilityLevel, num);
		}
		items.Add(abilityUpgrade);
	}

	public static void GetAbilityUpgrade_SetTrap(string abilityID, List<StoreData.Item> items)
	{
		AbilitySchema mainData;
		StoreData.Item abilityUpgrade = GetAbilityUpgrade(abilityID, out mainData);
		int abilityLevel = Singleton<Profile>.Instance.GetAbilityLevel(abilityID);
		int num = abilityLevel + 1;
		if (num > 1)
		{
			float secs = mainData.Extrapolate(abilityLevel, (AbilityLevelSchema als) => als.effectDuration);
			float secs2 = mainData.Extrapolate(num, (AbilityLevelSchema als) => als.effectDuration);
			abilityUpgrade.details.AddStat("duration", GetSecondsString(secs), GetSecondsString(secs2));
			abilityUpgrade.details.AddStat("strength_stats", Mathf.RoundToInt(mainData.Extrapolate(abilityLevel, (AbilityLevelSchema als) => als.damage)).ToString(), Mathf.RoundToInt(mainData.Extrapolate(num, (AbilityLevelSchema als) => als.damage)).ToString());
			abilityUpgrade.details.SetColumns(abilityLevel, num);
		}
		items.Add(abilityUpgrade);
	}

	public static void GetAbilityUpgrade_Inspire(string abilityID, List<StoreData.Item> items)
	{
		AbilitySchema mainData;
		StoreData.Item abilityUpgrade = GetAbilityUpgrade(abilityID, out mainData);
		int abilityLevel = Singleton<Profile>.Instance.GetAbilityLevel(abilityID);
		int num = abilityLevel + 1;
		if (num > 1)
		{
			abilityUpgrade.details.SetColumns(abilityLevel, num);
			abilityUpgrade.details.AddStat("strength_stats", Mathf.RoundToInt(mainData.Extrapolate(abilityLevel, (AbilityLevelSchema als) => als.damageMultEachTarget)).ToString(), Mathf.RoundToInt(mainData.Extrapolate(num, (AbilityLevelSchema als) => als.damageMultEachTarget)).ToString());
		}
		items.Add(abilityUpgrade);
	}

	private static string GetPercentString(float rawVal)
	{
		return string.Format("{0}%", Mathf.CeilToInt(rawVal * 100f).ToString());
	}

	private static string GetSecondsString(float secs)
	{
		return Mathf.RoundToInt(secs).ToString();
	}

	private static string CombineLightningDamage(float damage, float DOTDamage)
	{
		return string.Format("{0}+{1}", Mathf.RoundToInt(damage), Mathf.RoundToInt(DOTDamage));
	}

	private static void EnsureProperInitialLevel(string abilityID)
	{
		if (Singleton<Profile>.Instance.GetAbilityLevel(abilityID) == 0)
		{
			Singleton<Profile>.Instance.SetAbilityLevel(abilityID, 1);
			Singleton<Profile>.Instance.Save();
		}
	}

	private static void LevelUpAbility(Cost cost, string abilityID, bool isLastUpgrade)
	{
		Singleton<Profile>.Instance.SetAbilityLevel(abilityID, Singleton<Profile>.Instance.GetAbilityLevel(abilityID) + 1);
		Singleton<Profile>.Instance.Save();
		if (Singleton<AbilitiesDatabase>.Instance.GlobalAbilityIDs.Contains(abilityID))
		{
			SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", StringUtils.GetStringFromStringRef("MenuFixedStrings", "store_items_global"), GluiAgentBase.Order.Redraw, null, null);
		}
		else
		{
			foreach (DataBundleRecordHandle<HeroSchema> allHero in Singleton<HeroesDatabase>.Instance.AllHeroes)
			{
				if (Array.Find(allHero.Data.Abilities, (AbilitySchema ab) => string.Equals(ab.id, abilityID)) != null)
				{
					SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", allHero.Data.id, GluiAgentBase.Order.Redraw, null, null);
					break;
				}
			}
		}
		AbilitySchema abilitySchema = Singleton<AbilitiesDatabase>.Instance[abilityID];
		Singleton<Analytics>.Instance.LogEvent("AbilityUpgradePurchased", Analytics.Param("ItemName", abilityID), Analytics.Param("UpgradeLevel", Singleton<Profile>.Instance.GetAbilityLevel(abilityID)), Analytics.Param("Cost", cost.price), Analytics.Param("Currency", cost.currencyAnalyticCode), Analytics.Param("PlayerLevel", Singleton<Profile>.Instance.playerLevel));
		if (isLastUpgrade && abilitySchema.upgradeAchievement != null && !string.IsNullOrEmpty(abilitySchema.upgradeAchievement.Key))
		{
			Singleton<Achievements>.Instance.SetAchievementCompletionCount(abilitySchema.upgradeAchievement.Key, 1);
			Singleton<Achievements>.Instance.CheckMetaAchievement("AllUpgrades");
		}
	}
}
