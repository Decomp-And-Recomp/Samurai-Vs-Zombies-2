using UnityEngine;

[AddComponentMenu("Input Gesture/Input Gesture Flick Diagonal DownRight")]
public class InputGesture_FlickDiagonal_DownRight : InputGesture_Flick
{
	protected override Vector2 flickDirection
	{
		get
		{
			return new Vector2(1f, -1f);
		}
	}

	protected override InputEvent GestureCallback(InputGestureStatus gestureStatus)
	{
		Vector2 cursorPosition = gestureStatus.Hand.fingers[0].CursorPosition;
		return new InputEvent(InputEvent.EEventType.OnGestureFlickDownRight, cursorPosition, 0);
	}
}
