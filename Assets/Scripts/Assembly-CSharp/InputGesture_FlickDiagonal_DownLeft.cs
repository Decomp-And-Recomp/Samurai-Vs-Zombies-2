using UnityEngine;

[AddComponentMenu("Input Gesture/Input Gesture Flick Diagonal DownLeft")]
public class InputGesture_FlickDiagonal_DownLeft : InputGesture_Flick
{
	protected override Vector2 flickDirection
	{
		get
		{
			return new Vector2(-1f, -1f);
		}
	}

	protected override InputEvent GestureCallback(InputGestureStatus gestureStatus)
	{
		Vector2 cursorPosition = gestureStatus.Hand.fingers[0].CursorPosition;
		return new InputEvent(InputEvent.EEventType.OnGestureFlickDownLeft, cursorPosition, 0);
	}
}
