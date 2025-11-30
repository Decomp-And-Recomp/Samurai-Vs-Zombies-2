using UnityEngine;

public class BouncyScrollInput
{
	public delegate void OnVector2Callback(Vector2 pt);

	private const float kDefaultScrollTriggerTreshold = 12f;

	private const int kVelocityHistoryNum = 3;

	private const float kScrollSlowDownFactor = 0.1f;

	private Vector2 mScrollPosition;

	private Rect mArea;

	private float mExtraTouchBorder;

	private Vector2? mTouchStart;

	private bool mDragMode;

	private bool mUseOvershoot = true;

	private OnVector2Callback mOnSimpleTouch;

	private Vector2 mMaxScroll;

	private int mSnapToGrid;

	private bool mTouchesEnabled = true;

	private float mScrollTriggerTreshold_original;

	private float mScrollTriggerTreshold;

	private float mBounceFactor;

	private Vector2?[] mPreviousVelocities;

	private Vector2 mSwipeVelocity;

	private Vector2? mAutoScrollTarget;

	private float mDeltaTimeThisUpdate;

	private float mDefaultFrameRate;

	private Vector2? mTouchPosition;

	private bool mJustTouched;

	private Camera mUICamera;

	public float ScrollTriggerTreshold
	{
		get
		{
			return mScrollTriggerTreshold_original;
		}
		set
		{
			mScrollTriggerTreshold_original = value;
			mScrollTriggerTreshold = value;
			if (Screen.dpi > 0f)
			{
				mScrollTriggerTreshold *= Screen.dpi / 126f;
			}
		}
	}

	public bool LockedX { get; set; }

	public bool LockedY { get; set; }

	public Rect Area
	{
		get
		{
			return mArea;
		}
		set
		{
			mArea = value;
		}
	}

	public float ExtraTouchBorder
	{
		get
		{
			return mExtraTouchBorder;
		}
		set
		{
			mExtraTouchBorder = value;
		}
	}

	public Vector2 ScrollMax
	{
		get
		{
			return mMaxScroll;
		}
		set
		{
			mMaxScroll = new Vector2(Mathf.Max(0f, value.x), Mathf.Max(0f, value.y));
		}
	}

	public Vector2 ScrollPosition
	{
		get
		{
			return mScrollPosition;
		}
		set
		{
			mScrollPosition = value;
		}
	}

	public OnVector2Callback OnSimpleTouch
	{
		get
		{
			return mOnSimpleTouch;
		}
		set
		{
			mOnSimpleTouch = value;
		}
	}

	public bool UseOverShoot
	{
		get
		{
			return mUseOvershoot;
		}
		set
		{
			mUseOvershoot = value;
		}
	}

	public Vector2 VisualScrollPosition
	{
		get
		{
			Vector2 result = mScrollPosition;
			if (result.x < 0f)
			{
				result.x /= 3f;
			}
			if (result.y < 0f)
			{
				result.y /= 3f;
			}
			if (result.x > mMaxScroll.x)
			{
				result.x = (result.x - mMaxScroll.x) / 3f + mMaxScroll.x;
			}
			if (result.y > mMaxScroll.y)
			{
				result.y = (result.y - mMaxScroll.y) / 3f + mMaxScroll.y;
			}
			return result;
		}
	}

	public int SnapToGridDelta
	{
		get
		{
			return mSnapToGrid;
		}
		set
		{
			mSnapToGrid = value;
		}
	}

	public bool TouchesEnabled
	{
		get
		{
			return mTouchesEnabled;
		}
		set
		{
			mTouchesEnabled = value;
		}
	}

	public bool IsScrolling
	{
		get
		{
			return TouchesEnabled && mTouchStart.HasValue;
		}
	}

	public bool IsDragging
	{
		get
		{
			return IsScrolling && mDragMode;
		}
	}

	private float force;
	private bool negativeForce;

	public BouncyScrollInput()
	{
		ScrollTriggerTreshold = 12f;
		mDefaultFrameRate = Application.targetFrameRate;
		if (mDefaultFrameRate <= 0f)
		{
			mDefaultFrameRate = 60f;
		}
		mTouchStart = null;
		mAutoScrollTarget = null;
		mPreviousVelocities = new Vector2?[3];
		ClearVelocityHistory();
		mUICamera = ObjectUtils.FindFirstCamera(LayerMask.NameToLayer("GLUI"));
	}

