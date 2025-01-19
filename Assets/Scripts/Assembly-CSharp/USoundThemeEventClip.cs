using System.Runtime.CompilerServices;
using UnityEngine;

public class USoundThemeEventClip
{
	public delegate void OnKilledDelegate(USoundThemeEventClip clip);

	private USoundThemeEventSetSchema soundEvent;

	private float volumeFadePercent = 1f;

	private float volumeFadeIncrement;

	private AnimationState parentAnimState;

	private bool tiedToAnimState;

	private AudioSource audioSource;

	private bool paused;

	private float volume = 1f;

	private float startDelayTimer;

	private Transform audioSourceTransform;

	private Transform parentTransform;

	private bool hasParentTransform;

	public USoundThemeEventSetSchema SoundEvent
	{
		get
		{
			return soundEvent;
		}
	}

	public AudioSource ActiveAudioSource
	{
		get
		{
			return audioSource;
		}
	}

	public float Volume
	{
		get
		{
			return volume;
		}
		set
		{
			volume = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public bool Paused
	{
		get
		{
			return paused;
		}
		set
		{
			SetPaused(value);
		}
	}

	public bool IsDead
	{
		get
		{
			return !audioSource;
		}
	}

	public bool IsPlaying
	{
		get
		{
			return (bool)audioSource && (paused || audioSource.isPlaying || startDelayTimer > 0f) && volumeFadeIncrement >= 0f;
		}
	}

	public DataBundleRecordHandle<USoundThemeClipsetSchema> ClipsetHandle { get; set; }

	public DataBundleRecordHandle<USoundThemeEventSetSchema> EventSetHandle { get; set; }

	[method: MethodImpl(32)]
	public event OnKilledDelegate OnKilledCallback;

	private void SetPaused(bool shouldBePaused)
	{
		if (shouldBePaused != paused && (!shouldBePaused || audioSource.isPlaying))
		{
			paused = shouldBePaused;
			if (shouldBePaused)
			{
				audioSource.Pause();
			}
			else
			{
				audioSource.Play();
			}
		}
	}

	public void TieLifetimeToAnim(AnimationState animState)
	{
		tiedToAnimState = true;
		parentAnimState = animState;
	}

	public void BeginPlayback(AudioSource audio, USoundThemeEventSetSchema soundEventData, Transform playerTransform)
	{
		if ((bool)soundEventData && (bool)audio)
		{
			audioSource = audio;
			audioSourceTransform = audioSource.transform;
			parentTransform = playerTransform;
			soundEvent = soundEventData;
			soundEventData.ActiveClips.Add(this);
			startDelayTimer = soundEventData.startDelay;
			if (soundEvent.fadeInTime > 0f)
			{
				volumeFadePercent = 0f;
				volumeFadeIncrement = 1f / soundEvent.fadeInTime;
			}
			else
			{
				volumeFadePercent = 1f;
				volumeFadeIncrement = 0f;
			}
			UpdatePosition();
			UpdateVolume();
			audioSource.pitch = soundEvent.basePitch + Random.Range(0f - soundEvent.pitchVariance, soundEvent.pitchVariance);
			audioSource.rolloffMode = ((!soundEvent.logarithmicRolloff) ? AudioRolloffMode.Linear : AudioRolloffMode.Logarithmic);
			audioSource.loop = soundEvent.loop;
			audioSource.priority = Mathf.Clamp(soundEvent.priority, 0, 255);
			audioSource.spread = soundEvent.stereoSpread;
			audioSource.dopplerLevel = soundEvent.dopplerLevel;
			hasParentTransform = playerTransform != null && soundEvent.loop;
			if (soundEvent.maxDistanceOverride > 0f && soundEvent.maxDistanceOverride > soundEvent.minDistanceOverride)
			{
				audioSource.minDistance = soundEvent.minDistanceOverride;
				audioSource.maxDistance = soundEvent.maxDistanceOverride;
			}
			if (SingletonSpawningMonoBehaviour<USoundThemeManager>.Instance.IsBusPaused(soundEvent.BusNumber))
			{
				paused = true;
			}
			else if (startDelayTimer <= 0f)
			{
				audioSource.Play();
			}
		}
	}

	public void UpdateClip()
	{
		if (!audioSource || paused)
		{
			return;
		}
		if (startDelayTimer > 0f)
		{
			startDelayTimer -= Time.deltaTime;
			if (!(startDelayTimer <= 0f))
			{
				return;
			}
			audioSource.Play();
		}
		if (!audioSource.isPlaying)
		{
			Kill();
			return;
		}
		if (hasParentTransform && (parentTransform == null || !parentTransform.gameObject.activeInHierarchy))
		{
			Kill();
			return;
		}
		if (volumeFadeIncrement != 0f)
		{
			volumeFadePercent += volumeFadeIncrement * Time.deltaTime;
			if (volumeFadePercent >= 1f)
			{
				volumeFadePercent = 1f;
				volumeFadeIncrement = 0f;
			}
			else if (volumeFadePercent <= 0f && volumeFadeIncrement < 0f)
			{
				audioSource.volume = 0f;
				Kill();
				return;
			}
		}
		if (tiedToAnimState && (parentAnimState == null || !parentAnimState.enabled))
		{
			parentAnimState = null;
			Stop();
		}
		UpdatePosition();
		UpdateVolume();
	}

	private void UpdateVolume()
	{
		if ((bool)audioSource)
		{
			float num = volume * AudioUtils.MasterSoundThemeVolume * volumeFadePercent;
			if ((bool)soundEvent)
			{
				num *= Mathf.Clamp(soundEvent.volume, 0f, 1f);
			}
			audioSource.volume = num;
		}
	}

	private void UpdatePosition()
	{
		if ((bool)audioSourceTransform)
		{
			if ((bool)parentTransform)
			{
				audioSourceTransform.position = parentTransform.position;
			}
			else if (soundEvent.loop)
			{
				Stop();
			}
		}
	}

	public void Stop()
	{
		if (paused)
		{
			paused = false;
			Kill();
		}
		else
		{
			if (volumeFadeIncrement < 0f)
			{
				return;
			}
			if (soundEvent.fadeOutTime <= 0f)
			{
				volumeFadeIncrement = -1f;
				volumeFadePercent = 0f;
				if ((bool)audioSource)
				{
					audioSource.volume = 0f;
				}
			}
			else
			{
				volumeFadeIncrement = 0f - 1f / soundEvent.fadeOutTime;
			}
		}
	}

	public void Kill()
	{
		if ((bool)audioSource)
		{
			audioSource.Stop();
		}
		if (this.OnKilledCallback != null)
		{
			this.OnKilledCallback(this);
		}
		audioSource = null;
		audioSourceTransform = null;
		parentTransform = null;
	}

	public static implicit operator bool(USoundThemeEventClip obj)
	{
		return obj != null;
	}
}
