using System;
using System.Collections.Generic;

public class PlayModesManager : Singleton<PlayModesManager>
{
	public enum GameDirection
	{
		LeftToRight = 0,
		RightToLeft = 1
	}

	private string mModeID;

	private PlayModeSchema[] mData;

	private PlayModeSchema mSelectedModeData;

	private List<KeyValuePair<string, string>> mPathSubstitutions = new List<KeyValuePair<string, string>>();

	private GameDirection mGameDirection;

	public static string UdamanTableName
	{
		get
		{
			return "PlayModes";
		}
	}

	public string selectedMode
	{
		get
		{
			return mModeID;
		}
		set
		{
			if (!(mModeID == value))
			{
				mModeID = value;
				ReloadData();
				Singleton<Profile>.Instance.PostLoadSyncing();
			}
		}
	}

	public PlayModeSchema selectedModeData
	{
		get
		{
			return mSelectedModeData;
		}
	}

	public PlayModeSchema[] allModes
	{
		get
		{
			return mData;
		}
	}

	public string revivePotionID
	{
		get
		{
			return mSelectedModeData.revivePotion.Key;
		}
	}

	public GameDirection gameDirection
	{
		get
		{
			return mGameDirection;
		}
	}

	public bool Attacking { get; set; }

	public PlayModesManager()
	{
		mModeID = "classic";
		ReloadData();
	}

	public PlayModeSchema GetModeData(string id)
	{
		return Array.Find(mData, (PlayModeSchema m) => m.id.Key == id);
	}

	public string ApplyPathSubstitutions(string path)
	{
		foreach (KeyValuePair<string, string> mPathSubstitution in mPathSubstitutions)
		{
			if (path.Contains(mPathSubstitution.Key))
			{
				return path.Replace(mPathSubstitution.Key, mPathSubstitution.Value);
			}
		}
		return path;
	}

	private void ReloadData()
	{
		if (DataBundleRuntime.Instance != null && DataBundleRuntime.Instance.Initialized)
		{
			mData = DataBundleUtils.InitializeRecords<PlayModeSchema>(UdamanTableName);
			PlayModeSchema[] array = mData;
			foreach (PlayModeSchema playModeSchema in array)
			{
				playModeSchema.Initialize(UdamanTableName);
			}
			mSelectedModeData = GetModeData(mModeID);
			if (mSelectedModeData == null)
			{
				mSelectedModeData = mData[0];
			}
			InitPathSubstitutions();
			DetermineGameDirection();
		}
	}

	private void InitPathSubstitutions()
	{
		mPathSubstitutions.Clear();
	}

	public void DetermineGameDirection()
	{
		if (Singleton<Profile>.Instance.inVSMultiplayerWave && Attacking)
		{
			mGameDirection = GameDirection.RightToLeft;
		}
		else
		{
			mGameDirection = (mSelectedModeData.rightToLeft ? GameDirection.RightToLeft : GameDirection.LeftToRight);
		}
	}
}
