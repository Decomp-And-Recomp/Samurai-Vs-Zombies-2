using System;
using System.Collections.Generic;
using UnityEngine;

public class CharactersManager : WeakGlobalInstance<CharactersManager>
{
	public enum ELanePreference
	{
		any = 0,
		front = 1,
		back = 2,
		center = 3
	}

	private const float kCloseDistRange = 0.45f;

	private const float kMinTimeBetweenAttacks = 0.25f;

	private const float kLaneWidth = 0.15f;

	private const float kLaneCharacterValue = 2500f;

	protected Character[] mMount = new Character[InGameImpl.kMaxPlayers];

	private List<Character> mCharacters = new List<Character>();

	private List<Character> mDyingCharacters = new List<Character>();

	private List<Character> mInactiveCharacters = new List<Character>();

	private List<Character>[] mPlayerCharacters;

	public int helpersCount
	{
		get
		{
			int num = 0;
			foreach (Character mCharacter in mCharacters)
			{
				if (mCharacter.ownerId == 0 && !mCharacter.isPlayer)
				{
					num++;
				}
			}
			return num - 1;
		}
	}

	public int enemiesCount
	{
		get
		{
			int num = 0;
			foreach (Character mCharacter in mCharacters)
			{
				if (mCharacter.ownerId == 1 && !mCharacter.dynamicSpawn)
				{
					num++;
				}
			}
			return num;
		}
	}

	public float enemyMeleePreAttackDelay { get; private set; }

	public float enemyRangedPreAttackDelay { get; private set; }

	public float allyMeleePreAttackDelay { get; private set; }

	public float allyRangedPreAttackDelay { get; private set; }

	public Action postUpdateFunc { get; set; }

	public List<Character> allAlive
	{
		get
		{
			return mCharacters;
		}
	}

	public List<Character> allEnemies
	{
		get
		{
			List<Character> list = new List<Character>();
			foreach (Character mCharacter in mCharacters)
			{
				if (mCharacter.isEnemy)
				{
					list.Add(mCharacter);
				}
			}
			return list;
		}
	}

	public List<Character> allUniqueAllies
	{
		get
		{
			List<Character> list = new List<Character>();
			foreach (Character mCharacter in mCharacters)
			{
				if (!mCharacter.isEnemy && mCharacter.isUnique)
				{
					list.Add(mCharacter);
				}
			}
			foreach (Character mInactiveCharacter in mInactiveCharacters)
			{
				if (!mInactiveCharacter.isEnemy && mInactiveCharacter.isUnique)
				{
					list.Add(mInactiveCharacter);
				}
			}
			return list;
		}
	}

	public CharactersManager()
	{
		SetUniqueInstance(this);
		mPlayerCharacters = new List<Character>[InGameImpl.kMaxPlayers];
		for (int i = 0; i < InGameImpl.kMaxPlayers; i++)
		{
			mPlayerCharacters[i] = new List<Character>();
		}
	}

	public void Update()
	{
		if (enemyMeleePreAttackDelay > 0f)
		{
			enemyMeleePreAttackDelay = Mathf.Max(0f, enemyMeleePreAttackDelay - Time.deltaTime);
		}
		if (enemyRangedPreAttackDelay > 0f)
		{
			enemyRangedPreAttackDelay = Mathf.Max(0f, enemyRangedPreAttackDelay - Time.deltaTime);
		}
		if (allyMeleePreAttackDelay > 0f)
		{
			allyMeleePreAttackDelay = Mathf.Max(0f, allyMeleePreAttackDelay - Time.deltaTime);
		}
		if (allyRangedPreAttackDelay > 0f)
		{
			allyRangedPreAttackDelay = Mathf.Max(0f, allyRangedPreAttackDelay - Time.deltaTime);
		}
		foreach (Character mCharacter in mCharacters)
		{
			mCharacter.Update();
		}
		foreach (Character mDyingCharacter in mDyingCharacters)
		{
			mDyingCharacter.Update();
		}
		UpdateBattle();
		if (postUpdateFunc != null)
		{
			postUpdateFunc();
			postUpdateFunc = null;
		}
	}

	public void AddCharacter(Character c)
	{
		mCharacters.Add(c);
		if (c.ownerId >= 0)
		{
			mPlayerCharacters[c.ownerId].Add(c);
		}
	}

	public void KillCharacter(Character c)
	{
		c.health = 0f;
	}

	public void DestroyCharacter(Character c)
	{
		mCharacters.Remove(c);
		if (c.ownerId >= 0)
		{
			mPlayerCharacters[c.ownerId].Remove(c);
		}
		mDyingCharacters.Remove(c);
		c.Destroy();
	}

	public void AddMount(Character h, int ownerId)
	{
		mCharacters.Add(h);
		SetMount(ownerId, h);
	}

