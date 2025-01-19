using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterModelController : TaggedAnimPlayerController
{
	public enum EPostDeathAction
	{
		vanish = 0,
		melt = 1,
		ascend = 2,
		stay = 3
	}

	public delegate void ChargeMoveAction(float oldZPos, float newZPos);

	public const float kSpeedDefault = 1f;

	private const float kAimRotationSpeed = 150f;

	private const float kAimResetSpeed = 150f;

	private const float kKnockbackSpeed = 2f;

	private const float kKnockbackArcHeight = 0.62f;

	private const float kSimulatedKnockbackLandTime = 0.25f;

	private const float kJumpTime = 0.75f;

	private const float kJumpSpeed = 3f;

	private const float kJumpArcHeight = 1.25f;

	private const float kSimulatedJumpLandTime = 0f;

	private const float kMaxBackpedalTime = 1.3f;

	private const float kFacingTowardTurnSpeed = 180f;

	private const float kResetFacingTurnSpeed = 180f;

	private const float kKnockbackGravityAccel = 9.8f;

	public const float kKnockbackDistance = 1f;

	public bool attackActive;

	public float animatedWalkSpeed = 1f;

	public GameObject arrowImpactEffect;

	public GameObject bladeImpactEffect;

	public GameObject bladeCriticalImpactEffect;

	public GameObject bluntImpactEffect;

	public GameObject bluntCriticalImpactEffect;

	public bool snapToGround = true;

	private float mMaxBackPedalTime = 1.3f;

	public Action onMeleeAttackDelivery;

	public Action onRangedAttackDelivery;

	public Action<bool> onMeleeAttack;

	private Transform mCachedTransform;

	private float mSpeed;

	private float mConstraintLeft;

	private float mConstraintRight;

	private float mSpeedModifier = 1f;

	private float mImpactPauseTime;

	private FacingType mWalkDirection;

	private FacingType mFacing;

	private bool mAttackDeliveryTriggered;

	private int mUpdatesNoSnap = 1;

	private int mUpdatesUntilNextSnap;

	private bool mUseRangedAttackIdle;

	private bool mUseCowerIdle;

	private bool mPlayingHurtAnim;

	private bool mPlayingVictoryAnim;

	private bool mFirstUpdate;

	private bool mAttacking;

	private float mStunnedTimer;

	private float mKnockbackTime;

	private float mTotalKnockbackTime;

	private float mKnockbackAcceleration;

	private Vector3 mKnockbackVelocity;

	private float mJumpTime;

	private AutoPaperdoll mAutoPaperdoll;

	private UdamanSoundThemePlayer mSoundPlayer;

	private GameObject mProjectileInstance;

	private Action mOnAttackDelivery;

	private EPostDeathAction mPostDeathAction;

	private float? mAimAngle;

	private float? mHeadAngle;

	private List<GameObject> mMaterialSwitchIgnoreObjects = new List<GameObject>(1);

	private float mScaleTarget = 1f;

	private float mScaleRate;

	private float mOldScale = 1f;

	private float mScale = 1f;

	private CharacterModelController mAttachedCharacter;

	private Transform mAttachedToXForm;

	public Material materialOverride { get; set; }

	public float MaxBackPedalTime
	{
		get
		{
			return mMaxBackPedalTime;
		}
		set
		{
			mMaxBackPedalTime = value;
		}
	}

	public Vector3 KnockbackTargetPoint { get; private set; }

	public float KnockbackTime
	{
		get
		{
			return mKnockbackTime;
		}
		private set
		{
			mKnockbackTime = value;
		}
	}

	public Vector3 KnockbackVelocity
	{
		get
		{
			return mKnockbackVelocity;
		}
		private set
		{
			mKnockbackVelocity = value;
		}
	}

	public AutoPaperdoll autoPaperdoll
	{
		get
		{
			return mAutoPaperdoll;
		}
	}

	public Vector3 position
	{
		get
		{
			return mCachedTransform.position;
		}
		set
		{
			mCachedTransform.position = value;
		}
	}

	public float speed
	{
		get
		{
			return mSpeed;
		}
		set
		{
			mSpeed = value;
		}
	}

	public float constraintLeft
	{
		get
		{
			return mConstraintLeft;
		}
		set
		{
			mConstraintLeft = value;
		}
	}

	public float constraintRight
	{
		get
		{
			return mConstraintRight;
		}
		set
		{
			mConstraintRight = value;
		}
	}

	public FacingType facing
	{
		get
		{
			return mFacing;
		}
		set
		{
			SetFacing(value);
		}
	}

	public float baseFacingAngle
	{
		get
		{
			return (mFacing == FacingType.Left) ? 180 : 0;
		}
	}

	public override float speedModifier
	{
		get
		{
			return mSpeedModifier;
		}
		set
		{
			if (mWalkDirection == FacingType.Unchanged)
			{
				base.baseAnimSpeed = value;
				if (value < mSpeedModifier)
				{
					base.currAnimSpeed = value;
				}
			}
			if (value >= 0f)
			{
				mSpeedModifier = value;
			}
			else
			{
				mSpeedModifier = 1f;
			}
		}
	}

	public float impactPauseTime
	{
		get
		{
			return mImpactPauseTime;
		}
		set
		{
			mImpactPauseTime = value;
		}
	}

	public Action animEndedCallback
	{
		set
		{
			base.currAnimChangedCallback = (Action)Delegate.Combine(base.currAnimChangedCallback, value);
		}
	}

	public string ProjectileType { get; private set; }

	public bool startedDieAnim { get; set; }

	public bool readyToVanish
	{
		get
		{
			return startedDieAnim && !isAnimationPlaying && mPostDeathAction == EPostDeathAction.vanish;
		}
	}

	public bool isMoving
	{
		get
		{
			return mWalkDirection != FacingType.Unchanged;
		}
	}

	public bool isEffectivelyIdle
	{
		get
		{
			return !isInHurtState && !isPitThrown && (base.actionAnimState == null || base.actionAnim == "backpedalturn" || mPlayingVictoryAnim);
		}
	}

	public bool isInKnockback
	{
		get
		{
			return mKnockbackTime > 0f;
		}
	}

	public bool isPitThrown { get; set; }

	public bool isInJump
	{
		get
		{
			return mJumpTime > 0f;
		}
	}

	public bool isInHurtState
	{
		get
		{
			return mKnockbackTime != 0f || mPlayingHurtAnim || mStunnedTimer > 0f || startedDieAnim;
		}
	}

	public string currentAnimation
	{
		get
		{
			if (base.actionAnimState == null)
			{
				return base.baseAnim;
			}
			return base.currAnim;
		}
	}

	public bool isAnimationPlaying
	{
		get
		{
			return !IsDone();
		}
	}

	public float currentAnimationLength
	{
		get
		{
			return base.currAnimState.length;
		}
	}

	public float xPos { get; set; }

	public float stunnedTimer
	{
		get
		{
			return mStunnedTimer;
		}
		set
		{
			mStunnedTimer = value;
		}
	}

	public UdamanSoundThemePlayer SoundPlayer
	{
		get
		{
			return mSoundPlayer;
		}
	}

	public ChargeMoveAction onChargeMove { get; set; }

	public int updatesNoSnap
	{
		get
		{
			return mUpdatesNoSnap;
		}
		set
		{
			mUpdatesNoSnap = value;
		}
	}

	public FacingType walkDirection
	{
		get
		{
			return mWalkDirection;
		}
	}

	public bool isCharging { get; set; }

	public CharacterModelController target { get; set; }

	public bool isAttacking
	{
		get
		{
			return mAttacking;
		}
		private set
		{
			mAttacking = value;
		}
	}

	public void Awake()
	{
		startedDieAnim = false;
		xPos = 0f;
		mFirstUpdate = true;
		mConstraintLeft = 0f;
		mConstraintRight = 0f;
		mSpeedModifier = 1f;
		mImpactPauseTime = 0f;
		mWalkDirection = FacingType.Unchanged;
		mAttackDeliveryTriggered = false;
		mUpdatesNoSnap = 1;
		mUpdatesUntilNextSnap = 0;
		mUseRangedAttackIdle = false;
		mUseCowerIdle = false;
		mPlayingHurtAnim = false;
		mPlayingVictoryAnim = false;
		mStunnedTimer = 0f;
		mKnockbackTime = 0f;
		mProjectileInstance = null;
		mOnAttackDelivery = null;
		mPostDeathAction = EPostDeathAction.vanish;
		mAimAngle = null;
		mHeadAngle = null;
		mScaleTarget = 1f;
		mScaleRate = 0f;
		mOldScale = 1f;
		mScale = 1f;
		base.transform.localScale = Vector3.one;
		base.transform.localRotation = Quaternion.identity;
		mAttachedCharacter = null;
		mSpeed = 1f;
		mCachedTransform = base.transform;
		SetFacing(FacingType.Right);
		if (mAutoPaperdoll == null)
		{
			mAutoPaperdoll = GetComponent<AutoPaperdoll>();
		}
		if (mSoundPlayer == null)
		{
			mSoundPlayer = GetComponent<UdamanSoundThemePlayer>();
		}
		UpdateRandomAnimSets();
		isCharging = false;
		mMaterialSwitchIgnoreObjects.Clear();
		Init();
	}

	public void ResetPaperdoll()
	{
		mAutoPaperdoll = GetComponent<AutoPaperdoll>();
	}

	public void Start()
	{
		xPos = mCachedTransform.position.x;
		if (snapToGround && WeakGlobalInstance<RailManager>.Instance != null)
		{
			mCachedTransform.position = new Vector3(Mathf.Clamp(xPos, WeakGlobalInstance<RailManager>.Instance.GetMinX(mCachedTransform.position.z), WeakGlobalInstance<RailManager>.Instance.GetMaxX(mCachedTransform.position.z)), WeakGlobalInstance<RailManager>.Instance.GetY(mCachedTransform.position.z), mCachedTransform.position.z);
		}
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			renderer.enabled = false;
			if (materialOverride != null)
			{
				renderer.sharedMaterial = materialOverride;
			}
		}
		isPitThrown = false;
	}

	public void Update()
	{
		if (mFirstUpdate)
		{
			mFirstUpdate = false;
			PlaySoundEvent("Spawn");
			Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.enabled = true;
			}
		}
		if (mStunnedTimer > 0f)
		{
			mStunnedTimer = Mathf.Max(0f, mStunnedTimer - Time.deltaTime);
			if (mStunnedTimer <= 0f)
			{
				SetBaseAnimToIdle();
			}
		}
		UpdateScale();
		if (mAttachedCharacter != null)
		{
			mAttachedCharacter.transform.localPosition = Vector3.zero;
		}
		if (impactPauseTime > 0f)
		{
			base.paused = true;
			impactPauseTime -= Time.deltaTime;
			if (!(impactPauseTime <= 0f))
			{
				return;
			}
			impactPauseTime = 0f;
		}
		else if (base.paused)
		{
			base.paused = false;
		}
		if (startedDieAnim && !isAnimationPlaying && mPostDeathAction != 0 && mPostDeathAction != EPostDeathAction.stay)
		{
			StartPostDeathAction();
		}
		if (isCharging)
		{
			UpdateCharging();
			return;
		}
		UpdateWalking();
		UpdateAttackDelivery();
	}

	public override void PlayAnimation(string anim)
	{
		mPlayingVictoryAnim = false;
		base.PlayAnimation(anim);
	}

	public void ShowRoomWalk()
	{
		SetBaseAnim("walk");
		if (base.animPlayer.baseAnimState != null)
		{
			base.animPlayer.baseAnimTime = UnityEngine.Random.Range(0f, base.animPlayer.baseAnimState.length);
		}
		facing = mWalkDirection;
	}

	public void StartWalkLeft()
	{
		if (!isInHurtState)
		{
			mWalkDirection = FacingType.Left;
			StartWalkAnim();
		}
	}

	public void StartWalkRight()
	{
		if (!isInHurtState)
		{
			mWalkDirection = FacingType.Right;
			StartWalkAnim();
		}
	}

	private void StartWalkAnim()
	{
		mPlayingVictoryAnim = false;
		if (mWalkDirection != facing && HasAnim("backpedal"))
		{
			if (base.baseAnim == "backpedal" && HasAnim("runback") && base.currAnimTime >= MaxBackPedalTime)
			{
				if (base.baseAnim != "runback")
				{
					SetBaseAnim("runback");
					PlayAnimation("backpedalturn");
				}
			}
			else if (base.baseAnim != "runback")
			{
				SetBaseAnim("backpedal");
			}
		}
		else
		{
			SetBaseAnim("walk");
			facing = mWalkDirection;
		}
		if (currentAnimation != "backpedalturn")
		{
			RevertToBaseAnim();
		}
	}

	public void StopWalking()
	{
		mWalkDirection = FacingType.Unchanged;
		SetBaseAnimToIdle();
	}

	public void Idle()
	{
		StopWalking();
		if (!isInHurtState)
		{
			RevertToBaseAnim();
		}
	}

	public void PlayVictoryAnim()
	{
		if (mPlayingVictoryAnim || !HasAnim("victory"))
		{
			return;
		}
		bool flag = HasAnim("victoryloop");
		StopWalking();
		PlayAnimGroupRandom("victory", flag ? WrapMode.Once : WrapMode.Loop);
		if (flag)
		{
			base.currAnimDoneCallback = (TaggedAnimPlayer.TaggedAnimCallback)Delegate.Combine(base.currAnimDoneCallback, (TaggedAnimPlayer.TaggedAnimCallback)delegate
			{
				PlayAnimGroup("victoryloop", WrapMode.Loop);
			});
		}
		mPlayingVictoryAnim = true;
	}

	public void Attack(float inMaxTime)
	{
		StopWalking();
		PlayAnimation("attack", inMaxTime);
		attackActive = false;
		mAttackDeliveryTriggered = false;
		animEndedCallback = ClearAttackCallback;
		mOnAttackDelivery = onMeleeAttackDelivery;
		isAttacking = true;
		if (onMeleeAttack != null)
		{
			onMeleeAttack(true);
		}
	}

	public void RangeAttack(float inMaxTime)
	{
		SetArrowVisible(true);
		if (HasAnim("rangedattack"))
		{
			PlayAnimation("rangedattack", inMaxTime);
		}
		else if (HasAnim("cast"))
		{
			PlayAnimation("cast", inMaxTime);
		}
		StopWalking();
		SetBaseAnim("rangedattackidle");
		attackActive = false;
		mAttackDeliveryTriggered = false;
		animEndedCallback = ClearAttackCallback;
		mOnAttackDelivery = onRangedAttackDelivery;
	}

	public void SetUseRangedAttackIdle(bool useRangedIdle)
	{
		mUseRangedAttackIdle = useRangedIdle;
		if (isEffectivelyIdle && !isMoving)
		{
			SetBaseAnimToIdle();
		}
	}

	public void SetUseCowerIdle(bool useCowerIdle)
	{
		if (mUseCowerIdle != useCowerIdle)
		{
			mUseCowerIdle = useCowerIdle;
			if (isEffectivelyIdle && !isMoving)
			{
				SetBaseAnimToIdle();
			}
		}
	}

	public void PlaySoundEvent(string theSoundEvent)
	{
		if (WeakGlobalMonoBehavior<InGameImpl>.Exists && (bool)mSoundPlayer)
		{
			mSoundPlayer.PlaySoundEvent(theSoundEvent);
		}
	}

	public GameObject SpawnEffectAtJoint(GameObject effectPrefab, string theJointLabel, bool andAttach)
	{
		if (!WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			return null;
		}
		GameObject gameObject = mAutoPaperdoll.InstantiateObjectOnJoint(effectPrefab, theJointLabel, andAttach);
		EffectKiller.AddKiller(gameObject, GameObjectPool.DefaultObjectPool);
		AddMaterialSwitchIgnoreObj(gameObject);
		return gameObject;
	}

	public GameObject SpawnEffectAtJoint(GameObject effectPrefab, string theJointLabel, bool andAttach, float maxTime)
	{
		if (!WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			return null;
		}
		GameObject gameObject = mAutoPaperdoll.InstantiateObjectOnJoint(effectPrefab, theJointLabel, andAttach);
		EffectKiller effectKiller = EffectKiller.AddKiller(gameObject, GameObjectPool.DefaultObjectPool);
		effectKiller.maxLifetime = maxTime;
		AddMaterialSwitchIgnoreObj(gameObject);
		return gameObject;
	}

	public GameObject SpawnEffectAt(GameObject effectPrefab, Vector3 location)
	{
		if (!WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			return null;
		}
		GameObject gameObject = GameObjectPool.DefaultObjectPool.Acquire(effectPrefab);
		gameObject.transform.position = location;
		EffectKiller.AddKiller(gameObject, GameObjectPool.DefaultObjectPool);
		AddMaterialSwitchIgnoreObj(gameObject);
		return gameObject;
	}

	public GameObject InstantiateObjectOnJoint(GameObject prefab, string theJointLabel)
	{
		return mAutoPaperdoll.InstantiateObjectOnJoint(prefab, theJointLabel, true);
	}

	public void PerformSpecialAction(string theAnimName, Action onAttackDelivery)
	{
		StopWalking();
		PlayAnimation(theAnimName);
		attackActive = false;
		mAttackDeliveryTriggered = false;
		animEndedCallback = ClearAttackCallback;
		mOnAttackDelivery = onAttackDelivery;
	}

	public void PerformSpecialAction(string theAnimName, Action onAttackDelivery, float inMaxTime)
	{
		StopWalking();
		PlayAnimation(theAnimName, inMaxTime);
		attackActive = false;
		mAttackDeliveryTriggered = false;
		animEndedCallback = ClearAttackCallback;
		mOnAttackDelivery = onAttackDelivery;
	}

	public void PerformSpecialActionNoAttack(string theAnimName, Action onEndAnim)
	{
		StopWalking();
		PlayAnimation(theAnimName);
		attackActive = false;
		mAttackDeliveryTriggered = false;
		animEndedCallback = onEndAnim;
		mOnAttackDelivery = null;
	}

	public void PlayHurtAnim(string theAnimName)
	{
		if (HasAnim(theAnimName))
		{
			StopWalking();
			float num = speedModifier;
			mSpeedModifier = 1f;
			PlayAnimation(theAnimName);
			mSpeedModifier = num;
			mPlayingHurtAnim = true;
			animEndedCallback = ClearPlayingHurtAnim;
		}
	}

	public void LoopStunAnim(string theAnimName)
	{
		if (HasAnim(theAnimName))
		{
			StopWalking();
			SetBaseAnim(theAnimName);
			RevertToBaseAnim(0f);
		}
	}

	public void Knockback(Vector3 addedTranslation)
	{
		StopWalking();
		if (HasAnim("knockback"))
		{
			float num = speedModifier;
			mSpeedModifier = 1f;
			PlayAnimation("knockback");
			mSpeedModifier = num;
		}
		else
		{
			RevertToBaseAnim();
		}
		Vector3 vector = addedTranslation;
		float num2 = addedTranslation.y + 0.62f;
		vector.y = 0f;
		KnockbackTargetPoint = base.transform.position + vector;
		float magnitude = vector.magnitude;
		float num3 = (mTotalKnockbackTime = magnitude / 2f);
		mKnockbackTime = mTotalKnockbackTime;
		mKnockbackVelocity = vector / magnitude * 2f;
		float num4 = 2f * num2 / num3;
		mKnockbackAcceleration = 4f * (num2 - num4 * num3) / (num3 * num3);
		mKnockbackVelocity.y = num4;
	}

	public void Jump()
	{
		StopWalking();
		mJumpTime = 0.75f;
	}

	public void DeliverAttack()
	{
		mAttackDeliveryTriggered = true;
		if (mOnAttackDelivery != null)
		{
			mOnAttackDelivery();
		}
	}

	public void Die()
	{
		Die("die", EPostDeathAction.vanish);
	}

	public void Die(string animName, EPostDeathAction postDeathAction)
	{
		if (startedDieAnim || mPlayingHurtAnim)
		{
			return;
		}
		startedDieAnim = true;
		StopWalking();
		ClearBaseAnim();
		speedModifier = 1f;
		if (HasAnim(animName))
		{
			PlayAnimation(animName);
			switch (animName)
			{
			case "dieexplode":
				postDeathAction = EPostDeathAction.vanish;
				break;
			}
		}
		else
		{
			PlayAnimation("die");
		}
		mPostDeathAction = postDeathAction;
	}

	public void StartPostDeathAction()
	{
		switch (mPostDeathAction)
		{
		case EPostDeathAction.melt:
			mPostDeathAction = EPostDeathAction.stay;
			ModelMelter.MeltGameObject(base.gameObject, OnDieMeltDone);
			break;
		case EPostDeathAction.ascend:
			mPostDeathAction = EPostDeathAction.stay;
			ModelMelter.AscendGameObject(base.gameObject, OnDieMeltDone);
			break;
		}
	}

	private void OnDieMeltDone()
	{
		mPostDeathAction = EPostDeathAction.vanish;
	}

	public void ShowArrow()
	{
		SetArrowVisible(true);
	}

	public void HideArrow()
	{
		SetArrowVisible(false);
	}

	public void SetArrowVisible(bool v, string type)
	{
		if (WeakGlobalInstance<ProjectileManager>.Instance.ProjectileShownWhileAiming(type))
		{
			ProjectileType = type;
			SetArrowVisible(v);
		}
	}

	public void SetArrowVisible(bool v)
	{
		if (!string.IsNullOrEmpty(ProjectileType))
		{
			if (!v && mProjectileInstance != null)
			{
				UnityEngine.Object.Destroy(mProjectileInstance);
				mProjectileInstance = null;
			}
			else if (v && mProjectileInstance == null && mAutoPaperdoll.HasJoint("arrow"))
			{
				ProjectileSchema projectileSchema = WeakGlobalInstance<ProjectileManager>.Instance[ProjectileType];
				mProjectileInstance = mAutoPaperdoll.InstantiateObjectOnJoint(projectileSchema.prefab, projectileSchema.material, "arrow");
			}
		}
	}

	public void AimTowards(string projectileType, Vector3 targetPos)
	{
		if (isPitThrown)
		{
			return;
		}
		AutoPaperdoll.LabeledJoint jointData = autoPaperdoll.GetJointData("aim_angle");
		if (jointData == null || jointData.joint == null)
		{
			return;
		}
		Transform joint = jointData.joint;
		Vector3 vector = WeakGlobalInstance<ProjectileManager>.Instance.ProjectileAimPosForTarget(projectileType, autoPaperdoll.GetJointPosition("projectile_spawn"), targetPos);
		Vector3 vector2 = vector - joint.position;
		Vector3 from = vector2;
		from.y = 0f;
		float num = Vector3.Angle(from, vector2);
		if (vector2.y < 0f)
		{
			num = 0f - num;
		}
		if (!mAimAngle.HasValue)
		{
			mAimAngle = 0f;
		}
		mAimAngle = Mathf.MoveTowardsAngle(mAimAngle.Value, num, 150f * Time.deltaTime);
		jointData = autoPaperdoll.GetJointData("aim_angle_2");
		if (jointData != null && !(jointData.joint == null))
		{
			Transform joint2 = jointData.joint;
			vector2 = targetPos - joint2.position;
			from = vector2;
			from.y = 0f;
			float num2 = Vector3.Angle(from, vector2);
			if (vector2.y < 0f)
			{
				num2 = 0f - num2;
			}
			if (joint2.IsChildOf(joint))
			{
				num2 -= num;
			}
			if (!mHeadAngle.HasValue)
			{
				mHeadAngle = 0f;
			}
			mHeadAngle = Mathf.MoveTowardsAngle(mHeadAngle.Value, num2, 150f * Time.deltaTime);
		}
	}

	public void ResetAimAngle()
	{
		if (isPitThrown)
		{
			return;
		}
		if (mAimAngle.HasValue)
		{
			mAimAngle = Mathf.MoveTowardsAngle(mAimAngle.Value, 0f, 150f * Time.deltaTime);
			if (mAimAngle == 0f)
			{
				mAimAngle = null;
			}
		}
		if (mHeadAngle.HasValue)
		{
			mHeadAngle = Mathf.MoveTowardsAngle(mHeadAngle.Value, 0f, 150f * Time.deltaTime);
			if (mHeadAngle == 0f)
			{
				mHeadAngle = null;
			}
		}
	}

	public void FaceTowards(Vector3 targetPos)
	{
		if (isPitThrown)
		{
			return;
		}
		Vector2 to = new Vector2(targetPos.z - mCachedTransform.position.z, targetPos.x - mCachedTransform.position.x);
		float num = Mathf.Abs(Vector2.Angle(Vector2.right, to));
		Vector3 eulerAngles = mCachedTransform.eulerAngles;
		if (num != eulerAngles.y)
		{
			if (targetPos.x < mCachedTransform.position.x)
			{
				num = 0f - num;
			}
			eulerAngles.y = Mathf.MoveTowardsAngle(eulerAngles.y, num, 180f * Time.deltaTime);
			mCachedTransform.eulerAngles = eulerAngles;
		}
	}

	public void ResetFacingAngle()
	{
		if (isPitThrown)
		{
			return;
		}
		float num = baseFacingAngle;
		Vector3 eulerAngles = mCachedTransform.eulerAngles;
		if (eulerAngles.y != num)
		{
			eulerAngles.y = Mathf.MoveTowardsAngle(eulerAngles.y, num, 180f * Time.deltaTime);
			if (isInHurtState)
			{
				eulerAngles.y = num;
			}
			mCachedTransform.eulerAngles = eulerAngles;
		}
	}

	public void Destroy()
	{
	}

	private void UpdateWalking()
	{
		if (impactPauseTime > 0f)
		{
			return;
		}
		if (mKnockbackTime != 0f)
		{
			UpdateKnockback();
			return;
		}
		float num = mSpeed * mSpeedModifier;
		if (base.baseAnim == "runback")
		{
			num *= Singleton<Config>.Instance.data.GetFloat(TextDBSchema.ChildKey("Game", "runbackspeedmult"));
		}
		if (mWalkDirection != 0)
		{
			num *= (float)mWalkDirection;
			MoveBy(num * Time.deltaTime);
		}
		base.baseAnimSpeed = Mathf.Abs(num / animatedWalkSpeed);
	}

	private void UpdateAttackDelivery()
	{
		if (mOnAttackDelivery == null)
		{
			return;
		}
		if (mAttackDeliveryTriggered || impactPauseTime > 0f)
		{
			if (!attackActive)
			{
				mAttackDeliveryTriggered = false;
			}
		}
		else if (attackActive)
		{
			DeliverAttack();
		}
	}

	private void UpdateCharging()
	{
		if (mJumpTime != 0f)
		{
			UpdateJump();
			return;
		}
		float z = base.transform.position.z;
		float num = 5f * Time.deltaTime * (float)facing;
		if (onChargeMove != null)
		{
			onChargeMove(z, z + num);
		}
		MoveBy(num);
	}

	private void MoveBy(float zDelta)
	{
		float z = mCachedTransform.position.z;
		float num = Mathf.Clamp(mCachedTransform.position.z + zDelta, mConstraintLeft, mConstraintRight);
		if ((impactPauseTime > 0f || zDelta == 0f) && z == num)
		{
			return;
		}
		mUpdatesUntilNextSnap--;
		if (snapToGround && mUpdatesUntilNextSnap <= 0 && WeakGlobalInstance<RailManager>.Instance != null)
		{
			mCachedTransform.position = new Vector3(Mathf.Clamp(xPos, WeakGlobalInstance<RailManager>.Instance.GetMinX(num), WeakGlobalInstance<RailManager>.Instance.GetMaxX(num)), WeakGlobalInstance<RailManager>.Instance.GetY(num), num);
			mUpdatesUntilNextSnap = mUpdatesNoSnap;
		}
		else
		{
			mCachedTransform.position = new Vector3(mCachedTransform.position.x, mCachedTransform.position.y, num);
		}
		if (facing == FacingType.Unchanged)
		{
			if (zDelta > 0f)
			{
				facing = FacingType.Right;
			}
			else if (zDelta < 0f)
			{
				facing = FacingType.Left;
			}
		}
	}

	private void UpdateKnockback()
	{
		if (mKnockbackTime < 0f)
		{
			mKnockbackTime += Time.deltaTime;
			if (mKnockbackTime > 0f)
			{
				mKnockbackTime = 0f;
			}
			return;
		}
		mKnockbackTime -= Time.deltaTime;
		float z = Mathf.Clamp(mCachedTransform.position.z + mKnockbackVelocity.z * Time.deltaTime, mConstraintLeft, mConstraintRight);
		float a = ((!snapToGround || WeakGlobalInstance<RailManager>.Instance == null) ? mCachedTransform.position.y : WeakGlobalInstance<RailManager>.Instance.GetY(z));
		float num = mCachedTransform.position.y;
		if (mKnockbackTime <= 0f)
		{
			if (HasAnim("knockbackland") && !startedDieAnim && !mPlayingHurtAnim)
			{
				PlayAnimation("knockbackland");
				mKnockbackTime = 0f;
			}
			else
			{
				mKnockbackTime = -0.25f;
			}
		}
		else
		{
			num += mKnockbackVelocity.y * Time.deltaTime;
			mKnockbackVelocity.y += mKnockbackAcceleration * Time.deltaTime;
			num = Mathf.Max(a, num);
		}
		float x = mCachedTransform.position.x + mKnockbackVelocity.x * Time.deltaTime;
		mCachedTransform.position = new Vector3(x, num, z);
	}

	private void UpdateJump()
	{
		if (mJumpTime < 0f)
		{
			mJumpTime += Time.deltaTime;
			if (mJumpTime > 0f)
			{
				mJumpTime = 0f;
			}
			return;
		}
		mJumpTime -= Time.deltaTime;
		float z = Mathf.Clamp(mCachedTransform.position.z + 3f * Time.deltaTime * (float)facing, mConstraintLeft, mConstraintRight);
		float num = ((!snapToGround || WeakGlobalInstance<RailManager>.Instance == null) ? mCachedTransform.position.y : WeakGlobalInstance<RailManager>.Instance.GetY(z));
		if (mJumpTime <= 0f)
		{
			if (HasAnim("jumpland") && !startedDieAnim && !mPlayingHurtAnim)
			{
				PlayAnimation("jumpland");
				mJumpTime = 0f;
			}
			else
			{
				mJumpTime = -0f;
			}
		}
		else
		{
			float num2 = mJumpTime / 0.75f - 0.5f;
			num += 1.25f - 5f * (num2 * num2);
		}
		float x = Mathf.Clamp(xPos, WeakGlobalInstance<RailManager>.Instance.GetMinX(z), WeakGlobalInstance<RailManager>.Instance.GetMaxX(z));
		mCachedTransform.position = new Vector3(x, num, z);
	}

	private void SetFacing(FacingType newFacing)
	{
		if (mFacing != newFacing)
		{
			Vector3 eulerAngles = mCachedTransform.eulerAngles;
			eulerAngles.y = ((newFacing == FacingType.Left) ? 180 : 0);
			mCachedTransform.eulerAngles = eulerAngles;
			mFacing = newFacing;
		}
	}

	private void SetBaseAnimToIdle()
	{
		if (mUseCowerIdle && HasAnim("cower"))
		{
			SetBaseAnim("cower");
		}
		else if (mUseRangedAttackIdle && HasAnim("rangedattackidle"))
		{
			SetBaseAnim("rangedattackidle");
		}
		else
		{
			SetBaseAnim("idle");
		}
	}

	private void LateUpdate()
	{
		if (!mAimAngle.HasValue || base.paused)
		{
			return;
		}
		AutoPaperdoll.LabeledJoint jointData = autoPaperdoll.GetJointData("aim_angle");
		if (jointData != null && !(jointData.joint == null))
		{
			Vector3 axis = mCachedTransform.rotation * Vector3.right;
			Quaternion quaternion = Quaternion.AngleAxis(0f - mAimAngle.Value, axis);
			Transform joint = jointData.joint;
			joint.rotation = quaternion * joint.rotation;
			jointData = autoPaperdoll.GetJointData("aim_angle_2");
			if (jointData != null && !(jointData.joint == null) && mHeadAngle.HasValue)
			{
				quaternion = Quaternion.AngleAxis(0f - mHeadAngle.Value, axis);
				joint = jointData.joint;
				joint.rotation = quaternion * joint.rotation;
			}
		}
	}

	private void ClearAttackCallback()
	{
		mOnAttackDelivery = null;
		isAttacking = false;
		if (onMeleeAttack != null)
		{
			onMeleeAttack(false);
		}
	}

	private void ClearPlayingHurtAnim()
	{
		mPlayingHurtAnim = false;
	}

	public void ChargeWithAnim(string animName)
	{
		Jump();
		SetBaseAnim(animName);
		isCharging = true;
	}

	public void AddMaterialSwitchIgnoreObj(GameObject objToIgnore)
	{
		if (objToIgnore == null || mMaterialSwitchIgnoreObjects.Contains(objToIgnore))
		{
			return;
		}
		for (int num = mMaterialSwitchIgnoreObjects.Count - 1; num >= 0; num--)
		{
			if (mMaterialSwitchIgnoreObjects[num] == null || !mMaterialSwitchIgnoreObjects[num].activeSelf)
			{
				mMaterialSwitchIgnoreObjects.RemoveAt(num);
			}
		}
		mMaterialSwitchIgnoreObjects.Add(objToIgnore);
	}

	public List<GameObject> GetMaterialSwitchIgnoreObjects()
	{
		return mMaterialSwitchIgnoreObjects;
	}

	public void AttachCharacterToJoint(CharacterModelController toAttach, AutoPaperdoll.LabeledJoint jointData)
	{
		mAttachedCharacter = toAttach;
		Transform transform = mAttachedCharacter.gameObject.transform;
		mAttachedToXForm = jointData.joint.transform;
		transform.parent = jointData.joint.transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
	}

	public void DetachTarget()
	{
		if (mAttachedCharacter != null)
		{
			Transform transform = mAttachedCharacter.gameObject.transform;
			if (transform.parent == mAttachedToXForm)
			{
				transform.parent = null;
			}
			mAttachedCharacter = null;
			mAttachedToXForm = null;
		}
	}

	public void SetScaleOverTime(float targetScale, float scaleTime)
	{
		mScaleTarget = targetScale;
		if (scaleTime <= 0f)
		{
			mScale = targetScale;
		}
		else
		{
			mScaleRate = (mScaleTarget - mScale) / scaleTime;
		}
	}

	private void UpdateScale()
	{
		float f = mScaleTarget - mScale;
		if (Mathf.Abs(f) <= Mathf.Abs(mScaleRate * Time.deltaTime))
		{
			mScale = mScaleTarget;
		}
		mScale += mScaleRate * Time.deltaTime;
		if (mScale != mOldScale)
		{
			mOldScale = mScale;
			base.transform.localScale = Vector3.one * mScale;
		}
	}

	public Vector3 GetRelativeJointPosition(string theJointLabel)
	{
		if (mAutoPaperdoll != null)
		{
			return mAutoPaperdoll.GetRelativeJointPosition(theJointLabel);
		}
		return Vector3.zero;
	}
}
