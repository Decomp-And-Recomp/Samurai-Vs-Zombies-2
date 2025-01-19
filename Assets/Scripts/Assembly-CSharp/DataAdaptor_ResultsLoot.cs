using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DataAdaptor_ResultsLoot : DataAdaptorBase
{
	public GameObject icon;

	public GameObject text_Name;

	public GameObject text_Counter;

	public GameObject XSprite;

	public DataAdaptor_CollectionItem collectible_Loot;

	public override void SetData(object data)
	{
		PlayStatistics.Data.LootEntry lootEntry = (PlayStatistics.Data.LootEntry)data;
		KeyValuePair<string, int> keyValuePair = new KeyValuePair<string, int>(string.Empty, 0);
		Texture2D texture2D = null;
		string text = string.Empty;
		int num = 0;
		if (!string.IsNullOrEmpty(lootEntry.packId))
		{
			DataBundleRecordKey dataBundleRecordKey = new DataBundleRecordKey("DealPacks", lootEntry.packId);
			DealPackSchema dealPackSchema = DataBundleRuntime.Instance.InitializeRecord<DealPackSchema>(dataBundleRecordKey);
			num = lootEntry.num;
			if (dealPackSchema != null)
			{
				text = StringUtils.GetStringFromStringRef(dealPackSchema.displayName);
				texture2D = dealPackSchema.icon;
				if (dealPackSchema.icon == null)
				{
					string value = DataBundleRuntime.Instance.GetValue<string>(typeof(DealPackSchema), "DealPacks", lootEntry.packId, "icon", true);
					SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource(value, 1);
					if (cachedResource != null)
					{
						texture2D = (dealPackSchema.icon = cachedResource.Resource as Texture2D);
					}
				}
			}
		}
		else
		{
			keyValuePair = CashIn.StandardizeItemID(lootEntry.id, lootEntry.num);
			text = "!!!!! " + keyValuePair.Key;
			num = keyValuePair.Value;
			switch (CashIn.GetType(keyValuePair.Key))
			{
			case CashIn.ItemType.Coins:
				text = StringUtils.GetStringFromStringRef("MenuFixedStrings", "coins");
				texture2D = ResourceCache.GetCachedResource(GetCoinsIconPath(keyValuePair.Value), 1).Resource as Texture2D;
				break;
			case CashIn.ItemType.Souls:
				if (keyValuePair.Key == "zombieHead")
				{
					text = StringUtils.GetStringFromStringRef("MenuFixedStrings", "zombieHead");
					texture2D = ResourceCache.GetCachedResource(GetZombieHeadIconPath(keyValuePair.Value), 1).Resource as Texture2D;
				}
				else
				{
					text = StringUtils.GetStringFromStringRef("MenuFixedStrings", "souls");
					texture2D = ResourceCache.GetCachedResource(GetSoulsIconPath(keyValuePair.Value)).Resource as Texture2D;
				}
				break;
			case CashIn.ItemType.Gems:
				text = StringUtils.GetStringFromStringRef("MenuFixedStrings", "gems");
				texture2D = ResourceCache.GetCachedResource(GetGemsIconPath(keyValuePair.Value), 1).Resource as Texture2D;
				break;
			case CashIn.ItemType.Potion:
			{
				PotionSchema potionSchema = Singleton<PotionsDatabase>.Instance[keyValuePair.Key];
				texture2D = potionSchema.icon;
				text = StringUtils.GetStringFromStringRef(potionSchema.displayName);
				break;
			}
			case CashIn.ItemType.Charm:
			{
				CharmSchema charmSchema = Singleton<CharmsDatabase>.Instance[keyValuePair.Key];
				if (WeakGlobalMonoBehavior<InGameImpl>.Exists && charmSchema.icon == null)
				{
					Singleton<CharmsDatabase>.Instance.LoadInGameData(keyValuePair.Key, false);
				}
				texture2D = charmSchema.icon;
				text = StringUtils.GetStringFromStringRef(charmSchema.displayName);
				break;
			}
			case CashIn.ItemType.Helper:
			{
				HelperSchema helperSchema2 = Singleton<HelpersDatabase>.Instance[keyValuePair.Key];
				bool goldenHelperUnlocked = Singleton<Profile>.Instance.GetGoldenHelperUnlocked(keyValuePair.Key);
				if (WeakGlobalMonoBehavior<InGameImpl>.Exists && ((!goldenHelperUnlocked && helperSchema2.HUDIcon == null) || (goldenHelperUnlocked && helperSchema2.TryGetChampionIcon() == null)))
				{
					Singleton<HelpersDatabase>.Instance.LoadInGameData(keyValuePair.Key);
				}
				texture2D = ((!goldenHelperUnlocked) ? helperSchema2.HUDIcon : helperSchema2.TryGetChampionIcon());
				text = StringUtils.GetStringFromStringRef(helperSchema2.displayName);
				num = -1;
				break;
			}
			case CashIn.ItemType.GoldenHelper:
			{
				string id = HelpersDatabase.HelperIDFromGoldenHelperID(keyValuePair.Key);
				HelperSchema helperSchema = Singleton<HelpersDatabase>.Instance[id];
				if (WeakGlobalMonoBehavior<InGameImpl>.Exists && helperSchema.TryGetChampionIcon() == null)
				{
					Singleton<HelpersDatabase>.Instance.LoadInGameData(id);
				}
				texture2D = helperSchema.TryGetChampionIcon();
				text = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "golden_helper_name_format"), StringUtils.GetStringFromStringRef(helperSchema.displayName));
				num = -1;
				break;
			}
			case CashIn.ItemType.Hero:
			{
				HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[keyValuePair.Key];
				texture2D = heroSchema.icon;
				text = StringUtils.GetStringFromStringRef(heroSchema.displayName);
				num = -1;
				break;
			}
			case CashIn.ItemType.MysteryBox:
				text = StringUtils.GetStringFromStringRef("MenuFixedStrings", "Menu_MysteryBox");
				texture2D = ResourceCache.GetCachedResource(GetMysteryBoxIconPath(keyValuePair.Value), 1).Resource as Texture2D;
				break;
			case CashIn.ItemType.HeroUpgrade:
			{
				string[] array = keyValuePair.Key.Split('.');
				if (array.Length != 2)
				{
					throw new Exception("Invalid Hero Upgrade id: " + keyValuePair.Key);
				}
				num++;
				text = ((num <= 3 || string.Compare(array[1], "leadership", true) != 0) ? string.Format(StringUtils.GetStringFromStringRef("MenuFixedStrings", "stat_level_upgrade"), num - 1) : StringUtils.GetStringFromStringRef("MenuFixedStrings", "Menu_MaxLevel"));
				HeroSchema heroSchema2 = Singleton<HeroesDatabase>.Instance[array[0]];
				switch (array[1].ToLower())
				{
				case "level":
					texture2D = heroSchema2.icon;
					break;
				case "rangedweapon":
					texture2D = ResourceCache.GetCachedResource(heroSchema2.RangedWeapon.Levels[keyValuePair.Value].IconPath, 1).Resource as Texture2D;
					break;
				case "meleeweapon":
					texture2D = ResourceCache.GetCachedResource(heroSchema2.MeleeWeapon.Levels[keyValuePair.Value].IconPath, 1).Resource as Texture2D;
					break;
				case "armor":
					texture2D = heroSchema2.GetArmorLevel(keyValuePair.Value).icon;
					break;
				case "leadership":
					texture2D = ResourceCache.GetCachedResource(Singleton<PlayModesManager>.Instance.selectedModeData.IconPath).Resource as Texture2D;
					break;
				}
				num = -1;
				break;
			}
			case CashIn.ItemType.Ability:
			{
				AbilitySchema abilitySchema = Singleton<AbilitiesDatabase>.Instance[keyValuePair.Key];
				text = string.Format(StringUtils.GetStringFromStringRef("MenuFixedStrings", "stat_level_upgrade"), num);
				texture2D = ResourceCache.GetCachedResource(abilitySchema.IconPath, 1).Resource as Texture2D;
				num = -1;
				break;
			}
			default:
			{
				int collectibleID;
				if (Singleton<Profile>.Instance.MultiplayerData.GetLootCollectibleID(keyValuePair.Key, out collectibleID))
				{
					CollectionSchema collectionSet;
					collectible_Loot.SetData(Singleton<Profile>.Instance.MultiplayerData.GetCollectionItemData(collectibleID, out collectionSet));
					if (text_Counter != null)
					{
						text_Counter.SetActive(false);
					}
					return;
				}
				break;
			}
			}
		}
		SetGluiTextInChild(text_Name, text);
		if (num < 0)
		{
			text_Counter.SetActive(false);
		}
		else
		{
			text_Counter.SetActive(true);
			SetGluiTextInChild(text_Counter, "x " + num);
		}
		if (texture2D != null)
		{
			SetGluiSpriteInChild(icon, texture2D);
		}
		else if (string.IsNullOrEmpty(keyValuePair.Key))
		{
		}
		if (XSprite != null)
		{
			XSprite.SetActive(!Singleton<PlayStatistics>.Instance.data.victory);
		}
	}

	private string GetCoinsIconPath(int numGems)
	{
		return "UI/Textures/DynamicIcons/Misc/Currency_Soft_Temp";
	}

	private string GetSoulsIconPath(int numGems)
	{
		return "UI/Textures/DynamicIcons/Misc/Currency_Soul_Temp";
	}

	private string GetZombieHeadIconPath(int num)
	{
		return "UI/Textures/DynamicIcons/Consumables/Special_ZombieHead";
	}

	private string GetGemsIconPath(int numGems)
	{
		return "UI/Textures/DynamicIcons/Misc/Loot_Gem";
	}

	private string GetMysteryBoxIconPath(int numBoxes)
	{
		return "UI/Textures/DynamicIcons/Consumables/Consume_MysteryBox";
	}
}
