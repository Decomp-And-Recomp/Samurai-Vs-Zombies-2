using System;
using System.Collections.Generic;
using UnityEngine;

public class Character : Weakable
{
	private struct HealthTextInfo
	{
		public float lifetime;

		public GameObject obj;

		public HealthTextInfo(GameObject o)
		{
			obj = o;
			lifetime = 0f;
		}
	}

	private struct BuffInfo
	{
		public float percentDamageToAdd;

		public float speedModifier;

		public float duration;

		public object source;

		public GameObject effect;
	}

	public delegate float ExplosionRangeDelegate();

	private const float kAutoRecoveryDelay = 2f;

	private const float kImpactPauseTime = 0.12f;

	private const int kMinKnockbackChance = 1;

	private const float kAttackRangeLeeway = 1.3f;

	private const float kMaxMoveDelay = 0.5f;

	private const float kLeniencyTimer = 1.2f;

	private const float kLeniencyTreshold = 0.1f;

	private const float kExplosionRange = 2.5f;

	protected const float kKnockbackTime = 0.5f;

	private const float kMaxHealthPercentToPreferFrontLine = 0.4f;

	private const float kMaxHealthPercentToPreferWeaker = 0.2f;

	private const float kCloseDistRange = 35f;

	public ExplosionRangeDelegate explosionRange = () => 2.5f;

	private static GameObjectPool sObjectPool = new GameObjectPool();

	private List<HealthTextInfo> healthTextObjects = new List<HealthTextInfo>();

	private Camera mObjectCamera;

	private Camera mGluiCamera;

	private bool? mAcceptsBuffs;

	private bool mActive = true;

	private float mSpeedBuffModifier = 1f;

	private CharacterModelController mCtrl;

	private CharacterStats mStats;

	private float mMeleeAttackDelay;

	private float mRangedAttackDelay;

	private float mMoveDelay;

	private bool mMeleeAttackDelivered;

	protected bool mRangedAttackDelivered;

	private ResourceDrops mResourceDrops;

	private float mAutoRecoveryDelay;

	private float? mLeniencyTimer;

	private bool mInMeleeAttackCycle;

	private bool mInRangedAttackCycle;

	private BuffIconClient mBuffIconClient;

	private List<GameObject> mPaperDollMeleeWeapon = new List<GameObject>();

	private GameObject mPaperDollRangedWeapon;

	protected List<MeleeWeaponTrail> mMeleeWeaponTrail;

	protected bool mHasDOT;

	protected float mDOTdamage;

	protected float mDOTfrequency;

	protected float mDOTduration;

	protected float mDOTcurrentTime;

	protected float mDOTpauseTime;

	protected Color? mDOTColorTint;

	protected Character mDOTAttacker;

	protected bool supportColorFlash = true;

	protected Dictionary<int, Color> mOriginalMaterialColors = new Dictionary<int, Color>(3);

	private List<BuffInfo> mBuffReceievedInfos = new List<BuffInfo>();

	private bool mBuffChanged;

	protected float mDamageBuffPercent;

	private WeakPtr<Character> mSingleAttackTarget;

	private List<WeakPtr<Character>> mTargetedAttackers = new List<WeakPtr<Character>>();

	private string[] kMeleeJointIDs = new string[2] { "melee_weapon", "melee_weapon_2" };

	protected bool mIsLeftToRightGameplay = true;

	public Transform transform { get; private set; }

	public bool acceptsBuffs
	{
		get
		{
			if (!mAcceptsBuffs.HasValue)
			{
				mAcceptsBuffs = !isPlayer && !isBase;
			}
			return mAcceptsBuffs.Value;
		}
	}

	public BuffIconClient buffIcon
	{
		get
		{
			return mBuffIconClient;
		}
	}

	public Dictionary<int, Color> OriginalMaterialColors
	{
		get
		{
			if (mOriginalMaterialColors.Count <= 0)
			{
				SkinnedMeshRenderer[] componentsInChildren = controlledObject.GetComponentsInChildren<SkinnedMeshRenderer>();
				SkinnedMeshRenderer[] array = componentsInChildren;
				foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
				{
					if (skinnedMeshRenderer != null)
					{
						Material material = skinnedMeshRenderer.material;
						int instanceID = material.GetInstanceID();
						Color value = (material.HasProperty("_RimColor") ? material.GetColor("_RimColor") : (material.HasProperty("_Color") ? material.GetColor("_Color") : ((!material.HasProperty("_MainColor")) ? Color.white : material.GetColor("_MainColor"))));
						mOriginalMaterialColors[instanceID] = value;
					}
				}
				MeshRenderer[] componentsInChildren2 = controlledObject.GetComponentsInChildren<MeshRenderer>();
				MeshRenderer[] array2 = componentsInChildren2;
				foreach (MeshRenderer meshRenderer in array2)
				{
					if (meshRenderer != null)
					{
						Material material2 = meshRenderer.material;
						int instanceID2 = material2.GetInstanceID();
						Color value2 = (material2.HasProperty("_RimColor") ? material2.GetColor("_RimColor") : (material2.HasProperty("_Color") ? material2.GetColor("_Color") : ((!material2.HasProperty("_MainColor")) ? Color.white : material2.GetColor("_MainColor"))));
						mOriginalMaterialColors[instanceID2] = value2;
					}
				}
			}
			return mOriginalMaterialColors;
		}
	}

	public bool invuln { get; set; }

	public float timeSinceDamaged { get; set; }

	public bool impactPauseSuspended { get; set; }

	public GameObject meleeWeaponPrefab
	{
		get
		{
			if (mPaperDollMeleeWeapon.Count > 0)
			{
				return mPaperDollMeleeWeapon[0];
			}
			return null;
		}
		set
		{
			List<GameObject> list = new List<GameObject>(1);
			list.Add(value);
			meleeWeaponPrefabAsList = list;
		}
	}

	public List<GameObject> meleeWeaponPrefabAsList
	{
		get
		{
			return mPaperDollMeleeWeapon;
		}
		set
		{
			DestroyMeleeWeaponPrefabs();
			mPaperDollMeleeWeapon = value;
			if (mPaperDollMeleeWeapon == null)
			{
				mPaperDollMeleeWeapon = new List<GameObject>();
			}
			mMeleeWeaponTrail = new List<MeleeWeaponTrail>();
			for (int i = 0; i < Mathf.Min(kMeleeJointIDs.Length, mPaperDollMeleeWeapon.Count); i++)
			{
				GameObject gameObject = mPaperDollMeleeWeapon[i];
				if (!(gameObject != null))
				{
					continue;
				}
				mPaperDollMeleeWeapon[i] = controller.InstantiateObjectOnJoint(gameObject, kMeleeJointIDs[i]);
				mMeleeWeaponTrail.AddRange(mPaperDollMeleeWeapon[i].GetComponentsInChildren<MeleeWeaponTrail>());
				SetMeleeWeaponTrailVisible(false);
				GameObject meleeUpgradeEffect = GetMeleeUpgradeEffect();
				if (meleeUpgradeEffect != null)
				{
					GameObject gameObject2 = controller.SpawnEffectAtJoint(meleeUpgradeEffect, kMeleeJointIDs[i], true);
					if ((bool)gameObject2)
					{
						gameObject2.transform.parent = mPaperDollMeleeWeapon[i].transform;
					}
				}
			}
		}
	}

	public GameObject rangedWeaponPrefab
	{
		get
		{
			return mPaperDollRangedWeapon;
		}
		set
		{
			if (mPaperDollRangedWeapon != null)
			{
				GameObjectPool.DefaultObjectPool.Release(mPaperDollRangedWeapon);
			}
			if (!(value != null))
			{
				return;
			}
			mPaperDollRangedWeapon = controller.InstantiateObjectOnJoint(value, "ranged_weapon");
			GameObject rangedUpgradeEffect = GetRangedUpgradeEffect();
			if (rangedUpgradeEffect != null)
			{
				GameObject gameObject = controller.SpawnEffectAtJoint(rangedUpgradeEffect, "ranged_weapon", true);
				if (gameObject != null)
				{
					gameObject.transform.parent = mPaperDollRangedWeapon.transform;
				}
			}
		}
	}

