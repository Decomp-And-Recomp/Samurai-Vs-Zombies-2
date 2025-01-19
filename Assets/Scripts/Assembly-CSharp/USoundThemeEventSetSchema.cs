using System.Collections.Generic;
using UnityEngine;

[DataBundleClass(Category = "Audio")]
public class USoundThemeEventSetSchema
{
	[DataBundleKey(Schema = typeof(DynamicEnum), Table = "SoundThemeEnum")]
	[DataBundleField(ColumnWidth = 300)]
	public DataBundleRecordKey type;

	[DataBundleField(ColumnWidth = 300, StaticResource = true)]
	public AudioClip singleClip;

	[DataBundleField(ColumnWidth = 200)]
	[DataBundleSchemaFilter(typeof(USoundThemeClipsetSchema), false)]
	public DataBundleRecordTable clipset;

	[DataBundleField(ColumnWidth = 60)]
	[DataBundleDefaultValue(1f)]
	public float volume;

	[DataBundleField(ColumnWidth = 80)]
	[DataBundleDefaultValue(1f)]
	public float basePitch;

	[DataBundleField(ColumnWidth = 100)]
	public float pitchVariance;

	[DataBundleField(ColumnWidth = 80)]
	public float percentSilent;

	[DataBundleField(ColumnWidth = 60, TooltipInfo = "0 = no limit, less than zero keeps the oldest and don't play the newest, more than zero plays the newest and kills the oldest.")]
	public int playLimit;

	[DataBundleField(ColumnWidth = 50)]
	public bool loop;

	[DataBundleField(ColumnWidth = 110, TooltipInfo = "Only uses positive numbers and Max needs to be larger than Min.")]
	public float minDistanceOverride;

	[DataBundleField(ColumnWidth = 110, TooltipInfo = "Only uses positive numbers and Max needs to be larger than Min.")]
	public float maxDistanceOverride;

	[DataBundleField(ColumnWidth = 100)]
	public bool logarithmicRolloff;

	[DataBundleField(ColumnWidth = 80)]
	public float startDelay;

	[DataBundleField(ColumnWidth = 80)]
	public float fadeInTime;

	[DataBundleField(ColumnWidth = 80)]
	public float fadeOutTime;

	[DataBundleField(ColumnWidth = 110, TooltipInfo = "0 to 360, Unity default is 0.")]
	public float stereoSpread;

	[DataBundleDefaultValue(1f)]
	[DataBundleField(ColumnWidth = 110, TooltipInfo = "0 to 5, Unity default is 1.")]
	public float dopplerLevel;

	[DataBundleField(ColumnWidth = 50, TooltipInfo = "A USoundThemePlayer can only play one voice event at a time.")]
	public bool isVoice;

	[DataBundleDefaultValue(128)]
	[DataBundleField(ColumnWidth = 60, TooltipInfo = "0 = highest and 255 = lowest, Unity default is 128.")]
	public int priority;

	[DataBundleSchemaFilter(typeof(USoundBusSchema), false)]
	[DataBundleField(ColumnWidth = 200)]
	public DataBundleRecordKey bus;

	[DataBundleField(ColumnWidth = 100)]
	public bool excludeOnLowEnd;

	protected List<USoundThemeEventClip> activeClips = new List<USoundThemeEventClip>();

	protected USoundThemeSetSchema soundTheme;

	protected int busNumber;

	public USoundThemeSetSchema SoundTheme
	{
		get
		{
			return soundTheme;
		}
	}

	public List<USoundThemeEventClip> ActiveClips
	{
		get
		{
			return activeClips;
		}
	}

	public int BusNumber
	{
		get
		{
			return busNumber;
		}
	}

	public void Init(USoundThemeSetSchema theme)
	{
		soundTheme = theme;
		busNumber = SingletonSpawningMonoBehaviour<USoundThemeManager>.Instance.GetBusNumber(bus.Key);
	}

	public void PauseAllActiveClips(bool paused)
	{
		foreach (USoundThemeEventClip activeClip in activeClips)
		{
			if ((bool)activeClip)
			{
				activeClip.Paused = paused;
			}
		}
	}

	public void UpdateActiveClips()
	{
		for (int num = activeClips.Count - 1; num >= 0; num--)
		{
			activeClips[num].UpdateClip();
			if (activeClips[num].IsDead)
			{
				activeClips.RemoveAt(num);
			}
		}
	}

	public bool CanPlayEvent()
	{
		if (Random.Range(0f, 1f) < percentSilent)
		{
			return false;
		}
		if (playLimit == 0)
		{
			return true;
		}
		if (playLimit > 0 && activeClips.Count >= playLimit)
		{
			if (activeClips[0] != null)
			{
				activeClips[0].Stop();
			}
		}
		else if (playLimit < 0 && activeClips.Count >= -playLimit)
		{
			return false;
		}
		return true;
	}

	public static implicit operator bool(USoundThemeEventSetSchema obj)
	{
		return obj != null;
	}
}
