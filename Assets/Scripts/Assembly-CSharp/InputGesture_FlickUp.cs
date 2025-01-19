using UnityEngine;

[AddComponentMenu("Input Gesture/Input Gesture Flick Up")]
public class InputGesture_FlickUp : InputGesture_Flick
{
	protected override Vector2 flickDirection
	{
		get
		{
			return new Vector2(0f, 1f);
		}
	}

	protected override InputEvent GestureCallback(InputGestureStatus gestureStatus)
	{
		Vector2 cursorPosition = gestureStatus.Hand.fingers[0].CursorPosition;
		return new InputEvent(InputEvent.EEventType.OnGestureFlickUp, cursorPosition, 0);
	}
}
