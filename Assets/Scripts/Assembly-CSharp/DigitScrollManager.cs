using UnityEngine;

public class DigitScrollManager
{
	public delegate void OnValueChangedCallback(int newValue);

	public OnValueChangedCallback onValueChanged;

	private bool mFirstUpdate = true;

	private int mCurrentValue;

	private int mMinValue;

	private int mMaxValue;

	private GluiScrollList mList;

	public DigitScrollManager(GameObject scrollObject, int initialValue, int minValue, int maxValue)
	{
		mCurrentValue = initialValue;
		mMaxValue = maxValue;
		mMinValue = minValue;
		mList = scrollObject.GetComponent<GluiScrollList>();
	}

	public void Update()
	{
		if (mFirstUpdate)
		{
			mFirstUpdate = false;
			SetShownValue(mCurrentValue);
		}
		CheckValueChange();
	}

	public bool OnUIEvent(string eventID)
	{
		return false;
	}

	public void SetShownValue(int v)
	{
		mList.SnapTo(v - 1, GluiScrollList.SelectionSnap.Instant_Center);
	}

	private void CheckValueChange()
	{
		if (mList.IsScrolling)
		{
			return;
		}
		int num = mList.FindClosestIndexToCenter() + 1;
		if (num < 0)
		{
			mFirstUpdate = true;
			return;
		}
		if (num < mMinValue)
		{
			num = mMinValue;
			SetShownValue(num);
		}
		if (num > mMaxValue)
		{
			num = mMaxValue;
			SetShownValue(num);
		}
		if (num != mCurrentValue)
		{
			mCurrentValue = num;
			if (onValueChanged != null)
			{
				onValueChanged(mCurrentValue);
			}
		}
	}
}
