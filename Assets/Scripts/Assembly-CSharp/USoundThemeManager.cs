using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Audio/Udaman Sound Theme Manager")]
public class USoundThemeManager : SingletonSpawningMonoBehaviour<USoundThemeManager>
{
	public static int kDefaultBus;

	protected SortedList<string, USoundThemeSetSchema> activeSoundThemes = new SortedList<string, USoundThemeSetSchema>();

	protected UdamanSoundThemePlayer player;

	protected DataBundleTableHandle<USoundBusSchema> busesTable;

	protected bool paused;

	protected bool busesInited;

	protected uint gameplayBusesMask;

	protected Transform tran;

	protected int mResourceLevel;

	public Transform TransformCached
	{
		get
		{
			return tran;
		}
	}

	public UdamanSoundThemePlayer USoundThemePlayer
	{
		get
		{
			return player;
		}
	}

	public void SetResourceLevel(int level)
	{
		mResourceLevel = level;
	}

	protected override void Awake()
	{
		base.Awake();
		tran = base.transform;
		if (DataBundleRuntime.Instance != null)
		{
			InitBuses();
		}
		player = base.gameObject.AddComponent<UdamanSoundThemePlayer>();
	}

	protected void InitBuses()
	{
		busesTable = new DataBundleTableHandle<USoundBusSchema>("SoundEventBuses");
		gameplayBusesMask = (uint)(1 << kDefaultBus);
		for (int i = 0; i < busesTable.Data.Length; i++)
		{
			if (busesTable.Data[i].pauseWithGameplay)
			{
				gameplayBusesMask += (uint)(1 << i + 1);
			}
		}
		busesInited = true;
	}

	public void Update()
	{
		foreach (KeyValuePair<string, USoundThemeSetSchema> activeSoundTheme in activeSoundThemes)
		{
			if ((bool)activeSoundTheme.Value)
			{
				activeSoundTheme.Value.UpdateLoadedEvents();
			}
		}
	}

	public void PauseSoundThemes(bool pause, uint busMask = 0u)
	{
		if (!busesInited)
		{
			InitBuses();
		}
		if ((!pause || paused) && (pause || !paused))
		{
			return;
		}
		if (busMask == 0)
		{
			busMask = gameplayBusesMask;
		}
		foreach (KeyValuePair<string, USoundThemeSetSchema> activeSoundTheme in activeSoundThemes)
		{
			if ((bool)activeSoundTheme.Value)
			{
				activeSoundTheme.Value.PauseEvents(pause, busMask);
			}
		}
		paused = pause;
	}

	public bool IsBusPaused(int busNumber)
	{
		if (!paused)
		{
			return false;
		}
		return ((uint)(1 << busNumber) & gameplayBusesMask) != 0;
	}

	public int GetBusNumber(string busName)
	{
		if (!busesInited)
		{
			InitBuses();
		}
		for (int i = 0; i < busesTable.Data.Length; i++)
		{
			if (busesTable.Data[i].busName == busName)
			{
				return i + 1;
			}
		}
		return kDefaultBus;
	}

	public USoundThemeSetSchema GetSoundTheme(DataBundleRecordKey soundThemeKey)
	{
		if (DataBundleRecordKey.IsNullOrEmpty(soundThemeKey))
		{
			return null;
		}
		USoundThemeSetSchema value = null;
		if (activeSoundThemes.TryGetValue(soundThemeKey.Key, out value))
		{
			if (!value.isLoaded)
			{
				value.Init();
				value.ResourceLevel = mResourceLevel;
			}
			return value;
		}
		if (DataBundleRuntime.Instance == null)
		{
			return null;
		}
		USoundThemeSetSchema uSoundThemeSetSchema = DataBundleRuntime.Instance.InitializeRecord<USoundThemeSetSchema>(soundThemeKey);
		if ((bool)uSoundThemeSetSchema)
		{
			activeSoundThemes.Add(soundThemeKey.Key, uSoundThemeSetSchema);
			uSoundThemeSetSchema.Init();
			uSoundThemeSetSchema.ResourceLevel = mResourceLevel;
			return uSoundThemeSetSchema;
		}
		return null;
	}

	public void UnloadSoundTheme(DataBundleRecordKey soundThemeKey)
	{
		if (!DataBundleRecordKey.IsNullOrEmpty(soundThemeKey))
		{
			USoundThemeSetSchema value = null;
			if (activeSoundThemes.TryGetValue(soundThemeKey.Key, out value))
			{
				value.Unload();
				activeSoundThemes.Remove(soundThemeKey.Key);
			}
		}
	}

	public void UnloadResourceLevel(int level)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, USoundThemeSetSchema> activeSoundTheme in activeSoundThemes)
		{
			if (activeSoundTheme.Value == null || activeSoundTheme.Value.ResourceLevel == level)
			{
				list.Add(activeSoundTheme.Key);
				if (activeSoundTheme.Value != null)
				{
					activeSoundTheme.Value.Unload();
				}
			}
		}
		foreach (string item in list)
		{
			activeSoundThemes.Remove(item);
		}
	}
}