	public CharacterStats stats
	{
		get
		{
			return mStats;
		}
		set
		{
			mStats = value;
			controller.speed = mStats.speed;
		}
	}

	public bool isInLeniencyMode
	{
		get
		{
			float? num = mLeniencyTimer;
			return num.HasValue;
		}
	}

	public float health
	{
		get
		{
			return mStats.health;
		}
		set
		{
			if (value < mStats.health)
			{
				if ((isPlayer || !isEnemy) && autoHealthRecovery >= 0f && WeakGlobalMonoBehavior<InGameImpl>.Instance.allAlliesInvincibleTimer > 0f)
				{
					return;
				}
				if (isPlayer)
				{
					mAutoRecoveryDelay = 2f;
				}
			}
			mStats.health = Mathf.Clamp(value, 0f, mStats.maxHealth);
			if (isPlayer)
			{
				if (mStats.health <= 0.1f)
				{
					float? num = mLeniencyTimer;
					if (!num.HasValue)
					{
						mLeniencyTimer = 1.2f;
						mStats.health = 0.1f;
					}
					else
					{
						float? num2 = mLeniencyTimer;
						if (num2.HasValue && num2.Value > 0f)
						{
							mStats.health = 0.1f;
						}
					}
				}
				else
				{
					mLeniencyTimer = null;
				}
			}
			if (mStats.health <= 0f)
			{
				Die();
			}
		}
	}

	public float maxHealth
	{
		get
		{
			return mStats.maxHealth;
		}
		set
		{
			mStats.maxHealth = Mathf.Max(0f, value);
			mStats.health = Mathf.Min(mStats.health, mStats.maxHealth);
		}
	}

	public float autoHealthRecovery
	{
		get
		{
			return mStats.autoHealthRecovery;
		}
		set
		{
			mStats.autoHealthRecovery = value;
		}
	}

	public virtual bool isOver
	{
		get
		{
			return health <= 0f && controller.readyToVanish;
		}
	}

	public CharacterModelController.EPostDeathAction postDeathAction
	{
		get
		{
			return isEnemy ? CharacterModelController.EPostDeathAction.melt : ((!isPlayer) ? ((!isBase) ? CharacterModelController.EPostDeathAction.ascend : CharacterModelController.EPostDeathAction.vanish) : CharacterModelController.EPostDeathAction.vanish);
		}
	}

	public CharacterModelController controller
	{
		get
		{
			return mCtrl;
		}
		private set
		{
			mCtrl = value;
		}
	}

	public GameObject controlledObject
	{
		get
		{
			if (controller == null)
			{
				return null;
			}
			return controller.gameObject;
		}
		set
		{
			if (mCtrl != null && mCtrl.gameObject != null)
			{
				UnityEngine.Object.Destroy(mCtrl.gameObject);
				mCtrl = null;
			}
			if (value != null)
			{
				controller = value.GetComponent<CharacterModelController>();
				if (!controller)
				{
					controller = value.AddComponent<CharacterModelController>();
				}
				controller.onMeleeAttackDelivery = OnAttackDelivery;
				controller.onRangedAttackDelivery = OnRangedAttackDelivery;
				controller.onMeleeAttack = OnMeleeAttack;
				transform = controller.transform;
				mBuffIconClient = new BuffIconClient(controller);
				mObjectCamera = ObjectUtils.FindFirstCamera(controller.gameObject.layer);
			}
			mGluiCamera = ObjectUtils.FindFirstCamera(LayerMask.NameToLayer("GLUI"));
		}
	}

	public GameObject mountPrototype { get; private set; }

	public string uniqueID
	{
		get
		{
			return mStats.uniqueID;
		}
		set
		{
			mStats.uniqueID = value;
		}
	}

	public bool isUnique
	{
		get
		{
			return mStats.isUnique;
		}
		set
		{
			mStats.isUnique = value;
		}
	}

	public bool isEnemy
	{
		get
		{
			return mStats.isEnemy;
		}
		set
		{
			mStats.isEnemy = value;
		}
	}

	public bool isPlayer
	{
		get
		{
			return mStats.isPlayer;
		}
		set
		{
			mStats.isPlayer = value;
		}
	}

	public bool isBase
	{
		get
		{
			return mStats.isBase;
		}
		set
		{
			mStats.isBase = value;
		}
	}

	public bool isGateRusher
	{
		get
		{
			return mStats.isGateRusher;
		}
		set
		{
			mStats.isGateRusher = value;
		}
	}

	public bool isFlying
	{
		get
		{
			return mStats.isFlying;
		}
		set
		{
			mStats.isFlying = value;
		}
	}

	public string spawnFriendID
	{
		get
		{
			return mStats.spawnFriendID;
		}
		set
		{
			mStats.spawnFriendID = value;
		}
	}

	public bool exploseOnMelee
	{
		get
		{
			return mStats.exploseOnMelee;
		}
		set
		{
			mStats.exploseOnMelee = value;
		}
	}

	public bool enemyIgnoresMe
	{
		get
		{
			return mStats.enemyIgnoresMe;
		}
		set
		{
			mStats.enemyIgnoresMe = value;
		}
	}

	public float meleeFreeze
	{
		get
		{
			return mStats.meleeFreeze;
		}
		set
		{
			mStats.meleeFreeze = value;
		}
	}

	public bool isBoss
	{
		get
		{
			return mStats.isBoss;
		}
		set
		{
			mStats.isBoss = value;
		}
	}

	public bool isMount
	{
		get
		{
			return mStats.isMount;
		}
		set
		{
			mStats.isMount = value;
		}
	}

	public bool isMounted
	{
		get
		{
			return mStats.isMounted;
		}
		set
		{
			mStats.isMounted = value;
		}
	}

	public bool isActive
	{
		get
		{
			return mActive && controller != null;
		}
		set
		{
			mActive = value;
			if (controller != null)
			{
				controller.enabled = value;
			}
		}
	}

	public bool canMeleeAttack
	{
		get
		{
			return isEffectivelyIdle && meleeAttackRange > 0f && (mInMeleeAttackCycle || meleePreAttackDelay <= 0f) && (mMeleeAttackDelay <= 0f || !mMeleeAttackDelivered);
		}
	}

	public bool canMeleeFliers
	{
		get
		{
			return mStats.canMeleeFliers;
		}
		set
		{
			mStats.canMeleeFliers = value;
		}
	}

	public bool canMeleeProjectiles
	{
		get
		{
			return mStats.canMeleeProjectiles;
		}
		set
		{
			mStats.canMeleeProjectiles = value;
		}
	}

	public bool canUseRangedAttack
	{
		get
		{
			return isEffectivelyIdle && bowAttackRange > 0f && bowProjectile != "None" && !string.IsNullOrEmpty(bowProjectile) && (mInRangedAttackCycle || rangedPreAttackDelay <= 0f) && (mRangedAttackDelay <= 0f || !mRangedAttackDelivered);
		}
	}

	public float meleePreAttackDelay
	{
		get
		{
			return isPlayer ? 0f : ((!isEnemy) ? WeakGlobalInstance<CharactersManager>.Instance.allyMeleePreAttackDelay : WeakGlobalInstance<CharactersManager>.Instance.enemyMeleePreAttackDelay);
		}
	}

	public float rangedPreAttackDelay
	{
		get
		{
			return isPlayer ? 0f : ((!isEnemy) ? WeakGlobalInstance<CharactersManager>.Instance.allyRangedPreAttackDelay : WeakGlobalInstance<CharactersManager>.Instance.enemyRangedPreAttackDelay);
		}
	}

	public bool isSpellCasterOnAllies
	{
		get
		{
			return (mStats.projectile == "HealBolt" || mStats.projectile == "EvilHealBolt") && bowAttackRange > 0f;
		}
	}

	public bool onlyCorruptibleTargets
	{
		get
		{
			return mStats.projectile == "Corruption";
		}
	}

	public string corruptionID { get; protected set; }

	public bool hasRangedAttack
	{
		get
		{
			return bowAttackRange > 0f && mStats.projectile != "HealBolt" && mStats.projectile != "None" && !string.IsNullOrEmpty(mStats.projectile);
		}
	}

