using System;

public class Weakable : IWeakable
{
	public WeakReference weakSelfReference { get; set; }

	public Weakable()
	{
		weakSelfReference = new WeakReference(this);
	}

	public void BreakWeakLinks()
	{
		weakSelfReference = null;
	}
}
