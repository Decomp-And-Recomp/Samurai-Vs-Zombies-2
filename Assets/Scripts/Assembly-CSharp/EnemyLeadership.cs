using System.Collections.Generic;
using UnityEngine;

public class EnemyLeadership : Leadership
{
	public enum EPositionPriority
	{
		kAttackEnemyHero = 0,
		kDefendGate = 1,
		kAttackGate = 2,
		kAttackWithHelpers = 3,
		kRecover = 4
	}

	public class SpawnGroup
	{
		public List<int> unitsToSpawn = new List<int>();

		public bool spawnBoss;

		public bool levelUp;

		public float leadershipCost;
	}

	private float mPositionPriorityAdjustTimer;

	private EPositionPriority mPositionPriority;

	private Character mFurthestEnemy;

	private Character mFurthestHelper;

	private int mMoveDir;

	private float[] mAbilityCooldown;

	private float[] mMaxAbilityCooldown;

	private int mAbilityCount;

	private int mAbilityToUse;

	private float[] mHelperCooldown;

	private string mSpecialBossName;

	private Character mBossCharacter;

	private bool mNoOffense;

	private float mNoOffenseTimer;

	private float mDesiredPositionOffset;

	private float mMaxPositionOffset;

	private float mMinPositionOffset;

	private float mPositionBuffer;

	private float mAbilityCooldownFactor;

	private List<SpawnGroup> mSpawnList;

	private List<int> mCurrentSpawnGroup;

	private int mTotalSpawnCount;

	private int mUnitsKilled;

	private int mSpawnGroupIndex;

	private bool mLevelingUp;

	private float mSpawnDelay;

	private bool mDelayingGroup;

	public EnemyLeadership(int playerIndex)
	{
		mOwnerId = playerIndex;
		mIsLeftToRightGameplay = Singleton<PlayModesManager>.Instance.gameDirection == PlayModesManager.GameDirection.RightToLeft;
		mLeadershipData = DataBundleUtils.InitializeRecord<LeadershipSchema>(new DataBundleRecordKey(Leadership.UdamanTable, Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.heroId));
		mMaxLevel = mLeadershipData.maxLevel;
		SetLeadershipLevel(Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.leadershipLevel);
		string[] selectedHelperIDs = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.GetSelectedHelperIDs();
		string[] array = selectedHelperIDs;
		foreach (string text in array)
		{
			if (!string.IsNullOrEmpty(text))
			{
				HelperTypeData item = LoadHelperData(text);
				mHelperTypes.Add(item);
			}
		}
		string[] array2 = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.abilityIdList.ToArray();
		mResources = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.bannersCollected * 10;
		mResourcesSpentOnHelpers = 0f;
		mResourcesSpentOnUpgrades = 0f;
		mAbilityCooldownFactor = 1f;
		if (Singleton<Profile>.Instance.MultiplayerData.TweakValues != null)
		{
			mResources += Singleton<Profile>.Instance.MultiplayerData.TweakValues.startingLeadership;
			mAbilityCooldownFactor *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.heroAbilityFrequency;
		}
		mAbilityCooldown = new float[MultiplayerProfileLoadout.kMaxAbilities];
		mMaxAbilityCooldown = new float[MultiplayerProfileLoadout.kMaxAbilities];
		mAbilityCount = 0;
		mAbilityToUse = -1;
		for (int j = 0; j < array2.Length; j++)
		{
			string text2 = array2[j];
			if (!string.IsNullOrEmpty(text2))
			{
				AbilitySchema schema = Singleton<AbilitiesDatabase>.Instance.GetSchema(text2);
				if (schema != null)
				{
					mMaxAbilityCooldown[j] = schema.cooldown;
					mAbilityCount++;
					continue;
				}
				break;
			}
			break;
		}
		int num = Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData.defensiveBuffs[0];
		mSpecialBossName = WaveManager.SpecialBossName(num);
		mNoOffenseTimer = -1f;
		if (mSpecialBossName != string.Empty)
		{
			Singleton<EnemiesDatabase>.Instance.LoadInGameData(mSpecialBossName);
			mNoOffense = true;
		}
		if (Singleton<PlayModesManager>.Instance.Attacking)
		{
			mNoOffenseTimer = 120f;
		}
		BuildSpawnList();
	}

