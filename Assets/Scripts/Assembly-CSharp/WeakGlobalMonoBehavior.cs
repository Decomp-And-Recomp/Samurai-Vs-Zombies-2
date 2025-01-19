using UnityEngine;

public class WeakGlobalMonoBehavior<T> : MonoBehaviour
{
	private static TypedWeakReference<T> mUniqueInstance;

	public static T Instance
	{
		get
		{
			if (mUniqueInstance == null)
			{
				return default(T);
			}
			return mUniqueInstance.ptr;
		}
	}

	public static bool Exists
	{
		get
		{
			return mUniqueInstance != null && mUniqueInstance.ptr != null;
		}
	}

	protected void SetUniqueInstance(T ptr)
	{
		mUniqueInstance = new TypedWeakReference<T>(ptr);
	}
}
