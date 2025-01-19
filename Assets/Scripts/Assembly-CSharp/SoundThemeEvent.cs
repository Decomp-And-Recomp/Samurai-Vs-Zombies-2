using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SoundThemeEvent
{
	public string name = string.Empty;

	public AudioClip[] clips = new AudioClip[0];

	public int percentChanceNone;

	public float pitchVariance;

	public float volume = 1f;

	public bool isVoice;

	public int playLimit;

	public int priority;

	public bool trackPositionAndLifetime;

	public float fadeInTime;

	public float fadeOutTime;

	public SoundThemeCustomEffect customEffect;

	public bool loop;

	public float minDistanceOverride;

	public float maxDistanceOverride;

	public bool useLogarithmicRolloff;

	[NonSerialized]
	internal int lastClipPlayed = -1;

	[NonSerialized]
	private List<SoundThemeCustomEffect> mPlayingClips;

	public List<SoundThemeCustomEffect> playingClips
	{
		get
		{
			if (mPlayingClips == null)
			{
				mPlayingClips = new List<SoundThemeCustomEffect>();
			}
			return mPlayingClips;
		}
	}
}
