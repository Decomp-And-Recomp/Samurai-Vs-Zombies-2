using System.Collections.Generic;

public class CashIn
{
	public enum ItemType
	{
		Unknown = 0,
		Coins = 1,
		Gems = 2,
		Balls = 3,
		BoosterPack = 4,
		Potion = 5,
		Charm = 6,
		Helper = 7,
		Ability = 8,
		Hero = 9,
		GoldenHelper = 10,
		HeroUpgrade = 11,
		MysteryBox = 12,
		Souls = 13,
		Leadership = 14
	}

	private static float gemsToCoins;

	private static float soulsToCoins;

	private static float leadershipToCoins;

	public static bool From(string id, string val, string source)
	{
		return From(id, int.Parse(val), source);
	}

	public static bool From(string id, int val, string source)
	{
		KeyValuePair<string, int> keyValuePair = StandardizeItemID(id, val);
		if (keyValuePair.Value > 0)
		{
			switch (GetType(keyValuePair.Key))
			{
			case ItemType.Coins:
				Singleton<Profile>.Instance.AddCoins(keyValuePair.Value, source);
				break;
			case ItemType.Gems:
				Singleton<Profile>.Instance.AddGems(keyValuePair.Value, source);
				break;
			case ItemType.Balls:
				Singleton<Profile>.Instance.pachinkoBalls += keyValuePair.Value;
				break;
			case ItemType.Potion:
				Singleton<Profile>.Instance.SetNumPotions(keyValuePair.Key, Singleton<Profile>.Instance.GetNumPotions(keyValuePair.Key) + keyValuePair.Value);
				SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", "Consumables", GluiAgentBase.Order.Redraw, null, null);
				break;
			case ItemType.Charm:
				Singleton<Profile>.Instance.SetNumCharms(keyValuePair.Key, Singleton<Profile>.Instance.GetNumCharms(keyValuePair.Key) + keyValuePair.Value);
				SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", "Charms", GluiAgentBase.Order.Redraw, null, null);
				break;
			case ItemType.Helper:
				if (keyValuePair.Value >= 1 && Singleton<Profile>.Instance.GetHelperLevel(keyValuePair.Key) == 0)
				{
					HelperSchema helperSchema = Singleton<HelpersDatabase>.Instance[keyValuePair.Key];
					if (!string.IsNullOrEmpty(helperSchema.unlockPlayhavenRequest))
					{
						if (!SingletonMonoBehaviour<StoreMenuImpl>.Exists)
						{
							StoreMenuImpl.queuedPlayhavenRequest = helperSchema.unlockPlayhavenRequest;
						}
						else
						{
							ApplicationUtilities.MakePlayHavenContentRequest(helperSchema.unlockPlayhavenRequest);
						}
					}
				}
				Singleton<Profile>.Instance.SetHelperLevel(keyValuePair.Key, Singleton<Profile>.Instance.GetHelperLevel(keyValuePair.Key) + keyValuePair.Value);
				break;
			case ItemType.Ability:
				Singleton<Profile>.Instance.SetAbilityLevel(keyValuePair.Key, Singleton<Profile>.Instance.GetAbilityLevel(keyValuePair.Key) + keyValuePair.Value);
				SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", "Heroes", GluiAgentBase.Order.Redraw, null, null);
				break;
			case ItemType.Hero:
			{
				if (keyValuePair.Value <= 0 || Singleton<Profile>.Instance.GetHeroPurchased(keyValuePair.Key))
				{
					break;
				}
				Singleton<Profile>.Instance.SetHeroPurchased(keyValuePair.Key, true);
				SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", "Heroes", GluiAgentBase.Order.Redraw, null, null);
				HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[keyValuePair.Key];
				if (heroSchema == null)
				{
					break;
				}
				if (heroSchema.unlockAchievement != null)
				{
					Singleton<Achievements>.Instance.IncrementAchievement(heroSchema.unlockAchievement.Key, 1);
				}
				if (!string.IsNullOrEmpty(heroSchema.unlockPlayhavenRequest))
				{
					if (!SingletonMonoBehaviour<StoreMenuImpl>.Exists)
					{
						StoreMenuImpl.queuedPlayhavenRequest = heroSchema.unlockPlayhavenRequest;
					}
					else
					{
						ApplicationUtilities.MakePlayHavenContentRequest(heroSchema.unlockPlayhavenRequest);
					}
				}
				break;
			}
			case ItemType.HeroUpgrade:
				if (keyValuePair.Value > 0)
				{
					string[] array = keyValuePair.Key.Split('.');
					switch (array[1].ToLower())
					{
					case "level":
						Singleton<Profile>.Instance.SetHeroLevel(array[0], Singleton<Profile>.Instance.GetHeroLevel(array[0]) + keyValuePair.Value);
						break;
					case "rangedweapon":
						Singleton<Profile>.Instance.SetRangedWeaponLevel(array[0], Singleton<Profile>.Instance.GetRangedWeaponLevel(array[0]) + keyValuePair.Value);
						break;
					case "meleeweapon":
						Singleton<Profile>.Instance.SetMeleeWeaponLevel(array[0], Singleton<Profile>.Instance.GetMeleeWeaponLevel(array[0]) + keyValuePair.Value);
						break;
					case "armor":
						Singleton<Profile>.Instance.SetArmorLevel(array[0], Singleton<Profile>.Instance.GetArmorLevel(array[0]) + keyValuePair.Value);
						break;
					case "leadership":
						Singleton<Profile>.Instance.SetLeadershipLevel(array[0], Singleton<Profile>.Instance.GetLeadershipLevel(array[0]) + keyValuePair.Value);
						break;
					}
					SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", "Heroes", GluiAgentBase.Order.Redraw, null, null);
				}
				break;
			case ItemType.MysteryBox:
				if (keyValuePair.Value == 5)
				{
					GluiActionSender.SendGluiAction("POPUP_MYSTERY_BOX_5", null, null);
				}
				else
				{
					GluiActionSender.SendGluiAction("POPUP_MYSTERY_BOX", null, null);
				}
				break;
			case ItemType.GoldenHelper:
				if (keyValuePair.Value > 0)
				{
					Singleton<Profile>.Instance.SetGoldenHelperUnlocked(HelpersDatabase.HelperIDFromGoldenHelperID(id), true);
					SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", "Champions", GluiAgentBase.Order.Redraw, null, null);
					SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder("Store", "Allies", GluiAgentBase.Order.Redraw, null, null);
				}
				break;
			case ItemType.Souls:
				if (string.Compare(keyValuePair.Key, "zombieHead", true) == 0)
				{
					Singleton<Profile>.Instance.souls += Singleton<Profile>.Instance.GetMaxSouls() - Singleton<Profile>.Instance.souls;
					Singleton<Achievements>.Instance.IncrementAchievement("ZombieHead", 1);
				}
				else
				{
					Singleton<Profile>.Instance.souls += keyValuePair.Value;
				}
				break;
			default:
				return false;
			}
			return true;
		}
		return false;
	}

