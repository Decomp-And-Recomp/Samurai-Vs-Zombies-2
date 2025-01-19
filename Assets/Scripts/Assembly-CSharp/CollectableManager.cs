using System;
using System.Collections.Generic;
using UnityEngine;

public class CollectableManager : WeakGlobalInstance<CollectableManager>
{
	public const float kHeroCollectRange = 0.32f;

	public static WaveSpoils currentWaveSpoils = new WaveSpoils();

	private List<Collectable> mCollectables;

	private ResourceTemplate[] mResourceTemplates;

	private float mTotalDropWeight;

	private float mLeftEdge;

	private float mRightEdge;

	private float mCenterX;

	public float magnetMaxDist { get; private set; }

	public float magnetMinSpeed { get; private set; }

	public float magnetMaxSpeed { get; private set; }

	public float LeftEdge
	{
		get
		{
			return mLeftEdge;
		}
		set
		{
			mLeftEdge = value;
		}
	}

	public float RightEdge
	{
		get
		{
			return mRightEdge;
		}
		set
		{
			mRightEdge = value;
		}
	}

	public CollectableManager(float leftEdge, float rightEdge, float centerX)
	{
		SetUniqueInstance(this);
		mCollectables = new List<Collectable>();
		mLeftEdge = leftEdge;
		mRightEdge = rightEdge;
		mCenterX = centerX;
		mTotalDropWeight = 0f;
		currentWaveSpoils = new WaveSpoils();
		if (WeakGlobalMonoBehavior<InGameImpl>.Instance.HasWealthCharm())
		{
			CharmSchema charmSchema = Singleton<CharmsDatabase>.Instance[WeakGlobalMonoBehavior<InGameImpl>.Instance.activeCharm];
			magnetMaxDist = charmSchema.magnetRange;
			magnetMinSpeed = charmSchema.magnetMinPullSpeed;
			magnetMaxSpeed = charmSchema.magnetMaxPullSpeed;
		}
		else
		{
			magnetMaxDist = 0f;
			magnetMinSpeed = 0f;
			magnetMaxSpeed = 0f;
		}
		TextDBSchema[] array = ((!Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive()) ? DataBundleUtils.InitializeRecords<TextDBSchema>("resources") : DataBundleUtils.InitializeRecords<TextDBSchema>("resources_MP"));
		if (array == null)
		{
			return;
		}
		string text = ((!string.IsNullOrEmpty(Singleton<Profile>.Instance.playModeSubSection)) ? Singleton<Profile>.Instance.playModeSubSection : "classic");
		mResourceTemplates = new ResourceTemplate[9];
		string[] names = Enum.GetNames(typeof(ECollectableType));
		Array values = Enum.GetValues(typeof(ECollectableType));
		for (int i = 0; i < names.Length; i++)
		{
			string text2 = names[i];
			int @int = array.GetInt(TextDBSchema.ChildKey(text2, "amount"));
			if (@int < 1)
			{
				continue;
			}
			ECollectableType eCollectableType = (ECollectableType)(int)values.GetValue(i);
			ResourceTemplate resourceTemplate = new ResourceTemplate
			{
				amount = @int
			};
			SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource(string.Format("Assets/Game/Resources/{0}.prefab", array.GetString(TextDBSchema.ChildKey(text2, "prefab", text))), 1);
			if (cachedResource != null)
			{
				resourceTemplate.prefab = cachedResource.Resource as GameObject;
			}
			resourceTemplate.lifetime = array.GetFloat(TextDBSchema.ChildKey(text2, "lifetime"));
			resourceTemplate.weight = array.GetFloat(TextDBSchema.ChildKey(text2, "weight"));
			resourceTemplate.contentsTotalWeight = 0f;
			resourceTemplate.contents = null;
			resourceTemplate.postDeathContentsTotalWeight = 0f;
			resourceTemplate.postDeathContents = null;
			if (eCollectableType >= ECollectableType.presentA && eCollectableType <= ECollectableType.presentD)
			{
				string text3 = TextDBSchema.ChildKey(text2, "normal", text);
				string text4 = TextDBSchema.ChildKey(text2, "death", text);
				TextDBSchema[] array2 = array;
				foreach (TextDBSchema textDBSchema in array2)
				{
					if (textDBSchema.key.StartsWith(text3))
					{
						string text5 = textDBSchema.key.Substring(text3.Length + 1);
						if (IsValidPresentContent(text5) && !AlreadyHasPermenantItem(text5) && !ItemRestrictedByLevel(text5))
						{
							if (resourceTemplate.contents == null)
							{
								resourceTemplate.contents = new Dictionary<string, float>();
							}
							float num = float.Parse(textDBSchema.value);
							resourceTemplate.contents.Add(text5, num);
							resourceTemplate.contentsTotalWeight += num;
						}
					}
					else
					{
						if (!textDBSchema.key.StartsWith(text4))
						{
							continue;
						}
						string text6 = textDBSchema.key.Substring(text4.Length + 1);
						if (IsValidPresentContent(text6) && !AlreadyHasPermenantItem(text6) && !ItemRestrictedByLevel(text6))
						{
							if (resourceTemplate.postDeathContents == null)
							{
								resourceTemplate.postDeathContents = new Dictionary<string, float>();
							}
							float num2 = float.Parse(textDBSchema.value);
							resourceTemplate.postDeathContents.Add(text6, num2);
							resourceTemplate.postDeathContentsTotalWeight += num2;
						}
					}
				}
			}
			mResourceTemplates[(int)eCollectableType] = resourceTemplate;
			mTotalDropWeight += resourceTemplate.weight;
		}
	}

