using System.Collections.Generic;
using UnityEngine;

public class Leadership : WeakGlobalInstance<Leadership>
{
	public struct LeadershipCost
	{
		public float leadership;

		public int gems;

		public int coins;

		public LeadershipCost(string data)
		{
			leadership = 0f;
			gems = 0;
			coins = 0;
			string[] array = data.Split(',');
			if (array.Length >= 1)
			{
				leadership = float.Parse(array[0]);
			}
			if (array.Length >= 2)
			{
				gems = int.Parse(array[1]);
			}
			if (array.Length >= 3)
			{
				coins = int.Parse(array[2]);
			}
		}

		public LeadershipCost(float cost)
		{
			leadership = cost;
			gems = 0;
			coins = 0;
		}

		public bool canAfford(int playerId)
		{
			if (leadership > 0f)
			{
				return WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(playerId).resources >= leadership;
			}
			if (gems > 0)
			{
				return Singleton<Profile>.Instance.gems >= gems;
			}
			if (coins > 0)
			{
				return Singleton<Profile>.Instance.coins >= coins;
			}
			return false;
		}

		public void Spend(int playerId)
		{
			if (leadership > 0f)
			{
				WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(playerId).resources -= leadership;
			}
			else if (gems > 0)
			{
				if (playerId == 0)
				{
					Singleton<Profile>.Instance.SpendGems(gems);
					Singleton<Profile>.Instance.gems -= gems;
				}
			}
			else if (coins > 0 && playerId == 0)
			{
				Singleton<Profile>.Instance.SpendCoins(coins);
				Singleton<Profile>.Instance.gems -= coins;
			}
		}
	}

	public class HelperTypeData
	{
		private CharacterData mData;

		public int linkedHelper = -1;

		private int spawnAsGoldenCounter = -1;

		private static int spawnRate = -1;

		public bool goldenAvailable;

		public CharacterData data
		{
			get
			{
				return mData;
			}
		}

		public HelperTypeData(string helperID, int level)
		{
			mData = new CharacterData(helperID, level);
		}

		public void Update()
		{
			data.currentCooldown = Mathf.Min(data.totalCooldown, data.currentCooldown + Time.deltaTime);
		}

		public void EngageCooldown()
		{
			data.currentCooldown = 0f;
		}

		public bool ShouldSpawnAsGolden()
		{
			if (!goldenAvailable)
			{
				return false;
			}
			if (spawnAsGoldenCounter == -1)
			{
				if (spawnRate == -1)
				{
					spawnRate = SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable("GoldenHelperSpawnRate", 0);
				}
				spawnAsGoldenCounter = Random.Range(1, spawnRate);
			}
			spawnAsGoldenCounter--;
			if (spawnAsGoldenCounter == 0)
			{
				spawnAsGoldenCounter = Mathf.Max(1, spawnRate + Random.Range(-2, 2));
				return true;
			}
			return false;
		}
	}

	public delegate int LeadershipModifierFunc(int oldLeadership, int modifier);

	protected List<HelperTypeData> mHelperTypes = new List<HelperTypeData>();

	protected CharactersManager mCharManagerRef;

	protected BoxCollider mHelperSpawnArea;

	private float mHelpersZTarget;

	protected LeadershipSchema mLeadershipData;

	protected List<string> mUniquesAlive = new List<string>();

	protected int mLevel;

	protected int mMaxLevel;

	protected int mExperience;

	protected float mIncreaseRate;

	protected float mMaxResources;

	protected float mResources;

	protected float mLevelUpThreshold;

	protected float mResourcesSpentOnHelpers;

	protected float mResourcesSpentOnUpgrades;

	protected bool mIsLeftToRightGameplay = true;

	protected Dictionary<string, HelperTypeData> mHelperDataCache = new Dictionary<string, HelperTypeData>();

	protected int mOwnerId;

	protected Material mGoldenHelperMaterial;

	public static string UdamanTable
	{
		get
		{
			return "Leadership";
		}
	}

	public int numTypes
	{
		get
		{
			return mHelperTypes.Count;
		}
	}

