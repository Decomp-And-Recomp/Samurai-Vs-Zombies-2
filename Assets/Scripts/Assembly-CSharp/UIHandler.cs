using System.Collections.Generic;

public class UIHandler<T> : WeakGlobalMonoBehavior<T> where T : UIHandler<T>
{
	protected List<UIHandlerComponent> mComponents = new List<UIHandlerComponent>();

	private int mUpdateExpensiveVisualsNextIndex;

	public virtual void Awake()
	{
		SetUniqueInstance((T)this);
	}

	public virtual void Start()
	{
	}

	public virtual void Update()
	{
		if (mUpdateExpensiveVisualsNextIndex >= mComponents.Count)
		{
			mUpdateExpensiveVisualsNextIndex = 0;
		}
		int num = 0;
		foreach (UIHandlerComponent mComponent in mComponents)
		{
			mComponent.Update(mUpdateExpensiveVisualsNextIndex == num);
			num++;
		}
		mUpdateExpensiveVisualsNextIndex++;
	}

	public virtual bool OnUIEvent(string eventID)
	{
		foreach (UIHandlerComponent mComponent in mComponents)
		{
			if (mComponent.OnUIEvent(eventID))
			{
				return true;
			}
		}
		return false;
	}

	public void RegisterOnPressEvent(GluiStandardButtonContainer btn, string eventID)
	{
		if (btn != null)
		{
			btn.onPressActions = new string[1] { eventID };
		}
	}

	public void RegisterOnReleaseEvent(GluiStandardButtonContainer btn, string eventID)
	{
		if (btn != null)
		{
			btn.onReleaseActions = new string[1] { eventID };
		}
	}
}
