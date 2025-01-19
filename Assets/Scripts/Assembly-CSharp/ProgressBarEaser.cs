using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("ZGui/Glu/Progress Bar Easer")]
public class ProgressBarEaser : MonoBehaviour
{
	public enum Ease
	{
		EaseIn = 0,
		EaseInAndOut = 1,
		EaseOut = 2,
		Instant = 3,
		Linear = 4
	}

	private class EaseData
	{
		public float startValue;

		public float goalValue;

		public float elapsedTime;
	}

	private delegate float EaseMethod(float start, float goal, float t);

	public GluiMeter progressBar;

	public Ease easeType = Ease.Linear;

	public float easeSeconds = 1f;

	public float easeDelay;

	private static Dictionary<Ease, EaseMethod> easingMethods;

	private EaseData easeData;

	private float maxFill = 1f;

	private float elapsedDelayTime = 1f;

	public Ease EaseType
	{
		get
		{
			return easeType;
		}
		set
		{
			easeType = value;
		}
	}

	public float EaseSeconds
	{
		get
		{
			return easeSeconds;
		}
		set
		{
			easeSeconds = value;
		}
	}

	public bool Easing
	{
		get
		{
			return easeData != null;
		}
	}

	public float Value
	{
		get
		{
			return progressBar.Value;
		}
	}

	public float MaxFill
	{
		get
		{
			return maxFill;
		}
		set
		{
			maxFill = Mathf.Clamp01(value);
		}
	}

	static ProgressBarEaser()
	{
		easingMethods = new Dictionary<Ease, EaseMethod>();
		easingMethods.Add(Ease.EaseIn, Mathfx.Coserp);
		easingMethods.Add(Ease.EaseOut, Mathfx.Sinerp);
		easingMethods.Add(Ease.EaseInAndOut, Mathfx.Hermite);
		easingMethods.Add(Ease.Instant, (float start, float goal, float t) => goal);
		easingMethods.Add(Ease.Linear, Mathf.Lerp);
	}

	public void EaseTo(float goalValue)
	{
		easeData = new EaseData();
		easeData.startValue = progressBar.Value;
		easeData.goalValue = Mathf.Clamp01(goalValue);
		easeData.elapsedTime = 0f;
		elapsedDelayTime = 0f;
	}

	public void EaseFromTo(float startValue, float goalValue)
	{
		progressBar.Value = startValue;
		EaseTo(goalValue);
		elapsedDelayTime = 0f;
	}

	public void JumpTo(float newValue)
	{
		progressBar.Value = Mathf.Clamp(newValue, 0f, maxFill);
		easeData = null;
	}

	private void Update()
	{
		if (easeData != null)
		{
			if (elapsedDelayTime < easeDelay)
			{
				elapsedDelayTime += Time.deltaTime;
			}
			else if (easeData.goalValue > progressBar.Value && progressBar.Value >= maxFill)
			{
				progressBar.Value = maxFill;
				easeData = null;
			}
			else if (progressBar.Value == easeData.goalValue || easeData.elapsedTime >= easeSeconds)
			{
				progressBar.Value = easeData.goalValue;
				easeData = null;
			}
			else
			{
				float t = easeData.elapsedTime / easeSeconds;
				progressBar.Value = easingMethods[easeType](easeData.startValue, easeData.goalValue, t);
				easeData.elapsedTime += Time.deltaTime;
			}
		}
	}
}
