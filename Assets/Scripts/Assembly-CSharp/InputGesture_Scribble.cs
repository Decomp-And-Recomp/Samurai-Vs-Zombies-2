using UnityEngine;

public abstract class InputGesture_Scribble : InputGestureWithDataPoints
{
	protected abstract Vector2 scribbleCross { get; }

	protected override float Evaluate()
	{
		if (segments.Count == 0)
		{
			return 0f;
		}
		float num = 1f / (float)segments.Count;
		float num2 = 0f;
		for (int i = 0; i < segments.Count; i++)
		{
			Segment segment = segments[i];
			segment.weight = ((!((double)Mathf.Abs(Vector2.Dot((segment.start - segment.end).normalized, scribbleCross)) > 0.75)) ? (0f - num) : num);
			num2 += segment.weight;
		}
		return num2;
	}
}
