using System;
using UnityEngine;

internal class MusicSource
{
	[Flags]
	public enum State
	{
		Idle = 2,
		Paused = 4,
		Playing = 8,
		FadeIn = 0x10,
		FadeOut = 0x20,
		ExternalPause = 0x40
	}

	public State state = State.Idle;

	public float clipLength;

	public AudioSource audioSource;

	public string musicEventName = string.Empty;

	protected float volume = 1f;

	protected float volumeFadePercent;

	private DataBundleRecordHandle<UMusicSchema> handle;

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

	public float VolumeFadePercent
	{
		get
		{
			return volumeFadePercent;
		}
		set
		{
			volumeFadePercent = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public bool IsIdle
	{
		get
		{
			return IsFlagSet(State.Idle);
		}
	}

	public bool IsPlaying
	{
		get
		{
			return audioSource.isPlaying;
		}
	}

	public bool IsPaused
	{
		get
		{
			return IsFlagSet(State.Paused);
		}
	}

	public bool IsExternalPaused
	{
		get
		{
			return IsFlagSet(State.ExternalPause);
		}
	}

	public bool FadingIn
	{
		get
		{
			return IsFlagSet(State.FadeIn);
		}
		set
		{
			state &= ~State.FadeIn;
			if (value)
			{
				state |= State.FadeIn;
			}
		}
	}

	public bool FadingOut
	{
		get
		{
			return IsFlagSet(State.FadeOut);
		}
		set
		{
			state &= ~State.FadeOut;
			if (value)
			{
				state |= State.FadeOut;
			}
		}
	}

	public MusicSource(Transform parentTran)
	{
		GameObject gameObject = new GameObject("GameMusicSource");
		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.volume = 0f;
		Transform transform = gameObject.transform;
		transform.parent = parentTran;
		transform.localPosition = Vector3.zero;
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
	}

	public void UpdateMusic()
	{
		if ((bool)audioSource)
		{
			audioSource.volume = volume * AudioUtils.MasterMusicVolume * volumeFadePercent;
		}
	}

	public void BeginPlayback(UMusicEventSchema musicEvent)
	{
		if (!musicEvent || musicEvent.musicClip == null || string.IsNullOrEmpty(musicEvent.musicClip))
		{
			return;
		}
		Stop();
		musicEventName = musicEvent.eventName;
		handle = new DataBundleRecordHandle<UMusicSchema>(musicEvent.musicClip);
		if (handle == null)
		{
			return;
		}
		handle.Load(delegate(UMusicSchema music)
		{
			if ((bool)music.musicClip)
			{
				BeginPlaybackInternal(music.musicClip, musicEvent.musicSource != UMusicManager.MusicSourceID.Stinger);
			}
		});
	}

	public void BeginPlayback(AudioClip clip, bool loop)
	{
		Stop();
		BeginPlaybackInternal(clip, loop);
	}

	private void BeginPlaybackInternal(AudioClip clip, bool loop)
	{
		if ((bool)clip)
		{
			audioSource.clip = clip;
			audioSource.loop = loop;
			audioSource.Play();
			clipLength = clip.length;
			state = State.Playing;
		}
	}

	public float TimeToEnd()
	{
		return clipLength - audioSource.time;
	}

	public void Stop()
	{
		if (audioSource.clip != null)
		{
			audioSource.Stop();
			audioSource.clip = null;
		}
		if (handle != null)
		{
			handle.Dispose();
			handle = null;
		}
		clipLength = 0f;
		state = State.Idle;
	}

	public void Pause()
	{
		audioSource.Pause();
		state |= State.Paused;
	}

	public void ExternalPause()
	{
		audioSource.Pause();
		state |= State.ExternalPause;
	}

	public void Resume()
	{
		audioSource.Play();
		ClearFlag(State.Paused);
		ClearFlag(State.ExternalPause);
	}

	private bool IsFlagSet(State flag)
	{
		return (state & flag) != 0;
	}

	private void SetFlag(State flag)
	{
		state |= flag;
	}

	private void ClearFlag(State flag)
	{
		state &= ~flag;
	}

	public static implicit operator bool(MusicSource obj)
	{
		return obj != null;
	}
}