	public void KillMount(int ownerId)
	{
		KillCharacter(mMount[ownerId]);
		mMount[ownerId] = null;
	}

	public void SetMountActive(bool active, int ownerId)
	{
		if (mMount[ownerId] != null)
		{
			mMount[ownerId].isActive = active;
		}
	}

	public Character GetMount(int ownerId)
	{
		return mMount[ownerId];
	}

	public void SetMount(int ownerId, Character mount)
	{
		mMount[ownerId] = mount;
	}

	public void RegisterAttackStarted(int ownerId, bool isRanged)
	{
		if (ownerId == 1 && isRanged)
		{
			enemyRangedPreAttackDelay = 0.25f;
		}
		else if (ownerId == 1)
		{
			enemyMeleePreAttackDelay = 0.25f;
		}
		else if (isRanged)
		{
			allyRangedPreAttackDelay = 0.25f;
		}
		else
		{
			allyMeleePreAttackDelay = 0.25f;
		}
	}

	public Character GetBestRangedAttackTarget(Character attacker, GameRange range)
	{
		Character character = null;
		if (attacker == null)
		{
			return character;
		}
		List<Character> charactersInRange = GetCharactersInRange(range, 1 - attacker.ownerId);
		if (charactersInRange.Count == 0)
		{
			return character;
		}
		bool flag = !attacker.usesFocusedFire;
		float z = attacker.position.z;
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		bool flag2 = false;
		int num3 = int.MaxValue;
		bool onlyCorruptibleTargets = attacker.onlyCorruptibleTargets;
		foreach (Character item in charactersInRange)
		{
			if (!(item.health > 0f) || (onlyCorruptibleTargets && (item.isEnemy || item.isPlayer || string.IsNullOrEmpty(item.corruptionID) || ((item.ownerId != 0) ? (item.position.z >= WeakGlobalMonoBehavior<InGameImpl>.Instance.enemyGate.position.z - 1f) : (item.position.z <= WeakGlobalMonoBehavior<InGameImpl>.Instance.gate.position.z + 1f)))))
			{
				continue;
			}
			bool flag3 = false;
			bool flag4 = item.isFlying || item.isInKnockback || item.isInJump;
			float num4 = Mathf.Abs(item.position.z - z);
			int num5 = 0;
			if (flag)
			{
				num5 = item.DispersedAttackersCount(attacker);
			}
			if (character == null)
			{
				flag3 = true;
			}
			else
			{
				if (flag2 && !flag4)
				{
					continue;
				}
				if (flag4 && !flag2)
				{
					flag3 = true;
				}
				else
				{
					if (num4 > num + 0.45f)
					{
						continue;
					}
					if (flag && num5 < num3)
					{
						flag3 = true;
					}
					else if (num4 < num - 0.45f)
					{
						flag3 = true;
					}
					else
					{
						if (flag && num5 > num3)
						{
							continue;
						}
						if (item.health < num2)
						{
							flag3 = true;
						}
					}
				}
			}
			if (flag3)
			{
				character = item;
				num = num4;
				num2 = item.health;
				flag2 = item.isFlying;
				num3 = num5;
			}
		}
		return character;
	}

	public List<Character> GetCharactersInRange(GameRange range, int ownerId)
	{
		return GetCharactersInRange(range.left, range.right, ownerId);
	}

	public List<Character> GetCharactersInRange(float zMin, float zMax, int ownerId)
	{
		List<Character> list = new List<Character>();
		foreach (Character mCharacter in mCharacters)
		{
			if (((mCharacter.ownerId == ownerId && !mCharacter.enemyIgnoresMe) || ownerId < 0) && mCharacter.health > 0f && mCharacter.position.z >= zMin && mCharacter.position.z <= zMax)
			{
				list.Add(mCharacter);
			}
		}
		return list;
	}

	public List<Character> GetAlliesInRange(GameRange range, int ownerId)
	{
		return GetAlliesInRange(range.left, range.right, ownerId);
	}

	public List<Character> GetAlliesInRange(float zMin, float zMax, int ownerId)
	{
		List<Character> list = new List<Character>();
		foreach (Character mCharacter in mCharacters)
		{
			if (mCharacter.ownerId == ownerId && (!mCharacter.enemyIgnoresMe || mCharacter.isBlastoffFlyer) && mCharacter.health > 0f && mCharacter.position.z >= zMin && mCharacter.position.z <= zMax)
			{
				list.Add(mCharacter);
			}
		}
		return list;
	}

	public bool IsCharacterInRange(GameRange range, int ownerId, bool gateOnly, bool includeFlyers, bool injuredOnly, bool includeDecayingCharacters, bool onlyCorruptible)
	{
		return IsCharacterInRange(range.left, range.right, ownerId, gateOnly, includeFlyers, injuredOnly, includeDecayingCharacters, onlyCorruptible);
	}

