using UnityEngine;

public class FadeInOutEvent
{
	public enum FadeState
	{
		eFadeIn = 0,
		eHold = 1,
		eFadeOut = 2,
		eComplete = 3
	}

	private float mInterpolant;

	private float mFadeInTime;

	private float mHoldTime;

	private float mFadeOutTime;

	private float mCurrentTime;

	private FadeState state;

	public bool isComplete
	{
		get
		{
			return state == FadeState.eComplete;
		}
	}

	public FadeState State
	{
		get
		{
			return state;
		}
		set
		{
			state = value;
		}
	}

	public float interpolant
	{
		get
		{
			return mInterpolant;
		}
	}

	public FadeInOutEvent(float fadeInTime, float holdTime, float fadeOutTime)
	{
		mFadeInTime = Mathf.Max(0f, fadeInTime);
		mFadeOutTime = Mathf.Max(0f, fadeOutTime);
		mHoldTime = holdTime;
	}

	public void update()
	{
		mCurrentTime += Time.deltaTime;
		if (state == FadeState.eFadeIn)
		{
			if (mCurrentTime >= mFadeInTime)
			{
				state = FadeState.eHold;
				mCurrentTime -= mFadeInTime;
				mInterpolant = 1f;
				update();
			}
			else
			{
				mInterpolant = mCurrentTime / mFadeInTime;
			}
		}
		else if (state == FadeState.eHold)
		{
			if (mCurrentTime >= mHoldTime && mHoldTime >= 0f)
			{
				state = FadeState.eFadeOut;
				mCurrentTime -= mHoldTime;
				mInterpolant = 1f;
				update();
			}
			else
			{
				mInterpolant = 1f;
			}
		}
		else if (mCurrentTime >= mFadeOutTime)
		{
			state = FadeState.eComplete;
			mInterpolant = 0f;
		}
		else
		{
			mInterpolant = 1f - mCurrentTime / mFadeOutTime;
		}
	}

	public void Reset()
	{
		mCurrentTime = 0f;
		state = FadeState.eFadeIn;
	}
}
