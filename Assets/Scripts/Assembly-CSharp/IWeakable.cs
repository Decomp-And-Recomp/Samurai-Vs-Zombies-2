using System;

public interface IWeakable
{
	WeakReference weakSelfReference { get; set; }
}
