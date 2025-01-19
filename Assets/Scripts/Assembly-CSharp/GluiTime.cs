using UnityEngine;

public class GluiTime : WeakGlobalMonoBehavior<GluiTime>
{
	private const float kMaxDeltaTime = 0.1f;

	private static float mLastRecordedTime;

	private static float mCurrentDeltaTime = -1f;

	public static float deltaTime
	{
		get
		{
			if (mCurrentDeltaTime > 0f)
			{
				return mCurrentDeltaTime;
			}
			return Time.deltaTime;
		}
	}

	private void Start()
	{
		SetUniqueInstance(this);
	}

	private void Update()
	{
		mCurrentDeltaTime = Mathf.Min(0.1f, Time.realtimeSinceStartup - mLastRecordedTime);
		mLastRecordedTime = Time.realtimeSinceStartup;
	}
}