	public bool IsCharacterInRange(float zMin, float zMax, int ownerId, bool gateOnly, bool includeFlyers, bool injuredOnly, bool includeDecayingCharacters, bool onlyCorruptible)
	{
		foreach (Character mCharacter in mCharacters)
		{
			float z = mCharacter.position.z;
			if ((mCharacter.ownerId == ownerId || ownerId < 0) && z >= zMin && z <= zMax && mCharacter.health > 0f && (includeFlyers || (!mCharacter.isInKnockback && !mCharacter.isInJump)) && (!gateOnly || mCharacter.isBase) && (includeFlyers || !mCharacter.isFlying) && (!injuredOnly || mCharacter.health < mCharacter.maxHealth || mCharacter.mountedHealth < mCharacter.mountedHealthMax) && (mCharacter.autoHealthRecovery >= 0f || includeDecayingCharacters) && (!onlyCorruptible || (!string.IsNullOrEmpty(mCharacter.corruptionID) && ((mCharacter.ownerId != 0) ? (z < WeakGlobalMonoBehavior<InGameImpl>.Instance.enemyGate.transform.position.z - 1f) : (z > WeakGlobalMonoBehavior<InGameImpl>.Instance.gate.transform.position.z + 1f)))))
			{
				return true;
			}
		}
		return false;
	}

	public List<Character> GetCharactersInRangeMaxCount(float zMin, float zMax, int ownerId, float maxNum)
	{
		List<Character> list = new List<Character>();
		foreach (Character mCharacter in mCharacters)
		{
			if ((mCharacter.ownerId == ownerId || ownerId < 0) && mCharacter.health > 0f && mCharacter.position.z >= zMin && mCharacter.position.z <= zMax)
			{
				list.Add(mCharacter);
				if ((maxNum -= 1f) < 0f)
				{
					break;
				}
			}
		}
		return list;
	}

	public void idleAll()
	{
		foreach (Character mCharacter in mCharacters)
		{
			mCharacter.controller.Idle();
		}
	}

	public float GetBestSpawnXPos(Vector3 spawnPos, float sizeOfSpawnArea, ELanePreference preference, bool isEnemy, bool isFlyer, bool isRanged)
	{
		if (preference == ELanePreference.center)
		{
			return WeakGlobalMonoBehavior<InGameImpl>.Instance.heroSpawnPoint.position.x;
		}
		if (sizeOfSpawnArea <= 0f)
		{
			return spawnPos.x;
		}
		float num = sizeOfSpawnArea / 2f;
		float num2 = spawnPos.x - num;
		float max = num2 + sizeOfSpawnArea;
		float num3 = 0f;
		if (preference == ELanePreference.back || preference == ELanePreference.front)
		{
			num3 = 750f;
		}
		int num4 = (int)Mathf.Ceil(sizeOfSpawnArea / 0.15f);
		float[] array = new float[num4];
		for (int i = 0; i < num4; i++)
		{
			array[i] = 0f;
		}
		int num5 = positionToLane(WeakGlobalMonoBehavior<InGameImpl>.Instance.heroSpawnPoint.position.x, num2, num4);
		if (!isEnemy)
		{
			array[num5] = 25000f;
			if (num5 > 0)
			{
				array[num5 - 1] = 10000f;
			}
			if (num5 < num4 - 1)
			{
				array[num5 + 1] = 10000f;
			}
			if (num5 > 1)
			{
				array[num5 - 2] = 2500f;
			}
		}
		foreach (Character mCharacter in mCharacters)
		{
			if (mCharacter == null || !(mCharacter.health > 0f) || mCharacter.isEnemy != isEnemy || mCharacter.isFlying != isFlyer || mCharacter.bowAttackRange > 0f != isRanged)
			{
				continue;
			}
			int num6 = positionToLane(mCharacter.controller.xPos, num2, num4);
			float num7 = 2500f;
			num7 -= Mathf.Abs(mCharacter.position.z - spawnPos.z);
			if (num7 > 0f)
			{
				array[num6] += num7;
				num7 *= 0.5f;
				if (num6 > 0)
				{
					array[num6 - 1] += num7;
				}
				if (num6 < num4 - 1)
				{
					array[num6 + 1] += num7;
				}
				num7 *= 0.5f;
				if (num6 > 1)
				{
					array[num6 - 2] += num7;
				}
				if (num6 < num4 - 2)
				{
					array[num6 + 2] += num7;
				}
			}
		}
		int num8 = num5;
		switch (preference)
		{
		case ELanePreference.back:
			num8 = 0;
			break;
		case ELanePreference.front:
			num8 = num4 - 1;
			break;
		}
		float num9 = array[num8];
		for (int j = 0; j < num4; j++)
		{
			int num10 = j;
			switch (preference)
			{
			case ELanePreference.front:
				num10 = num4 - 1 - j;
				break;
			default:
				num10 = num5;
				if (((uint)j & (true ? 1u : 0u)) != 0)
				{
					num10 -= (j + 1) / 2;
					if (num10 < 0)
					{
						num10 = j;
						preference = ELanePreference.back;
					}
				}
				else
				{
					num10 += (j + 1) / 2;
					if (num10 >= num4)
					{
						num10 = num4 - 1 - j;
						preference = ELanePreference.front;
					}
				}
				break;
			case ELanePreference.back:
				break;
			}
			num10 = Mathf.Clamp(num10, 0, num4 - 1);
			float num11 = array[num10] + num3 * (float)j;
			if (num11 < num9)
			{
				num8 = num10;
				num9 = num11;
			}
		}
		float num12 = num2 + 0.15f * (float)num8 - 0.07350001f;
		float max2 = num12 + 0.135f;
		return Mathf.Clamp(UnityEngine.Random.Range(num12, max2), num2, max);
	}

