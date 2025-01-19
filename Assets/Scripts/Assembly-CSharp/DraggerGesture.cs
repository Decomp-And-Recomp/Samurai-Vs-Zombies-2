using UnityEngine;

internal class DraggerGesture
{
	public delegate void OnDragEvent(Vector2 pos);

	private const float kMaxWaitDistance = 3f;

	private bool mTouching;

	private bool mIsDragging;

	private float mTouchTimer;

	private float mDragTimerTrigger = 0.5f;

	public OnDragEvent onStart;

	public OnDragEvent onDrop;

	public OnDragEvent onDragMoveTo;

	public DraggerGesture(float triggerDelay = 0.5f)
	{
		mDragTimerTrigger = triggerDelay;
	}

	public void Update()
	{
		InputGestureStatus gestureStatus = SingletonMonoBehaviour<InputManager>.Instance.GestureStatus;
		FingerInfo[] fingers = gestureStatus.Hand.fingers;
		if (fingers.Length <= 0)
		{
			return;
		}
		if (fingers[0].IsFingerDown != mTouching)
		{
			mTouching = fingers[0].IsFingerDown;
			if (mTouching)
			{
				mTouchTimer = 0f;
				mIsDragging = false;
			}
			else if (mIsDragging)
			{
				if (onDrop != null)
				{
					onDrop(fingers[0].CursorReleasedPosition);
				}
				mIsDragging = false;
			}
		}
		else
		{
			if (!mTouching)
			{
				return;
			}
			if (mIsDragging)
			{
				if (onDragMoveTo != null)
				{
					onDragMoveTo(fingers[0].CursorPosition);
				}
				return;
			}
			if ((fingers[0].CursorPosition - fingers[0].CursorStartPosition).magnitude >= 3f)
			{
				mTouchTimer = 0f;
				return;
			}
			mTouchTimer += Time.deltaTime;
			if (mTouchTimer >= mDragTimerTrigger)
			{
				mIsDragging = true;
				if (onStart != null)
				{
					onStart(fingers[0].CursorStartPosition);
				}
			}
		}
	}
}
