using UnityEngine;

[AddComponentMenu("Glui Modulator/Value CountTo")]
public class GluiModulator_Value_CountTo : GluiModulator_CountTo_Base
{
	public enum ValueDirection
	{
		None = 0,
		Increase_CyclePastRangeMax = 1,
		Decrease_CyclePastRangeMin = 2
	}

	public enum ValueRange
	{
		None = 0,
		Percentage = 1
	}

	public ValueRange valueRange;

	public ValueDirection valueDirection;

	public string action_ReachMin;

	public string action_ReachMax;

	public override void Awake()
	{
		base.Awake();
		if (!skipCountingOnFirstChange)
		{
			float newValue = GetValue();
			OnValueChanged(ref newValue, newValue);
		}
	}

	protected override float PreCountingFilter(float newValue)
	{
		if (valueRange != 0)
		{
			float result = ExtendNewValue(newValue);
			previousValue = PostCountingFilter(previousValue.Value);
			currentInterpolatedValue = PostCountingFilter(currentInterpolatedValue);
			return result;
		}
		return newValue;
	}

	protected override float PostCountingFilter(float newValue)
	{
		if (valueRange == ValueRange.None)
		{
			return newValue;
		}
		float minValue;
		float maxValue;
		GetValueRange(out minValue, out maxValue);
		float num = maxValue - minValue;
		if (newValue < minValue)
		{
			newValue += num;
		}
		if (newValue > maxValue)
		{
			newValue -= num;
		}
		return newValue;
	}

	private float ExtendNewValue(float newValue)
	{
		float minValue;
		float maxValue;
		GetValueRange(out minValue, out maxValue);
		newValue = Mathf.Clamp(newValue, minValue, maxValue);
		switch (valueDirection)
		{
		case ValueDirection.Decrease_CyclePastRangeMin:
			if (newValue > previousValue.Value)
			{
				float num2 = maxValue - newValue;
				return minValue - num2;
			}
			break;
		case ValueDirection.Increase_CyclePastRangeMax:
			if (newValue < previousValue.Value)
			{
				float num = newValue - minValue;
				return maxValue + num;
			}
			break;
		}
		return newValue;
	}

	private void GetValueRange(out float minValue, out float maxValue)
	{
		ValueRange valueRange = this.valueRange;
		if (valueRange != 0 && valueRange == ValueRange.Percentage)
		{
			minValue = 0f;
			maxValue = 1f;
		}
		else
		{
			minValue = 0f;
			maxValue = 0f;
		}
	}

	protected override void InterpolatedValueChanged(float oldValue, float newValue)
	{
		float minValue;
		float maxValue;
		GetValueRange(out minValue, out maxValue);
		if (oldValue > minValue && newValue <= minValue)
		{
			GluiActionSender.SendGluiAction(action_ReachMin, base.gameObject, null);
		}
		if (oldValue < maxValue && newValue >= maxValue)
		{
			GluiActionSender.SendGluiAction(action_ReachMax, base.gameObject, null);
		}
	}
}
