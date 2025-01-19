using UnityEngine;

[AddComponentMenu("Input Gesture/Input Gesture Move")]
public class InputGesture_Move : InputGestureBase
{
	public override InputEvent UpdateGesture(InputGestureStatus gestureStatus, InputManager inputManager)
	{
		IInputDriver inputDevice = inputManager.InputDevice;
		if (inputDevice.GetTouchCount() == 1)
		{
			for (int i = 0; i < gestureStatus.Hand.fingers.Length; i++)
			{
				FingerInfo fingerInfo = gestureStatus.Hand.fingers[i];
				if (fingerInfo.CursorPosition != fingerInfo.OldCursorPosition)
				{
					fingerInfo.CursorDeltaMovement = fingerInfo.CursorPosition - fingerInfo.OldCursorPosition;
					return OnCursorMove(gestureStatus, i);
				}
			}
		}
		else
		{
			gestureStatus.IsDragging = false;
		}
		return null;
	}

	public InputEvent OnCursorMove(InputGestureStatus gestureStatus, int iFingerIndex)
	{
		Vector2 cursorPosition = gestureStatus.Hand.fingers[iFingerIndex].CursorPosition;
		return new InputEvent(InputEvent.EEventType.OnCursorMove, cursorPosition, iFingerIndex);
	}
}
