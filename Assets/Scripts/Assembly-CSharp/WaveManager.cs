using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WaveManager : WeakGlobalInstance<WaveManager>
{
	private enum CommandType
	{
		Spawn = 0,
		Delay = 1,
		LegionTag = 2,
		UserDefined = 3,
		Num = 4,
		Unknown = 5
	}

	public enum WaveType
	{
		Wave_SinglePlayer = 0,
		Wave_Bonus = 1,
		Wave_Multiplayer = 2,
		Wave_DailyChallenge = 3
	}

	public class WaveRecyclingMultipliers
	{
		public float enemiesHealth = 1f;

		public float enemiesDamages = 1f;

		public float drops = 1f;
	}

	public delegate void OnLegionCallback();

	private const float kSpawnDelayTimerMin = 1f;

	private const float kSpawnDelayTimerMax = 4f;

	private const int kEnemiesMax = 10;

	private string specialBossName = string.Empty;

	private int mWaveIndex;

	private WaveType mWaveType;

	private BoxCollider mEnemiesSpawnArea;

	private float mZTarget;

	private WaveSchema waveRootData;

	private Dictionary<string, CharacterData> mCharacterDataPool = new Dictionary<string, CharacterData>();

	private DataBundleTableHandle<HelperEnemySwapSchema> mCorruptionSwapData;

	private DataBundleTableHandle<EnemySwapSchema> mDeathSwapData;

	private WaveRecyclingMultipliers mLevelMultipliers = new WaveRecyclingMultipliers();

	private int mNextCommandToRun;

	private float mSpawnDelayTimer;

	private bool mSkipNextLegion;

	private string mTutorial = string.Empty;

	private int mTotalNumEnemies;

	private int mSpawnedEnemiesSoFar;

	private int mEnemiesKilledSoFar;

	private List<string> mAllDifferentEnemies = new List<string>();

	private List<int> mLegionMarkers = new List<int>();

	private int mVillageArchersLevel;

	private int mBellLevel;

	public bool isDone
	{
		get
		{
			return mNextCommandToRun >= waveRootData.Commands.Length && specialBossName == string.Empty;
		}
	}

	public WaveSchema Data
	{
		get
		{
			return waveRootData;
		}
	}

	public DataBundleRecordKey WaveRecordKey { get; private set; }

	public int totalEnemies
	{
		get
		{
			return mTotalNumEnemies;
		}
		set
		{
			mTotalNumEnemies = value;
		}
	}

	public int enemiesKilledSoFar
	{
		get
		{
			return mEnemiesKilledSoFar;
		}
	}

	public List<string> allDifferentEnemies
	{
		get
		{
			return mAllDifferentEnemies;
		}
	}

	public bool skipNextLegion
	{
		get
		{
			return mSkipNextLegion;
		}
		set
		{
			mSkipNextLegion = value;
		}
	}

	public BoxCollider enemiesSpawnArea
	{
		get
		{
			return mEnemiesSpawnArea;
		}
	}

	public List<int> legionMarkers
	{
		get
		{
			return mLegionMarkers;
		}
	}

	public int waveLevel
	{
		get
		{
			return mWaveIndex;
		}
	}

	public string tutorial
	{
		get
		{
			return mTutorial;
		}
	}

	public WaveRecyclingMultipliers multipliers
	{
		get
		{
			return mLevelMultipliers;
		}
	}

	public int villageArchersLevel
	{
		get
		{
			return mVillageArchersLevel;
		}
	}

	public int bellLevel
	{
		get
		{
			return mBellLevel;
		}
	}

	public HelperEnemySwapSchema[] helperEnemySwaps
	{
		get
		{
			return mCorruptionSwapData.Data;
		}
		set
		{
		}
	}

	private GameObject corruptionSpawnEffect { get; set; }

	[method: MethodImpl(32)]
	public event OnLegionCallback onLegionStart;

	public WaveManager(WaveType waveType, int waveIndex, BoxCollider enemiesSpawnArea, float zTarget)
	{
		SetUniqueInstance(this);
		mWaveType = waveType;
		mWaveIndex = waveIndex;
		mEnemiesSpawnArea = enemiesSpawnArea;
		mZTarget = zTarget;
		LoadData();
		AnalyseWaveCommandsForStats();
		mNextCommandToRun = 0;
		mEnemiesKilledSoFar = 0;
		if (Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive())
		{
			int level = Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData.defensiveBuffs[0];
			string text = SpecialBossName(level);
			if (text != string.Empty)
			{
				Singleton<EnemiesDatabase>.Instance.LoadInGameData(text);
				allDifferentEnemies.Add(text);
			}
		}
		if (helperEnemySwaps.Length > 0)
		{
			corruptionSpawnEffect = ResourceCache.GetCachedResource("FX/Corrupt", 1).Resource as GameObject;
		}
	}

	public static string SpecialBossName(int level)
	{
		string result = string.Empty;
		switch (level)
		{
		case 1:
			result = "oni";
			break;
		case 2:
			result = "jurogumo";
			break;
		case 3:
			result = "Shogun_SvZ2";
			break;
		}
		return result;
	}

	public void Update()
	{
		UpdateDelayTimer();
	}

	public void registerEnemyKilled(string enemyID)
	{
		mEnemiesKilledSoFar++;
		Singleton<Profile>.Instance.IncNumKillsOfEnemyType(enemyID);
	}

	public void AddSpecialRewardsToCollectables()
	{
		if (waveRootData.Rewards != null && Singleton<Profile>.Instance.GetWaveLevel(mWaveIndex) == 1)
		{
			WeakGlobalInstance<CollectableManager>.Instance.GiveResource(ECollectableType.coin, waveRootData.Rewards.coins);
			WeakGlobalInstance<CollectableManager>.Instance.GiveResource(ECollectableType.gem, waveRootData.Rewards.gems);
			WeakGlobalInstance<CollectableManager>.Instance.GiveResource(ECollectableType.pachinkoball, waveRootData.Rewards.pachinkoBalls);
			WeakGlobalInstance<CollectableManager>.Instance.GiveResource(ECollectableType.soul, waveRootData.Rewards.souls);
			WeakGlobalInstance<CollectableManager>.Instance.GiveSpecificPresent("revivePotion", waveRootData.Rewards.revive);
			WeakGlobalInstance<CollectableManager>.Instance.GiveSpecificPresent("power", waveRootData.Rewards.power);
			WeakGlobalInstance<CollectableManager>.Instance.GiveSpecificPresent("luck", waveRootData.Rewards.luck);
			WeakGlobalInstance<CollectableManager>.Instance.GiveSpecificPresent("friendship", waveRootData.Rewards.friendship);
			WeakGlobalInstance<CollectableManager>.Instance.GiveSpecificPresent("wealth", waveRootData.Rewards.wealth);
			WeakGlobalInstance<CollectableManager>.Instance.GiveSpecificPresent("haste", waveRootData.Rewards.haste);
			WeakGlobalInstance<CollectableManager>.Instance.GiveSpecificPresent("peace", waveRootData.Rewards.peace);
			WeakGlobalInstance<CollectableManager>.Instance.GiveSpecificPresent("healthPotion", waveRootData.Rewards.sushi);
			WeakGlobalInstance<CollectableManager>.Instance.GiveSpecificPresent("leadershipPotion", waveRootData.Rewards.tea);
		}
	}

	public static void LoadSceneForWave()
	{
		Singleton<Profile>.Instance.ClearBonusWaveData();
		WaveSchema waveData = GetWaveData(Singleton<Profile>.Instance.waveToPlay, Singleton<Profile>.Instance.waveTypeToPlay);
		LoadingScreen.LoadLevel(waveData.scene);
	}

	public WaveSchema GetWaveData()
	{
		return GetWaveData(mWaveIndex, mWaveType);
	}

	public static WaveSchema GetWaveData(DataBundleRecordKey recordKey, SeededRandomizer randomizer)
	{
		return WaveSchema.Initialize(recordKey, randomizer);
	}

	public static WaveSchema GetWaveData(int waveID, WaveType waveType)
	{
		switch (waveType)
		{
		case WaveType.Wave_Multiplayer:
		{
			string recordTable = Singleton<PlayModesManager>.Instance.selectedModeData.waves.RecordTable;
			WaveSchema waveSchema2 = WaveSchema.Initialize(DataBundleRuntime.TableRecordKey(recordTable, waveID.ToString()));
			if (waveSchema2 == null)
			{
			}
			return waveSchema2;
		}
		case WaveType.Wave_DailyChallenge:
		{
			ProceduralWaveSchema dailyChallengeProceduralWaveSchema = Singleton<Profile>.Instance.dailyChallengeProceduralWaveSchema;
			WaveSchema cachedWaveSchema = dailyChallengeProceduralWaveSchema.CachedWaveSchema;
			if (cachedWaveSchema == null)
			{
			}
			return cachedWaveSchema;
		}
		default:
		{
			int num = waveID;
			while (num > 0)
			{
				WaveSchema waveSchema = null;
				switch (waveType)
				{
				case WaveType.Wave_SinglePlayer:
				{
					PlayModeSchema selectedModeData = Singleton<PlayModesManager>.Instance.selectedModeData;
					if (selectedModeData.maxBaseWave >= num)
					{
						waveSchema = WaveSchema.Initialize(DataBundleRuntime.TableRecordKey(selectedModeData.waves.RecordTable, num.ToString()));
						break;
					}
					SeededRandomizer randomizer = new SeededRandomizer(num);
					if (num % selectedModeData.bonusWaveInterval == 0)
					{
						int num2 = num / selectedModeData.bonusWaveInterval % DataBundleRuntime.Instance.GetRecordTableLength(typeof(WaveSchema), selectedModeData.endlessBonusWaves) + 1;
						waveSchema = WaveSchema.Initialize(DataBundleRuntime.TableRecordKey(selectedModeData.endlessBonusWaves.RecordTable, num2.ToString()), randomizer);
					}
					else
					{
						int num3 = (num - 1) % DataBundleRuntime.Instance.GetRecordTableLength(typeof(WaveSchema), selectedModeData.endlessWaves) + 1;
						waveSchema = WaveSchema.Initialize(DataBundleRuntime.TableRecordKey(selectedModeData.endlessWaves.RecordTable, num3.ToString()), randomizer);
					}
					break;
				}
				}
				if (waveSchema == null)
				{
					num--;
					continue;
				}
				return waveSchema;
			}
			return null;
		}
		}
	}

	public DataBundleTableHandle<HelperEnemySwapSchema> GetHelperSwapData()
	{
		if (mCorruptionSwapData == null)
		{
			mCorruptionSwapData = new DataBundleTableHandle<HelperEnemySwapSchema>("CorruptionSwap");
		}
		return mCorruptionSwapData;
	}

	public DataBundleTableHandle<EnemySwapSchema> GetDeathSwapData()
	{
		if (mDeathSwapData == null)
		{
			mDeathSwapData = new DataBundleTableHandle<EnemySwapSchema>("OnDeathSwaps");
		}
		return mDeathSwapData;
	}

	public void ReplaceHelperWithEnemy(Helper h)
	{
		if (h == null || h.health <= 0f)
		{
			return;
		}
		string text = null;
		HelperEnemySwapSchema[] data = mCorruptionSwapData.Data;
		foreach (HelperEnemySwapSchema helperEnemySwapSchema in data)
		{
			if (helperEnemySwapSchema.helperSwapFrom.Key == h.id)
			{
				text = helperEnemySwapSchema.enemySwapTo.Key;
				break;
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			Enemy c = ConstructEnemyWithEffect(text, 0f, h.transform.position, true, corruptionSpawnEffect);
			WeakGlobalInstance<CharactersManager>.Instance.AddCharacter(c);
			WeakGlobalInstance<CharactersManager>.Instance.DestroyCharacter(h);
		}
	}

	private void LoadData()
	{
		waveRootData = GetWaveData();
		string recordTable = Singleton<PlayModesManager>.Instance.selectedModeData.waves.RecordTable;
		WaveRecordKey = DataBundleRuntime.TableRecordKey(recordTable, waveRootData.index.ToString());
		Singleton<PlayStatistics>.Instance.data.shouldShowRateMeDialog = waveRootData.showRateMe;
		Singleton<PlayStatistics>.Instance.data.shouldAwardMysteryBox = waveRootData.awardMysteryBox;
		GetHelperSwapData();
		mTutorial = waveRootData.tutorial;
		mVillageArchersLevel = waveRootData.villageArchers;
		mBellLevel = waveRootData.bell;
		LoadLevelMultipliers();
	}

	private void LoadLevelMultipliers()
	{
		if (Singleton<Profile>.Instance.inDailyChallenge || Singleton<Profile>.Instance.inMultiplayerWave)
		{
			return;
		}
		PlayModeSchema selectedModeData = Singleton<PlayModesManager>.Instance.selectedModeData;
		if (mWaveIndex <= selectedModeData.maxBaseWave)
		{
			int num = Singleton<Profile>.Instance.GetWaveLevel(mWaveIndex);
			if (num >= 2)
			{
				float @float = Singleton<Config>.Instance.data.GetFloat(TextDBSchema.ChildKey("waveLevelMultipliers", "enemiesHealth"));
				float float2 = Singleton<Config>.Instance.data.GetFloat(TextDBSchema.ChildKey("waveLevelMultipliers", "enemiesDamages"));
				float float3 = Singleton<Config>.Instance.data.GetFloat(TextDBSchema.ChildKey("waveLevelMultipliers", "drops"));
				mLevelMultipliers.enemiesHealth = CalcMultiplier(@float, num - 1);
				mLevelMultipliers.enemiesDamages = CalcMultiplier(float2, num - 1);
				mLevelMultipliers.drops = CalcMultiplier(float3, num - 1);
			}
		}
		else
		{
			int times = mWaveIndex - selectedModeData.maxBaseWave;
			float float4 = Singleton<Config>.Instance.data.GetFloat(TextDBSchema.ChildKey("waveLevelMultipliersEndless", "enemiesHealth"));
			float float5 = Singleton<Config>.Instance.data.GetFloat(TextDBSchema.ChildKey("waveLevelMultipliersEndless", "enemiesDamages"));
			float float6 = Singleton<Config>.Instance.data.GetFloat(TextDBSchema.ChildKey("waveLevelMultipliersEndless", "drops"));
			mLevelMultipliers.enemiesHealth = CalcMultiplier(float4, times);
			mLevelMultipliers.enemiesDamages = CalcMultiplier(float5, times);
			mLevelMultipliers.drops = CalcMultiplier(float6, times);
		}
	}

	private void RunNextCommand()
	{
		if (mNextCommandToRun >= waveRootData.Commands.Length)
		{
			if (specialBossName != string.Empty)
			{
				WeakGlobalInstance<CharactersManager>.Instance.AddCharacter(ConstructEnemy(specialBossName));
				specialBossName = string.Empty;
				WeakGlobalMonoBehavior<BannerManager>.Instance.OpenBanner(new BannerBoss(5f * WeakGlobalMonoBehavior<InGameImpl>.Instance.timeScalar));
			}
			return;
		}
		string command = GetCommand(mNextCommandToRun);
		mNextCommandToRun++;
		switch (GetCommandType(command))
		{
		case CommandType.UserDefined:
			WeakGlobalMonoBehavior<InGameImpl>.Instance.RunSpecialWaveCommand(command.ToLower());
			break;
		case CommandType.Delay:
		{
			KeyValuePair<float, float> keyValuePair = ExtractTimer(command);
			mSpawnDelayTimer += UnityEngine.Random.value * (keyValuePair.Value - keyValuePair.Key) + keyValuePair.Key;
			break;
		}
		case CommandType.Spawn:
		{
			bool flag = false;
			foreach (string mAllDifferentEnemy in mAllDifferentEnemies)
			{
				if (string.Compare(mAllDifferentEnemy, command, true) == 0)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				WeakGlobalInstance<CharactersManager>.Instance.AddCharacter(ConstructEnemy(command));
			}
			break;
		}
		case CommandType.LegionTag:
			if (command[0] != '(')
			{
				break;
			}
			if (mSkipNextLegion)
			{
				SkipToEndOfLegion();
				mSkipNextLegion = false;
			}
			else if (mSpawnedEnemiesSoFar == mEnemiesKilledSoFar)
			{
				WeakGlobalMonoBehavior<InGameImpl>.Instance.RunSpecialWaveCommand("@legionalert");
				if (this.onLegionStart != null)
				{
					this.onLegionStart();
				}
			}
			else
			{
				mNextCommandToRun--;
			}
			break;
		}
	}

	public void SpawnRandomEnemy(Vector3 position, float spawnRange)
	{
		string cmd = mAllDifferentEnemies[UnityEngine.Random.Range(0, mAllDifferentEnemies.Count - 1)];
		SpawnExtraEnemy(cmd, position, spawnRange);
	}

	public void SpawnLegendaryStrikeEnemy(Vector3 position, float spawnRange)
	{
		if (WeakGlobalMonoBehavior<InGameImpl>.Instance.LegendaryStrikeEnemies != null)
		{
			string cmd = WeakGlobalMonoBehavior<InGameImpl>.Instance.LegendaryStrikeEnemies[UnityEngine.Random.Range(0, WeakGlobalMonoBehavior<InGameImpl>.Instance.LegendaryStrikeEnemies.Count)];
			SpawnExtraEnemy(cmd, position, spawnRange);
		}
		else
		{
			SpawnRandomEnemy(position, spawnRange);
		}
	}

	public void SpawnExtraEnemy(string cmd, Vector3 position, float spawnRange)
	{
		bool flag = false;
		foreach (string mAllDifferentEnemy in mAllDifferentEnemies)
		{
			if (string.Compare(mAllDifferentEnemy, cmd, true) == 0)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			WeakGlobalInstance<CharactersManager>.Instance.AddCharacter(ConstructEnemy(cmd, spawnRange, position, true));
		}
	}

	private string GetCommand(int index)
	{
		return (!DataBundleRecordKey.IsNullOrEmpty(waveRootData.Commands[index].enemy)) ? waveRootData.Commands[index].enemy.Key : waveRootData.Commands[index].command;
	}

	private void SkipToEndOfLegion()
	{
		while (mNextCommandToRun < waveRootData.Commands.Length)
		{
			string command = GetCommand(mNextCommandToRun);
			CommandType commandType = GetCommandType(command);
			mNextCommandToRun++;
			switch (commandType)
			{
			case CommandType.Spawn:
				mEnemiesKilledSoFar++;
				break;
			case CommandType.LegionTag:
				if (command[0] == ')')
				{
					return;
				}
				break;
			}
		}
	}

	private KeyValuePair<float, float> ExtractTimer(string cmd)
	{
		string[] array = cmd.Split(',');
		if (array.Length != 2)
		{
			return new KeyValuePair<float, float>(-1f, -1f);
		}
		return new KeyValuePair<float, float>(float.Parse(array[0]), float.Parse(array[1]));
	}

	private void UpdateDelayTimer()
	{
		if (isDone)
		{
			return;
		}
		mSpawnDelayTimer -= Time.deltaTime;
		if (!(mSpawnDelayTimer > 0f))
		{
			mSpawnDelayTimer = 0f;
			if (WeakGlobalInstance<CharactersManager>.Instance.enemiesCount < 10)
			{
				RunNextCommand();
			}
		}
	}

	public Enemy ConstructEnemy(string enemyID)
	{
		if (!Singleton<EnemiesDatabase>.Instance.Contains(enemyID))
		{
			return null;
		}
		return ConstructEnemyWithEffect(enemyID, mEnemiesSpawnArea.size.x, mEnemiesSpawnArea.transform.position, false, null);
	}

	public Enemy ConstructEnemy(string enemyID, float sizeOfSpawnArea, Vector3 spawnPos, bool dynamicSpawn)
	{
		if (!Singleton<EnemiesDatabase>.Instance.Contains(enemyID))
		{
			return null;
		}
		return ConstructEnemyWithEffect(enemyID, sizeOfSpawnArea, spawnPos, dynamicSpawn, null);
	}

	private Enemy ConstructEnemyWithEffect(string enemyID, float sizeOfSpawnArea, Vector3 spawnPos, bool dynamicSpawn, GameObject effectToSpawn)
	{
		return SpawnEnemy(enemyID, GetCharacterData(enemyID), sizeOfSpawnArea, spawnPos, dynamicSpawn, effectToSpawn);
	}

	private Enemy SpawnEnemy(string uniqueID, CharacterData data, float sizeOfSpawnArea, Vector3 spawnPos, bool dynamicSpawn, GameObject effectToUse)
	{
		spawnPos.x = WeakGlobalInstance<CharactersManager>.Instance.GetBestSpawnXPos(spawnPos, sizeOfSpawnArea, data.lanePref, true, data.isFlying, data.bowAttackRange > 0f);
		Enemy enemy = new Enemy(data, mZTarget, spawnPos, 1);
		data.Setup(enemy);
		enemy.maxHealth *= mLevelMultipliers.enemiesHealth;
		enemy.health = enemy.maxHealth;
		enemy.meleeDamage *= mLevelMultipliers.enemiesDamages;
		enemy.bowDamage *= mLevelMultipliers.enemiesDamages;
		if (WeakGlobalMonoBehavior<InGameImpl>.Instance.HasPeaceCharm())
		{
			CharmSchema charmSchema = Singleton<CharmsDatabase>.Instance[WeakGlobalMonoBehavior<InGameImpl>.Instance.activeCharm];
			enemy.meleeDamage *= charmSchema.multiplier;
			enemy.bowDamage *= charmSchema.multiplier;
		}
		enemy.SetupFromCharacterData(data);
		if (data.resourceDropAlways != string.Empty)
		{
			Character selfPtr = enemy;
			string resourceID = data.resourceDropAlways;
			enemy.onDeathEvent = (Action)Delegate.Combine(enemy.onDeathEvent, (Action)delegate
			{
				SpawnExtraCollectable(resourceID, selfPtr);
			});
		}
		if (effectToUse != null)
		{
			enemy.controller.SpawnEffectAtJoint(effectToUse, "body_effect", false);
		}
		else
		{
			SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/DivineEnemy.prefab", 1);
			if (cachedResource != null)
			{
				enemy.controller.SpawnEffectAtJoint(cachedResource.Resource as GameObject, "body_effect", false);
			}
		}
		if (dynamicSpawn)
		{
			enemy.dynamicSpawn = true;
		}
		else
		{
			mSpawnedEnemiesSoFar++;
		}
		return enemy;
	}

	private void SpawnExtraCollectable(string collectableType, Character theEnemy)
	{
		if (theEnemy != null)
		{
			WeakGlobalInstance<CollectableManager>.Instance.ForceSpawnResourceType(collectableType, theEnemy.position);
		}
	}

	private void AnalyseWaveCommandsForStats()
	{
		mTotalNumEnemies = ((Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive() && Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData.defensiveBuffs[0] > 0) ? 1 : 0);
		mAllDifferentEnemies.Clear();
		for (int i = 0; i < waveRootData.Commands.Length; i++)
		{
			string data = GetCommand(i);
			switch (GetCommandType(data))
			{
			case CommandType.Spawn:
				mTotalNumEnemies++;
				if (Singleton<EnemiesDatabase>.Instance.Contains(data) && !mAllDifferentEnemies.Exists((string element) => element == data))
				{
					mAllDifferentEnemies.Add(data);
				}
				break;
			case CommandType.LegionTag:
				if (data[0] == '(')
				{
					mLegionMarkers.Add(mTotalNumEnemies);
				}
				break;
			}
		}
	}

	private CommandType GetCommandType(string cmd)
	{
		if (cmd.Length == 0)
		{
			return CommandType.Unknown;
		}
		if (cmd[0] == '@')
		{
			return CommandType.UserDefined;
		}
		if (cmd[0] == '(' || cmd[0] == ')')
		{
			return CommandType.LegionTag;
		}
		if (ExtractTimer(cmd).Key != -1f)
		{
			return CommandType.Delay;
		}
		return CommandType.Spawn;
	}

	private CharacterData GetCharacterData(string id)
	{
		if (!mCharacterDataPool.ContainsKey(id))
		{
			mCharacterDataPool.Add(id, new CharacterData(id, 1));
		}
		return mCharacterDataPool[id];
	}

	public bool HasCorruptionEnemy()
	{
		return allDifferentEnemies.Find((string s) => s.Contains("Amanojaku")) != null;
	}

	public void UnloadData()
	{
		mCharacterDataPool.Clear();
		mCorruptionSwapData.Unload();
		waveRootData = null;
	}

	private static float CalcMultiplier(float mult, int times)
	{
		float num = mult;
		num -= 1f;
		num *= (float)times;
		return num + 1f;
	}
}
