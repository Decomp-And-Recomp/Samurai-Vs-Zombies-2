using System;
using System.Collections.Generic;
using UnityEngine;

public class Helper : Character
{
	private static GameObjectPool characterPool = new GameObjectPool();

	private static float kChargeRange = 5f;

	public static int kPlatinumLevel = 10;

	private DataBundleRecordHandle<MaterialLookupSchema> materialLookupHandle { get; set; }

	public GameRange chargeAttackGameRange
	{
		get
		{
			if (base.controller.facing == FacingType.Left)
			{
				return new GameRange(base.position.z - kChargeRange, base.position.z);
			}
			return new GameRange(base.position.z, base.position.z + kChargeRange);
		}
	}

	public bool IsGolden { get; set; }

	public Helper(CharacterData data, float zTarget, Vector3 pos, int playerId)
	{
		base.ownerId = playerId;
		mIsLeftToRightGameplay = Singleton<PlayModesManager>.Instance.gameDirection == PlayModesManager.GameDirection.LeftToRight;
		if (playerId == 1)
		{
			mIsLeftToRightGameplay = !mIsLeftToRightGameplay;
		}
		base.id = data.id;
		InitializeModel(data);
		base.controller.position = pos;
		if (mIsLeftToRightGameplay)
		{
			base.controller.constraintLeft = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(playerId).helperSpawnArea.transform.position.z;
			base.controller.constraintRight = zTarget;
		}
		else
		{
			base.controller.constraintLeft = zTarget;
			base.controller.constraintRight = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(playerId).helperSpawnArea.transform.position.z;
		}
		StartWalkingTowardGoal();
		if (WeakGlobalInstance<WaveManager>.Instance != null)
		{
			HelperEnemySwapSchema[] helperEnemySwaps = WeakGlobalInstance<WaveManager>.Instance.helperEnemySwaps;
			HelperEnemySwapSchema[] array = helperEnemySwaps;
			foreach (HelperEnemySwapSchema helperEnemySwapSchema in array)
			{
				if (helperEnemySwapSchema.helperSwapFrom == (DataBundleRecordKey)base.id)
				{
					base.corruptionID = helperEnemySwapSchema.enemySwapTo;
				}
			}
		}
		base.BlocksHeroMovement = data.blocksHeroMovement;
		SetMirroredSkeleton(!mIsLeftToRightGameplay);
		SetDefaultFacing();
	}

	public void ReinitializeModel(CharacterData data)
	{
		Vector3 vector = base.controller.position;
		vector.x = base.controller.xPos;
		float constraintLeft = base.controller.constraintLeft;
		float constraintRight = base.controller.constraintRight;
		InitializeModel(data);
		base.controller.position = vector;
		base.controller.constraintLeft = constraintLeft;
		base.controller.constraintRight = constraintRight;
		StartWalkingTowardGoal();
	}

	private void InitializeModel(CharacterData data)
	{
		string text = data.id;
		if (WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			GameObject characterObject = Singleton<HelpersDatabase>.Instance.GetCharacterObject(text, base.ownerId);
			if (characterObject == null)
			{
				base.controlledObject = CharacterSchema.Deserialize(data.record);
			}
			else
			{
				base.controlledObject = characterPool.Acquire(characterObject);
			}
			base.controlledObject.transform.localScale = Vector3.one;
			CheckForSpecialControllerActions();
		}
		else
		{
			base.controlledObject = CharacterSchema.Deserialize(data.record);
		}
		base.controlledObject.SetActive(true);
		if (data.rangedWeaponPrefab != null)
		{
			base.rangedWeaponPrefab = data.rangedWeaponPrefab;
		}
		if (data.meleeWeaponPrefab != null)
		{
			base.meleeWeaponPrefab = data.meleeWeaponPrefab;
			if (base.ownerId != 0 && WeakGlobalMonoBehavior<InGameImpl>.Instance.HasPeaceCharm())
			{
				SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/Props/Weapons/chickensword.prefab", 1);
				if (cachedResource != null)
				{
					base.meleeWeaponPrefab = cachedResource.Resource as GameObject;
				}
			}
		}
		if (base.bowAttackRange > 0f && base.meleeAttackRange == 0f)
		{
			SetRangeAttackMode(true);
		}
		if (data.isCharger)
		{
			ChargeWalk();
		}
	}