	public bool isEffectivelyIdle
	{
		get
		{
			return controller.isEffectivelyIdle;
		}
	}

	public bool canMove
	{
		get
		{
			if (!isEffectivelyIdle)
			{
				return false;
			}
			UpdateMoveDelay();
			return mMoveDelay <= 0f;
		}
	}

	public bool isInKnockback
	{
		get
		{
			return controller.isInKnockback;
		}
	}

	public bool isInJump
	{
		get
		{
			return controller.isInJump;
		}
	}

	public EAttackType lastAttackTypeHitWith { get; protected set; }

	public Action onDeathEvent { get; set; }

	public float meleeAttackRange
	{
		get
		{
			return mStats.meleeAttackRange;
		}
		set
		{
			mStats.meleeAttackRange = Mathf.Abs(value);
		}
	}

	public GameRange meleeAttackGameRange
	{
		get
		{
			if (controller.facing == FacingType.Right)
			{
				return new GameRange(position.z, position.z + mStats.meleeAttackRange);
			}
			return new GameRange(position.z - mStats.meleeAttackRange, position.z);
		}
	}

	public GameRange meleeAttackHitGameRange
	{
		get
		{
			if (controller.facing == FacingType.Right)
			{
				return new GameRange(position.z, position.z + mStats.meleeAttackRange * 1.3f);
			}
			return new GameRange(position.z - mStats.meleeAttackRange * 1.3f, position.z);
		}
	}

	public float meleeAttackFrequency
	{
		get
		{
			return mStats.meleeAttackFrequency;
		}
		set
		{
			mStats.meleeAttackFrequency = Mathf.Abs(value);
		}
	}

	public bool meleeWeaponIsABlade
	{
		get
		{
			return mStats.meleeWeaponIsABlade;
		}
		set
		{
			mStats.meleeWeaponIsABlade = value;
		}
	}

	public int knockbackPower
	{
		get
		{
			return mStats.knockbackPower;
		}
		set
		{
			mStats.knockbackPower = value;
		}
	}

	public int knockbackPowerRanged
	{
		get
		{
			return mStats.knockbackPowerRanged;
		}
		set
		{
			mStats.knockbackPowerRanged = value;
		}
	}

	public int knockbackResistance
	{
		get
		{
			return mStats.knockbackResistance;
		}
		set
		{
			mStats.knockbackResistance = value;
		}
	}

	public float bowAttackRange
	{
		get
		{
			return mStats.bowAttackRange;
		}
		set
		{
			mStats.bowAttackRange = Mathf.Abs(value);
		}
	}

	public GameRange bowAttackGameRange
	{
		get
		{
			if (controller.facing == FacingType.Left)
			{
				return new GameRange(position.z - mStats.bowAttackRange, position.z - mStats.meleeAttackRange);
			}
			return new GameRange(position.z + mStats.meleeAttackRange, position.z + mStats.bowAttackRange);
		}
	}

	public GameRange bowAttackExtGameRange
	{
		get
		{
			if (controller.facing == FacingType.Left)
			{
				return new GameRange(position.z - mStats.bowAttackRange * 1.3f, position.z - mStats.meleeAttackRange);
			}
			return new GameRange(position.z + mStats.meleeAttackRange, position.z + mStats.bowAttackRange * 1.3f);
		}
	}

	public GameRange bowAttackHitGameRange
	{
		get
		{
			if (controller.facing == FacingType.Left)
			{
				return new GameRange(position.z - mStats.bowAttackRange * 1.3f, position.z);
			}
			return new GameRange(position.z, position.z + mStats.bowAttackRange * 1.3f);
		}
	}

	public GameRange buffEffectGameRange
	{
		get
		{
			return new GameRange(position.z - mStats.bowAttackRange, position.z + mStats.bowAttackRange);
		}
	}

	public float bowAttackFrequency
	{
		get
		{
			return mStats.bowAttackFrequency;
		}
		set
		{
			mStats.bowAttackFrequency = Mathf.Abs(value);
		}
	}

	public virtual float bowDamage
	{
		get
		{
			return mStats.bowAttackDamage * (1f + mDamageBuffPercent);
		}
		set
		{
			mStats.bowAttackDamage = value;
		}
	}

	public string bowProjectile
	{
		get
		{
			return mStats.projectile;
		}
		set
		{
			mStats.projectile = value;
		}
	}

	public DOTInfo dotInfo
	{
		get
		{
			return mStats.dotInfo;
		}
		set
		{
			mStats.dotInfo = value;
		}
	}

	public bool usesFocusedFire
	{
		get
		{
			return bowProjectile != "IceArrow";
		}
	}

	public List<WeakPtr<Character>> targetedAttackers
	{
		get
		{
			return mTargetedAttackers;
		}
	}

	public virtual float meleeDamage
	{
		get
		{
			return mStats.meleeAttackDamage * (1f + mDamageBuffPercent);
		}
		set
		{
			mStats.meleeAttackDamage = Mathf.Max(0f, value);
		}
	}

	public float damageBuffPercent
	{
		get
		{
			return mStats.damageBuffPercent;
		}
		set
		{
			mStats.damageBuffPercent = Mathf.Max(0f, value);
		}
	}

	public GameObject damageBuffEffect { get; set; }

	public float speedBuffModifier
	{
		get
		{
			return mSpeedBuffModifier;
		}
		set
		{
			mSpeedBuffModifier = value;
		}
	}

	public float buffFrequency { get; set; }

	public bool buffAffectsSelf { get; set; }

	public int leadershipCostModifierBuff
	{
		get
		{
			return mStats.leadershipCostModifierBuff;
		}
		set
		{
			mStats.leadershipCostModifierBuff = value;
		}
	}

	public string upgradeAlliesFrom
	{
		get
		{
			return mStats.upgradeAlliesFrom;
		}
		set
		{
			mStats.upgradeAlliesFrom = value;
		}
	}

	public string upgradeAlliesTo
	{
		get
		{
			return mStats.upgradeAlliesTo;
		}
		set
		{
			mStats.upgradeAlliesTo = value;
		}
	}

	public CanBuffFunc canBuffFunc
	{
		get
		{
			return mStats.canBuffFunc;
		}
		set
		{
			mStats.canBuffFunc = value;
		}
	}

	public float postAttackMoveDelay
	{
		get
		{
			float num = Mathf.Max(meleeAttackRange, bowAttackRange);
			num -= 150f;
			if (num <= 0f || isSpellCasterOnAllies)
			{
				return 0f;
			}
			return Mathf.Min(num / controller.speed, 0.5f);
		}
	}

	public Vector3 position
	{
		get
		{
			return controller.position;
		}
		set
		{
			controller.position = value;
		}
	}

	public ResourceDrops resourceDrops
	{
		get
		{
			return mResourceDrops;
		}
	}

	public bool soundEnabled
	{
		get
		{
			if (controlledObject != null)
			{
				SoundThemePlayer component = controlledObject.GetComponent<SoundThemePlayer>();
				if (component != null)
				{
					return component.enabled;
				}
			}
			return false;
		}
		set
		{
			if (!(controlledObject != null))
			{
				return;
			}
			SoundThemePlayer component = controlledObject.GetComponent<SoundThemePlayer>();
			if (component != null)
			{
				component.enabled = value;
				if (!value && component.GetComponent<AudioSource>() != null && component.GetComponent<AudioSource>().isPlaying)
				{
					component.GetComponent<AudioSource>().enabled = false;
					component.GetComponent<AudioSource>().Stop();
				}
			}
		}
	}

	public GameObject rootObject
	{
		get
		{
			return (!(controller != null)) ? null : controller.gameObject;
		}
	}

	protected bool isHealing { get; private set; }

	public HUDHealthBarMini healthBarMini { get; set; }

	protected Character singleAttackTarget
	{
		get
		{
			return (mSingleAttackTarget == null) ? null : mSingleAttackTarget.ptr;
		}
		set
		{
			if (singleAttackTarget == value)
			{
				return;
			}
			if (singleAttackTarget != null)
			{
				singleAttackTarget.targetedAttackers.RemoveAll((WeakPtr<Character> item) => item == null || item.ptr == null || item.ptr == this || item.ptr.health <= 0f);
			}
			if (value == null)
			{
				mSingleAttackTarget = null;
				return;
			}
			mSingleAttackTarget = new WeakPtr<Character>(value);
			value.targetedAttackers.Add(new WeakPtr<Character>(this));
		}
	}

