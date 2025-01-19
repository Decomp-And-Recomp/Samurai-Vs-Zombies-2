using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Input Gesture/Input Gesture Circle")]
public class InputGesture_Circle : InputGestureWithDataPoints
{
	private const int kSegmentMinLengthSquared = 16;

	public float validThreshold = 0.5f;

	protected override float evaluationValidThreshold
	{
		get
		{
			return validThreshold;
		}
	}

	protected override InputEvent GestureCallback(InputGestureStatus gestureStatus)
	{
		Vector2 cursorPosition = gestureStatus.Hand.fingers[0].CursorPosition;
		return new InputEvent(InputEvent.EEventType.OnGestureScribbleCircle, cursorPosition, 0);
	}

	protected override float Evaluate()
	{
		if (segments.Count == 0)
		{
			return 0f;
		}
		List<Segment>.Enumerator enumerator = segments.GetEnumerator();
		float num = 1f / (float)segments.Count;
		float num2 = 0f;
		Segment current = enumerator.Current;
		Vector2 vector = current.end - current.start;
		current.weight = ((!(vector.x > 0f)) ? num : (0f - num));
		if (vector.sqrMagnitude < 16f)
		{
			current.weight *= vector.sqrMagnitude / 16f;
		}
		num2 += current.weight;
		Vector2 rhs = default(Vector2);
		rhs.x = vector.y;
		rhs.y = 0f - vector.x;
		rhs.Normalize();
		while (enumerator.MoveNext())
		{
			Segment current2 = enumerator.Current;
			Vector2 vector2 = current2.end - current2.start;
			current2.weight = ((!(Vector2.Dot(vector2.normalized, rhs) > 0f)) ? (0f - num) : num);
			if (vector2.sqrMagnitude < 16f)
			{
				current2.weight *= vector2.sqrMagnitude / 16f;
			}
			num2 += current2.weight;
			rhs.x = vector2.y;
			rhs.y = 0f - vector2.x;
			rhs.Normalize();
		}
		return Mathf.Abs(num2);
	}
}
