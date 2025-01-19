using System;
using UnityEngine;

public class DisposableMonoBehaviour : MonoBehaviour, IDisposable
{
	private bool disposed;

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	~DisposableMonoBehaviour()
	{
		Dispose(false);
	}

	public void Dispose(bool isDisposing)
	{
		if (!disposed)
		{
			UnityThreadHelper.CallOnMainThread(delegate
			{
				OnDispose(isDisposing);
			});
			disposed = true;
		}
	}

	protected virtual void OnDispose(bool isDisposing)
	{
		if (isDisposing && !ApplicationUtilities.HasShutdown && base.gameObject != null)
		{
			ObjectUtils.DestroyImmediate(base.gameObject);
		}
	}
}