	private void OnChargeMove(float oldZ, float newZ)
	{
		List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(Mathf.Min(oldZ, newZ), Mathf.Max(oldZ, newZ), 1 - base.ownerId);
		int num = base.knockbackPower;
		foreach (Character item in charactersInRange)
		{
			if (CheckIfTargetValid(item) && (!item.isFlying || base.canMeleeFliers))
			{
				EAttackType attackType = EAttackType.Blunt;
				if (base.meleeWeaponIsABlade)
				{
					attackType = EAttackType.Blade;
				}
				else if (base.exploseOnMelee)
				{
					attackType = EAttackType.Explosion;
				}
				PerformKnockback(item, num, false, Vector3.zero);
				item.RecievedAttack(attackType, meleeDamage, this);
				num -= 100;
			}
		}
	}

	protected void OnChargeFly()
	{
		base.controller.ChargeWithAnim("fly");
		base.controller.onChargeMove = OnChargeMove;
	}

	protected void ChargeWalk()
	{
		base.controller.isCharging = true;
		base.controller.onChargeMove = OnChargeMove;
	}

	public override void Update()
	{
		base.Update();
		if (base.health == 0f || !base.isEffectivelyIdle || base.controller.impactPauseTime > 0f)
		{
			return;
		}
		float z = base.controller.position.z;
		bool flag = ((base.controller.facing != FacingType.Right) ? (z <= base.controller.constraintLeft) : (z >= base.controller.constraintRight));
		if (base.controller.isCharging)
		{
			if (flag)
			{
				base.health = 0f;
			}
		}
		else if (WeakGlobalMonoBehavior<InGameImpl>.Instance.gameOver)
		{
			if ((base.ownerId == 0 && WeakGlobalMonoBehavior<InGameImpl>.Instance.playerWon) || (base.ownerId != 0 && WeakGlobalMonoBehavior<InGameImpl>.Instance.enemiesWon))
			{
				base.controller.PlayVictoryAnim();
			}
			else if (Singleton<Profile>.Instance.inMultiplayerWave)
			{
				base.controller.Die();
			}
			else
			{
				base.controller.StopWalking();
			}
		}
		else if (base.isBlastoffFlyer && IsInChargeRangeOfOpponent())
		{
			SetRangeAttackMode(false);
			base.controller.PerformSpecialActionNoAttack("blastoff", OnChargeFly);
		}
		else if (base.canMeleeAttack && (IsInMeleeRangeOfOpponent(true) || (base.canMeleeProjectiles && IsInMeleeRangeOfProjectile())))
		{
			base.controller.SetUseCowerIdle(false);
			SetRangeAttackMode(false);
			if (base.exploseOnMelee)
			{
				AttackByExploding();
				return;
			}
			StartMeleeAttackDelayTimer();
			base.controller.Attack(base.meleeAttackFrequency);
		}
		else if ((base.isSpellCasterOnAllies && base.canUseRangedAttack && IsInRangeOfHurtAlly(false)) || (base.canUseRangedAttack && base.stats.projectile == "SpawnFriend" && IsInBowRangeOfOpponent()))
		{
			StartRangedAttackDelayTimer();
			if (base.bowProjectile == "SpawnFriend")
			{
				SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/DivineEnemy.prefab", 1);
				if (cachedResource != null)
				{
					GameObjectPool.DefaultObjectPool.Acquire(cachedResource.Resource as GameObject, base.position + new Vector3(50f, 0f, 0f), Quaternion.identity);
				}
				base.controller.PerformSpecialAction("cast", base.OnCastSpawnAllies, base.bowAttackFrequency / 2f);
			}
			else
			{
				base.controller.PerformSpecialAction("cast", base.OnCastHeal, base.bowAttackFrequency);
			}
		}
		else if (base.canUseRangedAttack && base.hasRangedAttack && IsInBowRangeOfOpponent())
		{
			SetRangeAttackMode(true);
			StartRangedAttackDelayTimer();
			base.singleAttackTarget = WeakGlobalInstance<CharactersManager>.Instance.GetBestRangedAttackTarget(this, base.bowAttackExtGameRange);
			if (base.singleAttackTarget != null)
			{
				base.controller.RangeAttack(base.bowAttackFrequency);
			}
		}
		else if (IsInBowRangeOfOpponent() && base.canMeleeProjectiles && base.timeSinceDamaged > 3f)
		{
			base.controller.SetUseCowerIdle(false);
			base.controller.Idle();
		}
		else if (IsInMeleeRangeOfOpponent(false))
		{
			base.controller.SetUseCowerIdle(!IsInMeleeRangeOfOpponent(true));
			base.controller.Idle();
		}
		else if (!base.canMove || flag || (IsInBowRangeOfOpponent() && !base.canMeleeProjectiles) || (base.isSpellCasterOnAllies && IsInRangeOfHurtAlly()) || (base.isMount && ((!base.LeftToRight) ? (base.transform.position.z - WeakGlobalMonoBehavior<InGameImpl>.Instance.GetHero(base.ownerId).position.z < 0f) : (WeakGlobalMonoBehavior<InGameImpl>.Instance.GetHero(base.ownerId).position.z - base.transform.position.z < 0f))))
		{
			base.controller.SetUseCowerIdle(false);
			base.controller.Idle();
			if (base.exploseOnMelee)
			{
				AttackByExploding();
			}
		}
		else
		{
			base.controller.SetUseCowerIdle(false);
			StartWalkingTowardGoal();
		}
	}

