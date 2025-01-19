using UnityEngine;

[AddComponentMenu("Input Gesture/Input Gesture Flick Down")]
public class InputGesture_FlickDown : InputGesture_Flick
{
	protected override Vector2 flickDirection
	{
		get
		{
			return new Vector2(0f, -1f);
		}
	}

	protected override InputEvent GestureCallback(InputGestureStatus gestureStatus)
	{
		Vector2 cursorPosition = gestureStatus.Hand.fingers[0].CursorPosition;
		return new InputEvent(InputEvent.EEventType.OnGestureFlickDown, cursorPosition, 0);
	}
}
