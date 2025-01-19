using UnityEngine;

public class GluiModulator_CountTo_Base : GluiModulator
{
	public enum Rounding
	{
		NoRounding = 0,
		Integer = 1,
		KeepTopTwoDigits_99XX = 2
	}

	public float secondsToCount = 1f;

	public bool skipCountingOnFirstChange = true;

	public Rounding roundingWhileCounting = Rounding.KeepTopTwoDigits_99XX;

	protected float? previousValue;

	protected float targetValue;

	protected float unfilteredTargetValue;

	protected float currentInterpolatedValue;

	private bool isCounting;

	private float timeStarted;

	public string countingSound = string.Empty;

	public float countingSoundInterval = 0.1f;

	private float countingSoundNextTime;

	private float countingSoundLastValue;

	protected float CurrentInterpolatedValue
	{
		get
		{
			return currentInterpolatedValue;
		}
		set
		{
			InterpolatedValueChanged(currentInterpolatedValue, value);
			currentInterpolatedValue = value;
		}
	}

	protected override void OnValueChanged(ref float newValue, float originalValue)
	{
		if (previousValue.HasValue && newValue != previousValue.Value)
		{
			StartCounting(newValue);
		}
		else
		{
			previousValue = newValue;
			currentInterpolatedValue = newValue;
		}
		newValue = previousValue.Value;
	}

	private void StartCounting(float newValue)
	{
		if (!(secondsToCount <= 0f))
		{
			timeStarted = Time.time;
			isCounting = true;
			targetValue = PreCountingFilter(newValue);
			unfilteredTargetValue = newValue;
			if (isCounting)
			{
				previousValue = CurrentInterpolatedValue;
			}
			CountingSoundReadyNext();
		}
	}

	public int String_LengthOfSamePrefix(string s, string other)
	{
		string text;
		string text2;
		if (s.Length < other.Length)
		{
			text = s;
			text2 = other;
		}
		else
		{
			text = other;
			text2 = s;
		}
		int i;
		for (i = 0; i < text.Length && text[i] == text2[i]; i++)
		{
		}
		return i;
	}

	private float RoundValue(float newValue, float oldValue, float finalTargetValue, float previousValue)
	{
		switch (roundingWhileCounting)
		{
		case Rounding.KeepTopTwoDigits_99XX:
		{
			string text = ((int)newValue).ToString();
			string text2 = ((int)oldValue).ToString();
			string text3 = ((int)finalTargetValue).ToString();
			string other = ((int)previousValue).ToString();
			int num = 2;
			int num2 = String_LengthOfSamePrefix(text, other);
			if (num2 > 0)
			{
				num += num2;
			}
			if (text.Length < num)
			{
				num = text.Length;
			}
			string text4 = text.Substring(0, num);
			int num3 = text.Length - text4.Length;
			int num4 = num3;
			if (text2.Length < num4)
			{
				num4 = text2.Length;
			}
			while (num4 < num3 && text3.Length > text4.Length)
			{
				text4 += text3[text4.Length];
				num3--;
			}
			if (num4 > 0)
			{
				text4 += text2.Substring(text2.Length - num4);
			}
			return float.Parse(text4);
		}
		case Rounding.Integer:
			return Mathf.Round(newValue);
		default:
			return newValue;
		}
	}

	protected virtual float PreCountingFilter(float newValue)
	{
		return newValue;
	}

	protected virtual float PostCountingFilter(float newValue)
	{
		return newValue;
	}

	public void Update()
	{
		if (!isCounting)
		{
			return;
		}
		float num = Time.time - timeStarted;
		if (num >= secondsToCount)
		{
			isCounting = false;
			targetValue = PostCountingFilter(targetValue);
			previousValue = unfilteredTargetValue;
			InterpolatedValueChanged(currentInterpolatedValue, targetValue);
			currentInterpolatedValue = unfilteredTargetValue;
			SetValue(unfilteredTargetValue);
		}
		else
		{
			CurrentInterpolatedValue = Mathf.Lerp(previousValue.Value, targetValue, num / secondsToCount);
			if (Time.time > countingSoundNextTime && CurrentInterpolatedValue != countingSoundLastValue)
			{
				CountingSoundPlay();
			}
			float newValue = RoundValue(CurrentInterpolatedValue, previousValue.Value, targetValue, GetValue());
			SetValue(PostCountingFilter(newValue));
		}
	}

	protected virtual void InterpolatedValueChanged(float oldValue, float newValue)
	{
	}

	private void CountingSoundPlay()
	{
		if (!string.IsNullOrEmpty(countingSound))
		{
			GluiGlobalSoundHandler.Instance.PlaySoundEvent(countingSound);
			CountingSoundReadyNext();
		}
	}

	private void CountingSoundReadyNext()
	{
		countingSoundNextTime = Time.time + countingSoundInterval;
		countingSoundLastValue = currentInterpolatedValue;
	}
}