	public static ItemType GetType(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return ItemType.Unknown;
		}
		KeyValuePair<string, int> keyValuePair = StandardizeItemID(id);
		if (string.Compare(keyValuePair.Key, "coins", true) == 0 || string.Compare(keyValuePair.Key, "coin", true) == 0)
		{
			return ItemType.Coins;
		}
		if (string.Compare(keyValuePair.Key, "gems", true) == 0 || string.Compare(keyValuePair.Key, "gem", true) == 0)
		{
			return ItemType.Gems;
		}
		if (string.Compare(keyValuePair.Key, "balls", true) == 0 || string.Compare(keyValuePair.Key, "ball", true) == 0)
		{
			return ItemType.Balls;
		}
		if (string.Compare(keyValuePair.Key, "booster_free", true) == 0)
		{
			return ItemType.BoosterPack;
		}
		if (string.Compare(keyValuePair.Key, "souls", true) == 0 || string.Compare(keyValuePair.Key, "soul", true) == 0 || string.Compare(keyValuePair.Key, "zombieHead", true) == 0)
		{
			return ItemType.Souls;
		}
		if (string.Compare(keyValuePair.Key, "leadership", true) == 0)
		{
			return ItemType.Leadership;
		}
		if (string.Compare(keyValuePair.Key, "mysterybox") == 0)
		{
			return ItemType.MysteryBox;
		}
		if (HelpersDatabase.IsGoldenHelperID(keyValuePair.Key))
		{
			return ItemType.GoldenHelper;
		}
		if (Singleton<PotionsDatabase>.Instance.Contains(keyValuePair.Key))
		{
			return ItemType.Potion;
		}
		if (Singleton<CharmsDatabase>.Instance.Contains(keyValuePair.Key))
		{
			return ItemType.Charm;
		}
		if (Singleton<HelpersDatabase>.Instance.Contains(keyValuePair.Key))
		{
			return ItemType.Helper;
		}
		if (Singleton<AbilitiesDatabase>.Instance.Contains(keyValuePair.Key))
		{
			return ItemType.Ability;
		}
		if (Singleton<HeroesDatabase>.Instance.Contains(keyValuePair.Key))
		{
			return ItemType.Hero;
		}
		string[] array = keyValuePair.Key.Split('.');
		if (array != null && array.Length == 2 && Singleton<HeroesDatabase>.Instance.Contains(array[0]))
		{
			return ItemType.HeroUpgrade;
		}
		return ItemType.Unknown;
	}

	public static int GetValueInCoins(string id, int amount)
	{
		KeyValuePair<string, int> keyValuePair = StandardizeItemID(id, amount);
		if (GetType(keyValuePair.Key) == ItemType.Coins)
		{
			return keyValuePair.Value;
		}
		if (GetType(keyValuePair.Key) == ItemType.Gems)
		{
			return ConvertGemsValueToCoins(keyValuePair.Value);
		}
		if (GetType(keyValuePair.Key) == ItemType.Souls)
		{
			return ConvertSoulsValueToCoins(keyValuePair.Value);
		}
		if (GetType(keyValuePair.Key) == ItemType.Leadership)
		{
			return ConvertLeadershipValueToCoins(keyValuePair.Value);
		}
		Cost? cost = null;
		if (Singleton<PotionsDatabase>.Instance.Contains(keyValuePair.Key))
		{
			cost = new Cost(Singleton<PotionsDatabase>.Instance[keyValuePair.Key].cost, 0f);
		}
		else if (Singleton<CharmsDatabase>.Instance.Contains(keyValuePair.Key))
		{
			cost = new Cost(Singleton<CharmsDatabase>.Instance[keyValuePair.Key].cost, 0f);
		}
		else if (Singleton<HelpersDatabase>.Instance.Contains(keyValuePair.Key))
		{
			cost = new Cost(Singleton<HelpersDatabase>.Instance[keyValuePair.Key].valueInCoins.ToString(), 0f);
		}
		if (cost.HasValue)
		{
			switch (cost.Value.currency)
			{
			case Cost.Currency.Soft:
				return cost.Value.price * keyValuePair.Value;
			case Cost.Currency.Hard:
				return ConvertGemsValueToCoins(cost.Value.price) * keyValuePair.Value;
			case Cost.Currency.Soul:
				return ConvertSoulsValueToCoins(cost.Value.price) * keyValuePair.Value;
			default:
				return 0;
			}
		}
		return 0;
	}

	public static int ConvertGemsValueToCoins(int amount)
	{
		if (gemsToCoins == 0f)
		{
			IAPSchema iAPSchema = SingletonSpawningMonoBehaviour<GluIap>.Instance.Products.Find((IAPSchema s) => string.Equals(s.productId, "com.glu.samuzombie2.PACK_GEMS_199"));
			IAPSchema iAPSchema2 = SingletonSpawningMonoBehaviour<GluIap>.Instance.Products.Find((IAPSchema s) => string.Equals(s.productId, "com.glu.samuzombie2.PACK_COINS_199"));
			int num = ((iAPSchema == null) ? 40 : iAPSchema.hardCurrencyAmount);
			int num2 = ((iAPSchema2 == null) ? 2000 : iAPSchema2.softCurrencyAmount);
			gemsToCoins = (float)num2 / (float)num;
		}
		return (int)((float)amount * gemsToCoins);
	}

	public static int ConvertSoulsValueToCoins(int amount)
	{
		if (soulsToCoins == 0f)
		{
			PotionSchema potionSchema = Singleton<PotionsDatabase>.Instance["souls"];
			Cost cost = new Cost(potionSchema.cost, 0f);
			switch (cost.currency)
			{
			case Cost.Currency.Soft:
				soulsToCoins = (float)cost.price / (float)potionSchema.amount;
				break;
			case Cost.Currency.Hard:
				soulsToCoins = (float)ConvertGemsValueToCoins(cost.price) / (float)potionSchema.amount;
				break;
			}
		}
		return (int)((float)amount * soulsToCoins);
	}

	public static int ConvertLeadershipValueToCoins(int amount)
	{
		if (leadershipToCoins == 0f)
		{
			PotionSchema potionSchema = Singleton<PotionsDatabase>.Instance["leadershipPotion"];
			Cost cost = new Cost(potionSchema.cost, 0f);
			switch (cost.currency)
			{
			case Cost.Currency.Soft:
				leadershipToCoins = (float)cost.price / (float)potionSchema.amount;
				break;
			case Cost.Currency.Hard:
				leadershipToCoins = (float)ConvertGemsValueToCoins(cost.price) / (float)potionSchema.amount;
				break;
			}
		}
		return (int)((float)amount * leadershipToCoins);
	}

	public static bool WillTriggerPopup(string id)
	{
		return string.Compare(StandardizeItemID(id).Key, "mysterybox", true) == 0;
	}

	public static KeyValuePair<string, int> StandardizeItemID(string id, int num = 0)
	{
		string[] array = id.Split('.');
		if (array.Length > 1)
		{
			string text = array[array.Length - 1];
			int result;
			if (int.TryParse(text, out result))
			{
				num = result;
				id = id.Substring(0, id.Length - (text.Length + 1));
			}
		}
		return new KeyValuePair<string, int>(id, num);
	}
}
