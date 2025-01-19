using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerWaveEventData : Singleton<PlayerWaveEventData>
{
	private static float kLegendaryStrikeDamageThreshold = 100f;

	private List<PlayerWaveEvent> mPlaybackEvents;

	private List<PlayerWaveEvent> mRecordEvents;

	private int mPlaybackEventIndex;

	private float mPlaybackTime;

	private int mPreviewEventIndex;

	private float mPreviewTime;

	private bool mIsPlayingBack;

	private float mAccumulatedDamage;

	public static readonly string[] LegendaryStrikeID = new string[4] { "LSArrowVolley", "LSLightningStrike", "LSRaiseDead", "LSTornado" };

	public bool LegendaryStrikeAvailable
	{
		get
		{
			return mAccumulatedDamage > kLegendaryStrikeDamageThreshold;
		}
	}

	public float LegendaryStrikeChargePercent
	{
		get
		{
			return mAccumulatedDamage / kLegendaryStrikeDamageThreshold;
		}
		set
		{
			mAccumulatedDamage = kLegendaryStrikeDamageThreshold * value;
		}
	}

	public PlayerWaveEventData()
	{
		mPlaybackEvents = new List<PlayerWaveEvent>();
		mRecordEvents = new List<PlayerWaveEvent>();
		kLegendaryStrikeDamageThreshold = Mathf.Max(1f, SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable("LegendaryStrikeAccumDamage", 1f));
	}

	public void StartWave()
	{
		mPlaybackTime = 0f;
		mPlaybackEventIndex = 0;
		mPreviewTime = 2f;
		mPreviewEventIndex = 0;
		mRecordEvents.Clear();
		mAccumulatedDamage = 0f;
	}

	public void Reset()
	{
		mPlaybackEvents.Clear();
		mRecordEvents.Clear();
		mAccumulatedDamage = 0f;
		mIsPlayingBack = false;
	}

	public void Update(float time)
	{
		mPlaybackTime += time;
		if (mIsPlayingBack)
		{
			while (mPlaybackEventIndex < mPlaybackEvents.Count && mPlaybackEvents[mPlaybackEventIndex].eventTime <= mPlaybackTime)
			{
				if (mPlaybackEvents[mPlaybackEventIndex].eventType == EPlayerWaveEvent.kLegendaryStrike)
				{
					Hero.DoLegendaryStrike(LegendaryStrikeID[mPlaybackEvents[mPlaybackEventIndex].eventData], 1);
				}
				mPlaybackEventIndex++;
			}
		}
		mPreviewTime += time;
		if (!mIsPlayingBack)
		{
			return;
		}
		while (mPreviewEventIndex < mPlaybackEvents.Count && mPlaybackEvents[mPreviewEventIndex].eventTime <= mPreviewTime)
		{
			if (mPlaybackEvents[mPlaybackEventIndex].eventType == EPlayerWaveEvent.kLegendaryStrike)
			{
				LSTriggerBanner bannerToAdd = new LSTriggerBanner(3f);
				WeakGlobalMonoBehavior<BannerManager>.Instance.OpenBanner(bannerToAdd);
			}
			mPreviewEventIndex++;
		}
	}

	public void AddLegendaryStrike(ELegendaryStrike strike)
	{
		PlayerWaveEvent item = default(PlayerWaveEvent);
		item.eventType = EPlayerWaveEvent.kLegendaryStrike;
		item.eventData = (int)strike;
		item.eventTime = mPlaybackTime;
		mRecordEvents.Add(item);
		if (Singleton<PlayModesManager>.Instance.Attacking || SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable("LegendaryStrikesHelpDefendObjects", 1) > 0)
		{
			LSSendBanner bannerToAdd = new LSSendBanner(3f);
			WeakGlobalMonoBehavior<BannerManager>.Instance.OpenBanner(bannerToAdd);
		}
	}

	public byte[] Pack()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (PlayerWaveEvent mRecordEvent in mRecordEvents)
		{
			binaryWriter.Write((byte)mRecordEvent.eventType);
			binaryWriter.Write((byte)mRecordEvent.eventData);
			binaryWriter.Write(mRecordEvent.eventTime);
		}
		return memoryStream.GetBuffer();
	}

	public void Unpack(byte[] bytes)
	{
		StartWave();
		mIsPlayingBack = true;
		if (bytes == null)
		{
			return;
		}
		MemoryStream input = new MemoryStream(bytes);
		BinaryReader binaryReader = new BinaryReader(input);
		while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length - 6)
		{
			PlayerWaveEvent item = default(PlayerWaveEvent);
			item.eventType = (EPlayerWaveEvent)binaryReader.ReadByte();
			item.eventData = binaryReader.ReadByte();
			item.eventTime = binaryReader.ReadSingle();
			if (item.eventTime > 0f)
			{
				mPlaybackEvents.Add(item);
			}
		}
	}

	public void AccumulateDamage(float damage)
	{
		mAccumulatedDamage += damage;
	}

	public void ResetAccumulatedDamage()
	{
		mAccumulatedDamage = 0f;
	}
}
