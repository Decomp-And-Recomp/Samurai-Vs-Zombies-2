using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
	private static GameObjectPool characterPool = new GameObjectPool();

	protected float mEatCooldownTimer;

	protected Character eatTarget { get; set; }

	public Enemy(CharacterData data, float zTarget, Vector3 pos, int playerId)
	{
		base.isEnemy = true;
		base.ownerId = playerId;
		mIsLeftToRightGameplay = Singleton<PlayModesManager>.Instance.gameDirection == PlayModesManager.GameDirection.LeftToRight;
		if (playerId != 0)
		{
			mIsLeftToRightGameplay = !mIsLeftToRightGameplay;
		}
		base.id = data.id;
		if (WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			GameObject characterObject = Singleton<EnemiesDatabase>.Instance.GetCharacterObject(data.id);
			if (characterObject == null)
			{
				base.controlledObject = CharacterSchema.Deserialize(data.record);
			}
			else
			{
				base.controlledObject = characterPool.Acquire(characterObject);
			}
			base.controlledObject.transform.localScale = Vector3.one;
		}
		else
		{
			GameObject characterObject2 = Singleton<HelpersDatabase>.Instance.GetCharacterObject(data.id, playerId);
			if (characterObject2 != null)
			{
				base.controlledObject = characterPool.Acquire(characterObject2);
			}
			else
			{
				base.controlledObject = CharacterSchema.Deserialize(data.record);
			}
		}
		base.controlledObject.SetActive(true);
		if (!string.IsNullOrEmpty(Singleton<Profile>.Instance.playModeSubSection))
		{
			SoundThemePlayer component = base.controlledObject.GetComponent<SoundThemePlayer>();
			if (component != null)
			{
				component.autoPlayEvent = string.Empty;
			}
		}
		base.controller.position = pos;
		if (WeakGlobalInstance<WaveManager>.Instance != null && WeakGlobalInstance<WaveManager>.Instance.enemiesSpawnArea != null && playerId == 1)
		{
			if (mIsLeftToRightGameplay)
			{
				base.controller.constraintRight = zTarget;
				base.controller.constraintLeft = WeakGlobalInstance<WaveManager>.Instance.enemiesSpawnArea.transform.position.z;
			}
			else
			{
				base.controller.constraintRight = WeakGlobalInstance<WaveManager>.Instance.enemiesSpawnArea.transform.position.z;
				base.controller.constraintLeft = zTarget;
			}
			StartWalkingTowardGoal();
		}
		else if (WeakGlobalMonoBehavior<InGameImpl>.Instance != null && WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(playerId) != null)
		{
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
		}
		if (base.bowAttackRange > 0f && base.meleeAttackRange == 0f)
		{
			SetRangeAttackMode(true);
		}
		base.BlocksHeroMovement = data.blocksHeroMovement;
		ActivateHealthBar();
		SetMirroredSkeleton((mIsLeftToRightGameplay && data.isEnemy) || (!mIsLeftToRightGameplay && !data.isEnemy));
		SetDefaultFacing();
	}

	public override void Update()
	{
		base.Update();
		if (base.charData != null && base.charData.eatCooldown > 0f)
		{
			mEatCooldownTimer += Time.deltaTime;
		}
		if (base.health == 0f || !base.isEffectivelyIdle || base.controller.impactPauseTime > 0f)
		{
			return;
		}
		bool flag = ((!mIsLeftToRightGameplay) ? (base.controller.position.z <= base.controller.constraintLeft) : (base.controller.position.z >= base.controller.constraintRight));
		if (base.canMeleeAttack && TryEating())
		{
			return;
		}
		if (base.canMeleeAttack && IsInMeleeRangeOfOpponent(true))
		{
			SetRangeAttackMode(false);
			if (base.exploseOnMelee)
			{
				AttackByExploding();
				return;
			}
			StartMeleeAttackDelayTimer();
			base.controller.Attack(base.meleeAttackFrequency);
		}
		else if (base.isSpellCasterOnAllies && base.canUseRangedAttack && IsInRangeOfHurtAlly())
		{
			StartRangedAttackDelayTimer();
			if (base.bowProjectile == "SpawnFriend")
			{
				base.controller.PerformSpecialAction("cast", base.OnCastSpawnAllies, base.bowAttackFrequency);
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
		else if (!base.canMove || flag || IsInMeleeRangeOfOpponent(false) || IsInBowRangeOfOpponent() || (base.isSpellCasterOnAllies && IsInRangeOfHurtAlly()))
		{
			base.controller.Idle();
		}
		else if (WeakGlobalMonoBehavior<InGameImpl>.Instance.gameOver)
		{
			if ((base.ownerId == 0 && WeakGlobalMonoBehavior<InGameImpl>.Instance.playerWon) || (base.ownerId != 0 && WeakGlobalMonoBehavior<InGameImpl>.Instance.enemiesWon))
			{
				base.controller.PlayVictoryAnim();
			}
			else
			{
				base.controller.StopWalking();
			}
		}
		else
		{
			StartWalkingTowardGoal();
		}
	}

	public override void Destroy()
	{
		if (WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			GameObject gameObject = base.controlledObject;
			Transform transform = gameObject.transform;
			int childCount = transform.GetChildCount();
			string text = Singleton<EnemiesDatabase>.Instance.ModelName(base.id);
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
	}

	public void SetupFromCharacterData(CharacterData data)
	{
		if (data.rangedWeaponPrefab != null)
		{
			base.rangedWeaponPrefab = data.rangedWeaponPrefab;
		}
		if (WeakGlobalMonoBehavior<InGameImpl>.Instance.HasPeaceCharm() && base.ownerId != 0)
		{
			if (data.meleeWeaponPrefab != null)
			{
				SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/Props/Weapons/chickensword.prefab", 1);
				if (cachedResource != null)
				{
					base.meleeWeaponPrefab = cachedResource.Resource as GameObject;
				}
			}
		}
		else
		{
			base.meleeWeaponPrefab = data.meleeWeaponPrefab;
		}
		if (data.spawnOnDeathTypes != null && data.spawnOnDeathTypes.Count > 0)
		{
			base.onDeathEvent = (Action)Delegate.Combine(base.onDeathEvent, (Action)delegate
			{
				SpawnOffspringMulti(data.spawnOnDeathTypes, data.spawnOnDeathNum);
			});
		}
	}

	private bool TryEating()
	{
		bool result = false;
		if (base.charData != null && base.charData.eatCooldown > 0f && mEatCooldownTimer >= base.charData.eatCooldown)
		{
			List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(base.meleeAttackHitGameRange, 1 - base.ownerId);
			float num = float.MaxValue;
			Character character = null;
			float num2 = ((base.controller.facing != FacingType.Right) ? base.meleeAttackHitGameRange.left : base.meleeAttackHitGameRange.right);
			foreach (Character item in charactersInRange)
			{
				float num3 = Mathf.Abs(item.transform.position.z - num2);
				if (num3 < num && item.charData != null && item.charData.canBeEaten)
				{
					num = num3;
					character = item;
				}
			}
			if (character != null)
			{
				eatTarget = character;
				eatTarget.isActive = false;
				eatTarget.controller.enabled = true;
				eatTarget.controller.SetUseCowerIdle(true);
				eatTarget.controller.Idle();
				eatTarget.StopMoving(5f);
				base.controller.target = character.controller;
				base.controller.PerformSpecialAction("eat", OnEatAttack);
				base.impactPauseSuspended = true;
				mEatCooldownTimer = 0f;
				result = true;
			}
		}
		return result;
	}

	private void OnEatAttack()
	{
		base.impactPauseSuspended = false;
		if (eatTarget != null)
		{
			Renderer[] componentsInChildren = eatTarget.controlledObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.enabled = false;
			}
			eatTarget.controller.SoundPlayer.PlaySoundEvent("Death");
			base.controller.DetachTarget();
			if (base.controller.target == eatTarget.controller)
			{
				base.controller.target = null;
			}
			eatTarget.health = 0f;
			eatTarget.isActive = true;
			eatTarget = null;
		}
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
}
