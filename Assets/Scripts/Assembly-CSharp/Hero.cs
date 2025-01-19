using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : Character
{
	private const float kChickenSwordDuration = 15f;

	private const float kMinLevel = 0.4f;

	private const float kMaxLevel = 0.6f;

	private const float kPulseSpeed = 1f;

	private static readonly float kEnemyHeroKeepDist = 0.9f;

	private GluiSprite mBloodEffectSprite;

	private GameObject mHudBloodEffect;

	private bool mBloodEffectActive;

	private float FSBloodEffectFadeLevel;

	private float mPulseDir = 1f;

	private HeroControls mPlayerControls;

	private bool mPerformingSpecialAttack;

	private string mHealthBarToShow = "Sprites/HUD/life_bar_green";

	private HeroSchema mainData;

	private GameObject mMPMeleeWeaponUpgradePrefab;

	private GameObject mMPRangedWeaponUpgradePrefab;

	private ArmorLevelSchema mCharacterArmorSchema;

	private ArmorLevelSchema mCollectionArmorSchema;

	private float mLeftConstraint;

	private float mRightConstraint;

	private DataBundleRecordHandle<MaterialLookupSchema> matLookupHandle { get; set; }

	public GameObject HUDBloodEffect
	{
		get
		{
			return mHudBloodEffect;
		}
		set
		{
			mHudBloodEffect = value;
			mBloodEffectSprite = mHudBloodEffect.GetComponent<GluiSprite>();
			mHudBloodEffect.SetActive(false);
		}
	}

	public string healthBarFileToUse
	{
		get
		{
			return mHealthBarToShow;
		}
	}

	public bool canUseSpecialAttack
	{
		get
		{
			return base.health > 0f && !mPerformingSpecialAttack && !base.controller.isInHurtState;
		}
	}

	private float meleeDamageModifier { get; set; }

	private float rangedDamageModifier { get; set; }

	public override float meleeDamage
	{
		get
		{
			return base.meleeDamage * meleeDamageModifier;
		}
	}

	public override float bowDamage
	{
		get
		{
			return base.bowDamage * meleeDamageModifier;
		}
	}

	public GameObject mountObject { get; private set; }

	public GameObject horseModel { get; private set; }

	public GameObject heroModel { get; private set; }

	public GameObject heroObject { get; private set; }

	public float mountedHealRate { get; private set; }

	public bool NoAttack { get; set; }

	public Hero(Transform spawnPoint, int playerId)
	{
		string heroID = Singleton<Profile>.Instance.heroID;
		if (playerId != 0)
		{
			heroID = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.heroId;
		}
		Init(spawnPoint.position, playerId, heroID, playerId == 0);
	}

	public Hero(Vector3 spawnPoint, int playerId, string heroID, bool isLocalPlayer)
	{
		Init(spawnPoint, playerId, heroID, isLocalPlayer);
	}

	public static CharacterStats GetHeroStats(HeroSchema mainData, int ownerId)
	{
		if (mainData == null)
		{
			mainData = Singleton<HeroesDatabase>.Instance[Singleton<Profile>.Instance.heroID];
		}
		int heroLevel = Singleton<Profile>.Instance.heroLevel;
		int level = Singleton<Profile>.Instance.swordLevel;
		int bowLevel = Singleton<Profile>.Instance.bowLevel;
		if (ownerId != 0)
		{
			heroLevel = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.heroLevel;
			level = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.meleeLevel;
			bowLevel = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.bowLevel;
		}
		CharacterStats result = default(CharacterStats);
		result.isPlayer = true;
		result.knockbackResistance = 100;
		result.maxHealth = mainData.MaxHealth(heroLevel);
		result.health = result.maxHealth;
		result.autoHealthRecovery = mainData.HealthRecovery(heroLevel);
		result.speed = mainData.GetLevel(heroLevel).speed;
		WeaponSchema meleeWeapon = mainData.MeleeWeapon;
		result.meleeWeaponIsABlade = meleeWeapon.isBladeWeapon;
		result.meleeAttackRange = meleeWeapon.GetLevel(level).attackRange;
		result.meleeAttackDamage += meleeWeapon.Damage(level);
		result.meleeAttackFrequency = meleeWeapon.GetLevel(level).attackFrequency;
		result.knockbackPower = meleeWeapon.GetLevel(level).knockbackPower;
		switch ((ownerId != 0) ? Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.swordsCollected : Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Sword"))
		{
		case 1:
			result.meleeAttackDamage *= 1.05f;
			break;
		case 2:
			result.meleeAttackDamage *= 1.1f;
			break;
		case 3:
			result.meleeAttackDamage *= 1.2f;
			break;
		}
		bool flag = Singleton<PlayModesManager>.Instance.selectedModeData.allowBowOnFirstWave1;
		if (!flag)
		{
			flag = Singleton<Profile>.Instance.waveAllowBow;
		}
		if (flag)
		{
			WeaponSchema rangedWeapon = mainData.RangedWeapon;
			result.bowAttackRange = rangedWeapon.GetLevel(bowLevel).attackRange;
			result.bowAttackDamage = rangedWeapon.Damage(bowLevel);
			result.bowAttackFrequency = rangedWeapon.GetLevel(bowLevel).attackFrequency;
			result.projectile = rangedWeapon.GetLevel(bowLevel).projectile.Key;
			result.knockbackPowerRanged = rangedWeapon.GetLevel(bowLevel).knockbackPower;
			DOTInfo dOTInfo = default(DOTInfo);
			dOTInfo.ratio = rangedWeapon.GetLevel(bowLevel).DOTDamageRatio;
			dOTInfo.duration = rangedWeapon.GetLevel(bowLevel).DOTDuration;
			dOTInfo.interval = rangedWeapon.GetLevel(bowLevel).DOTInterval;
			result.dotInfo = dOTInfo;
			switch ((ownerId != 0) ? Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.bowsCollected : Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("bow"))
			{
			case 1:
				result.bowAttackDamage *= 1.05f;
				break;
			case 2:
				result.bowAttackDamage *= 1.1f;
				break;
			case 3:
				result.bowAttackDamage *= 1.2f;
				break;
			}
		}
		if (ownerId == 0 && WeakGlobalMonoBehavior<InGameImpl>.Instance != null && WeakGlobalMonoBehavior<InGameImpl>.Instance.HasPowerCharm())
		{
			CharmSchema charmSchema = Singleton<CharmsDatabase>.Instance[WeakGlobalMonoBehavior<InGameImpl>.Instance.activeCharm];
			result.criticalChance = charmSchema.criticalChance;
			result.criticalMultiplier = charmSchema.multiplier;
		}
		else
		{
			result.criticalChance = Singleton<Config>.Instance.data.GetFloat(TextDBSchema.ChildKey("DefaultCriticals", "chance"));
			result.criticalMultiplier = Singleton<Config>.Instance.data.GetFloat(TextDBSchema.ChildKey("DefaultCriticals", "multiplier"));
		}
		result.canMeleeFliers = mainData.canMeleeFliers;
		if (ownerId != 0 && Singleton<Profile>.Instance.MultiplayerData.TweakValues != null)
		{
			result.meleeAttackDamage *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.heroMeleeDamage;
			result.bowAttackDamage *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.heroRangedDamage;
			result.speed *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.heroMoveSpeed;
			result.health *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.heroHealth;
			result.maxHealth *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.heroHealth;
			result.autoHealthRecovery *= Singleton<Profile>.Instance.MultiplayerData.TweakValues.heroHealthRegen;
		}
		return result;
	}

	public static string MaterialKey(string heroID, int collectionLevel)
	{
		switch (collectionLevel)
		{
		case 1:
		case 2:
		case 3:
			return DataBundleRuntime.TableRecordKey("HeroMaterials", heroID + "_ArmorSet");
		default:
			return DataBundleRuntime.TableRecordKey("HeroMaterials", heroID + "_Normal");
		}
	}

	private void AddExtraAnims(string id, GameObject character)
	{
		if (!Singleton<Profile>.Instance.inMultiplayerWave)
		{
			return;
		}
		string text = string.Empty;
		switch (id)
		{
		case "HeroAttack":
			text = "heroa";
			break;
		case "HeroBalanced":
			text = "herob";
			break;
		case "HeroDefense":
			text = "herod";
			break;
		case "HeroLeadership":
			text = "herol";
			break;
		case "HeroAbility":
			id = "HeroAbilities";
			text = "heroab";
			break;
		}
		GameObject gameObject = Resources.Load("Characters/" + id + "/" + text + "@stun") as GameObject;
		if (!(gameObject != null))
		{
			return;
		}
		Animation component = gameObject.GetComponent<Animation>();
		IEnumerator enumerator = component.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AnimationState animationState = (AnimationState)enumerator.Current;
			AnimationClip clip = animationState.clip;
			if (!(clip.name == "stun"))
			{
				continue;
			}
			Animation[] componentsInChildren = character.GetComponentsInChildren<Animation>();
			Animation[] array = componentsInChildren;
			foreach (Animation animation in array)
			{
				if (animation.GetClip("stun") == null)
				{
					animation.AddClip(clip, "stun");
				}
			}
		}
	}

	private void Init(Vector3 spawnPoint, int playerId, string heroID, bool isLocalPlayer)
	{
		base.isPlayer = true;
		base.ownerId = playerId;
		mIsLeftToRightGameplay = Singleton<PlayModesManager>.Instance.gameDirection == PlayModesManager.GameDirection.LeftToRight;
		if (playerId == 1)
		{
			mIsLeftToRightGameplay = !mIsLeftToRightGameplay;
		}
		meleeDamageModifier = 1f;
		rangedDamageModifier = 1f;
		bool includeSounds = playerId != 0 || isLocalPlayer;
		Singleton<HeroesDatabase>.Instance.LoadInGameData(heroID, playerId);
		mainData = Singleton<HeroesDatabase>.Instance[heroID];
		if (Singleton<Profile>.Instance.inBonusWave)
		{
			base.controlledObject = CharacterSchema.Deserialize(mainData.resourcesGhost, includeSounds);
		}
		else
		{
			base.controlledObject = CharacterSchema.Deserialize(mainData.resources, includeSounds);
		}
		base.controller.position = spawnPoint;
		base.controller.updatesNoSnap = 0;
		base.controller.MaxBackPedalTime = Singleton<HeroesDatabase>.Instance[Singleton<Profile>.Instance.heroID].backPedalTime;
		heroObject = base.controlledObject;
		heroModel = base.controller.animPlayer.jointAnimation.gameObject;
		AddExtraAnims(heroID, heroObject);
		CheckForUpgradeFXArmor(heroID);
		ResetController();
		SetDefaultFacing();
		SetMirroredSkeleton(base.controller.facing == FacingType.Left);
		base.stats = GetHeroStats(mainData, base.ownerId);
		if (WeakGlobalMonoBehavior<InGameImpl>.Exists && playerId == 0 && isLocalPlayer)
		{
			mPlayerControls = new HeroControls();
			mPlayerControls.onMoveLeft = onMoveLeft;
			mPlayerControls.onMoveRight = onMoveRight;
			mPlayerControls.onDontMove = onDontMove;
		}
		CheckForUpgradeFXSword();
		if (!Singleton<Profile>.Instance.inBonusWave)
		{
			RestoreMeleeWeapon();
		}
		if (base.ownerId != 0 && Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent != null)
		{
			base.resourceDrops.guaranteedCoinsAward = 25 * (Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.heroLevel + Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.meleeLevel + Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.bowLevel);
		}
		CheckForUpgradeFXBow();
		RestoreRangedWeapon();
		SetRangeAttackMode(false);
		NoAttack = true;
		if (WeakGlobalMonoBehavior<InGameImpl>.Instance != null && WeakGlobalMonoBehavior<InGameImpl>.Instance.HasWealthCharm())
		{
			SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/Magnet.prefab", 1);
			if (cachedResource != null)
			{
				base.controller.SpawnEffectAtJoint(cachedResource.Resource as GameObject, "body_effect", true);
			}
		}
		if (base.ownerId != 0)
		{
			ActivateHealthBar();
		}
		base.BlocksHeroMovement = mainData.blocksHeroMovement || SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable("HeroBlockMovement", true);
		if (!WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			return;
		}
		mLeftConstraint = WeakGlobalMonoBehavior<InGameImpl>.Instance.heroWalkLeftEdge.position.z;
		mRightConstraint = WeakGlobalMonoBehavior<InGameImpl>.Instance.heroWalkRightEdge.position.z;
		if (Singleton<Profile>.Instance.inVSMultiplayerWave)
		{
			if (base.LeftToRight)
			{
				mLeftConstraint -= 1f;
				mRightConstraint = WeakGlobalMonoBehavior<InGameImpl>.Instance.enemiesSpawnAreaRight.transform.position.z - 0.288f;
			}
			else
			{
				mRightConstraint += 1f;
				mLeftConstraint = WeakGlobalMonoBehavior<InGameImpl>.Instance.helpersSpawnAreaLeft.transform.position.z + 0.288f;
			}
			if (WeakGlobalInstance<CollectableManager>.Instance != null && base.ownerId == 0)
			{
				WeakGlobalInstance<CollectableManager>.Instance.LeftEdge = mLeftConstraint + 0.16f;
				WeakGlobalInstance<CollectableManager>.Instance.RightEdge = mRightConstraint - 0.16f;
			}
		}
	}

	private void CheckForUpgradeFXSword()
	{
		if ((base.ownerId == 0 && Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Sword") > 0) || (base.ownerId != 0 && Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.swordsCollected > 0))
		{
			SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/WeaponsCollectionsGlow.prefab", 1);
			if (cachedResource != null)
			{
				mMPMeleeWeaponUpgradePrefab = cachedResource.Resource as GameObject;
			}
		}
	}

	private void CheckForUpgradeFXBow()
	{
		if ((base.ownerId == 0 && Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Bow") > 0) || (base.ownerId != 0 && Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.bowsCollected > 0))
		{
			SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/WeaponsCollectionsGlow.prefab", 1);
			if (cachedResource != null)
			{
				mMPRangedWeaponUpgradePrefab = cachedResource.Resource as GameObject;
			}
		}
	}

	private void CheckForUpgradeFXArmor(string heroID)
	{
		int num = -1;
		num = ((base.ownerId != 0) ? Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.armorLevel : Singleton<Profile>.Instance.MultiplayerData.CollectionLevel("Armor"));
		Renderer componentInChildren = heroModel.GetComponentInChildren<Renderer>();
		if (componentInChildren != null)
		{
			string key = MaterialKey(heroID, num);
			DataBundleRecordKey dataBundleRecordKey = new DataBundleRecordKey(key);
			matLookupHandle = new DataBundleRecordHandle<MaterialLookupSchema>(dataBundleRecordKey);
			matLookupHandle.Load(null);
			if (matLookupHandle.Data != null && matLookupHandle.Data.material != null)
			{
				Material material = SchemaFieldAdapter.Deserialize(matLookupHandle.Data.material);
				if (material != null)
				{
					componentInChildren.sharedMaterial = SchemaFieldAdapter.Deserialize(material);
				}
			}
		}
		if (num > 0)
		{
			DataBundleRecordKey dataBundleRecordKey2 = new DataBundleRecordKey("CollectionSet", num.ToString());
			mCollectionArmorSchema = DataBundleRuntime.Instance.InitializeRecord<ArmorLevelSchema>(dataBundleRecordKey2);
			if (mCollectionArmorSchema != null)
			{
				GameObject visualFX = mCollectionArmorSchema.visualFX;
				if (visualFX != null)
				{
					base.controller.SpawnEffectAtJoint(visualFX, "body_effect", true);
				}
			}
		}
		int level = Singleton<Profile>.Instance.armorLevel;
		if (base.ownerId != 0)
		{
			level = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.armorCollected;
		}
		mCharacterArmorSchema = mainData.GetArmorLevel(level);
		if (mCharacterArmorSchema != null)
		{
			GameObject visualFX2 = mCharacterArmorSchema.visualFX;
			if (visualFX2 != null)
			{
				base.controller.SpawnEffectAtJoint(visualFX2, "body_effect", true);
			}
		}
	}

	private void UpdateFSBloodEffect()
	{
		if (HUDBloodEffect == null)
		{
			return;
		}
		if (base.health == 0f)
		{
			FSBloodEffectFadeLevel = Mathf.Max(0f, FSBloodEffectFadeLevel - Time.deltaTime * 1f);
		}
		else if (WeakGlobalMonoBehavior<InGameImpl>.Instance.gate.health > 0f && base.isInLeniencyMode)
		{
			if (!mBloodEffectActive)
			{
				mBloodEffectActive = true;
				mBloodEffectSprite.gameObject.SetActive(mBloodEffectActive);
			}
			if (FSBloodEffectFadeLevel == 0f)
			{
				FSBloodEffectFadeLevel = 0.6f;
				mPulseDir = -1f;
			}
			else
			{
				FSBloodEffectFadeLevel = Mathf.Clamp(FSBloodEffectFadeLevel + mPulseDir * (Time.deltaTime * 1f), 0.4f, 0.6f);
				if (FSBloodEffectFadeLevel == 0.4f)
				{
					mPulseDir = 1f;
				}
				else if (FSBloodEffectFadeLevel == 0.6f)
				{
					mPulseDir = -1f;
				}
			}
		}
		else
		{
			FSBloodEffectFadeLevel = 0f;
		}
		if (mBloodEffectActive)
		{
			if (FSBloodEffectFadeLevel == 0f)
			{
				mBloodEffectActive = false;
				mBloodEffectSprite.gameObject.SetActive(mBloodEffectActive);
			}
			else
			{
				mBloodEffectSprite.Color = new Color(1f, 1f, 1f, FSBloodEffectFadeLevel);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (base.health > 0f)
		{
			UpdateConstraints();
			if (mPlayerControls != null)
			{
				mPlayerControls.Update();
			}
		}
		UpdateAttacks();
		UpdateMount();
		UpdateFSBloodEffect();
	}

	private void UpdateConstraints()
	{
		Character[] array = WeakGlobalMonoBehavior<InGameImpl>.Instance.CharacterMgr.GetPlayerCharacters(1 - base.ownerId).ToArray();
		float num = -10000f;
		if (base.LeftToRight)
		{
			num = 10000f;
		}
		Character character = null;
		Character[] array2 = array;
		foreach (Character character2 in array2)
		{
			if (character2.health > 0f && character2.BlocksHeroMovement && ((base.LeftToRight && character2.transform.position.z < num && character2.transform.position.z > base.transform.position.z) || (!base.LeftToRight && character2.transform.position.z > num && character2.transform.position.z < base.transform.position.z)))
			{
				character = character2;
				num = character2.transform.position.z;
			}
		}
		if (character != null)
		{
			if (mIsLeftToRightGameplay)
			{
				base.controller.constraintRight = Mathf.Min(mRightConstraint, character.position.z - kEnemyHeroKeepDist);
			}
			else
			{
				base.controller.constraintLeft = Mathf.Max(mLeftConstraint, character.position.z + kEnemyHeroKeepDist);
			}
		}
		else
		{
			base.controller.constraintLeft = mLeftConstraint;
			base.controller.constraintRight = mRightConstraint;
		}
	}

	public override void Destroy()
	{
		if (mPlayerControls != null)
		{
			mPlayerControls.Dispose();
			mPlayerControls = null;
		}
		if (WeakGlobalMonoBehavior<InGameImpl>.Instance == null)
		{
			Object.Destroy(base.controlledObject);
		}
		if (matLookupHandle != null)
		{
			matLookupHandle.Unload();
		}
		base.Destroy();
	}

	public void UpdateAttacks()
	{
		if (!base.isEffectivelyIdle || base.controller.isMoving)
		{
			return;
		}
		if (WeakGlobalMonoBehavior<InGameImpl>.Instance.playerWon && base.ownerId == 0)
		{
			base.controller.PlayVictoryAnim();
		}
		else
		{
			if (Singleton<Profile>.Instance.inBonusWave)
			{
				return;
			}
			if (IsInMeleeRangeOfOpponent(true))
			{
				SetRangeAttackMode(false);
				if (base.canMeleeAttack)
				{
					StartMeleeAttackDelayTimer();
					base.controller.Attack(base.meleeAttackFrequency);
					NoAttack = false;
				}
			}
			else if (IsInBowRangeOfOpponent())
			{
				SetRangeAttackMode(true);
				if (base.canUseRangedAttack)
				{
					StartRangedAttackDelayTimer();
					base.singleAttackTarget = WeakGlobalInstance<CharactersManager>.Instance.GetBestRangedAttackTarget(this, base.bowAttackGameRange);
					if (base.singleAttackTarget != null)
					{
						NoAttack = false;
						base.controller.RangeAttack(base.bowAttackFrequency);
					}
				}
			}
			else if (base.controller.currentAnimation == "rangedattackidle" && base.controller.animPlayer.currAnimTime > 2f)
			{
				SetRangeAttackMode(false);
			}
		}
	}

	private float CalcDamageRemoved(EAttackType attackType, float damage, Character attacker, ArmorLevelSchema armorSchema, bool canReflect)
	{
		float num = damage;
		if (armorSchema != null)
		{
			switch (attackType)
			{
			case EAttackType.Blade:
			case EAttackType.BladeCritical:
			case EAttackType.Blunt:
			case EAttackType.BluntCritical:
			case EAttackType.Slice:
			case EAttackType.Trample:
			case EAttackType.Stomp:
				if (Random.Range(0f, 1f) <= armorSchema.meleeBlockRatio)
				{
					damage = 0f;
					if (armorSchema.blockFX != null)
					{
						base.controller.SpawnEffectAtJoint(armorSchema.blockFX, "impact_target", false);
					}
				}
				else
				{
					damage *= armorSchema.meleeDamageModifier;
				}
				break;
			case EAttackType.Arrow:
			case EAttackType.Shuriken:
			case EAttackType.FireArrow:
			case EAttackType.Explosion:
			case EAttackType.Bullet:
				if (Random.Range(0f, 1f) <= armorSchema.rangedBlockRatio)
				{
					damage = 0f;
					if (armorSchema.blockFX != null)
					{
						base.controller.SpawnEffectAtJoint(armorSchema.blockFX, "impact_target", false);
					}
				}
				else
				{
					damage *= armorSchema.rangedDamageModifier;
				}
				break;
			}
			if (canReflect && armorSchema.reflectDamageRatio > 0f && attacker != null)
			{
				attacker.RecievedAttack(attackType, num, this, false);
			}
		}
		return num - damage;
	}

	public override void RecievedAttack(EAttackType attackType, float damage, Character attacker, bool canReflect)
	{
		if (base.invuln)
		{
			return;
		}
		float num = base.health;
		float num2 = CalcDamageRemoved(attackType, damage, attacker, mCharacterArmorSchema, canReflect);
		float num3 = CalcDamageRemoved(attackType, damage, attacker, mCollectionArmorSchema, canReflect);
		damage -= num2;
		damage -= num3;
		if (base.mountedHealth > 0f)
		{
			base.mountedHealth -= damage;
			if (!(base.mountedHealth <= 0f))
			{
				RecievedAttackFX(attackType, damage, attacker);
				return;
			}
			damage = 0f - base.mountedHealth;
			DoDismount();
		}
		base.RecievedAttack(attackType, damage, attacker, canReflect);
		if (base.health > 0f && !mPerformingSpecialAttack)
		{
			float num4 = base.maxHealth / 3f * 2f;
			if (num > num4 && base.health <= num4)
			{
				SetRangeAttackMode(false);
				ForceKnockback(Vector3.zero);
			}
		}
	}

	public override void RecievedHealing(float healAmount)
	{
		base.RecievedHealing(healAmount);
		if (base.mountedHealthMax > 0f)
		{
			base.mountedHealth += healAmount;
			base.mountedHealth = Mathf.Min(base.mountedHealth, base.mountedHealthMax);
		}
	}

	public void UpdateMount()
	{
		Character mount = WeakGlobalInstance<CharactersManager>.Instance.GetMount(base.ownerId);
		if (base.health <= 0f)
		{
			if (mount != null && mount.health > 0f && mountObject == null)
			{
				mount.health = 0f;
			}
			return;
		}
		if (mount != null && mount.isActive && mountObject == null && mount.health > 0f)
		{
			float f = mount.position.z - base.position.z;
			if (Mathf.Abs(f) < 0.1f)
			{
				Renderer[] componentsInChildren = mount.controlledObject.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in componentsInChildren)
				{
					if (renderer.enabled)
					{
						DoMount(mount);
						break;
					}
				}
			}
		}
		if (base.mountedHealthMax > 0f && base.isHealing)
		{
			base.mountedHealth += mountedHealRate * Time.deltaTime;
			base.mountedHealth = Mathf.Min(base.mountedHealth, base.mountedHealthMax);
		}
	}

	private void ResetController()
	{
		if (WeakGlobalMonoBehavior<InGameImpl>.Instance != null)
		{
			base.controller.constraintLeft = WeakGlobalMonoBehavior<InGameImpl>.Instance.heroWalkLeftEdge.position.z;
			base.controller.constraintRight = WeakGlobalMonoBehavior<InGameImpl>.Instance.heroWalkRightEdge.position.z;
			if (Singleton<Profile>.Instance.inVSMultiplayerWave)
			{
				Hero hero = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetHero(1 - base.ownerId);
				if (hero != null && hero.health > 0f)
				{
					if (mIsLeftToRightGameplay)
					{
						base.controller.constraintRight = hero.position.z - kEnemyHeroKeepDist;
					}
					else
					{
						base.controller.constraintLeft = hero.position.z + kEnemyHeroKeepDist;
					}
				}
			}
		}
		base.controller.StopWalking();
	}

	private void SetAttachedToCharacter(Character newMount, string jointName)
	{
		if (newMount != null)
		{
			if (base.health == 0f)
			{
				return;
			}
			mountObject = newMount.controlledObject;
			mountObject.transform.parent = heroObject.transform;
			mountObject.transform.localPosition = Vector3.zero;
			horseModel = newMount.controller.animPlayer.jointAnimation.gameObject;
			Transform[] componentsInChildren = horseModel.GetComponentsInChildren<Transform>(true);
			foreach (Transform transform in componentsInChildren)
			{
				if (transform.name.Equals(jointName))
				{
					heroModel.transform.parent = transform;
					break;
				}
			}
			heroModel.transform.localPosition = Vector3.zero;
			base.controller.animPlayer.RegisterNewChildAnimationPlayer(horseModel.GetComponent<Animation>());
			base.controller.animNamePrefix = "horse";
		}
		else
		{
			Vector3 vector = base.position;
			heroModel.transform.parent = heroObject.transform;
			heroModel.transform.localPosition = Vector3.zero;
			heroModel.transform.localRotation = Quaternion.identity;
			heroModel.transform.localScale = Vector3.one;
			heroObject.transform.position = vector;
			horseModel.transform.parent = mountObject.transform;
			horseModel.transform.localPosition = Vector3.zero;
			mountObject.transform.position = vector;
			mountObject.transform.parent = null;
			mountObject.SetActive(true);
			mountObject = null;
			base.controller.animNamePrefix = string.Empty;
		}
		ResetController();
	}

	private void DoPostMount(Character newMount)
	{
		SetAttachedToCharacter(newMount, "warriorhorse:horse_midsection_jnt");
		mPerformingSpecialAttack = false;
		base.controller.animPlayer.actionAnimFadeOutSpeed = base.controller.animPlayer.defaultBlendSpeed;
		ResetInvulnToDefault();
	}

	private void DoMount(Character newMount)
	{
		base.invuln = true;
		mPerformingSpecialAttack = true;
		base.mountedHealthMax = newMount.health;
		base.mountedHealth = base.mountedHealthMax;
		mountedHealRate = newMount.knockbackResistance;
		meleeDamageModifier = 1f + newMount.meleeDamage * 0.01f;
		rangedDamageModifier = 1f + newMount.bowDamage * 0.01f;
		base.controller.speed = base.stats.speed * (1f + (float)newMount.knockbackPower * 0.01f);
		WeakGlobalInstance<CharactersManager>.Instance.SetMountActive(false, base.ownerId);
		base.controller.animPlayer.actionAnimFadeOutSpeed = 0f;
		base.controller.PerformSpecialActionNoAttack("mount", delegate
		{
			DoPostMount(newMount);
		});
		base.controller.SetBaseAnim("horseidle");
		newMount.buffIcon.Clear();
	}

	private void DoPostDismount()
	{
		mPerformingSpecialAttack = false;
		SetAttachedToCharacter(null, null);
		meleeDamageModifier = 1f;
		rangedDamageModifier = 1f;
		WeakGlobalInstance<CharactersManager>.Instance.SetMountActive(true, base.ownerId);
		WeakGlobalInstance<CharactersManager>.Instance.KillMount(base.ownerId);
	}

	private void DoDismount()
	{
		mPerformingSpecialAttack = true;
		base.mountedHealth = 0f;
		base.mountedHealthMax = 0f;
		meleeDamageModifier = 0f;
		rangedDamageModifier = 0f;
		base.controller.speed = base.stats.speed;
		DoPostDismount();
	}

	public void Revive()
	{
		if (Singleton<Profile>.Instance.inVSMultiplayerWave && base.BlocksHeroMovement)
		{
			if (mIsLeftToRightGameplay)
			{
				base.controlledObject.transform.position = WeakGlobalMonoBehavior<InGameImpl>.Instance.heroSpawnPointLeft.position;
			}
			else
			{
				base.controlledObject.transform.position = WeakGlobalMonoBehavior<InGameImpl>.Instance.heroSpawnPointRight.position;
			}
		}
		Renderer[] componentsInChildren = base.controlledObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			renderer.enabled = true;
		}
		ProceduralShaderManager.StopShaderEvents((ShaderEvent se) => se.gameObject == base.controlledObject);
		mHasDOT = false;
		SetRangeAttackMode(false);
		if (base.mountedHealth > 0f)
		{
			OnReviveAttack(null);
			return;
		}
		mPerformingSpecialAttack = true;
		base.invuln = true;
		base.controller.PerformSpecialAction("revive", delegate
		{
			OnReviveAttack(null);
		});
		base.controller.animEndedCallback = OnReviveDone;
	}

	public void ResetInvulnToDefault()
	{
		WeakGlobalMonoBehavior<InGameImpl>.Instance.ResetHeroInvulnToDefault(base.ownerId);
	}

	public AbilitySchema[] GetPotentialAbilities()
	{
		return mainData.Abilities;
	}

	public void DoGraveHands(string abilityID)
	{
	}

	public void DoNightOfTheDead(string abilityID)
	{
	}

	public void DoGroundShock(string abilityID)
	{
	}

	private void DoAbility(string animName, AbilitiesDatabase.OnAbilityActivateFunc abilityActivateFunction, AbilitiesDatabase.OnAbilityExecuteFunc abilityExecutionFunction)
	{
		mPerformingSpecialAttack = true;
		SetRangeAttackMode(false);
		if (abilityActivateFunction != null)
		{
			abilityActivateFunction(this);
		}
		if (!string.IsNullOrEmpty(animName))
		{
			base.controller.PerformSpecialAction(animName, delegate
			{
				abilityExecutionFunction(this);
			});
			base.controller.animEndedCallback = OnSpecialAttackDone;
			SetDefaultFacing();
		}
		else
		{
			abilityExecutionFunction(this);
			mPerformingSpecialAttack = false;
		}
	}

	public bool CanUseAbility()
	{
		return base.health > 0f && !base.isInKnockback;
	}

	public bool DoAbility(string abilityName)
	{
		return DoAbility(abilityName, false);
	}

	public bool DoAbility(string abilityName, bool bForce)
	{
		if (!bForce && (!CanUseAbility() || mPerformingSpecialAttack))
		{
			return false;
		}
		string animName = string.Empty;
		AbilitiesDatabase.OnAbilityActivateFunc activateFunc = null;
		AbilitiesDatabase.OnAbilityExecuteFunc execFunc = null;
		Singleton<AbilitiesDatabase>.Instance.GetAbilityInfoByName(abilityName, out animName, out activateFunc, out execFunc);
		DoAbility(animName, activateFunc, execFunc);
		AbilitySchema schema = Singleton<AbilitiesDatabase>.Instance.GetSchema(abilityName);
		if (schema != null && schema.soundTheme != null)
		{
			UdamanSoundThemePlayer soundPlayer = base.controller.SoundPlayer;
			if (soundPlayer != null)
			{
				if (schema.SoundEvent != null)
				{
					soundPlayer.PlaySoundEvent(schema.SoundEvent, schema.soundTheme);
				}
				else
				{
					soundPlayer.PlaySoundEvent("Spawn", schema.soundTheme);
				}
			}
		}
		return true;
	}

	public static void DoLegendaryStrike(string abilityName, int playerId)
	{
		string animName = string.Empty;
		AbilitiesDatabase.OnAbilityActivateFunc activateFunc = null;
		AbilitiesDatabase.OnAbilityExecuteFunc execFunc = null;
		Singleton<AbilitiesDatabase>.Instance.GetAbilityInfoByName(abilityName, out animName, out activateFunc, out execFunc);
		if (execFunc != null)
		{
			Character executor = null;
			if (playerId >= 0)
			{
				executor = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetHero(playerId);
			}
			execFunc(executor);
		}
		AbilitySchema schema = Singleton<AbilitiesDatabase>.Instance.GetSchema(abilityName);
		if (schema == null || !(schema.soundTheme != null))
		{
			return;
		}
		UdamanSoundThemePlayer soundPlayer = WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.controller.SoundPlayer;
		if (soundPlayer != null)
		{
			if (schema.SoundEvent != null)
			{
				soundPlayer.PlaySoundEvent(schema.SoundEvent, schema.soundTheme);
			}
			else
			{
				soundPlayer.PlaySoundEvent("Spawn", schema.soundTheme);
			}
		}
	}

	private void OnGraveHandsAttack(string abilityID)
	{
	}

	private void OnNightOfTheDeadAttack(string abilityID)
	{
	}

	private void OnReviveAttack(string abilityID)
	{
		base.health = base.maxHealth;
		if (WeakGlobalMonoBehavior<InGameImpl>.Instance.GetGate(base.ownerId) != null)
		{
			WeakGlobalMonoBehavior<InGameImpl>.Instance.GetGate(base.ownerId).Revive();
		}
		base.controller.startedDieAnim = false;
		List<Character> playerCharacters = WeakGlobalInstance<CharactersManager>.Instance.GetPlayerCharacters(1 - base.ownerId);
		foreach (Character item in playerCharacters)
		{
			if (!(item is Gate) && !(item is Hero))
			{
				if (item.isBoss)
				{
					item.RecievedAttack(EAttackType.Holy, Mathf.Floor(item.health * 0.5f), WeakGlobalMonoBehavior<InGameImpl>.Instance.hero);
				}
				else
				{
					item.RecievedAttack(EAttackType.Holy, item.maxHealth, WeakGlobalMonoBehavior<InGameImpl>.Instance.hero);
				}
			}
		}
	}

	public void SwitchToCursedWeapon(GameObject weaponPrefab)
	{
		if (weaponPrefab != null)
		{
			WeakGlobalMonoBehavior<InGameImpl>.Instance.RunAfterDelay(RestoreMeleeWeapon, 15f);
			DestroyMeleeWeaponPrefabs();
			base.meleeWeaponPrefab = weaponPrefab;
		}
	}

	private void OnGroundShockAttack(string abilityID)
	{
		float num = float.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute(abilityID, "spawnOffset"));
		Vector3 value = base.position;
		value.z += num;
		value.y = WeakGlobalInstance<RailManager>.Instance.GetY(value.z);
		SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource(string.Format("Assets/Game/Resources/{0}.prefab", Singleton<AbilitiesDatabase>.Instance.GetAttribute(abilityID, "fx")), 1);
		if (cachedResource != null)
		{
			GameObjectPool.DefaultObjectPool.Acquire(cachedResource.Resource as GameObject, value, Quaternion.identity);
		}
	}

	private void OnReviveDone()
	{
		ResetInvulnToDefault();
		OnSpecialAttackDone();
	}

	private void OnSpecialAttackDone()
	{
		mPerformingSpecialAttack = false;
	}

	private void RestoreMeleeWeapon()
	{
		DestroyMeleeWeaponPrefabs();
		WeaponSchema meleeWeapon = mainData.MeleeWeapon;
		List<GameObject> list = new List<GameObject>();
		int level = Singleton<Profile>.Instance.swordLevel;
		if (base.ownerId != 0)
		{
			level = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.meleeLevel;
		}
		list.Add(meleeWeapon.GetLevel(level).prefab);
		if (meleeWeapon.isDualWield)
		{
			list.Add(meleeWeapon.GetLevel(level).prefab);
		}
		base.meleeWeaponPrefabAsList = list;
	}

	private void RestoreRangedWeapon()
	{
		Object.Destroy(base.rangedWeaponPrefab);
		WeaponSchema rangedWeapon = mainData.RangedWeapon;
		int bowLevel = Singleton<Profile>.Instance.bowLevel;
		if (base.ownerId != 0)
		{
			bowLevel = Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.bowLevel;
		}
		base.rangedWeaponPrefab = rangedWeapon.GetLevel(bowLevel).prefab;
	}

	public void onMoveLeft()
	{
		if (base.health > 0f && !mPerformingSpecialAttack)
		{
			if (base.controller.walkDirection == FacingType.Right)
			{
				base.controller.StopWalking();
			}
			else
			{
				base.controller.StartWalkLeft();
			}
			if (base.controller.animPlayer.baseAnim == "runback")
			{
				SetRangeAttackMode(false);
			}
		}
	}

	public void onMoveRight()
	{
		if (base.health > 0f && !mPerformingSpecialAttack)
		{
			if (base.controller.walkDirection == FacingType.Left)
			{
				base.controller.StopWalking();
			}
			else
			{
				base.controller.StartWalkRight();
			}
			if (base.controller.animPlayer.baseAnim == "runback")
			{
				SetRangeAttackMode(false);
			}
		}
	}

	public void onDontMove()
	{
		if (base.health > 0f && base.controller.isMoving)
		{
			base.controller.StopWalking();
		}
	}

	private GameObject FindChildWithTag(GameObject go, string tagName)
	{
		return TraverseHierarchy(go.transform, tagName);
	}

	private GameObject TraverseHierarchy(Transform root, string tagName)
	{
		foreach (Transform item in root)
		{
			if (item.gameObject.tag == tagName)
			{
				return item.gameObject;
			}
			GameObject gameObject = TraverseHierarchy(item, tagName);
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		return null;
	}

	public List<Character> GetEnemiesAhead(float range)
	{
		if (mIsLeftToRightGameplay)
		{
			return WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(base.controller.position.z, base.controller.position.z + range, 1 - base.ownerId);
		}
		return WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(base.controller.position.z - range, base.controller.position.z, 1 - base.ownerId);
	}

	public List<Character> GetEnemiesAheadMaxCount(float range, int maxCount)
	{
		if (mIsLeftToRightGameplay)
		{
			return WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRangeMaxCount(base.controller.position.z, base.controller.position.z + range, 1 - base.ownerId, maxCount);
		}
		return WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRangeMaxCount(base.controller.position.z - range, base.controller.position.z, 1 - base.ownerId, maxCount);
	}

	public List<Character> GetEnemiesAround(float range)
	{
		return WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(base.controller.position.z - range, base.controller.position.z + range, 1 - base.ownerId);
	}

	private bool CheckAbilitySelected(string abilityID)
	{
		string[] array = ((base.ownerId != 0) ? Singleton<Profile>.Instance.MultiplayerData.CurrentOpponent.loadout.abilityIdList.ToArray() : Singleton<Profile>.Instance.GetSelectedAbilities().ToArray());
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (abilityID == text)
			{
				return true;
			}
		}
		return false;
	}

	protected override GameObject GetMeleeUpgradeEffect()
	{
		return mMPMeleeWeaponUpgradePrefab;
	}

	protected override GameObject GetRangedUpgradeEffect()
	{
		return mMPRangedWeaponUpgradePrefab;
	}

	public override void PerformKnockback(Character target, int knockbackPower, bool force, Vector3 positionAdjust)
	{
		if (base.LeftToRight && !target.isPlayer && !target.isBase && !target.isBoss && WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			Pit pit = WeakGlobalMonoBehavior<InGameImpl>.Instance.Pit;
			if (pit != null && pit.TryTaking(target))
			{
				BoxCollider area = pit.Area;
				positionAdjust.x = area.transform.position.x - target.position.x;
				float value = target.transform.position.z + (positionAdjust.z + 1f) * (0f - (float)target.controller.facing);
				value = Mathf.Clamp(value, pit.Range.left, pit.Range.right);
				if (target.LeftToRight)
				{
					positionAdjust.z = Mathf.Min(value - target.transform.position.z, 0f);
				}
				else
				{
					positionAdjust.z = Mathf.Max(value - target.transform.position.z, 0f);
				}
				positionAdjust.y = Mathf.Max(positionAdjust.y, 4f);
				if (target.TryKnockback(knockbackPower, force, positionAdjust))
				{
					pit.CatchTarget(target);
				}
				return;
			}
		}
		positionAdjust.z = (positionAdjust.z + 1f) * (0f - (float)target.controller.facing);
		target.TryKnockback(knockbackPower, force, positionAdjust);
	}
}