	public override void Update()
	{
		base.Update();
		if (base.hero == null)
		{
			CheckForLivingEnemies();
			return;
		}
		if (base.hero.health == 0f && !Singleton<PlayModesManager>.Instance.Attacking)
		{
			base.resources += Time.deltaTime * mIncreaseRate * 3f;
		}
		if (mNoOffense && mBossCharacter != null && mBossCharacter.health <= 0f)
		{
			mNoOffense = false;
		}
		mNoOffenseTimer -= Time.deltaTime;
		Hero hero = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetHero(1 - mOwnerId);
		for (int i = 0; i < mHelperTypes.Count; i++)
		{
			mHelperCooldown[i] += Time.deltaTime;
		}
		if (mLevelingUp)
		{
			if (base.isUpgradable)
			{
				LevelUp();
				mLevelingUp = false;
			}
			else if (mLevel == mMaxLevel)
			{
				mLevelingUp = false;
			}
			else if (Singleton<PlayModesManager>.Instance.Attacking && hero != null && hero.health > 0f)
			{
				int num = 0;
				foreach (Character playerCharacter in WeakGlobalInstance<CharactersManager>.Instance.GetPlayerCharacters(mOwnerId))
				{
					if (playerCharacter.health > 0f && playerCharacter.position.z < hero.position.z && !playerCharacter.isBase)
					{
						num++;
					}
				}
				float enemyAdvancePct = GetEnemyAdvancePct();
				if ((enemyAdvancePct > 0.5f && num == 0) || (enemyAdvancePct > 0.9f && num < 3))
				{
					mLevelingUp = false;
					MaybeSpawnBoss();
				}
			}
		}
		else if (mSpawnGroupIndex < mSpawnList.Count)
		{
			mSpawnDelay -= Time.deltaTime;
			if (mDelayingGroup && (Singleton<PlayModesManager>.Instance.Attacking || mResources >= mSpawnList[mSpawnGroupIndex].leadershipCost || mResources == mMaxResources))
			{
				mDelayingGroup = false;
			}
			if (mSpawnDelay <= 0f && !mDelayingGroup)
			{
				int num2 = 0;
				int num3 = mCurrentSpawnGroup[num2];
				bool flag = true;
				while (num2 < mCurrentSpawnGroup.Count)
				{
					if (mHelperCooldown[num3] < mHelperTypes[num3].data.totalCooldown)
					{
						num2++;
						if (num2 < mCurrentSpawnGroup.Count)
						{
							num3 = mCurrentSpawnGroup[num2];
						}
						continue;
					}
					bool uniqueLimited;
					if (IsAvailable(num3, out uniqueLimited))
					{
						break;
					}
					flag = false;
					int num4 = num3;
					if (uniqueLimited || mHelperTypes[num3].data.leadershipCost.leadership > mMaxResources || mHelperTypes[num3].data.isMount)
					{
						do
						{
							if (--num3 < 0)
							{
								num3 = mHelperTypes.Count - 1;
							}
							if (num3 >= 0 && num3 < mHelperTypes.Count && IsAvailable(num3, out uniqueLimited))
							{
								flag = true;
								break;
							}
						}
						while (num3 != num4);
					}
					if (num3 == num4 && !flag && mLevel < mMaxLevel)
					{
						mLevelingUp = true;
					}
					break;
				}
				if (num2 >= mCurrentSpawnGroup.Count)
				{
					flag = false;
				}
				if (flag)
				{
					Character character = Spawn(num3);
					if (character != null)
					{
						mHelperCooldown[num3] = 0f;
						mCurrentSpawnGroup.RemoveAt(num2);
						mSpawnDelay = Random.Range(0.15f, 0.5f);
						if (mCurrentSpawnGroup.Count == 0)
						{
							if (mSpawnGroupIndex < mSpawnList.Count - 1 || !Singleton<PlayModesManager>.Instance.Attacking)
							{
								mSpawnGroupIndex++;
							}
							if (mSpawnGroupIndex < mSpawnList.Count)
							{
								BeginSpawnGroup(mSpawnGroupIndex);
								mLevelingUp = mSpawnList[mSpawnGroupIndex].levelUp && mLevel < mMaxLevel;
								if (mSpawnList[mSpawnGroupIndex].spawnBoss)
								{
									MaybeSpawnBoss();
								}
							}
						}
					}
				}
			}
		}
		else
		{
			CheckForLivingEnemies();
		}
		mPositionPriorityAdjustTimer -= Time.deltaTime;
		if (mPositionPriorityAdjustTimer <= 0f)
		{
			UpdatePositionPriorities();
		}
		float z = base.hero.transform.position.z;
		float num5 = z;
		float num6 = z + 1f;
		float num7 = z - 1f;
		switch (mPositionPriority)
		{
		case EPositionPriority.kAttackWithHelpers:
			num5 = hero.transform.position.z;
			if (mAbilityToUse < 0)
			{
				num5 = ((mFurthestHelper == null) ? GetGateDefendPosition() : Mathf.Clamp(num5, mFurthestHelper.transform.position.z - 3f, mFurthestHelper.transform.position.z + 3f));
			}
			break;
		case EPositionPriority.kAttackEnemyHero:
			num5 = hero.transform.position.z;
			break;
		case EPositionPriority.kDefendGate:
			num5 = GetGateDefendPosition();
			break;
		case EPositionPriority.kAttackGate:
		{
			Gate gate = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetGate(1 - mOwnerId);
			if (gate != null)
			{
				num5 = gate.transform.position.z;
			}
			break;
		}
		case EPositionPriority.kRecover:
			if (mFurthestEnemy != null)
			{
				num5 = mFurthestEnemy.transform.position.z;
			}
			break;
		}
		if (base.hero.LeftToRight)
		{
			if (mNoOffense || mNoOffenseTimer > 0f)
			{
				float num8 = base.helperSpawnArea.transform.position.z + 15f;
				if (hero.transform.position.z - 2f * hero.bowAttackRange < num8)
				{
					num8 = hero.transform.position.z;
				}
				num5 = Mathf.Min(num5, num8);
			}
			num7 = num5 - mMaxPositionOffset;
			num6 = num5 - mMinPositionOffset;
		}
		else
		{
			if (mNoOffense || mNoOffenseTimer > 0f)
			{
				float num9 = base.helperSpawnArea.transform.position.z - 15f;
				if (hero.transform.position.z + 2f * hero.bowAttackRange > num9)
				{
					num9 = hero.transform.position.z;
				}
				num5 = Mathf.Max(num5, num9);
			}
			num6 = num5 + mMaxPositionOffset;
			num7 = num5 + mMinPositionOffset;
		}
		num7 = Mathf.Clamp(num7, base.hero.controller.constraintLeft, base.hero.controller.constraintRight);
		num6 = Mathf.Clamp(num6, base.hero.controller.constraintLeft, base.hero.controller.constraintRight);
		if (num7 > z || (mMoveDir == 1 && num7 > z - mPositionBuffer))
		{
			if (base.hero.isEffectivelyIdle)
			{
				base.hero.onMoveRight();
			}
			mMoveDir = 1;
		}
		else if (num6 < z || (mMoveDir == -1 && num6 < z + mPositionBuffer))
		{
			if (base.hero.isEffectivelyIdle)
			{
				base.hero.onMoveLeft();
			}
			mMoveDir = -1;
		}
		else
		{
			base.hero.onDontMove();
			mMoveDir = 0;
		}
		if (mAbilityToUse >= 0 && mMoveDir == 0 && mFurthestEnemy != null && mPositionPriority != EPositionPriority.kRecover && !base.hero.isInKnockback && base.hero.canMove && base.hero.isEffectivelyIdle)
		{
			float num10 = Mathf.Abs(mFurthestEnemy.transform.position.z - base.hero.transform.position.z);
			if (num10 < mMaxPositionOffset && num10 > mMinPositionOffset)
			{
				base.hero.DoAbility(Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.abilityIdList[mAbilityToUse]);
				mAbilityCooldown[mAbilityToUse] = 0f;
				mAbilityToUse = -1;
			}
		}
		for (int j = 0; j < mAbilityCount; j++)
		{
			mAbilityCooldown[j] += Time.deltaTime * mAbilityCooldownFactor;
		}
	}