	private float randomCriticalDamage
	{
		get
		{
			if (mStats.criticalMultiplier > 1f && UnityEngine.Random.value < mStats.criticalChance)
			{
				return mStats.meleeAttackDamage * mStats.criticalMultiplier - mStats.meleeAttackDamage;
			}
			return 0f;
		}
	}

	public bool dynamicSpawn { get; set; }

	public string id { get; set; }

	public int ownerId { get; set; }

	public float mountedHealth { get; protected set; }

	public float mountedHealthMax { get; protected set; }

	public CharacterData charData { get; private set; }

	public bool isBlastoffFlyer { get; protected set; }

	public bool LeftToRight
	{
		get
		{
			return mIsLeftToRightGameplay;
		}
		set
		{
			mIsLeftToRightGameplay = value;
		}
	}

	public bool BlocksHeroMovement { get; set; }

	public Character()
	{
		mResourceDrops = new ResourceDrops();
	}

	public virtual void Update()
	{
		mBuffIconClient.Update();
		float? num = mLeniencyTimer;
		if (num.HasValue)
		{
			float? num2 = mLeniencyTimer;
			mLeniencyTimer = ((!num2.HasValue) ? null : new float?(num2.Value - Time.deltaTime));
		}
		if (health > 0f)
		{
			UpdateBuffsAndAfflictions();
			UpdateAutoRecovery();
			UpdateAttack();
		}
		else
		{
			Die();
		}
		UpdateAiming();
		if (healthBarMini != null)
		{
			healthBarMini.Update();
		}
		timeSinceDamaged += Time.deltaTime;
		UpdateHealthTexts();
	}

	public virtual void Destroy()
	{
		GameObject go = controlledObject;
		ProceduralShaderManager.StopShaderEvents((ShaderEvent se) => se.gameObject == go);
		if (isUnique && WeakGlobalMonoBehavior<InGameImpl>.Instance != null && WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(ownerId) != null)
		{
			WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(ownerId).RegisterUnique(uniqueID, false);
		}
		DestroyMeleeWeaponPrefabs();
		rangedWeaponPrefab = null;
		mPaperDollMeleeWeapon.Clear();
		mPaperDollRangedWeapon = null;
		if (controller != null)
		{
			controller.Destroy();
			controller = null;
		}
		if (healthBarMini != null)
		{
			healthBarMini.Destroy();
			healthBarMini = null;
		}
		foreach (HealthTextInfo healthTextObject in healthTextObjects)
		{
			sObjectPool.Release(healthTextObject.obj);
		}
		EffectKiller[] componentsInChildren = go.GetComponentsInChildren<EffectKiller>(true);
		if (componentsInChildren != null)
		{
			EffectKiller[] array = componentsInChildren;
			foreach (EffectKiller effectKiller in array)
			{
				effectKiller.transform.parent = null;
				effectKiller.Cleanup();
			}
		}
		ClearBuffs();
		BreakWeakLinks();
	}

	private void CalcHealthTextPos(ref Vector3 currentPos, float lifetime)
	{
		currentPos.y += 1.5f + lifetime * 0.5f;
		currentPos = mObjectCamera.WorldToScreenPoint(currentPos);
		currentPos = mGluiCamera.ScreenToWorldPoint(currentPos);
		currentPos.z += 1000f;
	}

	private void AddHealthText(float damage)
	{
		SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/UI/Prefabs/HUD/HealthTextPF.prefab", 1);
		GameObject gameObject = sObjectPool.Acquire(cachedResource.Resource as GameObject);
		Vector3 currentPos = controlledObject.transform.position;
		CalcHealthTextPos(ref currentPos, 0f);
		gameObject.transform.position = currentPos;
		GluiText component = gameObject.GetComponent<GluiText>();
		component.Text = damage.ToString();
		component.Color = Color.red;
		gameObject.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
		HealthTextInfo item = new HealthTextInfo(gameObject);
		healthTextObjects.Add(item);
	}

	private void UpdateHealthTexts()
	{
		Vector3 vector = controlledObject.transform.position;
		for (int num = healthTextObjects.Count - 1; num >= 0; num--)
		{
			HealthTextInfo value = healthTextObjects[num];
			GameObject obj = value.obj;
			value.lifetime += Time.deltaTime;
			if (obj == null)
			{
				healthTextObjects.RemoveAt(num);
			}
			else if (value.lifetime > 2f)
			{
				sObjectPool.Release(obj);
				healthTextObjects.RemoveAt(num);
			}
			else
			{
				Vector3 currentPos = vector;
				float lifetime = value.lifetime;
				CalcHealthTextPos(ref currentPos, lifetime);
				Transform transform = obj.transform;
				transform.position = currentPos;
				value.lifetime += Time.deltaTime;
				healthTextObjects[num] = value;
			}
		}
	}

	public void ApplyBuff(float damagePercent, float speedModifier, float duration, object source, GameObject effect, string jointForAttach)
	{
		int num = mBuffReceievedInfos.FindIndex((BuffInfo n) => n.source == source);
		BuffInfo buffInfo = default(BuffInfo);
		buffInfo.percentDamageToAdd = damagePercent;
		buffInfo.speedModifier = speedModifier;
		buffInfo.duration = duration;
		buffInfo.source = source;
		if (num < 0)
		{
			if (effect != null)
			{
				buffInfo.effect = controller.InstantiateObjectOnJoint(effect, jointForAttach);
			}
			mBuffReceievedInfos.Add(buffInfo);
			mBuffChanged = true;
		}
		else
		{
			buffInfo.effect = mBuffReceievedInfos[num].effect;
			mBuffReceievedInfos[num] = buffInfo;
			mBuffChanged = true;
		}
		controller.AddMaterialSwitchIgnoreObj(buffInfo.effect);
	}

	private void ClearBuffs()
	{
		foreach (BuffInfo mBuffReceievedInfo in mBuffReceievedInfos)
		{
			GameObjectPool.DefaultObjectPool.Release(mBuffReceievedInfo.effect);
		}
		mBuffReceievedInfos.Clear();
	}