	public void ScrollUpdate(bool vertical)
	{
#if UNITY_STANDALONE || UNITY_EDITOR
		if (vertical)
		{
			if (negativeForce) mScrollPosition.y += force;
			else mScrollPosition.y -= force;

			if (mScrollPosition.y < 0f || mScrollPosition.y > mMaxScroll.y) force = 0;
		}
		else
		{
			if (negativeForce) mScrollPosition.x += force;
			else mScrollPosition.x -= force;

			if (mScrollPosition.x < 0f || mScrollPosition.x > mMaxScroll.x) force = 0;
		}

		if (force > 0)
		{
			force -= Time.unscaledDeltaTime * (Screen.height / 13);
		}
		else
		{
			force = 0;
			negativeForce = false;
		}
#endif
	}

	public void Update()
	{
		if (!IsAnySystemTouches())
		{
			mTouchPosition = null;
			mJustTouched = false;
		}
		mDeltaTimeThisUpdate = GluiTime.deltaTime;
		if (mDeltaTimeThisUpdate == 0f)
		{
			mDeltaTimeThisUpdate = 1f / mDefaultFrameRate;
		}
		mBounceFactor = Mathf.Clamp(1f - mDeltaTimeThisUpdate * 6f, 0f, 0.99f);
		if (mTouchesEnabled && mTouchPosition.HasValue)
		{
			Vector2 value = mTouchPosition.Value;
			if (!mTouchStart.HasValue)
			{
				BeginTouch(value);
			}
			else
			{
				ContinueTouch(value);
			}
		}
		else
		{
			if (mTouchStart.HasValue)
			{
				EndTouch();
			}
			if (mAutoScrollTarget.HasValue)
			{
				UpdateAutoScroll();
			}
			else
			{
				UpdateAutoBouncing();
				if (mSnapToGrid != 0)
				{
					UpdateSnapTo();
				}
			}
		}
		mJustTouched = false;
	}

	public void ScrollTo(Vector2 target)
	{
		mAutoScrollTarget = target;
	}

	private void ClearVelocityHistory()
	{
		for (int i = 0; i < 3; i++)
		{
			mPreviousVelocities[i] = null;
		}
	}

	private void BeginTouch(Vector2 touchPos)
	{
		if (mJustTouched)
		{
			mTouchStart = touchPos;
			mDragMode = false;
			ClearVelocityHistory();
		}
	}

	private void ContinueTouch(Vector2 pos)
	{
		mAutoScrollTarget = null;
		if (!mDragMode)
		{
			Vector2 vector = pos - mTouchStart.Value;
			if ((!LockedY && Mathf.Abs(vector.y) >= mScrollTriggerTreshold) || (!LockedX && Mathf.Abs(vector.x) >= mScrollTriggerTreshold))
			{
				mDragMode = true;
			}
		}
		if (!mDragMode)
		{
			return;
		}
		Vector2 vector2 = pos - mTouchStart.Value;
		mScrollPosition -= vector2;
		if (!mUseOvershoot)
		{
			mScrollPosition = SnapToMax(mScrollPosition);
		}
		mTouchStart = pos;
		if (mDeltaTimeThisUpdate > 0f)
		{
			for (int i = 0; i < 2; i++)
			{
				mPreviousVelocities[i + 1] = mPreviousVelocities[i];
			}
			mPreviousVelocities[0] = new Vector2((0f - vector2.x) / mDeltaTimeThisUpdate, (0f - vector2.y) / mDeltaTimeThisUpdate);
		}
	}

	public void Force(float velocity)
	{
		if (velocity > 0 && negativeForce)
		{
			negativeForce = false;
			force = 0;
		}
		else if (velocity < 0 && !negativeForce)
		{
			negativeForce = true;
			force = 0;
		}

		force += Mathf.Abs(velocity);
	}

	private void EndTouch()
	{
		mAutoScrollTarget = null;
		Vector2 value = mTouchStart.Value;
		mTouchStart = null;
		if (mDragMode)
		{
			mSwipeVelocity = Vector2.zero;
			int num = 0;
			for (int i = 0; i < 3; i++)
			{
				if (mPreviousVelocities[i].HasValue)
				{
					num++;
					mSwipeVelocity += mPreviousVelocities[i].Value;
				}
			}
			if (num > 0)
			{
				mSwipeVelocity.x /= num;
				mSwipeVelocity.y /= num;
			}
		}
		else if (mOnSimpleTouch != null)
		{
			mOnSimpleTouch(value);
		}
	}

