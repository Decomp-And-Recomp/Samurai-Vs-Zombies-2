using UnityEngine;

[AddComponentMenu("Audio/Sound Theme Manager")]
public class SoundThemeManager : MonoBehaviour
{
	private static SoundThemeManager instance;

	public SoundThemeCustomEffect globalDefaultCustomEffect;

	public SoundThemeCustomEffect globalForcedOverrideCustomEffect;

	public float globalVolume = 1f;

	public static SoundThemeManager Instance
	{
		get
		{
			if (instance == null && !ApplicationUtilities.HasShutdown)
			{
				GameObject gameObject = new GameObject();
				instance = gameObject.AddComponent<SoundThemeManager>();
				instance.globalVolume = 1f;
				gameObject.name = "Sound Theme Manager";
			}
			return instance;
		}
	}

	public void Awake()
	{
		if ((bool)instance)
		{
			Object.DestroyImmediate(instance);
		}
		instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}
	}

	public static void SetAllLoopingSoundsPaused(bool shouldBePaused)
	{
		Object[] array = Object.FindObjectsOfType(typeof(SoundThemeCustomEffect));
		Object[] array2 = array;
		foreach (Object @object in array2)
		{
			SoundThemeCustomEffect soundThemeCustomEffect = (SoundThemeCustomEffect)@object;
			if (soundThemeCustomEffect.sfxEvent.loop && soundThemeCustomEffect.isPlaying)
			{
				soundThemeCustomEffect.paused = shouldBePaused;
			}
		}
	}

	public SoundTheme GetSharedThemeForPlayer(SoundThemePlayer thePlayer)
	{
		if (thePlayer.events == null || thePlayer.events.Length <= 0)
		{
			return thePlayer.baseTheme;
		}
		string text = thePlayer.name.Replace("(Clone)", string.Empty);
		foreach (Transform item in base.transform)
		{
			if (item.name == text)
			{
				SoundTheme component = item.GetComponent<SoundTheme>();
				if ((bool)component && component.baseTheme == thePlayer.baseTheme && component.events.Length == thePlayer.events.Length)
				{
					return component;
				}
			}
		}
		GameObject gameObject = new GameObject(text);
		SoundTheme soundTheme = gameObject.AddComponent<SoundTheme>();
		gameObject.transform.parent = instance.transform;
		soundTheme.events = thePlayer.events;
		soundTheme.baseTheme = thePlayer.baseTheme;
		return soundTheme;
	}
}