	private void UpdateBuffsAndAfflictions()
	{
		float deltaTime = Time.deltaTime;
		if (mHasDOT)
		{
			if (mDOTduration > 0f)
			{
				mDOTduration -= deltaTime;
				mDOTcurrentTime -= deltaTime;
				if (mDOTcurrentTime <= 0f)
				{
					lastAttackTypeHitWith = EAttackType.DOT;
					if (mDOTAttacker == WeakGlobalMonoBehavior<InGameImpl>.Instance.hero)
					{
						Singleton<PlayerWaveEventData>.Instance.AccumulateDamage(Mathf.Min(health, mDOTdamage));
					}
					if (!invuln && !enemyIgnoresMe)
					{
						health -= mDOTdamage;
					}
					mDOTcurrentTime = mDOTfrequency;
					if (mDOTpauseTime > 0f)
					{
						controller.impactPauseTime = mDOTpauseTime;
					}
					if (mDOTColorTint.HasValue)
					{
						MaterialColorFlicker(mDOTColorTint.Value, 0f, 0.03f, 0f, mDOTfrequency);
					}
				}
			}
			else
			{
				mDOTduration = 0f;
				mDOTdamage = 0f;
				mDOTcurrentTime = 0f;
				mDOTfrequency = 0f;
				mDOTAttacker = null;
				mHasDOT = false;
			}
		}
		if (health <= 0f)
		{
			buffIcon.Hide("Assets/Game/Resources/FX/BuffIcon.prefab");
			ClearBuffs();
		}
		else
		{
			for (int num = mBuffReceievedInfos.Count - 1; num >= 0; num--)
			{
				BuffInfo value = mBuffReceievedInfos[num];
				float num2 = Mathf.Max(0f, value.duration - deltaTime);
				if (num2 <= 0f)
				{
					GameObjectPool.DefaultObjectPool.Release(value.effect);
					value.effect = null;
					mBuffReceievedInfos.RemoveAt(num);
					mBuffChanged = true;
				}
				else
				{
					value.duration = num2;
					mBuffReceievedInfos[num] = value;
				}
			}
			if (mBuffChanged)
			{
				float num3 = 0f;
				float num4 = 1f;
				bool flag = false;
				for (int num5 = mBuffReceievedInfos.Count - 1; num5 >= 0; num5--)
				{
					BuffInfo buffInfo = mBuffReceievedInfos[num5];
					if (buffInfo.effect == null)
					{
						flag = true;
					}
					num3 += buffInfo.percentDamageToAdd;
					num4 += buffInfo.speedModifier - 1f;
				}
				if (!flag)
				{
					buffIcon.Hide("Assets/Game/Resources/FX/BuffIcon.prefab");
				}
				else
				{
					buffIcon.Show("Assets/Game/Resources/FX/BuffIcon.prefab");
				}
				mDamageBuffPercent = num3;
				controller.speedModifier = Mathf.Clamp(num4, 0.3f, 2f);
				mBuffChanged = false;
			}
		}
		buffFrequency -= deltaTime;
		if ((damageBuffPercent != 0f || speedBuffModifier != 1f) && buffFrequency <= bowAttackFrequency * 0.5f)
		{
			buffFrequency = Mathf.Max(bowAttackFrequency, 1f);
			List<Character> alliesInRange = WeakGlobalInstance<CharactersManager>.Instance.GetAlliesInRange(buffEffectGameRange, ownerId);
			foreach (Character item in alliesInRange)
			{
				if ((buffAffectsSelf || (item != this && item.acceptsBuffs)) && (canBuffFunc == null || canBuffFunc(item)))
				{
					item.ApplyBuff(damageBuffPercent, speedBuffModifier, buffFrequency, this, damageBuffEffect, "body_effect");
				}
			}
		}
		if (!isEnemy && !isBase && WeakGlobalMonoBehavior<InGameImpl>.Instance != null && WeakGlobalMonoBehavior<InGameImpl>.Instance.allAlliesInvincibleTimer > 0f && acceptsBuffs && acceptsBuffs)
		{
			buffIcon.Show("Assets/Game/Resources/FX/BuffNightOfDeadIcon.prefab", WeakGlobalMonoBehavior<InGameImpl>.Instance.allAlliesInvincibleTimer, 10);
		}
		if (string.IsNullOrEmpty(upgradeAlliesFrom) || !(mRangedAttackDelay <= 0f) || string.IsNullOrEmpty(upgradeAlliesTo))
		{
			return;
		}
		mRangedAttackDelay = bowAttackFrequency;
		List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(buffEffectGameRange, ownerId);
		foreach (Character item2 in charactersInRange)
		{
			if (CheckIfTargetValid(item2) && item2.uniqueID == upgradeAlliesFrom)
			{
				Helper cToChange = item2 as Helper;
				if (uniqueID == "Swordsmith")
				{
					Singleton<Achievements>.Instance.IncrementAchievement("SwordsmithSwap", 1);
					WeakGlobalMonoBehavior<InGameImpl>.Instance.CanDoRevolutionAchievement = true;
				}
				else if (uniqueID == "Rifleman")
				{
					Singleton<Achievements>.Instance.IncrementAchievement("RiflesmithSwap", 1);
				}
				else if (uniqueID == "Horse_Master")
				{
					Singleton<Achievements>.Instance.IncrementAchievement("HorsemasterSwap", 1);
				}
				CharactersManager instance = WeakGlobalInstance<CharactersManager>.Instance;
				instance.postUpdateFunc = (Action)Delegate.Combine(instance.postUpdateFunc, (Action)delegate
				{
					WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(ownerId).ReplaceHelperWith(cToChange, upgradeAlliesTo);
				});
			}
		}
	}

