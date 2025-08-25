using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameImpl : WeakGlobalMonoBehavior<InGameImpl>
{
	private const int kMaxHelpers = 5;

	private const float kDialogPriority = 500f;

	private const float kWinPriority = 1000f;

	private const float kSlowMoEffectStartScale = 0.1f;

	private const float kSlowMoEffectIncrement = 0.8f;

	private const float kCameraZoomInOffset = 2f;

	private const float kCameraZoomOutSpeed = 0.4f;

	private const float kCameraMoveUpSpeed = 0.25f;

	private const float kMinTimeBetweenTutorials = 8f;

	private const int kWaveToShowEndGameNarrative = 70;

	public static readonly int kMaxPlayers = 2;

	public Transform heroSpawnPointLeft;

	public Transform heroSpawnPointRight;

	public GameObject enemiesTargetLeft;

	public GameObject vortexLeft;

	public GameObject vortexRight;

	public GameObject gateSparklesLeft;

	public GameObject enemiesTargetRight;

	public Camera gameCamera;

	public Transform heroWalkLeftEdge;

	public Transform heroWalkRightEdge;

	public BoxCollider enemiesSpawnAreaRight;

	public BoxCollider helpersSpawnAreaLeft;

	public GameObject bellRingerLocationObject;

	public GameObject bellLocationObject;

	public GameObject bellObject;

	public Transform[] villageArcher = new Transform[3];

	public Transform bannerLocObject;

	public BoxCollider pitArea;

	private float mTimeToNextTutorial;

	private float mHitEffectTimer;

	private float mHitEffectFrequency;

	private string mFriendshipHelperID;

	private BuffIconManager mBuffIconManager = new BuffIconManager();

	private Hero[] mHero = new Hero[kMaxPlayers];

	private Gate[] mGate = new Gate[kMaxPlayers];

	private Bell mBell;

	private Pit mPit;

	private CharactersManager mCharactersManager;

	private CollectableManager mCollectableManager;

	private ProjectileManager mProjectileManager;

	private WaveManager mWaveManager;

	private ProceduralShaderManager mShaderManager;

	private Leadership[] mLeadership = new Leadership[kMaxPlayers];

	private VillageArchers mVillageArchers;

	private float mTimeStarted;

	private List<string> mLegendaryStrikeEnemies = new List<string>();

	private RailManager mRailManager;

	private bool mGameOver;

	private float mAllAlliesInvincibleTimer;

	private float mCameraYOffset = 100f;

	private float mBaseTimeScale = 1f;

	private bool mStartedSlowMoFinisher;

	private float mGameTimer;

	private bool mReviveOfferDismissed;

	private PlayModesManager.GameDirection mGameDirection;

	private string mTagHeroID;

	private string mTagAbilityID;

	public Transform heroSpawnPoint
	{
		get
		{
			if (mGameDirection == PlayModesManager.GameDirection.RightToLeft)
			{
				return heroSpawnPointRight;
			}
			return heroSpawnPointLeft;
		}
	}

	public Transform enemySpawnPoint
	{
		get
		{
			if (mGameDirection == PlayModesManager.GameDirection.RightToLeft)
			{
				return heroSpawnPointLeft;
			}
			return heroSpawnPointRight;
		}
	}

	public GameObject enemiesTarget
	{
		get
		{
			if (mGameDirection == PlayModesManager.GameDirection.RightToLeft)
			{
				return enemiesTargetRight;
			}
			return enemiesTargetLeft;
		}
	}

	public GameObject heroTarget
	{
		get
		{
			if (mGameDirection == PlayModesManager.GameDirection.RightToLeft)
			{
				return enemiesTargetLeft;
			}
			return enemiesTargetRight;
		}
	}

	public BoxCollider enemiesSpawnArea
	{
		get
		{
			if (mGameDirection == PlayModesManager.GameDirection.RightToLeft)
			{
				return helpersSpawnAreaLeft;
			}
			return enemiesSpawnAreaRight;
		}
	}

	public BoxCollider helpersSpawnArea
	{
		get
		{
			if (mGameDirection == PlayModesManager.GameDirection.RightToLeft)
			{
				return enemiesSpawnAreaRight;
			}
			return helpersSpawnAreaLeft;
		}
	}

	public bool gameOver
	{
		get
		{
			return mGameOver;
		}
	}

	public bool playerWon
	{
		get
		{
			return mGameOver && hero.health > 0f;
		}
	}

	public bool enemiesWon
	{
		get
		{
			return mGameOver && hero.health <= 0f;
		}
	}

	public bool useSlowMoFinisher
	{
		get
		{
			if (mStartedSlowMoFinisher)
			{
				return true;
			}
			if (hero != null && (hero.controller.currentAnimation == "attack" || hero.controller.currentAnimation == "katanaslash"))
			{
				return true;
			}
			return false;
		}
	}

	public Hero hero
	{
		get
		{
			return mHero[0];
		}
	}

	public Hero enemyHero
	{
		get
		{
			return mHero[1];
		}
	}

	public Gate gate
	{
		get
		{
			return (mGate[0] == null) ? mGate[1] : mGate[0];
		}
	}

	public Gate enemyGate
	{
		get
		{
			return mGate[1];
		}
	}

	public string activeCharm { get; private set; }

	public float timeScalar
	{
		get
		{
			return (!useSlowMoFinisher) ? 1f : 0.8f;
		}
		set
		{
		}
	}

	public float TimeSinceStart
	{
		get
		{
			return Time.time - mTimeStarted;
		}
	}

	public float allAlliesInvincibleTimer
	{
		get
		{
			return mAllAlliesInvincibleTimer;
		}
		set
		{
			mAllAlliesInvincibleTimer = value;
		}
	}

	private GameObject fireworksResource { get; set; }

	public bool gamePaused
	{
		get
		{
			return Time.timeScale == 0f;
		}
		set
		{
			if (value != (Time.timeScale == 0f))
			{
				SingletonSpawningMonoBehaviour<USoundThemeManager>.Instance.PauseSoundThemes(value);
				if (value)
				{
					Time.timeScale = 0f;
				}
				else
				{
					Time.timeScale = mBaseTimeScale;
				}
				if (WeakGlobalMonoBehavior<HUD>.Exists)
				{
					WeakGlobalMonoBehavior<HUD>.Instance.HandleGamePause(value);
				}
			}
		}
	}

	public string FriendshipHelperID
	{
		get
		{
			if (mFriendshipHelperID == null)
			{
				CharmSchema charmSchema = Singleton<CharmsDatabase>.Instance[activeCharm];
				if (charmSchema == null || charmSchema.id != "friendship")
				{
					mFriendshipHelperID = string.Empty;
				}
				else
				{
					string table = "FriendshipCharmHelpers";
					DataBundleRecordTable dataBundleRecordTable = new DataBundleRecordTable(table);
					HelperListSchema[] array = dataBundleRecordTable.InitializeRecords<HelperListSchema>();
					int num = UnityEngine.Random.Range(0, array.Length);
					DataBundleRecordKey dataBundleRecordKey = new DataBundleRecordKey(array[num].ability);
					mFriendshipHelperID = DataBundleRuntime.RecordKey(dataBundleRecordKey);
				}
			}
			return mFriendshipHelperID;
		}
		set
		{
		}
	}

	public Pit Pit
	{
		get
		{
			return mPit;
		}
		set
		{
		}
	}

	public CharactersManager CharacterMgr
	{
		get
		{
			return mCharactersManager;
		}
	}

	public List<string> LegendaryStrikeEnemies
	{
		get
		{
			return mLegendaryStrikeEnemies;
		}
	}

	public float GameTimer
	{
		get
		{
			return mGameTimer;
		}
	}

	public bool CanDoRevolutionAchievement { get; set; }

	public string TagHeroID
	{
		get
		{
			return mTagHeroID;
		}
	}

	public string TagAbilityID
	{
		get
		{
			return mTagAbilityID;
		}
	}

	private void Awake()
	{
		SetUniqueInstance(this);
		if (!Singleton<Profile>.Exists)
		{
			StartCoroutine(Singleton<Profile>.Instance.Init());
		}
		SingletonSpawningMonoBehaviour<USoundThemeManager>.Instance.SetResourceLevel(1);
	}

	private IEnumerator Start()
	{
		while (!Singleton<Profile>.Instance.Initialized)
		{
			yield return null;
		}
		ApplicationUtilities._allowAutoSave = true;
		ResourceCache.DefaultCacheLevel = 1;
		Singleton<Achievements>.Instance.SuppressPartialReporting(true);
		fireworksResource = ResourceCache.GetCachedResource("FX/Fireworks", 1).Resource as GameObject;
		int currentWave = Singleton<Profile>.Instance.wave_SinglePlayerGame;
		if (!Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive() && !Singleton<Profile>.Instance.inDailyChallenge)
		{
			if (currentWave == 2 && Singleton<Profile>.Instance.GetWaveLevel(2) == 1)
			{
				Singleton<Profile>.Instance.SetSelectedHelpers(new List<string>(new string[1] { "Farmer" }));
				Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep17_StartWave2");
			}
			else if (currentWave == 1 && Singleton<Profile>.Instance.GetWaveLevel(1) == 1)
			{
				Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep3_StartWave1");
				ApplicationUtilities.MakePlayHavenContentRequest("tutorial_start");
			}
		}
		LoadingScreen.LogStep("InGame Start BEGIN");
		List<string> helpersToLoad = new List<string>();
		List<string> abilitiesToLoad = new List<string>();
		foreach (string ability in Singleton<Profile>.Instance.GetSelectedAbilities())
		{
			abilitiesToLoad.Add(ability);
		}
		activeCharm = Singleton<Profile>.Instance.selectedCharm;
		if (!string.IsNullOrEmpty(activeCharm))
		{
			CharmSchema charm = Singleton<CharmsDatabase>.Instance[activeCharm];
			string helperName = FriendshipHelperID;
			string abilityName = DataBundleRuntime.RecordKey(charm.abilityToActivate.Key.ToString());
			if (!string.IsNullOrEmpty(helperName) || !string.IsNullOrEmpty(abilityName))
			{
				List<string> currentAbilities = Singleton<Profile>.Instance.GetSelectedAbilities();
				if (!string.IsNullOrEmpty(abilityName))
				{
					currentAbilities.Add(abilityName);
				}
				else
				{
					currentAbilities.Add("ActiveCharm");
				}
				Singleton<Profile>.Instance.SetSelectedAbilities(currentAbilities);
				if (!string.IsNullOrEmpty(helperName) && !helpersToLoad.Contains(helperName))
				{
					helpersToLoad.Add(helperName);
				}
				if (!string.IsNullOrEmpty(abilityName) && !abilitiesToLoad.Contains(abilityName))
				{
					abilitiesToLoad.Add(abilityName);
				}
			}
		}
		if (Singleton<Profile>.Instance.GetSelectedAbilities().Contains("DivineIntervention") && Singleton<Profile>.Instance.GetSelectedHelpers().FindIndex((string s) => !Singleton<HelpersDatabase>.Instance[s].unique) < 0)
		{
			helpersToLoad.Add("Farmer");
		}
		foreach (string helper in Singleton<Profile>.Instance.GetSelectedHelpers())
		{
			if (!helpersToLoad.Contains(helper))
			{
				helpersToLoad.Add(helper);
			}
		}
		if (Singleton<Profile>.Instance.inVSMultiplayerWave)
		{
			foreach (string ability2 in Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.abilityIdList)
			{
				if (!string.IsNullOrEmpty(ability2) && abilitiesToLoad.Contains(ability2))
				{
					abilitiesToLoad.Add(ability2);
				}
			}
			string[] aiHelpers = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.GetSelectedHelperIDs();
			string[] array = aiHelpers;
			foreach (string helper2 in array)
			{
				if (!string.IsNullOrEmpty(helper2) && !helpersToLoad.Contains(helper2))
				{
					helpersToLoad.Add(helper2);
				}
			}
		}
		Singleton<CharmsDatabase>.Instance.LoadInGameData(Singleton<Profile>.Instance.selectedCharm, true);
		LoadingScreen.LogStep("CharmsDatabase.Instance.LoadInGameData");
		if (Singleton<Profile>.Instance.selectedCharm == "TagTeam")
		{
			List<string> possibleTag = new List<string>();
			string[] tagHeroes = new string[3] { "HeroBalanced", "HeroAttack", "HeroDefense" };
			string[] array2 = tagHeroes;
			foreach (string hero in array2)
			{
				if (Singleton<Profile>.Instance.heroID != hero && (!Singleton<Profile>.Instance.inMultiplayerWave || Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.heroId != hero))
				{
					possibleTag.Add(hero);
				}
			}
			mTagHeroID = possibleTag[UnityEngine.Random.Range(0, possibleTag.Count)];
			Singleton<HeroesDatabase>.Instance.LoadInGameData(mTagHeroID, 0);
			switch (mTagHeroID)
			{
			case "HeroBalanced":
				mTagAbilityID = "SummonLightning";
				break;
			case "HeroAttack":
				mTagAbilityID = "DaggerBarrage";
				break;
			case "HeroDefense":
				mTagAbilityID = "DivineWind";
				break;
			}
			if (!abilitiesToLoad.Contains(mTagAbilityID))
			{
				abilitiesToLoad.Add(mTagAbilityID);
			}
			Vector3 spawnPos = new Vector3(0f, -100f, 0f);
			Hero tagHero = new Hero(spawnPos, 0, TagHeroID, false);
			UnityEngine.Object.Destroy(tagHero.controlledObject);
			tagHero.Destroy();
		}
		Singleton<AbilitiesDatabase>.Instance.LoadInGameData(abilitiesToLoad, true);
		LoadingScreen.LogStep("AbilitiesDatabase.Instance.LoadInGameData");
		List<string> adhocHelpers = new List<string>();
		Singleton<HelpersDatabase>.Instance.LoadInGameData(helpersToLoad, adhocHelpers);
		LoadingScreen.LogStep("HelpersDatabase.Instance.LoadInGameData");
		Singleton<PotionsDatabase>.Instance.LoadInGameData();
		LoadingScreen.LogStep("PotionsDatabase.Instance.LoadInGameData");
		Singleton<PlayStatistics>.Instance.Reset();
		Singleton<PlayStatistics>.Instance.data.wavePlayed = Singleton<Profile>.Instance.waveToPlay;
		Singleton<PlayStatistics>.Instance.data.wavePlayedLevel = Singleton<Profile>.Instance.GetWaveLevel(Singleton<Profile>.Instance.waveToPlay);
		Singleton<PlayStatistics>.Instance.data.waveTypePlayed = Singleton<Profile>.Instance.waveTypeToPlay;
		Profile.UpdatePlayMode();
		mGameDirection = Singleton<PlayModesManager>.Instance.gameDirection;
		HideUnusedGate();
		mRailManager = new RailManager();
		mProjectileManager = new ProjectileManager();
		List<string> allEnemyKeys = new List<string>();
		if (!Singleton<Profile>.Instance.inVSMultiplayerWave)
		{
			mWaveManager = new WaveManager(Singleton<Profile>.Instance.waveTypeToPlay, Singleton<Profile>.Instance.waveToPlay, enemiesSpawnArea, enemiesTarget.transform.position.z);
			allEnemyKeys = new List<string>(mWaveManager.allDifferentEnemies);
			if (mWaveManager.HasCorruptionEnemy())
			{
				DataBundleTableHandle<HelperEnemySwapSchema> swapData = WeakGlobalInstance<WaveManager>.Instance.GetHelperSwapData();
				HelperEnemySwapSchema[] swapSchemas = swapData.Data;
				List<string> selectedHelpers = Singleton<Profile>.Instance.GetSelectedHelpers();
				HelperEnemySwapSchema[] array3 = swapSchemas;
				foreach (HelperEnemySwapSchema swapSchema2 in array3)
				{
					string swapFromKey2 = swapSchema2.helperSwapFrom.Key.ToString();
					if (selectedHelpers.Contains(swapFromKey2) || adhocHelpers.Contains(swapFromKey2))
					{
						allEnemyKeys.Add(swapSchema2.enemySwapTo.Key.ToString());
					}
				}
			}
		}
		if (Singleton<Profile>.Instance.inDailyChallenge)
		{
			mGameTimer = Singleton<Profile>.Instance.dailyChallengeProceduralWaveSchema.maxTime;
		}
		if (Singleton<Profile>.Instance.inMultiplayerWave)
		{
			mLegendaryStrikeEnemies = DataBundleRuntime.Instance.GetRecordKeys(typeof(EnemyListSchema), "LegendaryStrikeEnemies", false);
			foreach (string enemy in mLegendaryStrikeEnemies)
			{
				if (!allEnemyKeys.Contains(enemy))
				{
					allEnemyKeys.Add(enemy);
				}
			}
			if (Singleton<PlayModesManager>.Instance.Attacking)
			{
				mGameTimer = SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable("AttackTime", 300f);
			}
		}
		if (!Singleton<Profile>.Instance.inVSMultiplayerWave)
		{
			List<string> enemiesSoFar = new List<string>(allEnemyKeys);
			foreach (string enemyID in enemiesSoFar)
			{
				EnemySchema levelEnemySchema = Singleton<EnemiesDatabase>.Instance[enemyID];
				DataBundleRecordKey onDeathSpawnKey = levelEnemySchema.spawnOnDeath;
				if (!DataBundleRecordKey.IsNullOrEmpty(onDeathSpawnKey))
				{
					string spawnOnDeathKeyString = onDeathSpawnKey.Key.ToString();
					if (!string.IsNullOrEmpty(spawnOnDeathKeyString) && !allEnemyKeys.Contains(spawnOnDeathKeyString))
					{
						allEnemyKeys.Add(spawnOnDeathKeyString);
					}
				}
			}
			DataBundleTableHandle<EnemySwapSchema> enemyDeathSwapData = WeakGlobalInstance<WaveManager>.Instance.GetDeathSwapData();
			EnemySwapSchema[] enemyDeathSwapSchemas = enemyDeathSwapData.Data;
			EnemySwapSchema[] array4 = enemyDeathSwapSchemas;
			foreach (EnemySwapSchema swapSchema in array4)
			{
				string swapFromKey = swapSchema.swapFrom.Key.ToString();
				if (allEnemyKeys.Contains(swapFromKey))
				{
					allEnemyKeys.Add(swapSchema.swapTo.Key.ToString());
				}
			}
		}
		Singleton<EnemiesDatabase>.Instance.LoadInGameData(allEnemyKeys);
		LoadingScreen.LogStep("EnemiesDatabase.Instance.LoadInGameData");
		mLeadership[0] = new Leadership(0);
		int defenderId = 0;
		if (Singleton<Profile>.Instance.inVSMultiplayerWave && Singleton<PlayModesManager>.Instance.Attacking)
		{
			defenderId = 1;
		}
		mGate[defenderId] = new Gate(enemiesTargetLeft, defenderId);
		if (Singleton<Profile>.Instance.inVSMultiplayerWave)
		{
			mLeadership[1] = new EnemyLeadership(1);
		}
		mBell = new Bell(bellRingerLocationObject, bellLocationObject, defenderId);
		mPit = new Pit(pitArea, defenderId);
		int bannerLevel = Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Banner");
		if (Singleton<Profile>.Instance.inVSMultiplayerWave && Singleton<PlayModesManager>.Instance.Attacking)
		{
			bannerLevel = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.bannersCollected;
		}
		SharedResourceLoader.SharedResource res3;
		if (bannerLocObject != null && bannerLevel > 0)
		{
			res3 = ResourceCache.GetCachedResource("Assets/Game/Resources/Props/LevelProps/Banner.prefab", 1);
			if (res3 != null)
			{
				UnityEngine.Object.Instantiate(res3.Resource, bannerLocObject.position, Quaternion.identity);
			}
		}
		int flowerCount = Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Flower");
		if (Singleton<Profile>.Instance.inVSMultiplayerWave && Singleton<PlayModesManager>.Instance.Attacking)
		{
			flowerCount = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.flowersCollected;
		}
		if (flowerCount > 0)
		{
			res3 = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/GateDefense.prefab", 1);
			if (res3 != null)
			{
				mGate[defenderId].controller.SpawnEffectAt(res3.Resource as GameObject, gate.transform.position);
			}
		}
		mShaderManager = new ProceduralShaderManager();
		mCollectableManager = new CollectableManager(heroWalkLeftEdge.position.z + 0.16f, heroWalkRightEdge.position.z - 0.16f, heroSpawnPoint.position.x);
		mVillageArchers = new VillageArchers();
		mCharactersManager = new CharactersManager();
		if (mGate[0] != null)
		{
			mCharactersManager.AddCharacter(mGate[0]);
		}
		if (mGate[1] != null)
		{
			mCharactersManager.AddCharacter(mGate[1]);
		}
		LoadingScreen.LogStep("InGame load misc");
		CreateHero(0, heroSpawnPoint);
		if (Singleton<Profile>.Instance.inVSMultiplayerWave)
		{
			CreateHero(1, enemySpawnPoint);
		}
		mLeadership[0].characterManagerRef = mCharactersManager;
		mLeadership[0].helperSpawnArea = helpersSpawnArea;
		mLeadership[0].helpersZTarget = enemiesSpawnArea.transform.position.z;
		mLeadership[0].hero = mHero[0];
		if (mLeadership[1] != null)
		{
			mLeadership[1].characterManagerRef = mCharactersManager;
			mLeadership[1].helperSpawnArea = enemiesSpawnArea;
			mLeadership[1].helpersZTarget = enemiesTarget.transform.position.z;
			mLeadership[1].hero = mHero[1];
			mLeadership[0].helpersZTarget = heroTarget.transform.position.z;
		}
		ApplyInitialCharmEffects();
		LoadingScreen.LogStep("InGame load hero");
		mCameraYOffset = gameCamera.transform.position.y - heroSpawnPointLeft.transform.position.y;
		RenderSettings.fog = false;
		if (mWaveManager != null && !DataBundleRecordKey.IsNullOrEmpty(mWaveManager.Data.music))
		{
			SingletonSpawningMonoBehaviour<UMusicManager>.Instance.PlayByKey(mWaveManager.Data.music);
		}
		else
		{
			SingletonSpawningMonoBehaviour<UMusicManager>.Instance.PlayByKey(new DataBundleRecordKey(SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable<string>("DefaultWaveMusic")));
		}
		WeakGlobalMonoBehavior<HUD>.Instance.Init();
		WeakGlobalMonoBehavior<BannerManager>.Instance.Init();
		SingletonMonoBehaviour<TutorialMain>.Instance.Init();
		mTimeStarted = Time.time;
		if (!Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive() && !Singleton<Profile>.Instance.inDailyChallenge && SingletonMonoBehaviour<TutorialMain>.Instance.TutorialStartIfNeeded("Tutorial_Game01_ProtectGate"))
		{
			WeakGlobalMonoBehavior<HUD>.Instance.abilitiesEnabled = false;
			WeakGlobalMonoBehavior<HUD>.Instance.alliesEnabled = false;
			mTimeToNextTutorial = 7f;
		}
		else
		{
			mTimeToNextTutorial = 5f;
			RunAfterDelay(delegate
			{
				WeakGlobalMonoBehavior<BannerManager>.Instance.OpenBanner(new BannerStartWave(5f, Singleton<Profile>.Instance.waveToPlay));
			}, 2f);
		}
		if (currentWave == 1 && Singleton<Profile>.Instance.GetWaveLevel(1) == 1)
		{
			WeakGlobalMonoBehavior<HUD>.Instance.alliesEnabled = false;
			StartCoroutine(CheckForHeroMovement());
			StartCoroutine(CheckForHeroAttack());
		}
		StartCoroutine(CheckTutorials());
		if (!Singleton<Profile>.Instance.inDailyChallenge)
		{
			if (!Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive())
			{
				Singleton<Profile>.Instance.IncrementWaveAttemptCount(currentWave);
			}
			else
			{
				Singleton<Profile>.Instance.IncrementMPWaveAttemptCount(Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData.missionName);
			}
		}
		Singleton<Profile>.Instance.FlurrySession.ReportWaveStarted();
		Singleton<Analytics>.Instance.LogEvent("AudioSettings", Analytics.Param("DeviceVolume", NUF.GetHardwareVolume()), Analytics.Param("GameMusicVolume", AudioUtils.MasterMusicVolume), Analytics.Param("GameSfxVolume", AudioUtils.MasterSoundThemeVolume));
		res3 = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/FullScreenBloodEffect.prefab", 1);
		if (res3 != null)
		{
			this.hero.HUDBloodEffect = UnityEngine.Object.Instantiate(res3.Resource) as GameObject;
		}
		Singleton<PlayerWaveEventData>.Instance.StartWave();
		Shader.WarmupAllShaders();
		MemoryWarningHandler.Instance.unloadOnMemoryWarning = true;
		if (PortableQualitySettings.GetQuality() == EPortableQualitySetting.Low)
		{
			mHitEffectFrequency = 4f;
		}
		else if (PortableQualitySettings.GetQuality() == EPortableQualitySetting.Medium)
		{
			mHitEffectFrequency = 2.5f;
		}
		else
		{
			mHitEffectFrequency = 0.5f;
		}
		LoadingScreen.LogStep("InGame Start COMPLETE");
	}

	private IEnumerator CheckForKatanaSlashUse()
	{
		while (true)
		{
			yield return new WaitForSeconds(0f);
			if (SingletonMonoBehaviour<TutorialMain>.Instance.GetCurrentTutorial_Key() == "Tutorial_Game04_Ability")
			{
				if (!WeakGlobalMonoBehavior<HUD>.Instance.FindHUD<HUDAbilities>().IsAvailable("KatanaSlash"))
				{
					SingletonMonoBehaviour<TutorialMain>.Instance.TutorialDone();
					Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep6_AbilityWave1");
					yield break;
				}
				if (mGameOver)
				{
					break;
				}
			}
		}
		SingletonMonoBehaviour<TutorialMain>.Instance.TutorialDone();
	}

	private IEnumerator CheckForFarmerSpawn()
	{
		do
		{
			yield return new WaitForSeconds(0f);
		}
		while (!(SingletonMonoBehaviour<TutorialMain>.Instance.GetCurrentTutorial_Key() == "Tutorial_Game03_Ally") || (WeakGlobalInstance<CharactersManager>.Instance.helpersCount <= 0 && !mGameOver));
		SingletonMonoBehaviour<TutorialMain>.Instance.TutorialDone();
		Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep18_SummonFarmer");
	}

	private IEnumerator CheckTutorials()
	{
		while (!Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive() && !Singleton<Profile>.Instance.inDailyChallenge)
		{
			int currentWave = Singleton<Profile>.Instance.wave_SinglePlayerGame;
			if (mTimeToNextTutorial <= 0f)
			{
				if (currentWave <= 1)
				{
					if (SingletonMonoBehaviour<TutorialMain>.Instance.TutorialStartIfNeeded("Tutorial_Game02_Movement"))
					{
						yield return new WaitForSeconds(20f);
						mTimeToNextTutorial = 0f;
					}
					else if (SingletonMonoBehaviour<TutorialMain>.Instance.IsTutorialNeeded("Tutorial_Game04_Ability"))
					{
						List<Character> enemiesPlayerCanSee2 = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRangeMaxCount(hero.controller.position.z, hero.controller.position.z + 4f, 1, 1f);
						if (enemiesPlayerCanSee2.Count >= 1 && SingletonMonoBehaviour<TutorialMain>.Instance.GetCurrentTutorial_Key() != "Tutorial_Game04_Ability" && SingletonMonoBehaviour<TutorialMain>.Instance.TutorialStartIfNeeded("Tutorial_Game04_Ability"))
						{
							Singleton<Profile>.Instance.SetSelectedAbilities(new List<string>(new string[1] { "KatanaSlash" }));
							WeakGlobalMonoBehavior<HUD>.Instance.ResetAbilities();
							WeakGlobalMonoBehavior<HUD>.Instance.abilitiesEnabled = true;
							StartCoroutine(CheckForKatanaSlashUse());
							mTimeToNextTutorial = 0f;
							yield return new WaitForSeconds(5f);
						}
					}
				}
				else if (currentWave == 2 && SingletonMonoBehaviour<TutorialMain>.Instance.IsTutorialNeeded("Tutorial_Game03_Ally") && SingletonMonoBehaviour<TutorialMain>.Instance.GetCurrentTutorial_Key() != "Tutorial_Game03_Ally")
				{
					if (SingletonMonoBehaviour<TutorialMain>.Instance.TutorialStartIfNeeded("Tutorial_Game03_Ally"))
					{
						WeakGlobalMonoBehavior<HUD>.Instance.alliesEnabled = true;
						StartCoroutine(CheckForFarmerSpawn());
						mTimeToNextTutorial = 0f;
						yield return new WaitForSeconds(5f);
					}
				}
				else
				{
					if (currentWave != 4 || !SingletonMonoBehaviour<TutorialMain>.Instance.IsTutorialNeeded("Tutorial_Game05_Flying"))
					{
						break;
					}
					List<Character> enemiesPlayerCanSee = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(hero.controller.position.z, hero.controller.position.z + 4f, 1);
					int foundIndex = enemiesPlayerCanSee.FindIndex((Character c) => c.isFlying);
					if (foundIndex >= 0 && SingletonMonoBehaviour<TutorialMain>.Instance.GetCurrentTutorial_Key() != "Tutorial_Game05_Flying" && SingletonMonoBehaviour<TutorialMain>.Instance.TutorialStartIfNeeded("Tutorial_Game05_Flying"))
					{
						mTimeToNextTutorial = 0f;
						yield return new WaitForSeconds(5f);
					}
				}
			}
			mTimeToNextTutorial -= Time.deltaTime;
			yield return new WaitForSeconds(0f);
		}
	}

	private IEnumerator CheckForHeroMovement()
	{
		while (!hero.controller.isMoving)
		{
			yield return new WaitForSeconds(0f);
		}
		Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep4_MoveWave1");
	}

	private IEnumerator CheckForHeroAttack()
	{
		while (!hero.controller.isAttacking)
		{
			yield return new WaitForSeconds(0f);
		}
		Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep5_AttackWave1");
	}

	public void CreateHero(int index, Transform spawnPoint)
	{
		if (mHero[index] != null)
		{
			WeakGlobalInstance<CharactersManager>.Instance.DestroyCharacter(mHero[index]);
			mHero[index] = null;
		}
		mHero[index] = new Hero(spawnPoint, index);
		mHero[index].controller.constraintLeft = heroWalkLeftEdge.position.z;
		mHero[index].controller.constraintRight = heroWalkRightEdge.position.z;
		mCharactersManager.AddCharacter(mHero[index]);
	}

	private void OnDestroy()
	{
		Time.timeScale = 1f;
		if (!ApplicationUtilities.HasShutdown)
		{
			MemoryWarningHandler.Instance.unloadOnMemoryWarning = false;
			Singleton<AbilitiesDatabase>.Instance.UnloadData();
			Singleton<CharmsDatabase>.Instance.UnloadData();
			Singleton<HelpersDatabase>.Instance.UnloadData();
			Singleton<EnemiesDatabase>.Instance.UnloadData();
			Singleton<HeroesDatabase>.Instance.UnloadData();
			Singleton<PotionsDatabase>.Instance.UnloadData();
			mProjectileManager.UnloadData();
			if (mWaveManager != null)
			{
				mWaveManager.UnloadData();
			}
			if (mVillageArchers != null)
			{
				mVillageArchers.UnloadData();
			}
			if (mBell != null)
			{
				mBell.UnloadData();
			}
			if (mLeadership.Length > 0)
			{
				mLeadership[0].Clear();
			}
			mLeadership = null;
			mBuffIconManager = null;
			mHero = null;
			mGate = null;
			mBell = null;
			mPit = null;
			mCharactersManager = null;
			mCollectableManager = null;
			mProjectileManager = null;
			mWaveManager = null;
			mShaderManager = null;
			mVillageArchers = null;
			mRailManager = null;
			ResourceCache.UnloadAllAboveLevel(1);
			ResourceCache.DefaultCacheLevel = 0;
			SingletonSpawningMonoBehaviour<USoundThemeManager>.Instance.UnloadResourceLevel(1);
			SingletonSpawningMonoBehaviour<USoundThemeManager>.Instance.SetResourceLevel(0);
			GluiCore.sMaterialDictionary.Clear();
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (!gamePaused && pause)
		{
			GluiActionSender.SendGluiAction("POPUP_PAUSEMENU", null, null);
		}
	}

	private void Update()
	{
		if (!Singleton<Profile>.Instance.Initialized)
		{
			return;
		}
		if (gamePaused)
		{
			if (!mGameOver)
			{
				StoreMenuImpl.UpdateTapJoyPoints(base.gameObject);
			}
			return;
		}
		mHitEffectTimer -= Time.deltaTime;
		if (mGameOver)
		{
			if (useSlowMoFinisher)
			{
				mBaseTimeScale = Mathf.Min(1f, mBaseTimeScale + 0.8f * Time.deltaTime);
				Time.timeScale = mBaseTimeScale;
				mCameraYOffset += 0.25f * Time.deltaTime;
				gameCamera.transform.position = new Vector3(gameCamera.transform.position.x + 0.4f * Time.deltaTime, gameCamera.transform.position.y, gameCamera.transform.position.z);
			}
			mLeadership[0].Update();
			mCharactersManager.Update();
			mCollectableManager.Update();
			mProjectileManager.Update();
			mShaderManager.update();
			mBell.Update();
			mPit.Update();
			mVillageArchers.Update();
			return;
		}
		if (Debug.isDebugBuild && !WeakGlobalMonoBehavior<HUD>.Instance.gameObject.activeInHierarchy && Input.touchCount >= 2)
		{
			string text = Singleton<Profile>.Instance.GetSelectedAbilities()[0];
			if (!string.IsNullOrEmpty(text))
			{
				hero.DoAbility(text);
			}
		}
		mLeadership[0].Update();
		if (mLeadership[1] != null)
		{
			mLeadership[1].Update();
		}
		if (mWaveManager != null)
		{
			mWaveManager.Update();
		}
		mCharactersManager.Update();
		mCollectableManager.Update();
		mProjectileManager.Update();
		mShaderManager.update();
		mBell.Update();
		mPit.Update();
		mVillageArchers.Update();
		CheckWinLoseConditions();
		if (mAllAlliesInvincibleTimer > 0f)
		{
			mAllAlliesInvincibleTimer = Mathf.Max(0f, mAllAlliesInvincibleTimer - Time.deltaTime);
		}
		Singleton<PlayerWaveEventData>.Instance.Update(Time.deltaTime);
		if (mGameTimer > 0f)
		{
			mGameTimer -= Time.deltaTime;
			if (mGameTimer <= 0f)
			{
				mGameTimer = 0f;
				mHero[0].ForceDeath();
			}
		}
		StoreMenuImpl.UpdateTapJoyPoints(base.gameObject);
	}

	private void LateUpdate()
	{
		if (Singleton<Profile>.Instance.Initialized)
		{
			UpdateCamera();
		}
	}

	public Hero GetHero(int playerId)
	{
		if (playerId >= 0 && playerId < kMaxPlayers)
		{
			return mHero[playerId];
		}
		return null;
	}

	public Gate GetGate(int playerId)
	{
		if (playerId >= 0 && playerId < kMaxPlayers)
		{
			return mGate[playerId];
		}
		return null;
	}

	public Leadership GetLeadership(int playerIndex)
	{
		if (playerIndex >= 0 && playerIndex < kMaxPlayers)
		{
			return mLeadership[playerIndex];
		}
		return null;
	}

	public void RunSpecialWaveCommand(string cmd)
	{
		switch (cmd)
		{
		case "@bossalert":
			WeakGlobalMonoBehavior<BannerManager>.Instance.OpenBanner(new BannerBoss(5f * timeScalar));
			break;
		case "@legionalert":
			break;
		default:
			UnityEngine.Debug.Log("WARNING: Unknown wave command: " + cmd);
			break;
		}
	}

	public void ShowInGameStore(string itemID)
	{
	}

	public bool ShouldShowHitFX()
	{
		return mHitEffectTimer <= 0f;
	}

	public void ResetHitEffectTimer()
	{
		mHitEffectTimer = mHitEffectFrequency;
	}

	public void RunAfterDelay(Action fn, float delay)
	{
		if (fn != null)
		{
			StartCoroutine(RunAfterDelayInternal(fn, delay));
		}
	}

	public void RunNextUpdate(Action fn)
	{
		if (fn != null)
		{
			StartCoroutine(RunAfterYieldInternal(fn));
		}
	}

	public void Win()
	{
		Singleton<Profile>.Instance.FlurrySession.ReportWaveWon();
		Singleton<PlayStatistics>.Instance.data.victory = true;
		mGameOver = true;
		if (mWaveManager != null)
		{
			Singleton<Profile>.Instance.AddSeenEnemies(mWaveManager.allDifferentEnemies);
		}
		if (Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive())
		{
			Singleton<Profile>.Instance.MultiplayerData.FinishMultiplayerGameSession(true);
		}
		if (fireworksResource != null)
		{
			UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate(fireworksResource, hero.transform.position, Quaternion.identity), 10f);
		}
		CreateWinDialog();
		StartCoroutine(PostWinDelay());
		if (!Singleton<Profile>.Instance.inMultiplayerWave && Singleton<Profile>.Instance.waveToPlay > 2)
		{
			if (mGate[0] != null && mGate[0].NoDamage)
			{
				Singleton<Achievements>.Instance.IncrementAchievement("PerfectWave", 1);
			}
			if (mLeadership[0] != null)
			{
				if (mLeadership[0].ResourcesSpentOnHelpers == 0f)
				{
					Singleton<Achievements>.Instance.IncrementAchievement("OneManArmy", 1);
				}
				if (CanDoRevolutionAchievement && mLeadership[0].CanDoRevolutionAchievement)
				{
					Singleton<Achievements>.Instance.IncrementAchievement("Revolution", 1);
				}
			}
			if (mHero[0] != null && mHero[0].NoAttack)
			{
				Singleton<Achievements>.Instance.IncrementAchievement("NoAttack", 1);
			}
		}
		if (mWaveManager != null)
		{
			NarrativeSchema narrativeSchema = NarrativeSchema.NarrativeForWave(mWaveManager.WaveRecordKey);
			if (narrativeSchema != null)
			{
				GluiState_NarrativePanel.resourceToLoad = DataBundleRuntime.Instance.GetValue<string>(typeof(NarrativeSchema), NarrativeSchema.UdamanTableName, narrativeSchema.id, "prefab", true);
			}
		}
	}

	public void Lose()
	{
		Singleton<Profile>.Instance.FlurrySession.ReportWaveLost();
		mGameOver = true;
		if (Singleton<Profile>.Instance.inBonusWave)
		{
			Singleton<Profile>.Instance.bonusWaveToBeat++;
			Singleton<Profile>.Instance.wavesSinceLastBonusWave = 0;
		}
		if (Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive())
		{
			Singleton<Profile>.Instance.MultiplayerData.FinishMultiplayerGameSession(false);
		}
		CreateLoseDialog();
		Singleton<Profile>.Instance.Save();
		if (Singleton<Profile>.Instance.wave_SinglePlayerGame == 1 && Singleton<Profile>.Instance.GetWaveLevel(1) == 1 && !Singleton<Profile>.Instance.inMultiplayerWave)
		{
			Restart();
		}
		else
		{
			onFinishGameRequest();
		}
	}

	public void Restart()
	{
		WaveManager.LoadSceneForWave();
		Singleton<PlayerWaveEventData>.Instance.StartWave();
	}

	public void OnHudRestart()
	{
		if (!string.IsNullOrEmpty(activeCharm))
		{
			List<string> selectedAbilities = Singleton<Profile>.Instance.GetSelectedAbilities();
			CharmSchema charmSchema = Singleton<CharmsDatabase>.Instance[activeCharm];
			string text = charmSchema.abilityToActivate;
			if (!string.IsNullOrEmpty(text))
			{
				string item = DataBundleRuntime.RecordKey(text);
				selectedAbilities.Remove(item);
				Singleton<Profile>.Instance.SetSelectedAbilities(selectedAbilities);
				WeakGlobalMonoBehavior<HUD>.Instance.ResetAbilities();
			}
		}
	}

	public void OnReviveDialog_Accept()
	{
		hero.Revive();
		mReviveOfferDismissed = false;
		Singleton<Profile>.Instance.globalPlayerRating += 20;
		Singleton<Achievements>.Instance.IncrementAchievement("UseRevive", 1);
		if (Singleton<Profile>.Instance.inVSMultiplayerWave && Singleton<PlayModesManager>.Instance.Attacking)
		{
			mGameTimer = SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable("AttackTime", 300f);
		}
		else if (Singleton<Profile>.Instance.inDailyChallenge)
		{
			mGameTimer = Singleton<Profile>.Instance.dailyChallengeProceduralWaveSchema.maxTime;
		}
	}

	public void OnReviveDialog_Dismissed()
	{
		mReviveOfferDismissed = true;
	}

	private void onFinishGameRequest()
	{
		if (Singleton<Profile>.Instance.waveTypeToPlay == WaveManager.WaveType.Wave_SinglePlayer && Singleton<PlayStatistics>.Instance.data.victory)
		{
			Singleton<Achievements>.Instance.CheckThresholdAchievement("CompleteWave1", Singleton<PlayStatistics>.Instance.data.wavePlayed);
			Singleton<Achievements>.Instance.CheckThresholdAchievement("CompleteWave2", Singleton<PlayStatistics>.Instance.data.wavePlayed);
		}
		Singleton<PlayerWaveEventData>.Instance.Reset();
		if (Singleton<PlayStatistics>.Instance.data.victory && LootListController.allLoots.Count > 0)
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MainMenuScreen", "Menu_Results");
		}
		else
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MainMenuScreen", "Menu_Results_NoLoot");
		}
		StoreMenuImpl.ReturnedFromGame = true;
		LoadingScreen.LoadLevel("AllMenus");
	}

	private void UpdateCamera()
	{
		gameCamera.transform.position = new Vector3(gameCamera.transform.position.x, mHero[0].position.y + mCameraYOffset, mHero[0].position.z);
	}

	private void CheckWinLoseConditions()
	{
		if (mGameOver)
		{
			return;
		}
		bool flag = false;
		if (mHero[0].isOver)
		{
			if ((Singleton<Profile>.Instance.wave_SinglePlayerGame > 1 || Singleton<Profile>.Instance.GetWaveLevel(1) > 1) && !mReviveOfferDismissed)
			{
				WeakGlobalInstance<CollectableManager>.Instance.OpenPresents(true);
				GluiActionSender.SendGluiAction("POPUP_REVIVE", base.gameObject, null);
				return;
			}
			flag = true;
		}
		else
		{
			if (mGate[0] != null && mGate[0].health == 0f && mHero[0] != null)
			{
				mHero[0].ForceDeath();
			}
			if (mGate[1] != null && mHero[1] != null && mGate[1].health == 0f)
			{
				mHero[1].ForceDeath();
			}
		}
		if (flag)
		{
			Lose();
		}
		else if (Singleton<Profile>.Instance.inVSMultiplayerWave)
		{
			if (Singleton<PlayModesManager>.Instance.Attacking)
			{
				if (mGate[1] != null && mGate[1].health <= 0f)
				{
					Win();
					if (mHero[1] != null)
					{
						mHero[1].health = 0f;
					}
				}
			}
			else if (mLeadership[1].GetPercentDoneWithWave() == 1f && mCharactersManager.GetPlayerCharacters(1).Count <= 1 && mHero[1].health == 0f)
			{
				Win();
			}
		}
		else if (mWaveManager.isDone && mCharactersManager.enemiesCount == 0)
		{
			Win();
		}
	}

	private void CreateWinDialog()
	{
		if (useSlowMoFinisher)
		{
			mBaseTimeScale = 0.1f;
			Time.timeScale = mBaseTimeScale;
			gameCamera.transform.position = new Vector3(gameCamera.transform.position.x - 2f, gameCamera.transform.position.y, gameCamera.transform.position.z);
			mCameraYOffset /= 2f;
			mStartedSlowMoFinisher = true;
		}
		RunAfterDelay(delegate
		{
			WeakGlobalMonoBehavior<BannerManager>.Instance.OpenBanner(new BannerWinGame(5f * timeScalar));
		}, timeScalar * 0.5f);
	}

	private void CreateLoseDialog()
	{
		RunAfterDelay(delegate
		{
			WeakGlobalMonoBehavior<BannerManager>.Instance.OpenBanner(new BannerLoseGame(5f));
		}, 0.5f);
	}

	private IEnumerator PostWinDelay()
	{
		float waitTime = Singleton<Config>.Instance.data.GetFloat(TextDBSchema.ChildKey("Game", "postWinTime"));
		yield return new WaitForSeconds(waitTime);
		RegisterWin();
		onFinishGameRequest();
	}

	private void RegisterWin()
	{
		if (WeakGlobalInstance<WaveManager>.Instance != null)
		{
			WeakGlobalInstance<WaveManager>.Instance.AddSpecialRewardsToCollectables();
		}
		WeakGlobalInstance<CollectableManager>.Instance.BankAllResources();
		NextWave();
		Singleton<Profile>.Instance.Save();
	}

	private void NextWave()
	{
		if (Singleton<Profile>.Instance.inBonusWave)
		{
			Singleton<Profile>.Instance.bonusWaveToBeat++;
			Singleton<Profile>.Instance.wavesSinceLastBonusWave = 0;
		}
		else if (!Singleton<Profile>.Instance.inMultiplayerWave && !Singleton<Profile>.Instance.inDailyChallenge)
		{
			Singleton<Profile>.Instance.SetWaveLevel(Singleton<Profile>.Instance.wave_SinglePlayerGame, Singleton<Profile>.Instance.GetWaveLevel(Singleton<Profile>.Instance.wave_SinglePlayerGame) + 1);
			Singleton<Profile>.Instance.wave_SinglePlayerGame++;
			if (Singleton<Profile>.Instance.GetWaveLevel(Singleton<Profile>.Instance.wave_SinglePlayerGame) == 0)
			{
				Singleton<Profile>.Instance.SetWaveLevel(Singleton<Profile>.Instance.wave_SinglePlayerGame, 1);
			}
			if (Singleton<PlayModesManager>.Instance.selectedMode == "classic")
			{
				Singleton<Profile>.Instance.wavesSinceLastBonusWave++;
			}
			WaveSchema waveData = WaveManager.GetWaveData(Singleton<Profile>.Instance.wave_SinglePlayerGame, Singleton<Profile>.Instance.waveTypeToPlay);
			Singleton<Profile>.Instance.heroID = waveData.recommendedHero.Key;
			if (Singleton<Profile>.Instance.GetWaveLevel(Singleton<PlayStatistics>.Instance.data.wavePlayed) == 2)
			{
				ResultsMenuImpl.UnlockedFeature unlockedHero = ResultsMenuImpl.GetUnlockedHero();
				Singleton<Profile>.Instance.AutoEquipNewAbilities(Singleton<PlayStatistics>.Instance.data.wavePlayed + 1, (unlockedHero == null) ? null : unlockedHero.id);
			}
		}
	}

	private IEnumerator RunAfterDelayInternal(Action fn, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (fn != null)
		{
			fn();
		}
	}

	private IEnumerator RunAfterYieldInternal(Action fn)
	{
		yield return null;
		if (fn != null)
		{
			fn();
		}
	}

	private void SpendCharm()
	{
		if (!Singleton<Profile>.Instance.inBonusWave)
		{
			string selectedCharm = Singleton<Profile>.Instance.selectedCharm;
			if (selectedCharm != string.Empty)
			{
				Singleton<Profile>.Instance.SetNumCharms(selectedCharm, Mathf.Max(0, Singleton<Profile>.Instance.GetNumCharms(selectedCharm) - 1));
				Singleton<Profile>.Instance.selectedCharm = string.Empty;
				Singleton<Profile>.Instance.globalPlayerRating += 20;
			}
		}
	}

	public void ApplyInitialCharmEffects()
	{
		CharmSchema charmSchema = Singleton<CharmsDatabase>.Instance[activeCharm];
		if (charmSchema != null)
		{
			UseCharm();
		}
	}

	public void UseCharm()
	{
		CharmSchema charmSchema = Singleton<CharmsDatabase>.Instance[activeCharm];
		if (charmSchema == null)
		{
			return;
		}
		if (charmSchema.id == "Friendship")
		{
			string table = "FriendshipCharmHelpers";
			DataBundleRecordTable dataBundleRecordTable = new DataBundleRecordTable(table);
			HelperListSchema[] array = dataBundleRecordTable.InitializeRecords<HelperListSchema>();
			int num = UnityEngine.Random.Range(0, array.Length);
			DataBundleRecordKey dataBundleRecordKey = new DataBundleRecordKey(array[num].ability);
			FriendshipHelperID = DataBundleRuntime.RecordKey(dataBundleRecordKey);
		}
		if (!string.IsNullOrEmpty(charmSchema.abilityToActivate.Key))
		{
			Singleton<Achievements>.Instance.IncrementAchievement("UseCharm", 1);
		}
		else
		{
			if (HasCommandCharm())
			{
				GetLeadership(0).ApplyLeadershipCostBuff(charmSchema.leadershipReduction);
			}
			Singleton<Achievements>.Instance.IncrementAchievement("UseCharm", 1);
		}
		SpendCharm();
	}

	public bool HasHasteCharm()
	{
		return activeCharm == "haste" || activeCharm.Contains("+haste");
	}

	public bool HasPowerCharm()
	{
		return activeCharm == "power" || activeCharm.Contains("+power");
	}

	public bool HasWealthCharm()
	{
		return activeCharm == "wealth" || activeCharm.Contains("+wealth");
	}

	public bool HasPeaceCharm()
	{
		return activeCharm == "peace" || activeCharm.Contains("+peace");
	}

	public bool HasLuckCharm()
	{
		return activeCharm == "luck" || activeCharm.Contains("+luck");
	}

	public bool HasCommandCharm()
	{
		return activeCharm == "command";
	}

	public bool HasMagicCharm()
	{
		return activeCharm == "magic";
	}

	private void ShowReviveDialog()
	{
	}

	public bool SpawnFriendshipHelper()
	{
		if (!string.IsNullOrEmpty(FriendshipHelperID))
		{
			Character character = WeakGlobalInstance<Leadership>.Instance.ForceSpawn(FriendshipHelperID);
			if (character != null && mWaveManager != null)
			{
				character.maxHealth *= mWaveManager.multipliers.enemiesHealth;
				character.health = character.maxHealth;
				character.meleeDamage *= mWaveManager.multipliers.enemiesDamages;
				return true;
			}
		}
		return false;
	}

	private void HideUnusedGate()
	{
		if (Singleton<Profile>.Instance.inVSMultiplayerWave)
		{
			return;
		}
		switch (mGameDirection)
		{
		case PlayModesManager.GameDirection.LeftToRight:
			if (enemiesTargetRight != null)
			{
				UnityEngine.Object.DestroyImmediate(enemiesTargetRight, true);
				enemiesTargetRight = null;
			}
			break;
		case PlayModesManager.GameDirection.RightToLeft:
			if (enemiesTargetLeft != null)
			{
				UnityEngine.Object.DestroyImmediate(enemiesTargetLeft, true);
				enemiesTargetLeft = null;
				if (vortexLeft != null)
				{
					UnityEngine.Object.DestroyImmediate(vortexLeft, true);
					vortexLeft = null;
				}
				if (gateSparklesLeft != null)
				{
					UnityEngine.Object.DestroyImmediate(gateSparklesLeft, true);
					gateSparklesLeft = null;
				}
			}
			break;
		}
	}

	public bool SetCheatAbility(string ability)
	{
		if (Singleton<AbilitiesDatabase>.Instance.GetSchema(ability) != null)
		{
			List<string> list = new List<string>(1);
			list.Add(ability);
			Singleton<Profile>.Instance.SetSelectedAbilities(list);
			return true;
		}
		return false;
	}

	public void SetHeroInvulnByDefault(bool toSet)
	{
		Singleton<Profile>.Instance.debugHeroInvuln = toSet;
		ResetHeroInvulnToDefault(0);
	}

	public void ResetHeroInvulnToDefault(int ownerID)
	{
		bool invuln = ownerID == 0 && Singleton<Profile>.Instance.debugHeroInvuln;
		GetHero(ownerID).invuln = invuln;
		if (GetGate(ownerID) != null)
		{
			GetGate(ownerID).invuln = invuln;
		}
	}
}