	public List<HelperTypeData> availableHelperTypes
	{
		get
		{
			return mHelperTypes;
		}
		private set
		{
		}
	}

	public CharactersManager characterManagerRef
	{
		get
		{
			return mCharManagerRef;
		}
		set
		{
			mCharManagerRef = value;
		}
	}

	public BoxCollider helperSpawnArea
	{
		get
		{
			return mHelperSpawnArea;
		}
		set
		{
			mHelperSpawnArea = value;
		}
	}

	public float helpersZTarget
	{
		get
		{
			return mHelpersZTarget;
		}
		set
		{
			mHelpersZTarget = value;
		}
	}

	public float maxResources
	{
		get
		{
			return mMaxResources;
		}
	}

	public float resources
	{
		get
		{
			return mResources;
		}
		set
		{
			mResources = Mathf.Clamp(value, 0f, mMaxResources);
		}
	}

	public int level
	{
		get
		{
			return mLevel;
		}
		set
		{
			SetLeadershipLevel(value);
		}
	}

	public int maxLevel
	{
		get
		{
			return mMaxLevel;
		}
	}

	public Hero hero { get; set; }

	public float levelUpThreshold
	{
		get
		{
			return mLevelUpThreshold;
		}
	}

	public bool isUpgradable
	{
		get
		{
			if (mLevel == mMaxLevel)
			{
				return false;
			}
			if (mResources < mLevelUpThreshold)
			{
				return false;
			}
			return true;
		}
	}

	public float ResourcesSpentOnHelpers
	{
		get
		{
			return mResourcesSpentOnHelpers;
		}
		set
		{
			mResourcesSpentOnHelpers = value;
		}
	}

	public float ResourcesSpentOnUpgrades
	{
		get
		{
			return mResourcesSpentOnUpgrades;
		}
		set
		{
			mResourcesSpentOnUpgrades = value;
		}
	}

	public bool CanDoRevolutionAchievement { get; set; }

	public Leadership()
	{
	}

	public Leadership(int playerIndex)
	{
		mOwnerId = playerIndex;
		if (playerIndex == 0)
		{
			SetUniqueInstance(this);
			mIsLeftToRightGameplay = Singleton<PlayModesManager>.Instance.gameDirection == PlayModesManager.GameDirection.LeftToRight;
			mLeadershipData = DataBundleUtils.InitializeRecord<LeadershipSchema>(new DataBundleRecordKey(UdamanTable, Singleton<Profile>.Instance.heroID));
			mMaxLevel = mLeadershipData.maxLevel;
			SetLeadershipLevel(Singleton<Profile>.Instance.initialLeadershipLevel);
			List<string> selectedHelpers = Singleton<Profile>.Instance.GetSelectedHelpers();
			foreach (string item2 in selectedHelpers)
			{
				HelperTypeData item = LoadHelperData(item2);
				mHelperTypes.Add(item);
			}
			CanDoRevolutionAchievement = mHelperTypes.Count == 2 && (mHelperTypes[0].data.id == "Farmer" || mHelperTypes[1].data.id == "Farmer") && (mHelperTypes[0].data.id == "Swordsmith" || mHelperTypes[1].data.id == "Swordsmith");
			int num = Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Banner");
			mResources = num * 10;
		}
		mResourcesSpentOnHelpers = 0f;
		mResourcesSpentOnUpgrades = 0f;
	}

	public void ApplyLeadershipCostBuff(float leadershipChange)
	{
		foreach (HelperTypeData mHelperType in mHelperTypes)
		{
			mHelperType.data.leadershipCost.leadership = Mathf.Max(mHelperType.data.leadershipCost.leadership - leadershipChange, 3f);
			WeakGlobalMonoBehavior<HUD>.Instance.ResetLeadershipCosts();
		}
	}

	public virtual void Update()
	{
		resources += Time.deltaTime * mIncreaseRate;
		foreach (HelperTypeData mHelperType in mHelperTypes)
		{
			mHelperType.Update();
		}
	}

	public float GetCoolDown(int typeIndex)
	{
		if (mHelperTypes[typeIndex].data.totalCooldown == 0f)
		{
			return 1f;
		}
		return mHelperTypes[typeIndex].data.currentCooldown / mHelperTypes[typeIndex].data.totalCooldown;
	}

