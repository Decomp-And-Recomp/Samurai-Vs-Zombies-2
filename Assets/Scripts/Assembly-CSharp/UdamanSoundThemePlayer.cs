using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Audio/Udaman Sound Theme Player")]
public class UdamanSoundThemePlayer : MonoBehaviour
{
	private const string defaultClipName = "Clip";

	private static GameObjectPool pool = new GameObjectPool();

	public float defaultMinDistance = 1f;

	public float defaultMaxDistance = 500f;

	protected bool keepSoundsInMemory = true;

	[DataBundleSchemaFilter(typeof(USoundThemeSetSchema), false)]
	[HideInInspector]
	public DataBundleRecordKey soundThemeKey;

	[DataBundleSchemaFilter(typeof(DynamicEnum), false)]
	[HideInInspector]
	[DataBundleRecordTableFilter("SoundThemeEnum")]
	public DataBundleRecordKey onLoadedSoundEvent;

	protected List<USoundThemeEventClip> playingClips = new List<USoundThemeEventClip>();

	protected USoundThemeEventClip voiceClip;

	protected bool hasPlayedOnLoadedEvent;

	private void Awake()
	{
		playingClips.Clear();
		voiceClip = null;
		hasPlayedOnLoadedEvent = false;
	}

	private void Start()
	{
		PlayOnLoadedSoundEvent();
	}

	public void PlayOnLoadedSoundEvent()
	{
		if (!hasPlayedOnLoadedEvent && onLoadedSoundEvent != null && !string.IsNullOrEmpty(onLoadedSoundEvent))
		{
			hasPlayedOnLoadedEvent = true;
			PlaySoundEvent(onLoadedSoundEvent.Key, soundThemeKey);
		}
	}

	private void OnClipKilledCallback(USoundThemeEventClip clip)
	{
		if (voiceClip == clip)
		{
			voiceClip = null;
		}
		playingClips.Remove(clip);
		if (clip != null && clip.ClipsetHandle != null && clip.ClipsetHandle.Data != null && clip.ClipsetHandle.Data.dontPreloadSound)
		{
			bool flag = false;
			foreach (USoundThemeEventClip playingClip in playingClips)
			{
				if (playingClip != null && playingClip.ClipsetHandle == clip.ClipsetHandle)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				clip.ClipsetHandle.Unload();
			}
		}
		GameObject gameObject = clip.ActiveAudioSource.gameObject;
		if (gameObject != null)
		{
			gameObject.transform.parent = SingletonSpawningMonoBehaviour<USoundThemeManager>.Instance.gameObject.transform;
			gameObject.name = "Clip";
			pool.Release(gameObject);
		}
	}

	public void PlaySoundEventThisAnimOnly(AnimationEvent animEvent)
	{
		if (!string.IsNullOrEmpty(animEvent.stringParameter))
		{
			USoundThemeEventClip uSoundThemeEventClip = PlaySoundEvent(animEvent.stringParameter);
			if ((bool)uSoundThemeEventClip && !(animEvent.animationState == null))
			{
				uSoundThemeEventClip.TieLifetimeToAnim(animEvent.animationState);
			}
		}
	}

	public void PlaySoundEventFromTaggedAnim(AnimationEvent animEvent)
	{
		PlaySoundEvent(animEvent.stringParameter);
	}

	public void StopPlayingSoundEventSingleOccurrence(string soundEvent)
	{
		StopPlayingSoundEvent(soundEvent, true);
	}

	public void StopPlayingSoundEventAllOccurrences(string soundEvent)
	{
		StopPlayingSoundEvent(soundEvent, false);
	}

	private void StopPlayingSoundEvent(string soundEvent, bool singleOccurrence)
	{
		foreach (USoundThemeEventClip playingClip in playingClips)
		{
			if ((bool)playingClip.SoundEvent && playingClip.SoundEvent.type != null && playingClip.SoundEvent.type.Key == soundEvent)
			{
				playingClip.Stop();
				if (singleOccurrence)
				{
					break;
				}
			}
		}
	}

	public USoundThemeEventClip PlaySoundEvent(string soundEvent)
	{
		return PlaySoundEvent(soundEvent, soundThemeKey);
	}

