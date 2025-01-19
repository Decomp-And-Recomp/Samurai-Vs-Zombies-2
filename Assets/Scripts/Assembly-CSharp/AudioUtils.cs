using UnityEngine;

public class AudioUtils
{
	private const string kMusicVolumeKey = "MusicVolume";

	private const string kSoundThemeVolumeKey = "SoundThemeVolume";

	private static bool initialized;

	private static float masterMusicVolume = 1f;

	private static float masterSoundThemeVolume = 1f;

	private static float musicVolumePlayer = 1f;

	private static float soundThemeVolumePlayer = 1f;

	private static float musicVolumeDesigner = 1f;

	private static float soundThemeVolumeDesigner = 1f;

	private static float musicVolumeCode = 1f;

	private static float soundThemeVolumeCode = 1f;

	public static float MusicVolumePlayer
	{
		get
		{
			return musicVolumePlayer;
		}
		set
		{
			musicVolumePlayer = Mathf.Clamp(value, 0f, 1f);
			PlayerPrefs.SetFloat("MusicVolume", musicVolumePlayer);
			CalculateMasterMusicVolume();
		}
	}

	public static float SoundThemeVolumePlayer
	{
		get
		{
			return soundThemeVolumePlayer;
		}
		set
		{
			soundThemeVolumePlayer = Mathf.Clamp(value, 0f, 1f);
			PlayerPrefs.SetFloat("SoundThemeVolume", soundThemeVolumePlayer);
			CalculateMasterSoundThemeVolume();
		}
	}

	public static float MusicVolumeDesigner
	{
		get
		{
			return musicVolumeDesigner;
		}
	}

	public static float SoundThemeVolumeDesigner
	{
		get
		{
			return soundThemeVolumeDesigner;
		}
	}

	public static float MusicVolumeCode
	{
		get
		{
			return musicVolumeCode;
		}
		set
		{
			musicVolumeCode = Mathf.Clamp(value, 0f, 1f);
			CalculateMasterMusicVolume();
		}
	}

	public static float SoundThemeVolumeCode
	{
		get
		{
			return soundThemeVolumeCode;
		}
		set
		{
			soundThemeVolumeCode = Mathf.Clamp(value, 0f, 1f);
			CalculateMasterSoundThemeVolume();
		}
	}

	public static float MasterMusicVolume
	{
		get
		{
			if (!initialized)
			{
				InitializeVolumes();
			}
			return masterMusicVolume;
		}
	}

	public static float MasterSoundThemeVolume
	{
		get
		{
			if (!initialized)
			{
				InitializeVolumes();
			}
			return masterSoundThemeVolume;
		}
	}

	private static void InitializeVolumes()
	{
		musicVolumePlayer = Mathf.Clamp(PlayerPrefs.GetFloat("MusicVolume", 1f), 0f, 1f);
		soundThemeVolumePlayer = Mathf.Clamp(PlayerPrefs.GetFloat("SoundThemeVolume", 1f), 0f, 1f);
		musicVolumeDesigner = Mathf.Clamp(SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable("MusicVolume", 1f), 0f, 1f);
		soundThemeVolumeDesigner = Mathf.Clamp(SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable("SoundThemeVolume", 1f), 0f, 1f);
		CalculateMasterMusicVolume();
		CalculateMasterSoundThemeVolume();
		initialized = true;
	}

	private static void CalculateMasterMusicVolume()
	{
		masterMusicVolume = musicVolumePlayer * musicVolumeDesigner * musicVolumeCode;
	}

	private static void CalculateMasterSoundThemeVolume()
	{
		masterSoundThemeVolume = soundThemeVolumePlayer * soundThemeVolumeDesigner * soundThemeVolumeCode;
	}
}
