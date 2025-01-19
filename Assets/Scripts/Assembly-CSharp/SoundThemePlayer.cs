using System;
using UnityEngine;

[AddComponentMenu("Audio/Sound Theme Player")]
public class SoundThemePlayer : MonoBehaviour
{
	public static GameObjectPool pool = new GameObjectPool();

	public SoundTheme baseTheme;

	public float defaultMinDistance = 1f;

	public float defaultMaxDistance = 500f;

	public SoundThemeCustomEffect defaultCustomEffect;

	public Vector3 positionOffset = Vector3.zero;

	public string autoPlayEvent = string.Empty;

	public SoundThemeEvent[] events = new SoundThemeEvent[0];

	private SoundThemeCustomEffect mCurrentVoiceClip;

	private void Awake()
	{
		if (events != null && events.Length > 0)
		{
			baseTheme = SoundThemeManager.Instance.GetSharedThemeForPlayer(this);
		}
	}

	private void Start()
	{
		if (!string.IsNullOrEmpty(autoPlayEvent))
		{
			PlaySoundEvent(autoPlayEvent);
		}
	}

	public AudioSource PlaySoundEvent(string theName)
	{
		SoundThemeEvent soundEvent = GetSoundEvent(theName);
		if (soundEvent != null)
		{
			if (soundEvent.percentChanceNone > 0 && UnityEngine.Random.Range(0, 100) < soundEvent.percentChanceNone)
			{
				return null;
			}
			if (soundEvent.clips == null || soundEvent.clips.Length == 0)
			{
				return null;
			}
			if (soundEvent.playLimit < 0 && soundEvent.playingClips != null && soundEvent.playingClips.Count != 0 && soundEvent.playingClips.Count >= -soundEvent.playLimit)
			{
				return null;
			}
			while (soundEvent.playLimit > 0 && soundEvent.playingClips != null && soundEvent.playingClips.Count != 0 && soundEvent.playingClips.Count >= soundEvent.playLimit)
			{
				SoundThemeCustomEffect soundThemeCustomEffect = soundEvent.playingClips[0];
				soundEvent.playingClips.Remove(soundThemeCustomEffect);
				pool.Release(soundThemeCustomEffect.gameObject);
			}
			if (soundEvent.loop)
			{
				SoundThemeCustomEffect soundThemeCustomEffect2 = FindTrackedSoundEvent(theName);
				if (soundThemeCustomEffect2 != null && soundThemeCustomEffect2.isPlaying)
				{
					return soundThemeCustomEffect2.cachedAudioSource;
				}
			}
			int num;
			if (soundEvent.clips.Length > 1 && soundEvent.lastClipPlayed >= 0 && soundEvent.lastClipPlayed < soundEvent.clips.Length)
			{
				num = -1;
				for (int num2 = UnityEngine.Random.Range(1, soundEvent.clips.Length); num2 > 0; num2--)
				{
					num++;
					if (num == soundEvent.lastClipPlayed)
					{
						num++;
					}
				}
				if (num >= soundEvent.clips.Length)
				{
					num = UnityEngine.Random.Range(0, soundEvent.clips.Length);
				}
			}
			else
			{
				num = UnityEngine.Random.Range(0, soundEvent.clips.Length);
			}
			soundEvent.lastClipPlayed = num;
			AudioClip audioClip = soundEvent.clips[num];
			if (audioClip != null)
			{
				SoundThemeCustomEffect soundThemeCustomEffect3 = SoundThemeManager.Instance.globalForcedOverrideCustomEffect;
				if (soundThemeCustomEffect3 == null)
				{
					soundThemeCustomEffect3 = soundEvent.customEffect;
				}
				if (soundThemeCustomEffect3 == null)
				{
					soundThemeCustomEffect3 = defaultCustomEffect;
				}
				if (soundThemeCustomEffect3 == null && SoundThemeManager.Instance.globalDefaultCustomEffect != null)
				{
					soundThemeCustomEffect3 = SoundThemeManager.Instance.globalDefaultCustomEffect;
				}
				GameObject gameObject;
				AudioSource audioSource;
				if (soundThemeCustomEffect3 != null)
				{
					gameObject = pool.Acquire(soundThemeCustomEffect3.gameObject);
					audioSource = gameObject.GetComponent<AudioSource>();
					gameObject.GetComponent<AudioSource>().clip = audioClip;
					soundThemeCustomEffect3 = gameObject.GetComponent<SoundThemeCustomEffect>();
				}
				else
				{
					gameObject = pool.Acquire("SoundThemeCustomEffect", new Type[2]
					{
						typeof(AudioSource),
						typeof(SoundThemeCustomEffect)
					});
					audioSource = gameObject.GetComponent<AudioSource>();
					soundThemeCustomEffect3 = gameObject.GetComponent<SoundThemeCustomEffect>();
					audioSource.clip = audioClip;
					if (defaultMaxDistance > 0f && defaultMaxDistance > defaultMinDistance)
					{
						audioSource.minDistance = defaultMinDistance;
						audioSource.maxDistance = defaultMaxDistance;
					}
				}
				if (soundEvent.loop)
				{
					soundEvent.trackPositionAndLifetime = true;
				}
				if (soundEvent.trackPositionAndLifetime)
				{
					gameObject.transform.parent = base.transform;
				}
				gameObject.transform.position = base.transform.position;
				gameObject.transform.localPosition += positionOffset;
				if (soundEvent.isVoice)
				{
					if (mCurrentVoiceClip != null)
					{
						pool.Release(mCurrentVoiceClip.gameObject);
					}
					mCurrentVoiceClip = soundThemeCustomEffect3;
				}
				soundThemeCustomEffect3.BeginPlayback(soundEvent);
				return audioSource;
			}
		}
		return null;
	}

