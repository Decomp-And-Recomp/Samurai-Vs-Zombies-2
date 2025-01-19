using UnityEngine;

public class Collectable
{
	private enum CollectableState
	{
		MoveToGround = 0,
		WaitForPickup = 1,
		Collected = 2,
		TimeOutFade = 3,
		Destroy = 4
	}

	private const float kSpawnTimeFactor = 2f;

	private const float kSpawnArcHeight = 0.96f;

	private const float kMagnetHeight = 0.64f;

	private const float kMagnetVerticalVel = 1.3f;

	private const float kFadeOutTime = 4f;

	private const string kShaderColorName = "_TintColor";

	private GameObject mObject;

	private float mFriction;

	private ECollectableType mType = ECollectableType.Count;

	private Animation mAnimPlayer;

	private Transform mTransform;

	private Material mMaterial;

	private int mValue;

	private bool mShouldStartFadeOut;

	private bool mWasCollected;

	private float mTimeLeftAlive;

	private CollectableState mState;

	private Vector3 mStartPos;

	private Vector3 mTargetPos;

	private Vector3 mVel;

	private float mTime;

	private float mMagnetHeightOffset;

	private Color mOriginalColor;

	public bool wasAtLeftOfHero { get; private set; }

	public bool isReadyToDie { get; private set; }

	public bool isDestroyed { get; private set; }

	public bool isReadyToBeCollected
	{
		get
		{
			return mState == CollectableState.WaitForPickup || mState == CollectableState.TimeOutFade;
		}
	}

	public ECollectableType type
	{
		get
		{
			return mType;
		}
	}

	public Vector3 position
	{
		get
		{
			return mTransform.position;
		}
		set
		{
			mTransform.position = value;
		}
	}

	public Collectable(ECollectableType type, ResourceTemplate template, Vector3 startPos, Vector3 targetPos)
	{
		mStartPos = startPos;
		mTargetPos = targetPos;
		mObject = GameObjectPool.DefaultObjectPool.Acquire(template.prefab);
		mAnimPlayer = mObject.GetComponent<Animation>();
		mTransform = mObject.transform;
		mMaterial = mObject.GetComponentInChildren<Renderer>().material;
		mState = CollectableState.MoveToGround;
		mTime = 0f;
		mType = type;
		mValue = template.amount;
		mTimeLeftAlive = template.lifetime;
		mShouldStartFadeOut = false;
		wasAtLeftOfHero = targetPos.z < WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.position.z;
		isReadyToDie = false;
		mOriginalColor = Color.grey;
		WeakGlobalInstance<CollectableManager>.Instance.RecordLootDropped(mType, mValue);
	}

	public void Update()
	{
		switch (mState)
		{
		case CollectableState.MoveToGround:
			UpdateSpawnPosition();
			if (position == mTargetPos)
			{
				mTime = 0f;
				mStartPos = mTargetPos;
				mState = CollectableState.WaitForPickup;
				mAnimPlayer.Play("IdlePickup");
				mAnimPlayer.wrapMode = WrapMode.Loop;
			}
			break;
		case CollectableState.WaitForPickup:
			UpdateTimer();
			UpdateMagnetEffect();
			if (mShouldStartFadeOut)
			{
				mTime = 0f;
				mState = CollectableState.TimeOutFade;
			}
			break;
		case CollectableState.Collected:
			mTimeLeftAlive -= Time.deltaTime;
			if (mTimeLeftAlive <= 0f)
			{
				mState = CollectableState.Destroy;
				Destroy();
			}
			break;
		case CollectableState.TimeOutFade:
			UpdateFadeOut();
			UpdateMagnetEffect();
			break;
		case CollectableState.Destroy:
			isReadyToDie = true;
			break;
		}
	}