	private void CheckForLivingEnemies()
	{
		bool flag = false;
		foreach (Character playerCharacter in WeakGlobalInstance<CharactersManager>.Instance.GetPlayerCharacters(mOwnerId))
		{
			if (playerCharacter.health > 0f && !playerCharacter.isMount)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			mUnitsKilled = mTotalSpawnCount;
		}
	}

	private void MaybeSpawnBoss()
	{
		if (!string.IsNullOrEmpty(mSpecialBossName))
		{
			HelperTypeData data = new HelperTypeData(mSpecialBossName, 1);
			mBossCharacter = SpawnHelper(data, mHelperSpawnArea.size.x, mHelperSpawnArea.transform.position);
			mSpecialBossName = string.Empty;
			if (mBossCharacter != null)
			{
				WeakGlobalMonoBehavior<BannerManager>.Instance.OpenBanner(new BannerBoss(3f));
				mBossCharacter.BlocksHeroMovement = SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable("BossBlockMovement", true);
			}
		}
	}

	private float GetGateDefendPosition()
	{
		float z = base.hero.transform.position.z;
		if (mFurthestEnemy != null)
		{
			z = mFurthestEnemy.transform.position.z;
		}
		if (base.hero.LeftToRight)
		{
			return Mathf.Min(z, base.helperSpawnArea.transform.position.z + 15f);
		}
		return Mathf.Max(z, base.helperSpawnArea.transform.position.z - 15f);
	}