	private void UpdateAutoScroll()
	{
		mSwipeVelocity = Vector2.zero;
		Vector2 value = mAutoScrollTarget.Value;
		mScrollPosition.x = (value.x - mScrollPosition.x) * mDeltaTimeThisUpdate + mScrollPosition.x;
		mScrollPosition.y = (value.y - mScrollPosition.y) * mDeltaTimeThisUpdate + mScrollPosition.y;
		if (Mathf.Abs(mScrollPosition.x - value.x) < 1f && Mathf.Abs(mScrollPosition.y - value.y) < 1f)
		{
			mScrollPosition = value;
			mAutoScrollTarget = null;
		}
	}

	private void UpdateAutoBouncing()
	{
		if (mScrollPosition.x < 0f)
		{
			mScrollPosition.x *= mBounceFactor;
			if (mScrollPosition.x > -1f)
			{
				mScrollPosition.x = 0f;
			}
		}
		if (mScrollPosition.y < 0f)
		{
			mScrollPosition.y *= mBounceFactor;
			if (mScrollPosition.y > -1f)
			{
				mScrollPosition.y = 0f;
			}
		}
		if (mScrollPosition.x > mMaxScroll.x)
		{
			mScrollPosition.x = (mScrollPosition.x - mMaxScroll.x) * mBounceFactor + mMaxScroll.x;
			if (mScrollPosition.x < mMaxScroll.x - 1f)
			{
				mScrollPosition.x = mMaxScroll.x;
			}
		}
		if (mScrollPosition.y > mMaxScroll.y)
		{
			mScrollPosition.y = (mScrollPosition.y - mMaxScroll.y) * mBounceFactor + mMaxScroll.y;
			if (mScrollPosition.y < mMaxScroll.y - 1f)
			{
				mScrollPosition.y = mMaxScroll.y;
			}
		}
		if (!mUseOvershoot)
		{
			mScrollPosition = SnapToMax(mScrollPosition);
		}
		if (mSwipeVelocity.x != 0f)
		{
			mScrollPosition.x += mSwipeVelocity.x * mDeltaTimeThisUpdate;
			mSwipeVelocity.x *= Mathf.Clamp(1f - mDeltaTimeThisUpdate / 0.1f, 0f, 1f);
		}
		if (mSwipeVelocity.y != 0f)
		{
			mScrollPosition.y += mSwipeVelocity.y * mDeltaTimeThisUpdate;
			mSwipeVelocity.y *= Mathf.Clamp(1f - mDeltaTimeThisUpdate / 0.1f, 0f, 1f);
		}
	}

	private void UpdateSnapTo()
	{
		mScrollPosition.x = SnapToGrid(mScrollPosition.x, mMaxScroll.x);
		mScrollPosition.y = SnapToGrid(mScrollPosition.y, mMaxScroll.y);
	}

	private float SnapToGrid(float val, float max)
	{
		if (val <= 0f || val >= max)
		{
			return val;
		}
		if (mSnapToGrid != 0)
		{
			int num = (int)val % mSnapToGrid;
			if (num < mSnapToGrid / 2)
			{
				return mSnapToGrid * ((int)val / mSnapToGrid);
			}
			return mSnapToGrid * ((int)val / mSnapToGrid + 1);
		}
		return 0f;
	}

	private Vector2 SnapToMax(Vector2 pos)
	{
		Vector2 result = pos;
		result.x = Mathf.Clamp(result.x, 0f, mMaxScroll.x);
		result.y = Mathf.Clamp(result.y, 0f, mMaxScroll.y);
		return result;
	}

	public bool RecordInput(InputEvent e)
	{
		switch (e.EventType)
		{
		case InputEvent.EEventType.OnCursorDown:
		case InputEvent.EEventType.OnCursorMove:
			mJustTouched |= !mTouchPosition.HasValue;
			mTouchPosition = TouchToUI(e.Position);
			break;
		case InputEvent.EEventType.OnCursorUp:
		case InputEvent.EEventType.OnCursorExit:
			mTouchPosition = null;
			break;
		}
		return true;
	}

	private Vector2 TouchToUI(Vector2 origPt)
	{
		Vector3 vector = mUICamera.ScreenToWorldPoint(new Vector3(origPt.x, (float)Screen.height - origPt.y, 0f));
		return new Vector2(vector.x, vector.y);
	}

	private static bool IsAnySystemTouches()
	{
		if (SingletonMonoBehaviour<InputManager>.Exists)
		{
			HandInfo hand = SingletonMonoBehaviour<InputManager>.Instance.Hand;
			FingerInfo[] fingers = hand.fingers;
			foreach (FingerInfo fingerInfo in fingers)
			{
				if (fingerInfo.IsFingerDown)
				{
					return true;
				}
			}
		}
		return false;
	}
}
