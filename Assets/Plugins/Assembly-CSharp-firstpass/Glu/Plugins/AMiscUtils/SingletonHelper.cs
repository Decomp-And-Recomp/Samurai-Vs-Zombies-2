using System;
using UnityEngine;

namespace Glu.Plugins.AMiscUtils
{
	public struct SingletonHelper<T> where T : Component
	{
		private bool initialized;

		private T instance;

		public T Instance
		{
			get
			{
				AssertInitialized();
				return instance;
			}
			set
			{
				value.ArgumentNotNull("value");
				instance = value;
			}
		}

		public bool IsInitialized
		{
			get
			{
				return initialized;
			}
		}

		public void Initialize()
		{
			if (initialized)
			{
				string message = "Can initialize {0} only once".Fmt(typeof(T));
				throw new InvalidOperationException(message).Throw();
			}
			initialized = true;
		}

		public void Destroy()
		{
			initialized = false;
			instance = (T)null;
		}

		public void AssertInitialized()
		{
			if (!IsInitialized)
			{
				string message = "{0} is not initialized".Fmt(typeof(T));
				throw new InvalidOperationException(message).Throw();
			}
		}
	}
}