	public void ResetCoolDown(int typeIndex)
	{
		mHelperTypes[typeIndex].data.currentCooldown = mHelperTypes[typeIndex].data.totalCooldown;
	}

	public string GetID(int typeIndex)
	{
		return mHelperTypes[typeIndex].data.id;
	}

	public Texture2D GetIconFile(int typeIndex)
	{
		return mHelperTypes[typeIndex].data.HUDIcon;
	}

	public Texture2D GetChampionIconFile(int typeIndex)
	{
		return mHelperTypes[typeIndex].data.championIcon;
	}

	public int GetCost(int typeIndex)
	{
		return (int)mHelperTypes[typeIndex].data.leadershipCost.leadership;
	}

	public bool IsAvailable(int typeIndex, out bool uniqueLimited)
	{
		uniqueLimited = false;
		if (mHelperTypes[typeIndex].data.unique)
		{
			string id = mHelperTypes[typeIndex].data.id;
			List<Character> allUniqueAllies = WeakGlobalInstance<CharactersManager>.Instance.allUniqueAllies;
			foreach (Character item in allUniqueAllies)
			{
				if (item.uniqueID == id && item.ownerId == mOwnerId)
				{
					uniqueLimited = true;
				}
			}
		}
		if (mHelperTypes[typeIndex].data.isMount && hero.health == 0f)
		{
			return false;
		}
		return GetCoolDown(typeIndex) == 1f && mHelperTypes[typeIndex].data.leadershipCost.canAfford(mOwnerId) && !uniqueLimited;
	}

	private void CheckForLeadershipCostBuff(HelperTypeData data, LeadershipModifierFunc modifierFunc)
	{
		bool flag = false;
		int leadershipCostModifierBuff = data.data.leadershipCostModifierBuff;
		if (leadershipCostModifierBuff != 0)
		{
			CanBuffFuncData canBuffFuncData = data.data.canBuffFuncData;
			foreach (HelperTypeData mHelperType in mHelperTypes)
			{
				CharacterData data2 = mHelperType.data;
				if ((canBuffFuncData == null || canBuffFuncData(data2)) && mHelperType != data)
				{
					data2.leadershipCost.leadership = modifierFunc((int)data2.leadershipCost.leadership, leadershipCostModifierBuff);
					flag = true;
				}
			}
		}
		if (flag)
		{
			WeakGlobalMonoBehavior<HUD>.Instance.ResetLeadershipCosts();
		}
	}

	public void CheckForLeadershipCostBuff(string helperID, LeadershipModifierFunc modifierFunc)
	{
		foreach (HelperTypeData mHelperType in mHelperTypes)
		{
			if (mHelperType.data.id == helperID)
			{
				CheckForLeadershipCostBuff(mHelperType, modifierFunc);
				break;
			}
		}
	}

	public Character Spawn(int typeIndex)
	{
		if (typeIndex < 0 || typeIndex >= mHelperTypes.Count)
		{
		}
		HelperTypeData helperTypeData = mHelperTypes[typeIndex];
		if (helperTypeData.data.unique)
		{
			bool flag = false;
			string id = mHelperTypes[typeIndex].data.id;
			List<Character> allUniqueAllies = WeakGlobalInstance<CharactersManager>.Instance.allUniqueAllies;
			foreach (Character item in allUniqueAllies)
			{
				if (item.uniqueID == id && item.ownerId == mOwnerId)
				{
					flag = true;
				}
			}
			if (flag)
			{
				return null;
			}
			RegisterUnique(helperTypeData.data.id, true);
		}
		helperTypeData.data.leadershipCost.Spend(mOwnerId);
		helperTypeData.EngageCooldown();
		mResourcesSpentOnHelpers += helperTypeData.data.leadershipCost.leadership;
		CheckForLeadershipCostBuff(helperTypeData, (int leadership, int modifier) => leadership + modifier);
		return SpawnHelper(helperTypeData, mHelperSpawnArea.size.x, mHelperSpawnArea.transform.position);
	}

