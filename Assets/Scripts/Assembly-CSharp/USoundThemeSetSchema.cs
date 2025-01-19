using System.Collections.Generic;
using UnityEngine;

[DataBundleClass(Category = "Audio")]
public class USoundThemeSetSchema
{
	[DataBundleKey]
	[DataBundleField(ColumnWidth = 300)]
	public string ThemeName;

	[DataBundleSchemaFilter(typeof(USoundThemeSetSchema), false)]
	[DataBundleField(ColumnWidth = 300)]
	public DataBundleRecordKey parentThemeKey;

	[DataBundleField(ColumnWidth = 300)]
	[DataBundleSchemaFilter(typeof(USoundThemeEventSetSchema), false)]
	public DataBundleRecordTable events;

	protected USoundThemeSetSchema parentTheme;

	protected List<USoundThemeEventSetSchema> playingEvents = new List<USoundThemeEventSetSchema>();

	protected Dictionary<string, DataBundleRecordHandle<USoundThemeEventSetSchema>> mLoadedEventSets;

	protected Dictionary<string, DataBundleRecordHandle<USoundThemeClipsetSchema>[]> mLoadedClipsets;

	protected int mResourceLevel;

	protected int mChildCount;

	protected bool mLoaded;

	protected int mSizeInMemory;

	public Dictionary<string, DataBundleRecordHandle<USoundThemeEventSetSchema>> LoadedEventSets
	{
		get
		{
			return mLoadedEventSets;
		}
	}

	public Dictionary<string, DataBundleRecordHandle<USoundThemeClipsetSchema>[]> LoadedClipsets
	{
		get
		{
			return mLoadedClipsets;
		}
	}

	public int ResourceLevel
	{
		get
		{
			return mResourceLevel;
		}
		set
		{
			mResourceLevel = value;
		}
	}

	public USoundThemeSetSchema ParentTheme
	{
		get
		{
			return parentTheme;
		}
	}

	public bool isLoaded
	{
		get
		{
			return mLoaded;
		}
	}

	public void AddChild()
	{
		mChildCount++;
	}

	public void ChildUnloaded()
	{
		mChildCount--;
		if (mChildCount <= 0)
		{
			Unload();
		}
	}

	public void Init()
	{
		if (mLoadedClipsets == null)
		{
			mLoadedClipsets = new Dictionary<string, DataBundleRecordHandle<USoundThemeClipsetSchema>[]>();
			mLoadedEventSets = new Dictionary<string, DataBundleRecordHandle<USoundThemeEventSetSchema>>();
		}
		mSizeInMemory = 0;
		List<AudioClip> list = new List<AudioClip>();
		if (!DataBundleRecordTable.IsNullOrEmpty(events))
		{
			bool flag = false;
			foreach (USoundThemeEventSetSchema item in events.EnumerateRecords<USoundThemeEventSetSchema>())
			{
				DynamicEnum dynamicEnum = DataBundleRuntime.Instance.InitializeRecord<DynamicEnum>(item.type);
				if (dynamicEnum == null)
				{
					continue;
				}
				string tableRecordKey = DataBundleRuntime.TableRecordKey(events.RecordTable, dynamicEnum.value);
				DataBundleRecordHandle<USoundThemeEventSetSchema> dataBundleRecordHandle = new DataBundleRecordHandle<USoundThemeEventSetSchema>(tableRecordKey);
				if (flag && dataBundleRecordHandle.Data.excludeOnLowEnd)
				{
					continue;
				}
				dataBundleRecordHandle.Load(null);
				mLoadedEventSets[dynamicEnum.value] = dataBundleRecordHandle;
				List<DataBundleRecordHandle<USoundThemeClipsetSchema>> list2 = new List<DataBundleRecordHandle<USoundThemeClipsetSchema>>();
				int recordTableLength = DataBundleRuntime.Instance.GetRecordTableLength(typeof(USoundThemeClipsetSchema), item.clipset);
				for (int i = 0; i < recordTableLength; i++)
				{
					string tableRecordKey2 = DataBundleRuntime.TableRecordKey(item.clipset, DataBundleRuntime.Instance.GetRecordKeys(typeof(USoundThemeClipsetSchema), item.clipset, false)[i]);
					DataBundleRecordHandle<USoundThemeClipsetSchema> dataBundleRecordHandle2 = new DataBundleRecordHandle<USoundThemeClipsetSchema>(tableRecordKey2);
					if (!flag || !dataBundleRecordHandle2.Data.excludeOnLowEnd)
					{
						if (!dataBundleRecordHandle2.Data.dontPreloadSound)
						{
							dataBundleRecordHandle2.Load(null);
						}
						list2.Add(dataBundleRecordHandle2);
					}
				}
				mLoadedClipsets[dynamicEnum.value] = list2.ToArray();
			}
		}
		mLoaded = true;
		if (string.IsNullOrEmpty(parentThemeKey))
		{
			return;
		}
		parentTheme = SingletonSpawningMonoBehaviour<USoundThemeManager>.Instance.GetSoundTheme(parentThemeKey);
		foreach (KeyValuePair<string, DataBundleRecordHandle<USoundThemeClipsetSchema>[]> loadedClipset in parentTheme.LoadedClipsets)
		{
			if (!mLoadedClipsets.ContainsKey(loadedClipset.Key))
			{
				mLoadedClipsets[loadedClipset.Key] = loadedClipset.Value;
			}
		}
		foreach (KeyValuePair<string, DataBundleRecordHandle<USoundThemeEventSetSchema>> loadedEventSet in parentTheme.LoadedEventSets)
		{
			if (!mLoadedEventSets.ContainsKey(loadedEventSet.Key))
			{
				mLoadedEventSets[loadedEventSet.Key] = loadedEventSet.Value;
			}
		}
		parentTheme.AddChild();
	}

	public void Unload()
	{
		mLoaded = false;
		if (parentTheme != null)
		{
			parentTheme.ChildUnloaded();
		}
		foreach (KeyValuePair<string, DataBundleRecordHandle<USoundThemeClipsetSchema>[]> mLoadedClipset in mLoadedClipsets)
		{
			DataBundleRecordHandle<USoundThemeClipsetSchema>[] value = mLoadedClipset.Value;
			foreach (DataBundleRecordHandle<USoundThemeClipsetSchema> dataBundleRecordHandle in value)
			{
				dataBundleRecordHandle.Dispose();
			}
		}
		foreach (KeyValuePair<string, DataBundleRecordHandle<USoundThemeEventSetSchema>> mLoadedEventSet in mLoadedEventSets)
		{
			mLoadedEventSet.Value.Dispose();
		}
		mLoadedClipsets.Clear();
		mLoadedEventSets.Clear();
	}

	public void UpdateLoadedEvents()
	{
		foreach (USoundThemeEventSetSchema playingEvent in playingEvents)
		{
			if (playingEvent != null)
			{
				playingEvent.UpdateActiveClips();
			}
		}
	}

	public void PauseEvents(bool pause, uint busMask = 65535)
	{
		foreach (USoundThemeEventSetSchema playingEvent in playingEvents)
		{
			if (playingEvent != null && ((uint)(1 << playingEvent.BusNumber) & busMask) != 0)
			{
				playingEvent.PauseAllActiveClips(pause);
			}
		}
	}

	public void AddPlayingEvent(USoundThemeEventSetSchema soundEvent)
	{
		if (!playingEvents.Contains(soundEvent))
		{
			playingEvents.Add(soundEvent);
		}
	}

	public static implicit operator bool(USoundThemeSetSchema obj)
	{
		return obj != null;
	}
}
