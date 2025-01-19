using System.Collections.Generic;
using UnityEngine;

public abstract class InputGestureWithDataPoints : InputGestureBase
{
	protected struct Segment
	{
		public Vector2 start;

		public Vector2 end;

		public float weight;
	}

	public float firstEvaluationDelay = 1f;

	private float evaluationTimer;

	private bool isTouching;

	protected List<Segment> segments = new List<Segment>();

	protected List<Vector2> rawPoints = new List<Vector2>();

	protected abstract float evaluationValidThreshold { get; }

	protected abstract float Evaluate();

	protected abstract InputEvent GestureCallback(InputGestureStatus gestureStatus);

	public override InputEvent UpdateGesture(InputGestureStatus gestureStatus, InputManager inputManager)
	{
		if (gestureStatus.DataGestureDone)
		{
			return null;
		}
		IInputDriver inputDevice = inputManager.InputDevice;
		if (inputDevice.GetTouchCount() == 1)
		{
			FingerInfo fingerInfo = gestureStatus.Hand.fingers[0];
			rawPoints.Add(fingerInfo.CursorPosition);
			if (!isTouching)
			{
				ResetTimer();
				isTouching = true;
			}
			else
			{
				evaluationTimer -= Time.deltaTime;
				if (evaluationTimer <= 0f)
				{
					ResetTimer();
					SegmentizePoints();
					float num = Evaluate();
					segments.Clear();
					if (num >= evaluationValidThreshold)
					{
						gestureStatus.DataGestureDone = true;
						return GestureCallback(gestureStatus);
					}
				}
			}
		}
		else
		{
			isTouching = false;
			rawPoints.Clear();
			segments.Clear();
		}
		return null;
	}

	private void ResetTimer()
	{
		evaluationTimer = firstEvaluationDelay;
	}

	private void SegmentizePoints()
	{
		if (rawPoints.Count > 1)
		{
			Vector2 start = rawPoints[0];
			Vector2 vector = rawPoints[1];
			Vector2 rhs = default(Vector2);
			rhs.x = start.y;
			rhs.y = 0f - start.x;
			rhs.Normalize();
			for (int i = 2; i < rawPoints.Count - 1; i++)
			{
				Vector2 vector2 = rawPoints[i];
				if ((double)Mathf.Abs(Vector2.Dot(vector2.normalized, rhs)) > 0.125)
				{
					Segment item = default(Segment);
					item.start = start;
					item.end = vector;
					start = vector;
					segments.Add(item);
					rhs.x = start.y;
					rhs.y = 0f - start.x;
					rhs.Normalize();
				}
				vector = vector2;
			}
			Segment item2 = default(Segment);
			item2.start = start;
			item2.end = vector;
			segments.Add(item2);
		}
		rawPoints.Clear();
	}
}
