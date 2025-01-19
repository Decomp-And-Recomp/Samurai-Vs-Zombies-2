using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Audio/Sound Theme")]
public class SoundTheme : MonoBehaviour
{
	public SoundTheme baseTheme;

	public SoundThemeEvent[] events = new SoundThemeEvent[0];

	[NonSerialized]
	private Dictionary<string, SoundThemeEvent> mEvents;

	private void Awake()
	{
		mEvents = new Dictionary<string, SoundThemeEvent>();
		if (events == null)
		{
			return;
		}
		for (int i = 0; i < events.Length; i++)
		{
			mEvents[events[i].name] = events[i];
			if (events[i].volume == 0f)
			{
				events[i].volume = 1f;
			}
			if (events[i].loop)
			{
				events[i].trackPositionAndLifetime = true;
			}
		}
	}

	public SoundThemeEvent GetEvent(string theName)
	{
		if (mEvents == null || events.Length != mEvents.Count)
		{
			Awake();
		}
		SoundThemeEvent value;
		if (mEvents.TryGetValue(theName, out value))
		{
			return value;
		}
		if (baseTheme != null)
		{
			return baseTheme.GetEvent(theName);
		}
		return null;
	}
}
