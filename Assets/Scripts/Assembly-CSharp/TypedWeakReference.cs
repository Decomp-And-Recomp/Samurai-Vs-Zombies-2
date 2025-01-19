using System;

public class TypedWeakReference<T>
{
	private WeakReference weakRef;

	public T ptr
	{
		get
		{
			if (weakRef != null)
			{
				return (T)weakRef.Target;
			}
			return default(T);
		}
		set
		{
			weakRef = new WeakReference(value);
		}
	}

	public TypedWeakReference()
	{
	}

	public TypedWeakReference(T ptr)
	{
		weakRef = new WeakReference(ptr);
	}
}
