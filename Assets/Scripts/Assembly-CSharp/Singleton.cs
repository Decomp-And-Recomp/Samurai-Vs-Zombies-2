using System;

public class Singleton<T> where T : class, new()
{
	private static T mUniqueInstance;

	public static T Instance
	{
		get
		{
			if (mUniqueInstance == null)
			{
				mUniqueInstance = new T();
			}
			return mUniqueInstance;
		}
	}

	public static bool Exists
	{
		get
		{
			return mUniqueInstance != null;
		}
	}

	public Singleton()
	{
		if (mUniqueInstance != null)
		{
			throw new InvalidOperationException(string.Concat("Singleton [", typeof(T), "] cannot be manually instantiated."));
		}
	}
}