	public override void Destroy()
	{
		if (WeakGlobalMonoBehavior<InGameImpl>.Exists && base.id != "Mount_Balanced")
		{
			GameObject gameObject = base.controlledObject;
			Transform transform = gameObject.transform;
			int childCount = transform.GetChildCount();
			string text = Singleton<HelpersDatabase>.Instance.ModelName(base.id, base.ownerId);
			for (int i = 0; i < childCount; i++)
			{
				Transform child = transform.GetChild(0);
				if (child.name != text)
				{
					GameObjectPool.DefaultObjectPool.Release(child.gameObject);
				}
			}
			characterPool.Release(base.controlledObject);
			base.Destroy();
		}
		else
		{
			GameObject obj = base.controlledObject;
			base.Destroy();
			UnityEngine.Object.Destroy(obj);
		}
		if (materialLookupHandle != null)
		{
			materialLookupHandle.Unload();
		}
	}

	public void SetSpawnOnDeath(List<string> spawnHelperIDs, int num)
	{
		base.onDeathEvent = (Action)Delegate.Combine(base.onDeathEvent, (Action)delegate
		{
			SpawnOffspringMulti(spawnHelperIDs, num);
		});
	}

	private void StartWalkingTowardGoal()
	{
		if (mIsLeftToRightGameplay)
		{
			base.controller.StartWalkRight();
		}
		else
		{
			base.controller.StartWalkLeft();
		}
	}

	private void CheckForSpecialControllerActions()
	{
		base.isBlastoffFlyer = base.controller.HasAnim("blastoff") && base.controller.HasAnim("fly");
	}

	protected bool IsInChargeRangeOfOpponent()
	{
		return base.meleeAttackRange > 0f && WeakGlobalInstance<CharactersManager>.Instance.IsCharacterInRange(chargeAttackGameRange, 1 - base.ownerId, base.isGateRusher, base.canMeleeFliers, false, true, false);
	}

	protected override void OnDied()
	{
		WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(base.ownerId).CheckForLeadershipCostBuff(base.id, (int leadership, int modifier) => leadership - modifier);
	}
}
