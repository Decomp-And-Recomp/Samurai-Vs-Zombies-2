using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterData
{
	public string id;

	public string displayName;

	public string prefabPath;

	public DataBundleRecordKey record;

	public Texture2D HUDIcon;

	public Texture2D championIcon;

	public bool unique;

	public Leadership.LeadershipCost leadershipCost;

	public float health;

	public float speedMin;

	public float speedMax;

	public float baseSpeedMin;

	public float baseSpeedMax;

	public bool isFlying;

	public bool canMeleeFliers;

	public bool canMeleeProjectiles;

	public bool exploseOnMelee;

	public bool enemyIgnoresMe;

	public bool canBeEaten;

	public float meleeFreeze;

	public float autoHealthRecovery;

	public float swordAttackRange;

	public float bowAttackRange;

	public float meleeDamage;

	public float bowDamage;

	public float attackFrequency;

	public float baseAttackFrequency;

	public float damageBuffPercent;

	public float eatCooldown;

	public int leadershipCostModifierBuff;

	public int knockbackPower;

	public int knockbackResistance;

	public string projectile;

	public CharactersManager.ELanePreference lanePref;

	public float totalCooldown;

	public float currentCooldown;

	public GameObject meleeWeaponPrefab;

	public GameObject rangedWeaponPrefab;

	public bool bladedWeapon;

	public string upgradeAlliesFrom;

	public string upgradeAlliesTo;

	public List<string> spawnOnDeathTypes;

	public int spawnOnDeathNum;

	public string spawnFriendID;

	public bool gateRusher;

	public bool isBoss;

	public float damageMultiplier = 1f;

	public int resourceDropMax;

	public int resourceDropMin;

	public int award;

	public string resourceDropAlways = string.Empty;

	public DOTInfo dotInfo;

	public bool isEnemy;

	public bool isMount;

	public bool isMounted;

	public bool blocksHeroMovement;

	public bool isCharger;

	public CanBuffFunc canBuffFunc;

	public CanBuffFuncData canBuffFuncData;

	public int summonAchievementMask;

	public CharacterData(string id, int level)
	{
		HelperSchema helperSchema = Singleton<HelpersDatabase>.Instance[id];
		if (helperSchema != null)
		{
			Load(helperSchema, level);
			return;
		}
		EnemySchema enemySchema = Singleton<EnemiesDatabase>.Instance[id];
		if (enemySchema != null)
		{
			Load(enemySchema);
			return;
		}
		throw new Exception("Unknown character ID (neither a helper nor an enemy)");
	}

	public void Setup(Character c)
	{
		c.uniqueID = id;
		speedMax = baseSpeedMax;
		speedMin = baseSpeedMin;
		attackFrequency = baseAttackFrequency;
		if (c.ownerId != 0)
		{
			c.resourceDrops.amountDropped.max = resourceDropMax;
			c.resourceDrops.amountDropped.min = resourceDropMin;
			c.resourceDrops.guaranteedCoinsAward = award;
		}
		else if (WeakGlobalMonoBehavior<InGameImpl>.Instance.HasHasteCharm() && c.acceptsBuffs)
		{
			c.buffIcon.Show("Assets/Game/Resources/FX/SpeedIcon.prefab");
			float multiplier = Singleton<CharmsDatabase>.Instance[WeakGlobalMonoBehavior<InGameImpl>.Instance.activeCharm].multiplier;
			attackFrequency /= multiplier;
			speedMax *= multiplier;
			speedMin *= multiplier;
		}
		c.isUnique = unique;
		c.meleeAttackFrequency = attackFrequency;
		c.bowAttackFrequency = attackFrequency;
		c.meleeAttackRange = swordAttackRange;
		c.bowAttackRange = bowAttackRange;
		c.bowProjectile = projectile;
		c.meleeDamage = meleeDamage;
		c.bowDamage = bowDamage;
		c.knockbackPower = knockbackPower;
		c.knockbackResistance = knockbackResistance;
		c.isFlying = isFlying;
		c.canMeleeFliers = canMeleeFliers;
		c.canMeleeProjectiles = canMeleeProjectiles;
		c.autoHealthRecovery = autoHealthRecovery;
		c.exploseOnMelee = exploseOnMelee;
		c.enemyIgnoresMe = enemyIgnoresMe;
		c.meleeFreeze = meleeFreeze;
		c.controller.speed = UnityEngine.Random.value * (speedMax - speedMin) + speedMin;
		c.meleeWeaponIsABlade = bladedWeapon;
		c.spawnFriendID = spawnFriendID;
		c.maxHealth = health;
		c.health = health;
		c.damageBuffPercent = damageBuffPercent;
		c.canBuffFunc = canBuffFunc;
		c.upgradeAlliesFrom = upgradeAlliesFrom;
		c.upgradeAlliesTo = upgradeAlliesTo;
		c.isGateRusher = gateRusher;
		c.isBoss = isBoss;
		c.dotInfo = dotInfo;
		c.isMount = isMount;
		c.isMounted = isMounted;
		c.leadershipCostModifierBuff = leadershipCostModifierBuff;
		c.AssignCharData(this);
	}

	public void PreloadPrefab()
	{
	}

	private void Load(HelperSchema data, int level)
	{
		HelperLevelSchema level2 = data.GetLevel(level);
		id = data.id;
		displayName = data.displayName;
		record = data.resources;
		HUDIcon = data.TryGetHUDIcon();
		championIcon = data.TryGetChampionIcon();
		leadershipCost = new Leadership.LeadershipCost(data.resourcesCost);
		currentCooldown = data.cooldownTimer;
		health = level2.health;
		speedMin = (baseSpeedMin = level2.speedMin);
		speedMax = (baseSpeedMax = level2.speedMax);
		unique = data.unique;
		isMount = data.isMount;
		isMounted = data.isMounted;
		canMeleeFliers = data.canMeleeFliers;
		canMeleeProjectiles = data.canMeleeProjectiles;
		exploseOnMelee = data.exploseOnMelee;
		enemyIgnoresMe = data.enemyIgnoresMe;
		canBeEaten = data.canBeEaten;
		BuffSchema buffSchema = level2.buffSchema;
		if (buffSchema != null)
		{
			damageBuffPercent = buffSchema.damageBuffPercent;
			leadershipCostModifierBuff = buffSchema.leadershipCostModifier;
			canBuffFunc = buffSchema.CanBuff;
			canBuffFuncData = buffSchema.CanBuff;
		}
		meleeWeaponPrefab = data.meleeWeaponPrefab;
		rangedWeaponPrefab = data.rangedWeaponPrefab;
		bladedWeapon = data.usesBladeWeapon;
		attackFrequency = (baseAttackFrequency = data.attackFrequency);
		knockbackPower = level2.knockbackPower;
		knockbackResistance = level2.knockbackResistance;
		totalCooldown = data.cooldownTimer;
		swordAttackRange = level2.meleeRange;
		meleeDamage = level2.meleeDamage;
		bowAttackRange = level2.bowRange;
		bowDamage = level2.bowDamage;
		blocksHeroMovement = data.blocksHeroMovement;
		isCharger = data.isCharger;
		award = data.award;
		resourceDropMax = data.resourceDropMax;
		resourceDropMin = data.resourceDropMin;
		summonAchievementMask = data.summonIndex;
		if (!DataBundleRecordKey.IsNullOrEmpty(level2.projectile))
		{
			projectile = level2.projectile.Key;
		}
		if (!DataBundleRecordKey.IsNullOrEmpty(data.lane))
		{
			lanePref = (CharactersManager.ELanePreference)(int)Enum.Parse(typeof(CharactersManager.ELanePreference), data.lane.Key);
		}
		if (!DataBundleRecordKey.IsNullOrEmpty(level2.upgradeAlliesFrom))
		{
			upgradeAlliesFrom = level2.upgradeAlliesFrom.Key;
		}
		if (!DataBundleRecordKey.IsNullOrEmpty(level2.upgradeAlliesTo))
		{
			upgradeAlliesTo = level2.upgradeAlliesTo.Key;
		}
	}

	private void Load(EnemySchema data)
	{
		id = data.id;
		displayName = data.displayName;
		record = data.resources;
		health = data.health;
		speedMin = (baseSpeedMin = data.speedMin);
		speedMax = (baseSpeedMax = data.speedMax);
		gateRusher = data.gateRusher;
		isBoss = data.boss;
		isFlying = data.flying;
		canMeleeFliers = data.canMeleeFliers;
		exploseOnMelee = data.exploseOnMelee;
		enemyIgnoresMe = data.enemyIgnoresMe;
		damageBuffPercent = data.damageBuffPercent;
		meleeWeaponPrefab = data.meleeWeaponPrefab;
		rangedWeaponPrefab = data.rangedWeaponPrefab;
		bladedWeapon = data.usesBladeWeapon;
		attackFrequency = (baseAttackFrequency = data.attackFrequency);
		knockbackPower = data.knockbackPower;
		knockbackResistance = data.knockbackResistance;
		swordAttackRange = data.meleeRange;
		meleeDamage = data.meleeDamage;
		bowAttackRange = data.bowRange;
		bowDamage = data.bowDamage;
		blocksHeroMovement = data.blocksHeroMovement;
		resourceDropMax = data.resourceDropMax;
		resourceDropMin = data.resourceDropMin;
		award = data.award;
		eatCooldown = data.eatCooldown;
		if (!DataBundleRecordKey.IsNullOrEmpty(data.spawnOnDeath))
		{
			spawnOnDeathTypes = new List<string>(1);
			spawnOnDeathTypes.Add(data.spawnOnDeath.Key.ToString());
		}
		spawnOnDeathNum = data.spawnOnDeathCount;
		if (WeakGlobalInstance<WaveManager>.Instance != null)
		{
			DataBundleTableHandle<EnemySwapSchema> deathSwapData = WeakGlobalInstance<WaveManager>.Instance.GetDeathSwapData();
			EnemySwapSchema[] data2 = deathSwapData.Data;
			EnemySwapSchema[] array = data2;
			foreach (EnemySwapSchema enemySwapSchema in array)
			{
				string text = enemySwapSchema.swapFrom.Key.ToString();
				if (text == id)
				{
					if (spawnOnDeathTypes == null)
					{
						spawnOnDeathTypes = new List<string>(1);
					}
					spawnOnDeathTypes.Add(enemySwapSchema.swapTo.Key.ToString());
				}
			}
		}
		if (!DataBundleRecordKey.IsNullOrEmpty(data.projectile))
		{
			projectile = data.projectile.Key;
		}
		if (!DataBundleRecordKey.IsNullOrEmpty(data.lane))
		{
			lanePref = (CharactersManager.ELanePreference)(int)Enum.Parse(typeof(CharactersManager.ELanePreference), data.lane.Key);
		}
		isEnemy = true;
	}
}
