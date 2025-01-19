using System;

public class WeakPtr<T> where T : IWeakable
{
	private WeakReference targetRef;

	public T ptr
	{
		get
		{
			if (targetRef != null && targetRef.IsAlive && ((IWeakable)targetRef.Target).weakSelfReference != null && ((IWeakable)targetRef.Target).weakSelfReference.IsAlive)
			{
				return (T)((IWeakable)targetRef.Target).weakSelfReference.Target;
			}
			return default(T);
		}
		set
		{
			targetRef = new WeakReference(value);
		}
	}

	public WeakPtr()
	{
	}

	public WeakPtr(T ptr)
	{
		targetRef = new WeakReference(ptr);
	}
}
