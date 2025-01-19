using UnityEngine;

[AddComponentMenu("Input Gesture/Input Gesture Flick Right")]
public class InputGesture_FlickRight : InputGesture_Flick
{
	protected override Vector2 flickDirection
	{
		get
		{
			return new Vector2(1f, 0f);
		}
	}

	protected override InputEvent GestureCallback(InputGestureStatus gestureStatus)
	{
		Vector2 cursorPosition = gestureStatus.Hand.fingers[0].CursorPosition;
		return new InputEvent(InputEvent.EEventType.OnGestureFlickRight, cursorPosition, 0);
	}
}
