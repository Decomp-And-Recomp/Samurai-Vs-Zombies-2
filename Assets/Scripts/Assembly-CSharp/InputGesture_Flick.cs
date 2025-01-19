using System;
using UnityEngine;

public abstract class InputGesture_Flick : InputGestureBase
{
	public float maxTouchDurationForFlick = 0.5f;

	public float maxAngleDeviationInDegrees = 20f;

	public float minFlickDistance = 30f;

	private float maxDotWithRight;

	private bool isTouching;

	private float startTouchTime;

	private Vector2 startPosition;

	private Vector2 endPosition;

	private Vector2 flickDirectionRight;

	protected abstract Vector2 flickDirection { get; }

	protected abstract InputEvent GestureCallback(InputGestureStatus gestureStatus);

	public void Start()
	{
		maxDotWithRight = Mathf.Cos((float)Math.PI / 180f * (90f - maxAngleDeviationInDegrees));
		flickDirectionRight = new Vector2(flickDirection.y, 0f - flickDirection.x);
	}

	public override InputEvent UpdateGesture(InputGestureStatus gestureStatus, InputManager inputManager)
	{
		IInputDriver inputDevice = inputManager.InputDevice;
		if (inputDevice.GetTouchCount() == 1)
		{
			if (!isTouching)
			{
				isTouching = true;
				startTouchTime = Time.time;
				startPosition = gestureStatus.Hand.fingers[0].CursorPosition;
				endPosition = Vector2.zero;
			}
			else
			{
				endPosition = gestureStatus.Hand.fingers[0].CursorPosition;
			}
		}
		else if (isTouching)
		{
			isTouching = false;
			if (Time.time < startTouchTime + maxTouchDurationForFlick && endPosition != Vector2.zero && Evaluate(startPosition, endPosition))
			{
				return GestureCallback(gestureStatus);
			}
		}
		return null;
	}

	protected bool Evaluate(Vector2 startPoint, Vector2 endPoint)
	{
		Vector2 vector = endPoint - startPoint;
		Vector2 normalized = vector.normalized;
		if (vector.magnitude >= minFlickDistance && Vector2.Dot(normalized, flickDirection) > 0f && Mathf.Abs(Vector2.Dot(normalized, flickDirectionRight)) < maxDotWithRight)
		{
			return true;
		}
		return false;
	}
}