	public int GetRandomHelperToSpawn()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < mHelperTypes.Count; i++)
		{
			if (!mHelperTypes[i].data.unique)
			{
				list.Add(i);
			}
		}
		if (list.Count == 0)
		{
			return -1;
		}
		return list[Random.Range(0, list.Count)];
	}

	public Character SpawnForFree(int typeIndex, float sizeOfSpawnArea, Vector3 spawnPos)
	{
		if (typeIndex < 0 || typeIndex >= mHelperTypes.Count)
		{
		}
		HelperTypeData data = mHelperTypes[typeIndex];
		return SpawnHelper(data, sizeOfSpawnArea, spawnPos);
	}

	public Character SpawnForFree(string helperID, float sizeOfSpawnArea, Vector3 spawnPos)
	{
		HelperTypeData data = LoadHelperData(helperID);
		return SpawnHelper(data, sizeOfSpawnArea, spawnPos);
	}

	public Character ForceSpawn(string helperID)
	{
		return SpawnHelper(LoadHelperData(helperID), mHelperSpawnArea.size.x, mHelperSpawnArea.transform.position);
	}

	public void ReplaceHelperWith(Helper h, string helperID)
	{
		if (h != null && !(h.health <= 0f))
		{
			int val = ((mOwnerId != 0) ? 1 : Singleton<Profile>.Instance.GetHelperLevel(helperID));
			if (mOwnerId == 0)
			{
				Singleton<Profile>.Instance.SetHelperLevel(helperID, Singleton<Profile>.Instance.GetHelperLevel(h.uniqueID), false);
			}
			else
			{
				Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.SetHelperLevel(helperID, Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.GetHelperLevel(h.uniqueID));
			}
			HelperTypeData data = LoadHelperData(helperID);
			h.ReinitializeModel(data.data);
			InitializeHelper(ref data, h, true);
			if (mOwnerId == 0)
			{
				Singleton<Profile>.Instance.SetHelperLevel(helperID, val, false);
			}
		}
	}

	public void LevelUp()
	{
		if (isUpgradable)
		{
			mResources -= mLevelUpThreshold;
			mResourcesSpentOnUpgrades += mLevelUpThreshold;
			SetLeadershipLevel(mLevel + 1);
		}
	}

	public void RegisterUnique(string id, bool isAlive)
	{
		if (isAlive)
		{
			if (!IsUniqueAlive(id))
			{
				mUniquesAlive.Add(id);
			}
		}
		else if (IsUniqueAlive(id))
		{
			mUniquesAlive.Remove(id);
		}
	}

	public virtual void UnitKilled()
	{
	}

	public virtual float GetPercentDoneWithWave()
	{
		return 0f;
	}

	protected HelperTypeData LoadHelperData(string helperID)
	{
		if (!mHelperDataCache.ContainsKey(helperID))
		{
			int num = 1;
			if (Singleton<HelpersDatabase>.Instance.Contains(helperID))
			{
				num = Singleton<Profile>.Instance.GetHelperLevel(helperID);
				if (mOwnerId > 0)
				{
					num = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.GetHelperLevel(helperID);
				}
			}
			HelperTypeData helperTypeData = new HelperTypeData(helperID, num);
			mHelperDataCache.Add(helperID, helperTypeData);
			List<string> spawnOnDeathTypes = helperTypeData.data.spawnOnDeathTypes;
			if (spawnOnDeathTypes != null)
			{
				foreach (string item in spawnOnDeathTypes)
				{
					LoadHelperData(item);
				}
			}
			if (!string.IsNullOrEmpty(helperTypeData.data.spawnFriendID))
			{
				LoadHelperData(helperTypeData.data.spawnFriendID);
			}
			if (helperTypeData.data.isMounted || helperTypeData.data.isMount)
			{
				int num2 = ((mOwnerId != 0) ? Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.horsesCollected : Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Horse"));
				float num3 = 1f;
				switch (num2)
				{
				case 1:
					num3 = 1.05f;
					break;
				case 2:
					num3 = 1.1f;
					break;
				case 3:
					num3 = 1.2f;
					break;
				}
				if (num3 > 1f)
				{
					helperTypeData.data.health *= num3;
					helperTypeData.data.meleeDamage *= num3;
					helperTypeData.data.bowDamage *= num3;
				}
			}
			if (mOwnerId != 0 && Singleton<Profile>.Instance.MultiplayerData.TweakValues != null)
			{
				helperTypeData.data.health *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.helperHealth;
				helperTypeData.data.meleeDamage *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.helperDamage;
				helperTypeData.data.bowDamage *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.helperDamage;
				helperTypeData.data.speedMax *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.helperMoveSpeed;
				helperTypeData.data.speedMin *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.helperMoveSpeed;
				helperTypeData.data.baseSpeedMax *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.helperMoveSpeed;
				helperTypeData.data.baseSpeedMin *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.helperMoveSpeed;
			}
			if (mOwnerId == 0)
			{
				helperTypeData.goldenAvailable = Singleton<Profile>.Instance.GetGoldenHelperUnlocked(helperID);
			}
			else
			{
				helperTypeData.goldenAvailable = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.IsGoldenHelperUnlocked(helperID);
			}
		}
		return mHelperDataCache[helperID];
	}

	public Character SpawnHelper(HelperTypeData data, float sizeOfSpawnArea, Vector3 spawnPos)
	{
		float num = Mathf.Max(data.data.swordAttackRange, data.data.bowAttackRange);
		if (data.data.id == "Rocky_Tman")
		{
			num = data.data.swordAttackRange;
		}
		num *= 0.5f;
		float zTarget = ((!mIsLeftToRightGameplay) ? (mHelpersZTarget + num) : (mHelpersZTarget - num));
		spawnPos.x = WeakGlobalInstance<CharactersManager>.Instance.GetBestSpawnXPos(spawnPos, sizeOfSpawnArea, data.data.lanePref, false, false, data.data.bowAttackRange > 0f);
		Character character = null;
		if (data.data.isEnemy)
		{
			Enemy enemy = new Enemy(data.data, zTarget, spawnPos, mOwnerId);
			character = enemy;
			enemy.SetupFromCharacterData(data.data);
		}
		else
		{
			Helper helper = new Helper(data.data, zTarget, spawnPos, mOwnerId);
			character = helper;
			if (data.data.spawnOnDeathTypes != null)
			{
				helper.SetSpawnOnDeath(data.data.spawnOnDeathTypes, data.data.spawnOnDeathNum);
			}
			if (mOwnerId == 0 && data.data.summonAchievementMask > 0)
			{
				Singleton<Achievements>.Instance.DoMaskAchievement("SummonAll", data.data.summonAchievementMask);
			}
		}
		InitializeHelper(ref data, character, false);
		if (character.isMount)
		{
			mCharManagerRef.AddMount(character, mOwnerId);
		}
		else
		{
			mCharManagerRef.AddCharacter(character);
		}
		if (mOwnerId != 0 && WeakGlobalMonoBehavior<InGameImpl>.Instance.HasPeaceCharm())
		{
			CharmSchema charmSchema = Singleton<CharmsDatabase>.Instance[WeakGlobalMonoBehavior<InGameImpl>.Instance.activeCharm];
			character.meleeDamage *= charmSchema.multiplier;
			character.bowDamage *= charmSchema.multiplier;
		}
		return character;
	}

	private void SwapMaterialsOnGameObjectAndChildren(GameObject obj, List<GameObject> objectsToNotMaterialSwitch, Material newMat)
	{
		if (!objectsToNotMaterialSwitch.Find((GameObject o) => obj == o))
		{
			Renderer renderer = obj.GetComponent<Renderer>();
			if (renderer != null)
			{
				Material material = Object.Instantiate(newMat) as Material;
				material.mainTexture = renderer.sharedMaterial.mainTexture;
				renderer.sharedMaterial = material;
			}
			Transform transform = obj.transform;
			for (int i = 0; i < transform.GetChildCount(); i++)
			{
				SwapMaterialsOnGameObjectAndChildren(transform.GetChild(i).gameObject, objectsToNotMaterialSwitch, newMat);
			}
		}
	}

	protected void InitializeHelper(ref HelperTypeData data, Character h, bool asReplacement)
	{
		h.id = data.data.id;
		float num = 1f;
		bool flag = false;
		if (asReplacement)
		{
			num = h.health / h.maxHealth;
			Helper helper = h as Helper;
			if (helper != null)
			{
				flag = helper.IsGolden;
			}
		}
		data.data.Setup(h);
		h.health *= num;
		if (data.ShouldSpawnAsGolden() || flag)
		{
			Helper helper2 = h as Helper;
			if (helper2 != null)
			{
				helper2.IsGolden = true;
			}
			HelperSchema helperSchema = Singleton<HelpersDatabase>.Instance[h.id];
			if (helperSchema.goldenHelperHealthMultiplier != 0f)
			{
				h.maxHealth *= helperSchema.goldenHelperHealthMultiplier;
				h.health *= helperSchema.goldenHelperHealthMultiplier;
			}
			if (helperSchema.goldenHelperDamageMultiplier != 0f)
			{
				h.meleeDamage *= helperSchema.goldenHelperDamageMultiplier;
				h.bowDamage *= helperSchema.goldenHelperDamageMultiplier;
			}
			if (helperSchema.goldenHelperSizeScale != 0f)
			{
				h.controlledObject.transform.localScale *= helperSchema.goldenHelperSizeScale;
			}
			if (mGoldenHelperMaterial == null)
			{
				SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource(SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable<string>("GoldenHelperMaterialPath"), 1);
				mGoldenHelperMaterial = cachedResource.Resource as Material;
			}
			GameObject controlledObject = h.controlledObject;
			List<GameObject> materialSwitchIgnoreObjects = h.controller.GetMaterialSwitchIgnoreObjects();
			SwapMaterialsOnGameObjectAndChildren(controlledObject, materialSwitchIgnoreObjects, mGoldenHelperMaterial);
			SharedResourceLoader.SharedResource cachedResource2 = ResourceCache.GetCachedResource(SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable<string>("GoldenHelperFXPath"), 1);
			if (cachedResource2 != null)
			{
				h.controller.SpawnEffectAtJoint(cachedResource2.Resource as GameObject, "body_effect", true);
			}
			if (!h.controlledObject.name.StartsWith("Golden"))
			{
				h.controlledObject.name = "Golden" + h.controlledObject.name;
			}
		}
		h.ActivateHealthBar();
		SharedResourceLoader.SharedResource cachedResource3 = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/Divine.prefab", 1);
		if (cachedResource3 != null)
		{
			h.controller.SpawnEffectAtJoint(cachedResource3.Resource as GameObject, "body_effect", false);
		}
	}

	protected void SetLeadershipLevel(int level)
	{
		mLevel = Mathf.Clamp(level, 0, mMaxLevel);
		switch (mLevel)
		{
		case 0:
			mIncreaseRate = mLeadershipData.resourcesPerSeconds0;
			mMaxResources = mLeadershipData.maxResource0;
			mLevelUpThreshold = mLeadershipData.levelUpThreshold0;
			break;
		case 1:
			mIncreaseRate = mLeadershipData.resourcesPerSeconds1;
			mMaxResources = mLeadershipData.maxResource1;
			mLevelUpThreshold = mLeadershipData.levelUpThreshold1;
			break;
		case 2:
			mIncreaseRate = mLeadershipData.resourcesPerSeconds2;
			mMaxResources = mLeadershipData.maxResource2;
			mLevelUpThreshold = mLeadershipData.levelUpThreshold2;
			break;
		case 3:
			mIncreaseRate = mLeadershipData.resourcesPerSeconds3;
			mMaxResources = mLeadershipData.maxResource3;
			mLevelUpThreshold = mLeadershipData.levelUpThreshold3;
			break;
		}
		if (mOwnerId != 0 && Singleton<Profile>.Instance.MultiplayerData.TweakValues != null)
		{
			mIncreaseRate *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.leadershipRate;
		}
	}

	protected bool IsUniqueAlive(string id)
	{
		return mUniquesAlive.Contains(id);
	}

	public void Clear()
	{
		mLeadershipData = null;
		mHelperDataCache.Clear();
		mHelperTypes.Clear();
	}
}