	private void UpdateBattle()
	{
		bool flag = WeakGlobalMonoBehavior<InGameImpl>.Instance.HasLuckCharm();
		for (int num = mInactiveCharacters.Count - 1; num >= 0; num--)
		{
			Character character = mInactiveCharacters[num];
			if (character.isActive)
			{
				mInactiveCharacters.Remove(character);
				mCharacters.Add(character);
			}
		}
		for (int num2 = mCharacters.Count - 1; num2 >= 0; num2--)
		{
			Character character2 = mCharacters[num2];
			if (!character2.isActive)
			{
				mInactiveCharacters.Add(character2);
				mCharacters.Remove(character2);
			}
			else if (character2.health <= 0f && !character2.isBase)
			{
				if (character2.ownerId != 0)
				{
					Singleton<Profile>.Instance.coins += character2.resourceDrops.guaranteedCoinsAward;
					Vector3 jointPosition = character2.controller.autoPaperdoll.GetJointPosition("impact_target");
					if (flag || (WeakGlobalInstance<WaveManager>.Instance != null && WeakGlobalInstance<WaveManager>.Instance.isDone && enemiesCount <= 1))
					{
						character2.resourceDrops.amountDropped.min = Mathf.Max(character2.resourceDrops.amountDropped.min, 1);
						character2.resourceDrops.amountDropped.max = Mathf.Max(character2.resourceDrops.amountDropped.min, character2.resourceDrops.amountDropped.max);
						WeakGlobalInstance<CollectableManager>.Instance.SpawnResources(character2.resourceDrops, jointPosition);
						if (flag)
						{
							SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/Confetti.prefab", 1);
							if (cachedResource != null)
							{
								GameObjectPool.DefaultObjectPool.Acquire(cachedResource.Resource as GameObject, jointPosition, Quaternion.identity);
							}
						}
					}
					WeakGlobalInstance<CollectableManager>.Instance.SpawnResources(character2.resourceDrops, jointPosition);
				}
				if (character2.ownerId != 0 && !character2.dynamicSpawn)
				{
					if (character2.isEnemy && WeakGlobalInstance<WaveManager>.Instance != null)
					{
						WeakGlobalInstance<WaveManager>.Instance.registerEnemyKilled(character2.uniqueID);
					}
					else if (WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(1) != null)
					{
						WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(1).UnitKilled();
					}
				}
				mDyingCharacters.Add(character2);
				mCharacters.RemoveAt(num2);
				if (character2.ownerId >= 0)
				{
					mPlayerCharacters[character2.ownerId].Remove(character2);
				}
			}
		}
		for (int num3 = mDyingCharacters.Count - 1; num3 >= 0; num3--)
		{
			Character character3 = mDyingCharacters[num3];
			if (character3.controller == null)
			{
				mDyingCharacters.RemoveAt(num3);
			}
			else if (character3.isOver && !character3.isPlayer && !character3.isBase)
			{
				character3.Destroy();
				mDyingCharacters.RemoveAt(num3);
			}
			else if (character3.health > 0f)
			{
				mCharacters.Add(character3);
				mDyingCharacters.RemoveAt(num3);
			}
		}
	}

	public void KillAllEnemies()
	{
		foreach (Character mCharacter in mCharacters)
		{
			if (mCharacter.isEnemy)
			{
				mCharacter.health = 0f;
			}
		}
	}

	public List<Character> GetPlayerCharacters(int playerId)
	{
		return mPlayerCharacters[playerId];
	}

	private int positionToLane(float xPos, float minX, int laneCount)
	{
		return Mathf.Clamp(Mathf.RoundToInt((xPos - minX) / 0.15f), 0, laneCount - 1);
	}
}