	public void OnCollected()
	{
		if (!mWasCollected)
		{
			mWasCollected = true;
			if (mMaterial != null && mMaterial.HasProperty("_TintColor"))
			{
				mMaterial.SetColor("_TintColor", new Color(mOriginalColor.r, mOriginalColor.g, mOriginalColor.b, 1f));
			}
			if (mAnimPlayer == null || mAnimPlayer["Pickup"] == null)
			{
				mState = CollectableState.Destroy;
				return;
			}
			mState = CollectableState.Collected;
			AnimationState animationState = mAnimPlayer["Pickup"];
			mAnimPlayer.Play("Pickup");
			mTimeLeftAlive = animationState.length;
		}
	}

	private void UpdateFadeOut()
	{
		if (!WeakGlobalMonoBehavior<InGameImpl>.Instance.gameOver)
		{
			if (mMaterial != null && mMaterial.HasProperty("_TintColor"))
			{
				mMaterial.SetColor("_TintColor", new Color(mOriginalColor.r, mOriginalColor.g, mOriginalColor.b, Mathf.Clamp(mTimeLeftAlive / 4f, 0f, 1f)));
			}
			mTimeLeftAlive -= Time.deltaTime;
			if (mTimeLeftAlive <= 0f)
			{
				mState = CollectableState.Destroy;
			}
		}
	}

	private void UpdateSpawnPosition()
	{
		mTime += Time.deltaTime * 2f;
		Vector3 vector = Vector3.Lerp(mStartPos, mTargetPos, mTime);
		if (mTime < 1f)
		{
			float num = mTime - 0.5f;
			vector.y += 0.96f - 3.84f * (num * num);
		}
		position = vector;
	}

	public void Destroy()
	{
		if (!isDestroyed)
		{
			if (mWasCollected)
			{
				WeakGlobalInstance<CollectableManager>.Instance.GiveResource(mType, mValue);
			}
			GameObjectPool.DefaultObjectPool.Release(mObject);
			mObject = null;
			isDestroyed = true;
		}
	}

	private void UpdateTimer()
	{
		if (mType == ECollectableType.Count)
		{
			return;
		}
		if (mShouldStartFadeOut || (WeakGlobalMonoBehavior<InGameImpl>.Instance.hero != null && WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.health > 0f && WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.controller.currentAnimation != "revive"))
		{
			mTimeLeftAlive -= Time.deltaTime;
			if (mTimeLeftAlive <= 4f)
			{
				mShouldStartFadeOut = true;
			}
		}
		else
		{
			mTimeLeftAlive += Time.deltaTime;
		}
	}

	private void UpdateMagnetEffect()
	{
		float magnetMaxDist = WeakGlobalInstance<CollectableManager>.Instance.magnetMaxDist;
		if (magnetMaxDist <= 0f)
		{
			if (mMagnetHeightOffset > 0f)
			{
				float num = Mathf.Min(1.3f, mMagnetHeightOffset);
				mMagnetHeightOffset -= num;
				Vector3 vector = position;
				vector.z -= num;
				position = vector;
			}
			return;
		}
		float z = WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.position.z;
		Vector3 vector2 = position;
		float num2 = Mathf.Abs(z - vector2.z);
		if (!(num2 > magnetMaxDist))
		{
			float magnetMinSpeed = WeakGlobalInstance<CollectableManager>.Instance.magnetMinSpeed;
			float magnetMaxSpeed = WeakGlobalInstance<CollectableManager>.Instance.magnetMaxSpeed;
			float num3 = magnetMinSpeed + (magnetMaxDist - num2) * (magnetMaxSpeed - magnetMinSpeed) / magnetMaxDist;
			if (wasAtLeftOfHero)
			{
				vector2.z += num3 * Time.deltaTime;
			}
			else
			{
				vector2.z -= num3 * Time.deltaTime;
			}
			if (mMagnetHeightOffset < 0.64f)
			{
				vector2.y -= mMagnetHeightOffset;
				mMagnetHeightOffset = Mathf.Min(mMagnetHeightOffset + 1.3f, 0.64f);
				vector2.y += mMagnetHeightOffset;
			}
			position = vector2;
		}
	}
}
