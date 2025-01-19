using System.Collections.Generic;
using UnityEngine;

public class SeededRandomizer
{
	private int mRandSeedToUse;

	private List<object> mClients;

	private List<int> mOldSeeds;

	public int seed
	{
		get
		{
			return mRandSeedToUse;
		}
		private set
		{
			mRandSeedToUse = value;
		}
	}

	public SeededRandomizer(int seed)
	{
		mClients = new List<object>();
		mOldSeeds = new List<int>();
		mRandSeedToUse = seed;
	}

	~SeededRandomizer()
	{
		if (mClients.Count > 0)
		{
			object obj = mClients[mClients.Count - 1];
		}
	}

	public void PushRandomSession(object clientObject)
	{
		if (clientObject != null && (mClients.Count <= 0 || clientObject != mClients[mClients.Count - 1]))
		{
			mOldSeeds.Add(Random.seed);
			mClients.Add(clientObject);
			Random.seed = mRandSeedToUse;
		}
	}

	public void PopRandomSession(object clientObject)
	{
		if (clientObject != null && (mClients.Count <= 0 || mClients[mClients.Count - 1] == clientObject))
		{
			Random.seed = mOldSeeds[mOldSeeds.Count - 1];
			mOldSeeds.RemoveAt(mOldSeeds.Count - 1);
			mClients.RemoveAt(mClients.Count - 1);
		}
	}

	public float NextRand(object clientObject)
	{
		if (clientObject == null)
		{
			return -1f;
		}
		if (mClients.Count > 0 && mClients[mClients.Count - 1] != clientObject)
		{
			return -1f;
		}
		return Random.value;
	}

	public int NextRange(int min, int max, object clientObject)
	{
		if (clientObject == null)
		{
			return -1;
		}
		if (mClients.Count > 0 && mClients[mClients.Count - 1] != clientObject)
		{
			return -1;
		}
		return Random.Range(min, max);
	}
}
