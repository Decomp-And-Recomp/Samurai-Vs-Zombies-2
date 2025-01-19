using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Audio/Udaman Music Manager")]
public class UMusicManager : SingletonSpawningMonoBehaviour<UMusicManager>
{
	public enum MusicSourceID
	{
		Music = 0,
		Stinger = 1,
		Ambient = 2,
		Num = 3
	}

	public enum FadeStyle
	{
		FadeOutFadeIn = 0,
		CrossFade = 1,
		StingerPlayOverMusic = 2
	}

	public delegate void OnMusicEventDoneCallback(string musicEventName);

	public const float kFadeTime = 0f;

	private MusicSource[] activeMusic = new MusicSource[3];

	private List<MusicSource> otherSources = new List<MusicSource>();

	private bool pausedIpodMusic;

	protected override void Awake()
	{
		base.Awake();
		Transform parentTran = base.transform;
		for (int i = 0; i < 3; i++)
		{
			activeMusic[i] = new MusicSource(parentTran);
		}
	}

	private void Update()
	{
		for (int i = 0; i < activeMusic.Length; i++)
		{
			if ((bool)activeMusic[i])
			{
				activeMusic[i].UpdateMusic();
			}
		}
		foreach (MusicSource otherSource in otherSources)
		{
			otherSource.UpdateMusic();
		}
	}

	private void OnApplicationPause(bool paused)
	{
		if (!paused)
		{
			if (activeMusic[0].IsExternalPaused)
			{
				activeMusic[0].Resume();
			}
			else if (NUF.IsIpodMusicPlaying())
			{
				activeMusic[0].ExternalPause();
			}
		}
	}

	public void PlayByName(string musicEventRecordKey, OnMusicEventDoneCallback onDoneCallback = null)
	{
		PlayByKey(musicEventRecordKey, onDoneCallback);
	}

	public void PlayByKey(DataBundleRecordKey musicEventKey, OnMusicEventDoneCallback onDoneCallback = null)
	{
		if (musicEventKey == null || string.IsNullOrEmpty(musicEventKey) || DataBundleRuntime.Instance == null)
		{
			return;
		}
		UMusicEventSchema uMusicEventSchema = DataBundleRuntime.Instance.InitializeRecord<UMusicEventSchema>(musicEventKey);
		if (!uMusicEventSchema || (activeMusic[(int)uMusicEventSchema.musicSource].IsPlaying && (DataBundleRecordKey)activeMusic[(int)uMusicEventSchema.musicSource].musicEventName == uMusicEventSchema.eventName))
		{
			return;
		}
		switch (uMusicEventSchema.musicSource)
		{
		case MusicSourceID.Stinger:
			if (!activeMusic[1].IsIdle)
			{
				return;
			}
			break;
		case MusicSourceID.Music:
			if (activeMusic[1].IsPlaying)
			{
				activeMusic[0].BeginPlayback(uMusicEventSchema);
				activeMusic[0].Pause();
				return;
			}
			if (NUF.IsIpodMusicPlaying())
			{
				activeMusic[0].BeginPlayback(uMusicEventSchema);
				activeMusic[0].ExternalPause();
				return;
			}
			break;
		}
		switch (uMusicEventSchema.fadeStyle)
		{
		case FadeStyle.FadeOutFadeIn:
			StartCoroutine(DoFadeOutFadeInTransition(uMusicEventSchema, onDoneCallback));
			break;
		case FadeStyle.CrossFade:
			StartCoroutine(DoCrossFadeTransition(uMusicEventSchema, onDoneCallback));
			break;
		case FadeStyle.StingerPlayOverMusic:
			if (uMusicEventSchema.musicSource == MusicSourceID.Stinger)
			{
				MusicSource musicSource = new MusicSource(base.transform);
				musicSource.BeginPlayback(uMusicEventSchema);
				StartCoroutine(DestroyWhenDone(musicSource, uMusicEventSchema.fadeOutTimes, uMusicEventSchema.fadeInTimes, onDoneCallback));
			}
			break;
		}
	}

