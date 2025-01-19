using UnityEngine;

[AddComponentMenu("Input Gesture/Input Gesture Tap and Press")]
public class InputGesture_TapAndPress : InputGestureBase
{
	private int numberOfTaps;

	private int tapFingerIndex;

	private float tapTimeCounter;

	public float maxSecondsForTap = 0.3f;

	private bool isTouching;

	private bool isReadyToStartTapping = true;

	public override InputEvent UpdateGesture(InputGestureStatus gestureStatus, InputManager inputManager)
	{
		IInputDriver inputDevice = inputManager.InputDevice;
		if (IsUpdatingTapCounter())
		{
			isReadyToStartTapping = false;
			tapTimeCounter += Time.deltaTime;
			if (inputDevice.GetTouchCount() == 1)
			{
				if (!isTouching)
				{
					tapTimeCounter = 0f;
					isTouching = true;
				}
			}
			else if (isTouching)
			{
				numberOfTaps++;
				isTouching = false;
			}
			if (tapTimeCounter >= maxSecondsForTap)
			{
				return EvaluateTaps(gestureStatus);
			}
		}
		else if (inputDevice.GetTouchCount() == 1)
		{
			if (isReadyToStartTapping)
			{
				StartTapping(0);
			}
		}
		else
		{
			isReadyToStartTapping = true;
		}
		return null;
	}

	private void IncrementNumberOfTaps()
	{
		numberOfTaps++;
	}

	private void StartTapping(int fingerIndex)
	{
		numberOfTaps = 0;
		tapTimeCounter = 0f;
		isTouching = true;
		tapFingerIndex = fingerIndex;
	}

	private bool IsUpdatingTapCounter()
	{
		return tapTimeCounter >= 0f;
	}

	private InputEvent EvaluateTaps(InputGestureStatus gestureStatus)
	{
		tapTimeCounter = -1f;
		Vector2 cursorPosition = gestureStatus.Hand.fingers[tapFingerIndex].CursorPosition;
		if (numberOfTaps >= 2)
		{
			return new InputEvent(InputEvent.EEventType.OnDoubleTap, cursorPosition, tapFingerIndex);
		}
		if (numberOfTaps >= 1)
		{
			return new InputEvent(InputEvent.EEventType.OnSingleTap, cursorPosition, tapFingerIndex);
		}
		return new InputEvent(InputEvent.EEventType.OnLongPress, cursorPosition, tapFingerIndex);
	}
}
