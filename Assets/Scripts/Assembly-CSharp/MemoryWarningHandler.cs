using System;
using System.Collections;
using UnityEngine;

public class MemoryWarningHandler : MonoBehaviour
{
	public bool trackMemoryOnStart;

	public bool unloadOnMemoryWarning;

	private static MemoryWarningHandler smInstance;

	private float mBaseMegabytes;

	private bool mTrackingMemory;

	public static MemoryWarningHandler Instance
	{
		get
		{
			return smInstance;
		}
	}

	public bool TrackMemory
	{
		get
		{
			return mTrackingMemory;
		}
		set
		{
			if (mTrackingMemory != value)
			{
				mTrackingMemory = value;
				if (mTrackingMemory)
				{
					long num = NUF.CurrentUsedMemory();
					float num2 = (float)((double)num / 1048576.0);
					mBaseMegabytes = (float)(int)(num2 * 100f) / 100f;
				}
			}
		}
	}

	public static void CreateInstance()
	{
		if (Instance == null && !ApplicationUtilities.HasShutdown)
		{
			GameObject gameObject = new GameObject("MemoryWarningHandler");
			gameObject.AddComponent<MemoryWarningHandler>();
		}
	}

	public static void DestroyInstance()
	{
		if (Instance != null)
		{
			UnityEngine.Object.Destroy(Instance.gameObject);
		}
	}

	public IEnumerator FreeMemory()
	{
		float prevMB = CalcCurrentMegabytesInUse();
		AsyncOperation unloadOp = Resources.UnloadUnusedAssets();
		while (!unloadOp.isDone)
		{
			yield return unloadOp;
		}
		if (!Application.isEditor)
		{
			float currMB = CalcCurrentMegabytesInUse();
		}
	}

	public IEnumerator GCCollect(int level)
	{
		yield return null;
		GC.Collect(level);
	}

	private void Awake()
	{
		if (smInstance == null)
		{
			smInstance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	private void Start()
	{
		NUF.StartMemoryWarningHandler(base.gameObject.name, "MemoryWarning");
		TrackMemory = trackMemoryOnStart;
	}

	private void Update()
	{
		if (TrackMemory)
		{
			float num = CalcCurrentMegabytesInUse();
		}
	}

	private void OnDestroy()
	{
		if (smInstance == this)
		{
			smInstance = null;
		}
	}

	private void MemoryWarning(string message)
	{
		if (unloadOnMemoryWarning)
		{
			StartCoroutine(FreeMemory());
		}
	}

	private static float CalcCurrentMegabytesInUse()
	{
		long num = NUF.CurrentUsedMemory();
		float num2 = (float)((double)num / 1048576.0);
		return (float)(int)(num2 * 100f) / 100f;
	}
}
