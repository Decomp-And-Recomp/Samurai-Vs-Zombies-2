using System.Collections.Generic;

public class ChaptersDatabase : Singleton<ChaptersDatabase>
{
	private TextDBSchema[] mChapters;

	private string[] mCachedChaptersIDs;

	private Dictionary<string, int[]> mCachedWaveRanges;

	public string[] allChapterIDs
	{
		get
		{
			return mCachedChaptersIDs;
		}
	}

	public ChaptersDatabase()
	{
		Init();
	}

	public void ResetCachedData()
	{
		Init();
	}

	public string GetAttribute(string chapterID, string attribute)
	{
		string key = TextDBSchema.ChildKey(chapterID, attribute);
		return mChapters.GetString(key);
	}

	public int[] GetWavesRange(string chapterID)
	{
		if (mCachedWaveRanges.ContainsKey(chapterID))
		{
			return mCachedWaveRanges[chapterID];
		}
		int[] waveRange = GetWaveRange(chapterID);
		mCachedWaveRanges.Add(chapterID, waveRange);
		return waveRange;
	}

	private void Init()
	{
		mCachedWaveRanges = new Dictionary<string, int[]>();
		if (DataBundleRuntime.Instance != null && DataBundleRuntime.Instance.Initialized)
		{
			mChapters = DataBundleUtils.InitializeRecords<TextDBSchema>("Chapters");
			int num = DynamicEnum.Count("Chapters");
			mCachedChaptersIDs = new string[num];
			for (int i = 0; i < num; i++)
			{
				mCachedChaptersIDs[i] = mChapters.GetString(TextDBSchema.LevelKey("all", i));
			}
		}
	}

	private int[] GetWaveRange(string chapterID)
	{
		int[] array = new int[2];
		string key = TextDBSchema.ChildKey(chapterID, "wavesRange");
		string[] array2 = mChapters.GetString(key).Split(',');
		if (array2.Length == 2)
		{
			for (int i = 0; i < 2; i++)
			{
				if (!int.TryParse(array2[i], out array[i]))
				{
					array[0] = 0;
					array[1] = 0;
					break;
				}
			}
		}
		return array;
	}
}
