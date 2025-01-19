using UnityEngine;

[AddComponentMenu("Input Gesture/Input Gesture Flick Diagonal UpLeft")]
public class InputGesture_FlickDiagonal_UpLeft : InputGesture_Flick
{
	protected override Vector2 flickDirection
	{
		get
		{
			return new Vector2(-1f, 1f);
		}
	}

	protected override InputEvent GestureCallback(InputGestureStatus gestureStatus)
	{
		Vector2 cursorPosition = gestureStatus.Hand.fingers[0].CursorPosition;
		return new InputEvent(InputEvent.EEventType.OnGestureFlickUpLeft, cursorPosition, 0);
	}
}