	private IEnumerator DoFadeOutFadeInTransition(UMusicEventSchema musicEvent, OnMusicEventDoneCallback onDoneCallback)
	{
		MusicSource originalSource = activeMusic[(int)musicEvent.musicSource];
		if (musicEvent.musicSource == MusicSourceID.Stinger)
		{
			originalSource = ((!activeMusic[0].IsPlaying) ? null : activeMusic[0]);
		}
		if ((bool)originalSource)
		{
			yield return StartCoroutine(FadeOut(originalSource, musicEvent.fadeOutTimes, null));
			originalSource.Pause();
		}
		MusicSource newSource = activeMusic[(int)musicEvent.musicSource];
		newSource.BeginPlayback(musicEvent);
		if (musicEvent.musicSource == MusicSourceID.Stinger)
		{
			StartCoroutine(FadeIn(newSource, 0f));
			while (newSource.IsPlaying)
			{
				yield return new WaitForFixedUpdate();
			}
			newSource.Stop();
			if (musicEvent.musicSource == MusicSourceID.Stinger)
			{
				originalSource = activeMusic[0];
			}
			if ((bool)originalSource)
			{
				originalSource.Resume();
				StartCoroutine(FadeIn(originalSource, musicEvent.fadeInTimes));
			}
		}
		else
		{
			StartCoroutine(FadeIn(newSource, musicEvent.fadeInTimes));
		}
	}

