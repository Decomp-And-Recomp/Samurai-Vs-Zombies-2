using System;
using UnityEngine;

[AddComponentMenu("Audio/Sound Theme Custom Effect")]
[RequireComponent(typeof(AudioSource))]
public class SoundThemeCustomEffect : MonoBehaviour
{
	[NonSerialized]
	private SoundThemeEvent mEventData;

	[NonSerialized]
	private float mVolumeFadePercent = 1f;

	[NonSerialized]
	private float mVolumeFadeIncrement;

	[NonSerialized]
	private AnimationState mParentAnimState;

	[NonSerialized]
	private bool mTiedToAnimState;

	[NonSerialized]
	private AudioSource mAudioSource;

	[NonSerialized]
	private bool mPaused;

	private float? initialVolume;

	public SoundThemeEvent sfxEvent
	{
		get
		{
			return mEventData;
		}
	}

	public AudioSource cachedAudioSource
	{
		get
		{
			return mAudioSource;
		}
	}

	public float volume { get; set; }

	public bool paused
	{
		get
		{
			return mPaused;
		}
		set
		{
			SetPaused(value);
		}
	}

	public bool isPlaying
	{
		get
		{
			return mAudioSource != null && (mPaused || mAudioSource.isPlaying) && mVolumeFadeIncrement >= 0f;
		}
	}

	public void Update()
	{
		if (mAudioSource == null || mPaused)
		{
			return;
		}
		if (!mAudioSource.isPlaying)
		{
			Kill();
			return;
		}
		if (mVolumeFadeIncrement != 0f)
		{
			mVolumeFadePercent += mVolumeFadeIncrement * Time.deltaTime;
			if (mVolumeFadePercent >= 1f)
			{
				mVolumeFadePercent = 1f;
				mVolumeFadeIncrement = 0f;
			}
			else if (mVolumeFadePercent <= 0f && mVolumeFadeIncrement < 0f)
			{
				mAudioSource.volume = 0f;
				Kill();
				return;
			}
		}
		if (mTiedToAnimState && (mParentAnimState == null || !mParentAnimState.enabled))
		{
			mParentAnimState = null;
			Stop();
		}
		UpdateVolume();
	}

	public void BeginPlayback(SoundThemeEvent eventData)
	{
		if (eventData != null)
		{
			if (mAudioSource == null)
			{
				mAudioSource = base.GetComponent<AudioSource>();
			}
			mEventData = eventData;
			if (mEventData.playLimit != 0)
			{
				mEventData.playingClips.Add(this);
			}
			if (!initialVolume.HasValue)
			{
				initialVolume = mAudioSource.volume;
			}
			else
			{
				mAudioSource.volume = initialVolume.Value;
			}
			volume = mAudioSource.volume * mEventData.volume;
			mAudioSource.pitch += UnityEngine.Random.Range(0f - mEventData.pitchVariance, mEventData.pitchVariance);
			mAudioSource.rolloffMode = ((!eventData.useLogarithmicRolloff) ? AudioRolloffMode.Linear : AudioRolloffMode.Logarithmic);
			if (mEventData.loop)
			{
				mAudioSource.loop = true;
			}
			mAudioSource.priority = Mathf.Clamp(mAudioSource.priority + mEventData.priority, 0, 255);
			if (mEventData.maxDistanceOverride > 0f && mEventData.maxDistanceOverride > mEventData.minDistanceOverride)
			{
				mAudioSource.minDistance = mEventData.minDistanceOverride;
				mAudioSource.maxDistance = mEventData.maxDistanceOverride;
			}
			if (mEventData.fadeInTime > 0f)
			{
				mVolumeFadePercent = 0f;
				mVolumeFadeIncrement = 1f / mEventData.fadeInTime;
			}
			else
			{
				mVolumeFadePercent = 1f;
				mVolumeFadeIncrement = 0f;
			}
			mAudioSource.Play();
		}
	}

	public void SetPaused(bool shouldBePaused)
	{
		if (shouldBePaused != mPaused && (!shouldBePaused || mAudioSource.isPlaying))
		{
			mPaused = shouldBePaused;
			base.enabled = !shouldBePaused;
			if (shouldBePaused)
			{
				mAudioSource.Pause();
			}
			else
			{
				mAudioSource.Play();
			}
		}
	}

	public void Stop()
	{
		if (mPaused)
		{
			mPaused = false;
			Kill();
		}
		else if (!(mVolumeFadeIncrement < 0f))
		{
			if (mEventData.fadeOutTime <= 0f)
			{
				mVolumeFadeIncrement = -1f;
				mVolumeFadePercent = 0f;
				mAudioSource.volume = 0f;
			}
			else
			{
				mVolumeFadeIncrement = 0f - 1f / mEventData.fadeOutTime;
			}
		}
	}

	public void TieLifetimeToAnim(AnimationState animState)
	{
		mTiedToAnimState = true;
		mParentAnimState = animState;
	}

	private void UpdateVolume()
	{
		mAudioSource.volume = Mathf.Clamp(volume * SoundThemeManager.Instance.globalVolume * mVolumeFadePercent, 0f, 1f);
	}

	private void Kill()
	{
		if (mAudioSource != null)
		{
			mAudioSource.Stop();
		}
		if (mEventData != null && mEventData.playingClips != null)
		{
			mEventData.playingClips.Remove(this);
		}
		SoundThemePlayer.pool.Release(base.gameObject);
	}
}
