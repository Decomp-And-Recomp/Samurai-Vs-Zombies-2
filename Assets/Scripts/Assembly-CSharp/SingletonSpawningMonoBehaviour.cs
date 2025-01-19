using System;
using UnityEngine;

public class SingletonSpawningMonoBehaviour<T> : MonoBehaviour where T : SingletonSpawningMonoBehaviour<T>
{
	private static SingletonSpawningMonoBehaviour<T> uniqueInstance;

	protected static bool applicationQuitting;

	public static T Instance
	{
		get
		{
			if (!Exists)
			{
				if (applicationQuitting || !Application.isPlaying)
				{
					return (T)null;
				}
				GameObject gameObject = new GameObject("Singleton " + typeof(T).ToString(), typeof(T));
				uniqueInstance = gameObject.GetComponent<T>();
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				Exists = true;
			}
			return (T)uniqueInstance;
		}
	}

	public static bool Exists { get; private set; }

	protected virtual void Awake()
	{
		if (uniqueInstance == null)
		{
			uniqueInstance = this;
		}
		else if (uniqueInstance != this)
		{
			throw new InvalidOperationException("Cannot have two instances of a SingletonMonoBehaviour : " + typeof(T).ToString() + ".");
		}
	}

	protected virtual void OnApplicationQuit()
	{
		applicationQuitting = true;
	}

	protected virtual void OnDestroy()
	{
		if (uniqueInstance == this)
		{
			Exists = false;
			uniqueInstance = null;
		}
	}
}