	public USoundThemeEventClip PlaySoundEvent(string soundEvent, string themeKey)
	{
		return PlaySoundEvent(soundEvent, new DataBundleRecordKey(themeKey));
	}

	public USoundThemeEventClip PlaySoundEvent(DynamicEnum soundEvent, DataBundleRecordKey theme)
	{
		return PlaySoundEvent(soundEvent.value, theme);
	}

	public USoundThemeEventClip PlaySoundEvent(DynamicEnum soundEvent)
	{
		return PlaySoundEvent(soundEvent.value, soundThemeKey);
	}

	public USoundThemeEventClip PlaySoundEvent(string eventName, DataBundleRecordKey themeKey)
	{
		if (DataBundleRuntime.Instance == null)
		{
			return null;
		}
		if (themeKey == null || string.IsNullOrEmpty(themeKey))
		{
			return null;
		}
		USoundThemeSetSchema soundTheme = SingletonSpawningMonoBehaviour<USoundThemeManager>.Instance.GetSoundTheme(themeKey);
		if (!soundTheme)
		{
			return null;
		}
		AudioClip clip = null;
		DataBundleRecordHandle<USoundThemeClipsetSchema>[] value = null;
		DataBundleRecordHandle<USoundThemeClipsetSchema> dataBundleRecordHandle = null;
		DataBundleRecordHandle<USoundThemeEventSetSchema> value2 = null;
		if (soundTheme.LoadedEventSets != null)
		{
			soundTheme.LoadedEventSets.TryGetValue(eventName, out value2);
		}
		if (value2 == null || value2.Data == null)
		{
			return null;
		}
		if ((bool)value2.Data.singleClip)
		{
			clip = value2.Data.singleClip;
		}
		else if (soundTheme.LoadedClipsets != null && soundTheme.LoadedClipsets.TryGetValue(eventName, out value))
		{
			int num = value.Length;
			if (num <= 0)
			{
				return null;
			}
			int num2 = UnityEngine.Random.Range(0, num);
			dataBundleRecordHandle = value[num2];
			if (dataBundleRecordHandle.Data != null)
			{
				if (dataBundleRecordHandle.Data.dontPreloadSound && !dataBundleRecordHandle.IsLoaded)
				{
					dataBundleRecordHandle.Load(null);
				}
				clip = dataBundleRecordHandle.Data.audioClip;
			}
		}
		AudioSource audioSource = null;
		string empty = string.Empty;
		if (value2.Data.isVoice)
		{
			if (voiceClip != null)
			{
				return null;
			}
			empty = "SoundThemeVoice_" + eventName;
		}
		else
		{
			empty = "SoundTheme_" + eventName;
		}
		USoundThemeEventClip uSoundThemeEventClip = new USoundThemeEventClip();
		if (!uSoundThemeEventClip)
		{
			return null;
		}
		GameObject gameObject = pool.Acquire("Clip", new Type[1] { typeof(AudioSource) });
		gameObject.name = empty;
		gameObject.transform.parent = SingletonSpawningMonoBehaviour<USoundThemeManager>.Instance.TransformCached;
		gameObject.transform.localPosition = Vector3.zero;
		audioSource = gameObject.GetComponent<AudioSource>();
		if (audioSource == null)
		{
			return null;
		}
		audioSource.clip = clip;
		audioSource.minDistance = defaultMinDistance;
		audioSource.maxDistance = defaultMaxDistance;
		if (value2.Data.isVoice)
		{
			voiceClip = uSoundThemeEventClip;
		}
		uSoundThemeEventClip.OnKilledCallback += OnClipKilledCallback;
		uSoundThemeEventClip.BeginPlayback(audioSource, value2.Data, base.transform);
		uSoundThemeEventClip.ClipsetHandle = dataBundleRecordHandle;
		uSoundThemeEventClip.EventSetHandle = value2;
		soundTheme.AddPlayingEvent(value2.Data);
		playingClips.Add(uSoundThemeEventClip);
		return uSoundThemeEventClip;
	}

	public static implicit operator bool(UdamanSoundThemePlayer obj)
	{
		return obj != null;
	}
}