	public void DoExplosionBodyFX(GameObject go)
	{
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			renderer.enabled = false;
		}
		SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/BodyExplosion.prefab", 1);
		if (cachedResource.Resource != null)
		{
			controller.SpawnEffectAt(cachedResource.Resource as GameObject, transform.position);
		}
	}

	public virtual void RecievedAttackFX(EAttackType attackType, float damage, Character attacker)
	{
		float num = ((!isPlayer && !canMeleeProjectiles && !impactPauseSuspended) ? 0.12f : 0f);
		bool flag = true;
		GameObject gameObject = null;
		bool flag2 = WeakGlobalMonoBehavior<InGameImpl>.Instance.ShouldShowHitFX();
		switch (attackType)
		{
		case EAttackType.Arrow:
		case EAttackType.FireArrow:
			controller.PlaySoundEvent("ArrowImpact");
			gameObject = controller.arrowImpactEffect;
			break;
		case EAttackType.Bullet:
			controller.PlaySoundEvent("BulletImpact");
			gameObject = controller.arrowImpactEffect;
			break;
		case EAttackType.Shuriken:
			num = 0f;
			goto case EAttackType.Arrow;
		case EAttackType.Blade:
			controller.PlaySoundEvent("SwordImpact");
			gameObject = controller.bladeImpactEffect;
			break;
		case EAttackType.Blunt:
			controller.PlaySoundEvent("BluntWepImpact");
			gameObject = controller.bladeImpactEffect;
			break;
		case EAttackType.BladeCritical:
			controller.PlaySoundEvent("SwordImpactBig");
			gameObject = controller.bladeCriticalImpactEffect;
			break;
		case EAttackType.Stealth:
			controller.PlaySoundEvent("SwordImpactBig");
			gameObject = controller.bladeImpactEffect;
			break;
		case EAttackType.BluntCritical:
			controller.PlaySoundEvent("BluntWepImpactBig");
			gameObject = controller.bladeCriticalImpactEffect;
			break;
		case EAttackType.Trample:
			if ((double)UnityEngine.Random.value < 0.5)
			{
				controller.PlaySoundEvent("SwordImpact");
				gameObject = controller.bladeImpactEffect;
			}
			else
			{
				controller.PlaySoundEvent("BluntWepImpact");
				gameObject = controller.bluntImpactEffect;
			}
			break;
		case EAttackType.Sonic:
			num = 0f;
			controller.StopWalking();
			controller.LoopStunAnim("stun");
			break;
		case EAttackType.Slice:
			if (!(health > damage))
			{
				break;
			}
			gameObject = controller.bladeImpactEffect;
			if (flag2)
			{
				SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/KatanaBlood.prefab", 1);
				if (cachedResource != null)
				{
					controller.SpawnEffectAtJoint(cachedResource.Resource as GameObject, "impact_target", false);
				}
			}
			break;
		case EAttackType.Lightning:
			flag = false;
			controller.PlayHurtAnim("shock");
			break;
		case EAttackType.Flash:
			num = 0f;
			flag = false;
			controller.StopWalking();
			controller.LoopStunAnim("stun");
			break;
		case EAttackType.Holy:
		case EAttackType.Stomp:
			flag = false;
			break;
		case EAttackType.DOT:
			flag = false;
			num = 0f;
			gameObject = null;
			break;
		case EAttackType.Explosion:
			if (health <= damage)
			{
				num = 0f;
			}
			break;
		}
		if (damage > 0f)
		{
			if (num > 0f)
			{
				controller.impactPauseTime = num;
			}
			if (gameObject != null && flag2)
			{
				controller.SpawnEffectAtJoint(gameObject, "impact_target", false);
				WeakGlobalMonoBehavior<InGameImpl>.Instance.ResetHitEffectTimer();
			}
			if (flag && supportColorFlash)
			{
				ProceduralShaderManager.postShaderEvent(new DiffuseFadeInOutShaderEvent(controller.gameObject, Color.red, 0f, 0f, 0.25f, OriginalMaterialColors, controller.GetMaterialSwitchIgnoreObjects()));
			}
			timeSinceDamaged = 0f;
			if (Singleton<Profile>.Instance.showHealthText)
			{
				AddHealthText(damage);
			}
		}
	}

	public void RecievedAttack(EAttackType attackType, float damage, Character attacker)
	{
		RecievedAttack(attackType, damage, attacker, true);
	}

	public virtual void RecievedAttack(EAttackType attackType, float damage, Character attacker, bool canReflect)
	{
		if (!(health <= 0f) && !invuln && !enemyIgnoresMe)
		{
			lastAttackTypeHitWith = attackType;
			RecievedAttackFX(attackType, damage, attacker);
			if (attacker == WeakGlobalMonoBehavior<InGameImpl>.Instance.hero)
			{
				Singleton<PlayerWaveEventData>.Instance.AccumulateDamage(Mathf.Min(health, damage));
			}
			health -= damage;
		}
	}

	public virtual void RecievedHealing(float healAmount)
	{
		if (!(health <= 0f) && (!isPlayer || !(health <= 0.1f) || WeakGlobalMonoBehavior<InGameImpl>.Instance.gate.health != 0f))
		{
			health += healAmount;
			SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource((healAmount >= 0f != isEnemy) ? "Assets/Game/Resources/FX/HealAura.prefab" : "Assets/Game/Resources/FX/EvilHealAura.prefab", 1);
			if (cachedResource != null)
			{
				controller.SpawnEffectAtJoint(cachedResource.Resource as GameObject, "impact_target", true, 0.5f);
			}
		}
	}

	public bool TryKnockback(int attemptPower)
	{
		return TryKnockback(attemptPower, false, Vector3.zero);
	}

	public bool TryKnockback(int attemptPower, bool force, Vector3 positionAdjust)
	{
		if ((!force && knockbackResistance >= 100) || attemptPower <= 0 || health <= 0f || isBase || isInKnockback)
		{
			return false;
		}
		if (attemptPower < 100)
		{
			attemptPower -= knockbackResistance;
		}
		if (attemptPower <= 1)
		{
			attemptPower = 1;
		}
		if (UnityEngine.Random.Range(0, 100) < attemptPower)
		{
			ForceKnockback(positionAdjust);
			return true;
		}
		return false;
	}

	public void ForceKnockback(Vector3 positionAdjustment)
	{
		if (!(health <= 0f) && !isBase)
		{
			controller.Knockback(positionAdjustment);
		}
	}

	public void Die()
	{
		string dieAnim = "die";
		switch (lastAttackTypeHitWith)
		{
		case EAttackType.BladeCritical:
		case EAttackType.Slice:
			if (!controller.isInKnockback)
			{
				dieAnim = "diehead";
			}
			break;
		case EAttackType.Explosion:
		case EAttackType.Sonic:
			dieAnim = "dieexplode";
			break;
		case EAttackType.Stealth:
			dieAnim = "diestealth";
			break;
		case EAttackType.Trample:
			if ((double)UnityEngine.Random.value < 0.2 && !controller.isInKnockback)
			{
				dieAnim = "diehead";
			}
			break;
		case EAttackType.Stomp:
			dieAnim = "diesmashed";
			break;
		case EAttackType.Lightning:
			dieAnim = "dieashes";
			break;
		}
		if (isEnemy && controller != null && !controller.startedDieAnim && WeakGlobalInstance<WaveManager>.Instance != null && WeakGlobalInstance<WaveManager>.Instance.isDone && WeakGlobalInstance<CharactersManager>.Instance.enemiesCount == 1)
		{
			dieAnim = "dieexplode";
			controller.impactPauseTime = 0f;
		}
		Die(dieAnim);
	}

	public void Die(string dieAnim)
	{
		buffIcon.Clear();
		if (dieAnim.Length < 3 || !(controller != null) || controller.currentAnimation == null || controller.currentAnimation.Length < 3 || !(dieAnim.Substring(0, 3) == "die") || !(controller.currentAnimation.Substring(0, 3) == "die"))
		{
			singleAttackTarget = null;
			if (onDeathEvent != null)
			{
				CharactersManager instance = WeakGlobalInstance<CharactersManager>.Instance;
				instance.postUpdateFunc = (Action)Delegate.Combine(instance.postUpdateFunc, onDeathEvent);
				onDeathEvent = null;
			}
			if (meleeWeaponPrefab != null)
			{
				SetRangeAttackMode(false);
			}
			if (controller != null)
			{
				controller.Die(dieAnim, postDeathAction);
			}
			OnDied();
		}
	}

	protected virtual void OnDied()
	{
	}

	public void StartMeleeAttackDelayTimer()
	{
		if (!isPlayer && !mInMeleeAttackCycle)
		{
			WeakGlobalInstance<CharactersManager>.Instance.RegisterAttackStarted(ownerId, false);
		}
		mMeleeAttackDelay = meleeAttackFrequency;
		mMoveDelay = postAttackMoveDelay;
		mMeleeAttackDelivered = false;
	}

	public void StartRangedAttackDelayTimer()
	{
		if (!isPlayer && !mInRangedAttackCycle && !isSpellCasterOnAllies)
		{
			WeakGlobalInstance<CharactersManager>.Instance.RegisterAttackStarted(ownerId, true);
		}
		mRangedAttackDelay = bowAttackFrequency;
		mMoveDelay = postAttackMoveDelay;
		mRangedAttackDelivered = false;
	}

	public void StopMoving(float stopTime)
	{
		mMoveDelay = Mathf.Max(stopTime, mMoveDelay);
	}

	public int DispersedAttackersCount(Character checkingAttacker)
	{
		targetedAttackers.RemoveAll((WeakPtr<Character> item) => item == null || item.ptr == null || item.ptr.health <= 0f || item.ptr.singleAttackTarget != this);
		int num = 0;
		foreach (WeakPtr<Character> targetedAttacker in targetedAttackers)
		{
			if (targetedAttacker.ptr != checkingAttacker && !targetedAttacker.ptr.usesFocusedFire)
			{
				num++;
			}
		}
		return num;
	}

	protected virtual GameObject GetMeleeUpgradeEffect()
	{
		return null;
	}

	public void DestroyMeleeWeaponPrefabs()
	{
		if (mPaperDollMeleeWeapon != null)
		{
			foreach (GameObject item in mPaperDollMeleeWeapon)
			{
				if (item != null)
				{
					GameObjectPool.DefaultObjectPool.Release(item);
				}
			}
			mPaperDollMeleeWeapon.Clear();
		}
		else
		{
			mPaperDollMeleeWeapon = new List<GameObject>();
		}
	}

	public void SetMeleeWeaponTrailVisible(bool visible)
	{
		if (mMeleeWeaponTrail == null)
		{
			return;
		}
		foreach (MeleeWeaponTrail item in mMeleeWeaponTrail)
		{
			item.enabled = visible;
		}
	}

	protected virtual GameObject GetRangedUpgradeEffect()
	{
		return null;
	}

	public void ForceDeath()
	{
		mLeniencyTimer = -1f;
		health = 0f;
	}

	public void SetDamageOverTime(float damagePerTick, float tickFrequency, float durationOfEffect, Character attacker)
	{
		mDOTcurrentTime = 0f;
		mDOTdamage = damagePerTick;
		mDOTfrequency = tickFrequency;
		mDOTduration = durationOfEffect;
		mDOTAttacker = attacker;
		mDOTpauseTime = 0f;
		mDOTColorTint = null;
		mHasDOT = true;
	}

	public void SetDamageOverTimeSpecialType(float damagePerTick, float tickFrequency, float durationOfEffect, Character attacker, float impactPauseTime, EAttackType attackType)
	{
		mDOTcurrentTime = 0f;
		mDOTdamage = damagePerTick;
		mDOTfrequency = tickFrequency;
		mDOTduration = durationOfEffect;
		mDOTAttacker = attacker;
		mDOTpauseTime = impactPauseTime;
		if (attackType == EAttackType.SoulBurn)
		{
			mDOTColorTint = Color.green;
		}
		else
		{
			mDOTColorTint = null;
		}
		mHasDOT = true;
	}

	public void MaterialColorFadeInOut(Color color, float fadeInTime, float holdTime, float fadeOutTime)
	{
		holdTime = Mathf.Max(0.1f, holdTime);
		ProceduralShaderManager.postShaderEvent(new DiffuseFadeInOutShaderEvent(controlledObject, color, fadeInTime, holdTime, fadeOutTime, OriginalMaterialColors, controller.GetMaterialSwitchIgnoreObjects()));
	}

	public void MaterialColorSetPermanentColor(Color color)
	{
		ProceduralShaderManager.postShaderEvent(new DiffusePermShaderEvent(controlledObject, color, controller.GetMaterialSwitchIgnoreObjects(), OriginalMaterialColors));
	}

	public void MaterialColorFlicker(Color color, float fadeInTime, float holdTime, float fadeOutTime, float durationOfEffect)
	{
		ProceduralShaderManager.postShaderEvent(new DiffuseFlickerShaderEvent(controlledObject, color, fadeInTime, holdTime, fadeOutTime, durationOfEffect, controller.GetMaterialSwitchIgnoreObjects(), OriginalMaterialColors));
	}

	public bool CheckIfTargetValid(Character target)
	{
		if (target == null || target.health <= 0f)
		{
			return false;
		}
		return true;
	}

	public bool CheckIfTargetValidForRangedAttack(Character target)
	{
		if (!CheckIfTargetValid(target))
		{
			return false;
		}
		if (!bowAttackHitGameRange.Contains(target.position.z))
		{
			return false;
		}
		return true;
	}

	public void ApplyIceEffect(float duration, Character source)
	{
		float speedModifier = 0.5f;
		string abilityID = "Lethargy";
		float fadeInTime = float.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute(abilityID, "ColorEffectFadeIn"));
		float fadeOutTime = float.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute(abilityID, "ColorEffectFadeOut"));
		SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/Lethargy.prefab", 1);
		GameObject effect = null;
		if (cachedResource != null)
		{
			effect = cachedResource.Resource as GameObject;
		}
		ApplyBuff(0f, speedModifier, duration, source, effect, "head_effect");
		Color color = new Color((float)int.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute(abilityID, "Red")) / 255f, (float)int.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute(abilityID, "Green")) / 255f, (float)int.Parse(Singleton<AbilitiesDatabase>.Instance.GetAttribute(abilityID, "Blue")) / 255f);
		MaterialColorFadeInOut(color, fadeInTime, duration, fadeOutTime);
	}

	private void SpawnOffspring(string friendID, int count)
	{
		if (controller.isPitThrown)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			Vector3 spawnPos = position;
			spawnPos.z += UnityEngine.Random.Range(-0.5f, 1f);
			spawnPos.x = WeakGlobalInstance<CharactersManager>.Instance.GetBestSpawnXPos(spawnPos, WeakGlobalInstance<Leadership>.Instance.helperSpawnArea.size.x, CharactersManager.ELanePreference.any, isEnemy, false, false);
			if (isEnemy)
			{
				Character character = null;
				bool flag = true;
				if (WeakGlobalInstance<WaveManager>.Instance != null)
				{
					character = WeakGlobalInstance<WaveManager>.Instance.ConstructEnemy(friendID, WeakGlobalInstance<WaveManager>.Instance.enemiesSpawnArea.size.x / 2f, spawnPos, true);
				}
				else if (WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(ownerId) != null)
				{
					Leadership.HelperTypeData data = new Leadership.HelperTypeData(friendID, 1);
					character = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(ownerId).SpawnHelper(data, 1.5f, spawnPos);
					flag = false;
				}
				if (character != null)
				{
					PerformKnockback(character, 100, false, Vector3.zero);
					if (flag)
					{
						WeakGlobalInstance<CharactersManager>.Instance.AddCharacter(character);
					}
				}
			}
			else if (WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(ownerId) != null)
			{
				Character character2 = WeakGlobalMonoBehavior<InGameImpl>.Instance.GetLeadership(ownerId).ForceSpawn(friendID);
				if (character2 != null)
				{
					character2.position = spawnPos;
					PerformKnockback(character2, 100, false, Vector3.zero);
				}
			}
		}
	}

	public void SpawnOffspringMulti(List<string> friendIDs, int count)
	{
		if (!controller.isPitThrown)
		{
			int i = 0;
			if (count > 0)
			{
				SpawnOffspring(friendIDs[0], count);
				i = 1;
			}
			for (; i < friendIDs.Count; i++)
			{
				SpawnOffspring(friendIDs[i], 1);
			}
		}
	}

	public void SetMirroredSkeleton(bool mirrored)
	{
		SkinnedMeshRenderer componentInChildren = controlledObject.GetComponentInChildren<SkinnedMeshRenderer>();
		if (!componentInChildren)
		{
			return;
		}
		Transform transform = componentInChildren.transform;
		Transform parent = componentInChildren.transform.parent;
		bool flag = false;
		int num = 0;
		while (parent != null && parent != null && !flag && num < 3)
		{
			for (int i = 0; i < parent.childCount; i++)
			{
				Transform child = parent.GetChild(i);
				if (child.name.Contains("root"))
				{
					transform = child;
					flag = true;
					break;
				}
			}
			parent = parent.parent;
			num++;
		}
		transform.localScale = new Vector3((!mirrored) ? 1 : (-1), 1f, 1f);
	}

	private void UpdateAutoRecovery()
	{
		isHealing = false;
		if (health != 0f && autoHealthRecovery != 0f)
		{
			if (mAutoRecoveryDelay > 0f)
			{
				mAutoRecoveryDelay -= Time.deltaTime;
			}
			if (mAutoRecoveryDelay <= 0f || !isPlayer)
			{
				health += Time.deltaTime * autoHealthRecovery;
				isHealing = true;
			}
		}
	}

	protected bool IsInMeleeRangeOfOpponent(bool thatCanAttack)
	{
		return meleeAttackRange > 0f && WeakGlobalInstance<CharactersManager>.Instance.IsCharacterInRange((!thatCanAttack || controller.isMoving) ? meleeAttackGameRange : meleeAttackHitGameRange, 1 - ownerId, isGateRusher, !thatCanAttack || canMeleeFliers, false, true, false);
	}

	protected bool IsInMeleeRangeOfProjectile()
	{
		return meleeAttackRange > 0f && WeakGlobalInstance<ProjectileManager>.Instance.IsProjectileInRange(meleeAttackHitGameRange, isEnemy);
	}

	protected bool IsInBowRangeOfOpponent()
	{
		return bowAttackRange > 0f && WeakGlobalInstance<CharactersManager>.Instance.IsCharacterInRange(bowAttackGameRange, 1 - ownerId, isGateRusher, true, false, true, onlyCorruptibleTargets);
	}

	protected bool IsInRangeOfHurtAlly()
	{
		return IsInRangeOfHurtAlly(true);
	}

	protected bool IsInRangeOfHurtAlly(bool includeDecayingCharacters)
	{
		return bowAttackRange > 0f && WeakGlobalInstance<CharactersManager>.Instance.IsCharacterInRange(buffEffectGameRange, ownerId, false, true, true, includeDecayingCharacters, false);
	}

	public void ActivateHealthBar()
	{
		if (WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			if (healthBarMini != null)
			{
				healthBarMini.Destroy();
			}
			healthBarMini = new HUDHealthBarMini(this);
			controller.AddMaterialSwitchIgnoreObj(healthBarMini.gameObject);
		}
	}

	protected void AttackByExploding()
	{
		mStats.health = 0f;
		Die("dieexplode");
		mStats.meleeAttackRange = explosionRange();
		OnAttackDelivery();
	}

	private void UpdateAttack()
	{
		if (mMeleeAttackDelay > 0f)
		{
			mInMeleeAttackCycle = true;
			mMeleeAttackDelay = Mathf.Max(0f, mMeleeAttackDelay - Time.deltaTime * controller.speedModifier);
		}
		else
		{
			mInMeleeAttackCycle = false;
		}
		if (mRangedAttackDelay > 0f)
		{
			mInRangedAttackCycle = true;
			mRangedAttackDelay = Mathf.Max(0f, mRangedAttackDelay - Time.deltaTime * controller.speedModifier);
		}
		else
		{
			mInRangedAttackCycle = false;
		}
		if (!controller.isAttacking || !canMeleeProjectiles)
		{
			return;
		}
		List<Projectile> projectiles = WeakGlobalInstance<ProjectileManager>.Instance.projectiles;
		foreach (Projectile item in projectiles)
		{
			if (isEnemy == item.shooter.isEnemy)
			{
				continue;
			}
			Vector3 location = item.transform.position;
			float z = location.z;
			if (meleeAttackHitGameRange.Contains(z))
			{
				SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/FX/ArmorSparks.prefab", 1);
				if (cachedResource != null)
				{
					controller.SpawnEffectAt(cachedResource.Resource as GameObject, location);
				}
				item.Destroy();
			}
		}
	}

	private void UpdateMoveDelay()
	{
		if (mMoveDelay > 0f && isEffectivelyIdle && mMeleeAttackDelay <= 0f && mRangedAttackDelay <= 0f)
		{
			mMoveDelay = Mathf.Max(0f, mMoveDelay - Time.deltaTime * controller.speedModifier);
		}
	}

	private void OnAttackDelivery()
	{
		mMeleeAttackDelivered = true;
		List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(meleeAttackHitGameRange, 1 - ownerId);
		int num = knockbackPower;
		float num2 = meleeFreeze;
		foreach (Character item in charactersInRange)
		{
			if (CheckIfTargetValid(item) && (!item.isFlying || canMeleeFliers))
			{
				float num3 = randomCriticalDamage;
				EAttackType attackType = EAttackType.Blunt;
				if (meleeWeaponIsABlade)
				{
					attackType = ((!(num3 > 0f)) ? EAttackType.Blade : EAttackType.BladeCritical);
				}
				else if (num3 > 0f)
				{
					attackType = EAttackType.BluntCritical;
				}
				else if (exploseOnMelee)
				{
					attackType = EAttackType.Explosion;
				}
				PerformKnockback(item, num, false, Vector3.zero);
				item.RecievedAttack(attackType, meleeDamage + randomCriticalDamage, this);
				num -= 100;
				if (num2 > 0f)
				{
					item.ApplyIceEffect(num2, this);
					num2 *= 0.6f;
				}
			}
		}
	}

	public virtual void PerformKnockback(Character target, int knockbackPower, bool force, Vector3 positionAdjust)
	{
		target.TryKnockback(knockbackPower, force, positionAdjust);
	}

	private void OnRangedAttackDelivery()
	{
		if (CheckIfTargetValidForRangedAttack(singleAttackTarget))
		{
			mRangedAttackDelivered = true;
			controller.SetArrowVisible(false);
			WeakGlobalInstance<ProjectileManager>.Instance.SpawnProjectile(bowProjectile, bowDamage, this, singleAttackTarget, controller.autoPaperdoll.GetJointPosition("projectile_spawn"));
		}
		else
		{
			controller.Idle();
		}
	}

	private void OnMeleeAttack(bool attacking)
	{
		SetMeleeWeaponTrailVisible(attacking);
	}

	protected void OnCastHeal()
	{
		mRangedAttackDelivered = true;
		List<Character> list = new List<Character>();
		List<Character> charactersInRange = WeakGlobalInstance<CharactersManager>.Instance.GetCharactersInRange(bowAttackHitGameRange, ownerId);
		float num = ((!(mountedHealthMax > 0f)) ? (health / maxHealth) : (mountedHealth / mountedHealthMax));
		foreach (Character item in charactersInRange)
		{
			if (item.health >= item.maxHealth)
			{
				list.Add(item);
				continue;
			}
			if (item.autoHealthRecovery < 0f)
			{
				list.Add(item);
				continue;
			}
			float num2 = item.health / item.maxHealth;
			if (num2 < num)
			{
				num = num2;
			}
		}
		foreach (Character item2 in list)
		{
			charactersInRange.Remove(item2);
		}
		list.Clear();
		foreach (Character item3 in charactersInRange)
		{
			float num3 = item3.health / item3.maxHealth;
			if (num3 > num + 0.4f && num3 > num + 0.2f)
			{
				list.Add(item3);
			}
		}
		foreach (Character item4 in list)
		{
			charactersInRange.Remove(item4);
		}
		list.Clear();
		float num4 = ((!isEnemy) ? float.MinValue : float.MaxValue);
		num = 1f;
		foreach (Character item5 in charactersInRange)
		{
			float num5 = item5.health / item5.maxHealth;
			if (!item5.isPlayer && !item5.isGateRusher && ((isEnemy && item5.position.z < num4) || (!isEnemy && item5.position.z > num4)) && num5 <= num + 0.4f)
			{
				num4 = item5.position.z;
				if (num5 < num)
				{
					num = num5;
				}
			}
		}
		foreach (Character item6 in charactersInRange)
		{
			if (Mathf.Abs(item6.position.z - num4) > 35f)
			{
				list.Add(item6);
			}
			else if (item6.health / item6.maxHealth > num + 0.2f)
			{
				list.Add(item6);
			}
		}
		foreach (Character item7 in list)
		{
			charactersInRange.Remove(item7);
		}
		list.Clear();
		Character character = null;
		foreach (Character item8 in charactersInRange)
		{
			if (character == null || item8.health < character.health)
			{
				character = item8;
			}
		}
		if (character == null)
		{
			character = ((isEnemy || !(Mathf.Abs(position.z - WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.position.z) <= bowAttackRange)) ? this : WeakGlobalMonoBehavior<InGameImpl>.Instance.hero);
		}
		controller.SetArrowVisible(false);
		WeakGlobalInstance<ProjectileManager>.Instance.SpawnProjectile(bowProjectile, bowDamage, this, character, controller.autoPaperdoll.GetJointPosition("projectile_spawn"));
	}

	protected void OnCastSpawnAllies()
	{
		if (spawnFriendID != null && !(spawnFriendID == string.Empty))
		{
			mRangedAttackDelivered = true;
			SpawnOffspring(spawnFriendID, 1);
		}
	}

	protected void SetRangeAttackMode(bool inRangeAttack)
	{
		if (rangedWeaponPrefab != null)
		{
			rangedWeaponPrefab.SetActive(inRangeAttack);
		}
		if (bowProjectile != null && WeakGlobalInstance<ProjectileManager>.Instance != null)
		{
			if (WeakGlobalInstance<ProjectileManager>.Instance.ProjectileNeedsBothHands(bowProjectile))
			{
				foreach (GameObject item in mPaperDollMeleeWeapon)
				{
					if (item != null)
					{
						item.SetActive(!inRangeAttack);
					}
				}
			}
			if (WeakGlobalInstance<ProjectileManager>.Instance.ProjectileShownWhileAiming(bowProjectile))
			{
				controller.SetArrowVisible(inRangeAttack, bowProjectile);
			}
		}
		controller.SetUseRangedAttackIdle(inRangeAttack);
	}

	private void UpdateAiming()
	{
		bool flag = controller.currentAnimation == "rangedattack" && !mRangedAttackDelivered;
		if (flag || controller.currentAnimation == "rangedattackidle")
		{
			if (!CheckIfTargetValidForRangedAttack(singleAttackTarget))
			{
				if (flag)
				{
					controller.Idle();
					return;
				}
				singleAttackTarget = WeakGlobalInstance<CharactersManager>.Instance.GetBestRangedAttackTarget(this, bowAttackExtGameRange);
				if (singleAttackTarget == null)
				{
					controller.ResetAimAngle();
					controller.ResetFacingAngle();
					return;
				}
			}
			Vector3 jointPosition = singleAttackTarget.controller.autoPaperdoll.GetJointPosition("impact_target");
			controller.FaceTowards(jointPosition);
			if (controller.autoPaperdoll.HasJoint("aim_angle"))
			{
				controller.AimTowards(bowProjectile, jointPosition);
			}
		}
		else if (hasRangedAttack && controller.currentAnimation != "rangedattack")
		{
			controller.ResetAimAngle();
			if (controller.currentAnimation != "idle")
			{
				controller.ResetFacingAngle();
			}
		}
	}

	protected void SetDefaultFacing()
	{
		controller.facing = (mIsLeftToRightGameplay ? FacingType.Right : FacingType.Left);
	}

	public void AssignCharData(CharacterData newData)
	{
		charData = newData;
	}
}