	private void AddToGroup(int helperIndex, int groupIndex, int[] spawnCount)
	{
		mTotalSpawnCount++;
		mSpawnList[groupIndex].unitsToSpawn.Insert(0, helperIndex);
		spawnCount[helperIndex]--;
		mSpawnList[groupIndex].leadershipCost += mHelperTypes[helperIndex].data.leadershipCost.leadership;
		if (mHelperTypes[helperIndex].linkedHelper >= 0)
		{
			AddToGroup(mHelperTypes[helperIndex].linkedHelper, groupIndex, spawnCount);
		}
	}

	private void BuildSpawnList()
	{
		mHelperTypes.Sort((HelperTypeData x, HelperTypeData y) => x.data.leadershipCost.leadership.CompareTo(y.data.leadershipCost.leadership));
		mHelperCooldown = new float[mHelperTypes.Count];
		foreach (HelperTypeData mHelperType in mHelperTypes)
		{
			if (string.IsNullOrEmpty(mHelperType.data.upgradeAlliesFrom))
			{
				continue;
			}
			for (int i = 0; i < mHelperTypes.Count; i++)
			{
				if (mHelperTypes[i].data.id == mHelperType.data.upgradeAlliesFrom)
				{
					mHelperType.linkedHelper = i;
					break;
				}
			}
		}
		mSpawnList = new List<SpawnGroup>();
		float num = mLeadershipData.resourcesPerSeconds0 + mLeadershipData.resourcesPerSeconds1 + mLeadershipData.resourcesPerSeconds2 + mLeadershipData.resourcesPerSeconds3;
		num /= 4f;
		float num2 = Mathf.Max(100f, Singleton<Profile>.Instance.MultiplayerData.TweakValues.attackLeadershipPool);
		float num3 = mHelperTypes.Count;
		float num4 = num2 / num3;
		int[] array = new int[mHelperTypes.Count];
		mTotalSpawnCount = 0;
		for (int j = 0; j < mHelperTypes.Count; j++)
		{
			array[j] = 1 + (int)(num4 / mHelperTypes[j].data.leadershipCost.leadership);
			mTotalSpawnCount += array[j];
		}
		int num5 = mTotalSpawnCount / 4;
		int num6 = 2;
		for (int k = 0; k < num5; k++)
		{
			SpawnGroup spawnGroup = new SpawnGroup();
			num6--;
			int num7 = 0;
			if (num6 <= 0)
			{
				spawnGroup.levelUp = true;
				num6 = 2;
				num7++;
			}
			if (k == num5 - 1)
			{
				spawnGroup.spawnBoss = true;
			}
			mSpawnList.Add(spawnGroup);
		}
		int num8 = num5 - 1;
		int num9 = mHelperTypes.Count - 1;
		bool flag = mHelperTypes.Count == 0;
		mTotalSpawnCount = 1;
		while (!flag)
		{
			AddToGroup(num9, num8, array);
			do
			{
				num8--;
				if (num8 < 0)
				{
					num8 = num5 - 1;
					break;
				}
			}
			while (num8 != 0 && mSpawnList[num8].unitsToSpawn.Count > mSpawnList[num8 - 1].unitsToSpawn.Count);
			int num10 = num9;
			do
			{
				num9--;
				if (num9 < 0)
				{
					num9 = mHelperTypes.Count - 1;
				}
				if (num9 == num10 && array[num9] <= 0)
				{
					flag = true;
				}
			}
			while (array[num9] == 0 && !flag);
		}
		int num11 = 0;
		while (num11 < mSpawnList.Count)
		{
			if (mSpawnList[num11].unitsToSpawn.Count == 0)
			{
				mSpawnList.RemoveAt(num11);
			}
			else
			{
				num11++;
			}
		}
		if (mSpecialBossName != string.Empty)
		{
			mTotalSpawnCount++;
		}
		mTotalSpawnCount = Mathf.Max(1, mTotalSpawnCount);
		mCurrentSpawnGroup = new List<int>();
		BeginSpawnGroup(0);
	}

	private void BeginSpawnGroup(int index)
	{
		mDelayingGroup = true;
		mCurrentSpawnGroup.Clear();
		if (index >= mSpawnList.Count)
		{
			return;
		}
		foreach (int item in mSpawnList[index].unitsToSpawn)
		{
			mCurrentSpawnGroup.Add(item);
		}
	}

