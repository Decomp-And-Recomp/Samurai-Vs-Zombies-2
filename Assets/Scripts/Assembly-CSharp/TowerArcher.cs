using UnityEngine;

public class TowerArcher : Character
{
	private DataBundleRecordHandle<CharacterSchema> resources;

	public TowerArcher(string characterRecordName, Vector3 pos, GameObject weaponProp, string projectileType, float damage, float range, float attackFrequency, bool againstPlayer)
	{
		CharacterStats characterStats = new CharacterStats
		{
			maxHealth = 100f,
			health = 100f,
			bowAttackFrequency = attackFrequency,
			bowAttackDamage = damage,
			bowAttackRange = range,
			meleeAttackRange = 1.5f,
			projectile = projectileType,
			isEnemy = againstPlayer
		};
		InitializeModel(characterRecordName);
		base.controller.position = pos;
		base.controller.facing = FacingType.Right;
		base.controller.snapToGround = false;
		base.stats = characterStats;
		base.ownerId = 0;
		if (Singleton<Profile>.Instance.inVSMultiplayerWave && Singleton<PlayModesManager>.Instance.Attacking)
		{
			base.ownerId = 1;
		}
		if (weaponProp != null)
		{
			base.rangedWeaponPrefab = weaponProp;
		}
		SetRangeAttackMode(true);
	}

	private void InitializeModel(string characterRecordName)
	{
		resources = new DataBundleRecordHandle<CharacterSchema>(characterRecordName);
		resources.Load(DataBundleResourceGroup.InGame, true, delegate(CharacterSchema schema)
		{
			schema.Initialize("Character");
			base.controlledObject = CharacterSchema.Deserialize(schema);
		});
	}

	public override void Update()
	{
		if (base.controlledObject == null)
		{
			return;
		}
		base.Update();
		if (base.health == 0f || !base.isEffectivelyIdle)
		{
			return;
		}
		if (WeakGlobalMonoBehavior<InGameImpl>.Instance.gameOver)
		{
			if (WeakGlobalMonoBehavior<InGameImpl>.Instance.playerWon != Singleton<PlayModesManager>.Instance.Attacking)
			{
				base.controller.PlayVictoryAnim();
			}
			else if (Singleton<Profile>.Instance.inVSMultiplayerWave)
			{
				base.controller.Die();
			}
		}
		else if (base.canUseRangedAttack && IsInBowRangeOfOpponent())
		{
			StartRangedAttackDelayTimer();
			base.singleAttackTarget = WeakGlobalInstance<CharactersManager>.Instance.GetBestRangedAttackTarget(this, base.bowAttackGameRange);
			base.controller.RangeAttack(base.bowAttackFrequency);
		}
	}

	public void UnloadData()
	{
		resources.Unload();
	}

	public override void RecievedAttack(EAttackType attackType, float damage, Character attacker, bool canReflect)
	{
	}
}
