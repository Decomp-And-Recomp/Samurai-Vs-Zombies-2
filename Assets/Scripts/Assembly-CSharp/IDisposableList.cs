using System;
using System.Collections.Generic;

public class IDisposableList
{
	private List<IDisposable> disposables = new List<IDisposable>();

	public void Add(IDisposable disposable)
	{
		disposables.Add(disposable);
	}

	public void DisposeAll()
	{
		foreach (IDisposable disposable in disposables)
		{
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		disposables = new List<IDisposable>();
	}
}