	private IEnumerator DoCrossFadeTransition(UMusicEventSchema musicEvent, OnMusicEventDoneCallback onDoneCallback)
	{
		if (musicEvent.musicSource == MusicSourceID.Stinger)
		{
			if (activeMusic[0].IsPlaying)
			{
				AudioClip musicClip = null;
				float timeInClip = 0f;
				if ((bool)activeMusic[0].audioSource)
				{
					musicClip = activeMusic[0].audioSource.clip;
					timeInClip = activeMusic[0].audioSource.time + musicEvent.fadeOutTimes;
					if (timeInClip >= activeMusic[0].clipLength)
					{
						timeInClip -= activeMusic[0].clipLength;
					}
				}
				StartCoroutine(FadeOutAndDestroy(activeMusic[0], musicEvent.fadeOutTimes, null));
				activeMusic[0] = new MusicSource(base.transform);
				activeMusic[0].BeginPlayback(musicClip, true);
				if ((bool)activeMusic[0].audioSource)
				{
					activeMusic[0].audioSource.time = timeInClip;
				}
				activeMusic[0].Pause();
			}
		}
		else if (activeMusic[(int)musicEvent.musicSource].IsPlaying)
		{
			StartCoroutine(FadeOutAndDestroy(activeMusic[(int)musicEvent.musicSource], musicEvent.fadeOutTimes, null));
			activeMusic[(int)musicEvent.musicSource] = new MusicSource(base.transform);
		}
		MusicSource newSource = activeMusic[(int)musicEvent.musicSource];
		newSource.BeginPlayback(musicEvent);
		StartCoroutine(FadeIn(newSource, musicEvent.fadeInTimes));
		if (musicEvent.musicSource != MusicSourceID.Stinger)
		{
			yield break;
		}
		while (newSource.IsPlaying)
		{
			float timeLeft = newSource.TimeToEnd();
			if (timeLeft <= musicEvent.fadeOutTimes)
			{
				StartCoroutine(FadeOutAndStop(newSource, timeLeft, onDoneCallback));
				if (!activeMusic[0].IsIdle)
				{
					activeMusic[0].Resume();
					StartCoroutine(FadeIn(activeMusic[0], musicEvent.fadeInTimes));
				}
				break;
			}
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator FadeOutAndPause(MusicSource musicSource, float fadeOutTime, OnMusicEventDoneCallback onDoneCallback)
	{
		yield return StartCoroutine(FadeOut(musicSource, fadeOutTime, onDoneCallback));
		musicSource.Pause();
	}

	private IEnumerator FadeOutAndStop(MusicSource musicSource, float fadeOutTime, OnMusicEventDoneCallback onDoneCallback)
	{
		yield return StartCoroutine(FadeOut(musicSource, fadeOutTime, onDoneCallback));
		musicSource.Stop();
	}

	private IEnumerator FadeOutAndDestroy(MusicSource musicSource, float fadeOutTime, OnMusicEventDoneCallback onDoneCallback)
	{
		otherSources.Add(musicSource);
		yield return StartCoroutine(FadeOut(musicSource, fadeOutTime, onDoneCallback));
		musicSource.Stop();
		if ((bool)musicSource.audioSource)
		{
			Object.Destroy(musicSource.audioSource.gameObject);
		}
		otherSources.Remove(musicSource);
	}

	private IEnumerator DestroyWhenDone(MusicSource musicSource, float fadeOutTime, float fadeInTime, OnMusicEventDoneCallback onDoneCallback)
	{
		otherSources.Add(musicSource);
		yield return StartCoroutine(FadeIn(musicSource, fadeInTime));
		while (musicSource.IsPlaying)
		{
			float timeLeft = musicSource.TimeToEnd();
			if (timeLeft <= fadeOutTime)
			{
				yield return StartCoroutine(FadeOut(musicSource, timeLeft, onDoneCallback));
				break;
			}
			yield return new WaitForFixedUpdate();
		}
		if (onDoneCallback != null)
		{
			onDoneCallback(musicSource.musicEventName);
		}
		musicSource.Stop();
		if ((bool)musicSource.audioSource)
		{
			Object.Destroy(musicSource.audioSource.gameObject);
		}
		otherSources.Remove(musicSource);
	}

	private IEnumerator FadeOut(MusicSource musicSource, float time, OnMusicEventDoneCallback onDoneCallback)
	{
		if (time <= 0f)
		{
			musicSource.VolumeFadePercent = 0f;
			yield break;
		}
		musicSource.FadingOut = true;
		float volumeFadePercent = musicSource.VolumeFadePercent;
		while (volumeFadePercent > 0f && musicSource.FadingOut)
		{
			volumeFadePercent -= Time.deltaTime / time;
			if (volumeFadePercent <= 0f)
			{
				volumeFadePercent = 0f;
				musicSource.FadingOut = false;
			}
			musicSource.VolumeFadePercent = volumeFadePercent;
			yield return new WaitForFixedUpdate();
		}
		if (onDoneCallback != null)
		{
			onDoneCallback(musicSource.musicEventName);
		}
	}

	private IEnumerator FadeIn(MusicSource musicSource, float time)
	{
		if (time <= 0f)
		{
			musicSource.VolumeFadePercent = 1f;
			yield break;
		}
		musicSource.FadingIn = true;
		float volumeFadePercent = musicSource.VolumeFadePercent;
		while (volumeFadePercent < 1f && musicSource.FadingIn)
		{
			volumeFadePercent += Time.deltaTime / time;
			if (volumeFadePercent >= 1f)
			{
				volumeFadePercent = 1f;
				musicSource.FadingIn = false;
			}
			musicSource.VolumeFadePercent = volumeFadePercent;
			yield return new WaitForFixedUpdate();
		}
	}

	private void OnTapjoyVideo(bool videoEnabled)
	{
		for (int i = 0; i < activeMusic.Length; i++)
		{
			if ((bool)activeMusic[i])
			{
				if (videoEnabled && activeMusic[i].IsPlaying)
				{
					activeMusic[i].ExternalPause();
				}
				else if (!videoEnabled && activeMusic[i].IsExternalPaused && !pausedIpodMusic)
				{
					activeMusic[i].Resume();
				}
			}
			AudioListener.pause = videoEnabled;
			if (pausedIpodMusic || NUF.IsIpodMusicPlaying())
			{
				pausedIpodMusic = videoEnabled;
				NUF.PauseIpodMusic(videoEnabled);
			}
		}
	}
}