	public void Update()
	{
		int num = 0;
		while (num < mCollectables.Count)
		{
			if (mCollectables[num].isReadyToBeCollected && IsInRange(WeakGlobalMonoBehavior<InGameImpl>.Instance.hero, mCollectables[num]))
			{
				mCollectables[num].OnCollected();
			}
			else if (mCollectables[num].isReadyToDie)
			{
				mCollectables[num].Destroy();
			}
			else
			{
				mCollectables[num].Update();
			}
			if (mCollectables[num].isDestroyed)
			{
				mCollectables.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	public void SpawnResources(ResourceDrops drops, Vector3 position)
	{
		int num = drops.amountDropped.randInRange();
		if (num <= 0)
		{
			return;
		}
		List<ECollectableType> list = new List<ECollectableType>();
		for (int i = 0; i < num; i++)
		{
			float num2 = UnityEngine.Random.Range(0f, mTotalDropWeight - float.Epsilon);
			for (int j = 0; j < 9; j++)
			{
				ECollectableType eCollectableType = (ECollectableType)j;
				num2 -= mResourceTemplates[j].weight;
				if (mResourceTemplates[j].prefab != null && num2 < 0f && mResourceTemplates[j].weight > 0f)
				{
					if (eCollectableType >= ECollectableType.presentA && eCollectableType <= ECollectableType.presentD)
					{
						mCollectables.Add(new Collectable(eCollectableType, mResourceTemplates[(int)eCollectableType], position, FindNewCollectableFinalPosition(position)));
						return;
					}
					list.Add(eCollectableType);
					break;
				}
			}
		}
		foreach (ECollectableType item in list)
		{
			if (mResourceTemplates[(int)item].prefab != null)
			{
				mCollectables.Add(new Collectable(item, mResourceTemplates[(int)item], position, FindNewCollectableFinalPosition(position)));
			}
		}
	}

	public void ForceSpawnResourceType(string dropType, Vector3 position)
	{
		foreach (int value in Enum.GetValues(typeof(ECollectableType)))
		{
			if (((ECollectableType)value).ToString().Equals(dropType, StringComparison.OrdinalIgnoreCase))
			{
				mCollectables.Add(new Collectable((ECollectableType)value, mResourceTemplates[value], position, FindNewCollectableFinalPosition(position)));
				break;
			}
		}
	}

	public void OpenPresents(bool fromPlayerDeath)
	{
		foreach (CollectedPresent present in currentWaveSpoils.presents)
		{
			if (present.hasBeenOpened)
			{
				continue;
			}
			present.hasBeenOpened = true;
			float num = 0f;
			Dictionary<string, float> dictionary = null;
			if (fromPlayerDeath)
			{
				num = UnityEngine.Random.Range(0f, mResourceTemplates[(int)present.type].postDeathContentsTotalWeight - float.Epsilon);
				dictionary = mResourceTemplates[(int)present.type].postDeathContents;
			}
			else
			{
				num = UnityEngine.Random.Range(0f, mResourceTemplates[(int)present.type].contentsTotalWeight - float.Epsilon);
				dictionary = mResourceTemplates[(int)present.type].contents;
			}
			if (dictionary != null)
			{
				try
				{
					foreach (KeyValuePair<string, float> item in dictionary)
					{
						num -= item.Value;
						if (num < 0f)
						{
							present.contents = item.Key;
							RemovePermenantItemFromFuturePresents(item.Key);
							break;
						}
					}
				}
				catch (Exception)
				{
				}
			}
			Singleton<PlayStatistics>.Instance.data.AddLoot(present.contents, 1, present.type);
			int valueInCoins = CashIn.GetValueInCoins(present.contents, 1);
			Singleton<PlayStatistics>.Instance.data.totalLootDroppedValue += valueInCoins;
			switch (CashIn.GetType(present.contents))
			{
			case CashIn.ItemType.Coins:
				Singleton<PlayStatistics>.Instance.data.droppedCoins++;
				break;
			case CashIn.ItemType.Gems:
				Singleton<PlayStatistics>.Instance.data.droppedGems++;
				break;
			case CashIn.ItemType.Souls:
				Singleton<PlayStatistics>.Instance.data.droppedSouls++;
				break;
			case CashIn.ItemType.Helper:
			case CashIn.ItemType.Ability:
				Singleton<PlayStatistics>.Instance.data.droppedUnlockables++;
				break;
			default:
				Singleton<PlayStatistics>.Instance.data.droppedOtherValue += valueInCoins;
				break;
			}
		}
	}

	public void GiveResource(ECollectableType type, int amount)
	{
		switch (type)
		{
		case ECollectableType.coin:
			if (Singleton<Profile>.Instance.GetUpgradeLevel("CoinDouble") > 0)
			{
				currentWaveSpoils.coins += amount * 2;
			}
			else
			{
				currentWaveSpoils.coins += amount;
			}
			break;
		case ECollectableType.soul:
			if (Singleton<Profile>.Instance.GetUpgradeLevel("SoulsDouble") > 0)
			{
				currentWaveSpoils.souls += amount * 2;
			}
			else
			{
				currentWaveSpoils.souls += amount;
			}
			break;
		case ECollectableType.leadership:
			WeakGlobalInstance<Leadership>.Instance.resources += amount;
			currentWaveSpoils.leadership += amount;
			break;
		case ECollectableType.gem:
			currentWaveSpoils.gems += amount;
			Singleton<PlayStatistics>.Instance.data.AddLoot("gems", amount);
			break;
		case ECollectableType.pachinkoball:
			currentWaveSpoils.balls += amount;
			break;
		case ECollectableType.presentA:
		case ECollectableType.presentB:
		case ECollectableType.presentC:
		{
			CollectedPresent collectedPresent = new CollectedPresent();
			collectedPresent.type = type;
			currentWaveSpoils.presents.Add(collectedPresent);
			break;
		}
		case ECollectableType.presentD:
			break;
		}
	}

	public void RecordLootDropped(ECollectableType type, int amount)
	{
		switch (type)
		{
		case ECollectableType.coin:
			if (Singleton<Profile>.Instance.GetUpgradeLevel("CoinDouble") > 0)
			{
				amount *= 2;
			}
			Singleton<PlayStatistics>.Instance.data.totalLootDroppedValue += amount;
			Singleton<PlayStatistics>.Instance.data.droppedCoins += amount;
			break;
		case ECollectableType.soul:
			if (Singleton<Profile>.Instance.GetUpgradeLevel("SoulsDouble") > 0)
			{
				amount *= 2;
			}
			Singleton<PlayStatistics>.Instance.data.totalLootDroppedValue += CashIn.GetValueInCoins("souls", amount);
			Singleton<PlayStatistics>.Instance.data.droppedSouls += amount;
			break;
		case ECollectableType.leadership:
			amount = CashIn.GetValueInCoins("leadership", amount);
			Singleton<PlayStatistics>.Instance.data.totalLootDroppedValue += amount;
			Singleton<PlayStatistics>.Instance.data.droppedOtherValue += amount;
			break;
		case ECollectableType.gem:
			Singleton<PlayStatistics>.Instance.data.totalLootDroppedValue += CashIn.GetValueInCoins("gems", amount);
			Singleton<PlayStatistics>.Instance.data.droppedGems += amount;
			break;
		}
	}

	public void GiveSpecificPresent(string id, int amount)
	{
		if (amount > 0)
		{
			CollectedPresent collectedPresent = new CollectedPresent();
			collectedPresent.type = ECollectableType.presentD;
			collectedPresent.contents = id;
			collectedPresent.hasBeenOpened = true;
			collectedPresent.amount = amount;
			currentWaveSpoils.presents.Add(collectedPresent);
			Singleton<PlayStatistics>.Instance.data.AddLoot(id, amount);
		}
	}

	public void BankAllResources()
	{
		Singleton<Profile>.Instance.ForceActiveSaveData(false);
		Singleton<Profile>.Instance.AddCoins(currentWaveSpoils.coins, "Collectables");
		Singleton<Profile>.Instance.AddGems(currentWaveSpoils.gems, "Collectables");
		Singleton<Profile>.Instance.souls += currentWaveSpoils.souls;
		Singleton<Profile>.Instance.pachinkoBalls += currentWaveSpoils.balls;
		OpenPresents(false);
		foreach (CollectedPresent present in currentWaveSpoils.presents)
		{
			int amount = present.amount;
			if (CashIn.From(present.contents, amount, "Present"))
			{
			}
		}
		Singleton<Profile>.Instance.ForceActiveSaveData(true);
	}

	private bool IsInRange(Hero hero, Collectable obj)
	{
		return (hero.position.z > obj.position.z && !obj.wasAtLeftOfHero) || (hero.position.z < obj.position.z && obj.wasAtLeftOfHero) || Mathf.Abs(hero.position.z - obj.position.z) <= 0.32f;
	}

	public Vector3 FindNewCollectableFinalPosition(Vector3 spawnPosition)
	{
		Vector3 zero = Vector3.zero;
		zero.x = mCenterX + 1.15f + UnityEngine.Random.Range(-0.02f, 0.02f);
		zero.z = spawnPosition.z + UnityEngine.Random.Range(-0.55f, 0.55f);
		zero.z = Mathf.Clamp(zero.z, mLeftEdge, mRightEdge);
		zero.y = WeakGlobalInstance<RailManager>.Instance.GetY(zero.z) + 0.26f;
		return zero;
	}

	private bool AlreadyHasPermenantItem(string theItemName)
	{
		if (Singleton<AbilitiesDatabase>.Instance.Contains(theItemName))
		{
			if (Singleton<Profile>.Instance.GetAbilityLevel(theItemName) > 0)
			{
				return true;
			}
		}
		else if (Singleton<HelpersDatabase>.Instance.Contains(theItemName) && Singleton<Profile>.Instance.GetHelperLevel(theItemName) > 0)
		{
			return true;
		}
		return false;
	}

	private bool ItemRestrictedByLevel(string theItemName)
	{
		bool flag = false;
		string[] allIDs = Singleton<HelpersDatabase>.Instance.allIDs;
		foreach (string strA in allIDs)
		{
			if (string.Compare(strA, theItemName, true) == 0)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		HelperSchema helperSchema = Singleton<HelpersDatabase>.Instance[theItemName];
		if (helperSchema != null)
		{
			int availableAtWave = helperSchema.availableAtWave;
			if (Singleton<Profile>.Instance.highestUnlockedWave >= availableAtWave)
			{
				return false;
			}
		}
		return true;
	}

	private void RemovePermenantItemFromFuturePresents(string theItemName)
	{
		if (!ArrayContainsString(Singleton<HelpersDatabase>.Instance.allIDs, theItemName) && !ArrayContainsString(Singleton<AbilitiesDatabase>.Instance.allIDs, theItemName))
		{
			return;
		}
		ResourceTemplate[] array = mResourceTemplates;
		foreach (ResourceTemplate resourceTemplate in array)
		{
			if (resourceTemplate.contents != null && resourceTemplate.contents.ContainsKey(theItemName))
			{
				resourceTemplate.contentsTotalWeight -= resourceTemplate.contents[theItemName];
				resourceTemplate.contents.Remove(theItemName);
			}
			if (resourceTemplate.postDeathContents != null && resourceTemplate.postDeathContents.ContainsKey(theItemName))
			{
				resourceTemplate.postDeathContentsTotalWeight -= resourceTemplate.postDeathContents[theItemName];
				resourceTemplate.postDeathContents.Remove(theItemName);
			}
		}
	}

	private bool ArrayContainsString(string[] theStringArray, string theStringToFind)
	{
		foreach (string text in theStringArray)
		{
			if (text.Equals(theStringToFind, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsValidPresentContent(string itemID)
	{
		return true;
	}
}