	public AudioSource PlaySoundEventWithVolume(string theName, float theVolume)
	{
		AudioSource audioSource = PlaySoundEvent(theName);
		if ((bool)audioSource)
		{
			SoundThemeCustomEffect component = audioSource.GetComponent<SoundThemeCustomEffect>();
			if ((bool)component)
			{
				component.volume = theVolume;
			}
		}
		return audioSource;
	}

	public void StopTrackedSoundEvent(string theName)
	{
		SoundThemeCustomEffect soundThemeCustomEffect = FindTrackedSoundEvent(theName);
		if (soundThemeCustomEffect != null)
		{
			soundThemeCustomEffect.Stop();
		}
	}

	public SoundThemeEvent GetSoundEvent(string theName)
	{
		if (baseTheme != null)
		{
			return baseTheme.GetEvent(theName);
		}
		return null;
	}

	public void PlaySoundEventThisAnimOnly(AnimationEvent animEvent)
	{
		AudioSource audioSource = PlaySoundEvent(animEvent.stringParameter);
		if ((bool)audioSource && !(animEvent.animationState == null))
		{
			SoundThemeCustomEffect component = audioSource.GetComponent<SoundThemeCustomEffect>();
			if ((bool)component)
			{
				component.TieLifetimeToAnim(animEvent.animationState);
			}
		}
	}

	private SoundThemeCustomEffect FindTrackedSoundEvent(string theName)
	{
		SoundThemeCustomEffect[] componentsInChildren = GetComponentsInChildren<SoundThemeCustomEffect>();
		SoundThemeCustomEffect[] array = componentsInChildren;
		foreach (SoundThemeCustomEffect soundThemeCustomEffect in array)
		{
			if (!(soundThemeCustomEffect.gameObject == base.gameObject) && soundThemeCustomEffect.sfxEvent != null && soundThemeCustomEffect.sfxEvent.name == theName && soundThemeCustomEffect.GetComponent<AudioSource>().isPlaying)
			{
				return soundThemeCustomEffect;
			}
		}
		return null;
	}

	public static void PlayClipAtPoint(AudioClip aClip, Vector3 position, float volume)
	{
		if (aClip != null)
		{
			SoundThemeEvent soundThemeEvent = new SoundThemeEvent();
			soundThemeEvent.clips = new AudioClip[1] { aClip };
			soundThemeEvent.name = aClip.name;
			soundThemeEvent.volume = volume;
			SoundThemeCustomEffect soundThemeCustomEffect = SoundThemeManager.Instance.globalForcedOverrideCustomEffect;
			if (soundThemeCustomEffect == null && SoundThemeManager.Instance.globalDefaultCustomEffect != null)
			{
				soundThemeCustomEffect = SoundThemeManager.Instance.globalDefaultCustomEffect;
			}
			GameObject gameObject = ((!(soundThemeCustomEffect != null)) ? pool.Acquire("SoundThemeCustomEffect", new Type[2]
			{
				typeof(AudioSource),
				typeof(SoundThemeCustomEffect)
			}) : pool.Acquire(soundThemeCustomEffect.gameObject));
			soundThemeCustomEffect = gameObject.GetComponent<SoundThemeCustomEffect>();
			gameObject.GetComponent<AudioSource>().clip = aClip;
			gameObject.transform.position = position;
			soundThemeCustomEffect.BeginPlayback(soundThemeEvent);
		}
	}
}