	private float GetEnemyAdvancePct()
	{
		float num = Mathf.Abs(mFurthestEnemy.transform.position.z - base.helperSpawnArea.transform.position.z);
		float num2 = Mathf.Abs(WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(1 - mOwnerId).helperSpawnArea.transform.position.z - base.helperSpawnArea.transform.position.z);
		return 1f - num / num2;
	}

	private void UpdatePositionPriorities()
	{
		mPositionPriority = EPositionPriority.kAttackWithHelpers;
		Character character = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetHero(1 - base.hero.ownerId);
		if (base.hero.health > base.hero.maxHealth * 0.5f || GetEnemyAdvancePct() > 0.9f)
		{
			mMaxPositionOffset = base.hero.meleeAttackRange;
			mMinPositionOffset = 0f;
			if (character != null)
			{
				float num = Mathf.Abs(character.transform.position.z - base.hero.transform.position.z);
				if (character.health < character.maxHealth * 0.4f || character.bowAttackRange > num)
				{
					mPositionPriority = EPositionPriority.kAttackEnemyHero;
				}
			}
		}
		else if (base.hero.health < base.hero.maxHealth * 0.25f || (mPositionPriority == EPositionPriority.kRecover && base.hero.health < base.hero.maxHealth * 0.4f))
		{
			mMinPositionOffset = 6f;
			mMaxPositionOffset = 8f;
			mPositionPriority = EPositionPriority.kRecover;
		}
		else
		{
			mMaxPositionOffset = base.hero.bowAttackRange;
			mMinPositionOffset = Mathf.Max(0f, Mathf.Min(mMaxPositionOffset - 0.5f, base.hero.meleeAttackRange + 0.5f));
		}
		mPositionPriorityAdjustTimer = 2f;
		Character[] array = base.characterManagerRef.GetPlayerCharacters(1 - mOwnerId).ToArray();
		float num2 = -10000f;
		if (base.hero.LeftToRight)
		{
			num2 = 10000f;
		}
		Character[] array2 = array;
		foreach (Character character2 in array2)
		{
			if (character2.health > 0f && ((base.hero.LeftToRight && character2.transform.position.z < num2) || (!base.hero.LeftToRight && character2.transform.position.z > num2)) && character2.controller != null)
			{
				mFurthestEnemy = character2;
				num2 = character2.transform.position.z;
			}
		}
		if (Singleton<PlayModesManager>.Instance.Attacking && mFurthestEnemy != null && mFurthestEnemy != WeakGlobalMonoBehavior<InGameImpl>.Instance.GetHero(1 - mOwnerId))
		{
			mPositionPriority = EPositionPriority.kDefendGate;
		}
		if (mFurthestEnemy != null && mPositionPriority != EPositionPriority.kRecover)
		{
			for (int j = 0; j < mAbilityCount; j++)
			{
				if (mAbilityCooldown[j] >= mMaxAbilityCooldown[j])
				{
					string abilityID = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.abilityIdList[j];
					AbilitySchema schema = Singleton<AbilitiesDatabase>.Instance.GetSchema(abilityID);
					if (schema != null)
					{
						mAbilityToUse = j;
						mMaxPositionOffset = schema.AIMaxRange;
						mMinPositionOffset = schema.AIMinRange;
					}
				}
			}
		}
		mDesiredPositionOffset = (mMaxPositionOffset + mMinPositionOffset) / 2f;
		float num3 = mDesiredPositionOffset - mMinPositionOffset;
		mPositionBuffer = Mathf.Min(num3 * 0.4f, 1f);
		Character[] array3 = base.characterManagerRef.GetPlayerCharacters(mOwnerId).ToArray();
		num2 = 10000f;
		mFurthestHelper = null;
		if (base.hero.LeftToRight)
		{
			num2 = -10000f;
		}
		Character[] array4 = array3;
		foreach (Character character3 in array4)
		{
			if (character3.health > 0f && character3 != base.hero && character3 != WeakGlobalMonoBehavior<InGameImpl>.Instance.GetGate(base.hero.ownerId) && ((base.hero.LeftToRight && character3.transform.position.z > num2) || (!base.hero.LeftToRight && character3.transform.position.z < num2)))
			{
				mFurthestHelper = character3;
			}
		}
	}

	public override void UnitKilled()
	{
		mUnitsKilled = Mathf.Min(mTotalSpawnCount - 1, mUnitsKilled + 1);
	}

	public override float GetPercentDoneWithWave()
	{
		return Mathf.Clamp((float)mUnitsKilled / (float)mTotalSpawnCount, 0f, 1f);
	}
}
