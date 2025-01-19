using System;
using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
	private static SingletonMonoBehaviour<T> uniqueInstance;

	public static T Instance
	{
		get
		{
			if (!Exists)
			{
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
			Exists = true;
		}
		else if (uniqueInstance != this)
		{
			throw new InvalidOperationException("Cannot have two instances of a SingletonMonoBehaviour : " + typeof(T).ToString() + ".");
		}
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
