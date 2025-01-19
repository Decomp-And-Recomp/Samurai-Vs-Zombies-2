using UnityEngine;

[AddComponentMenu("Input Gesture/Input Gesture Scribble Vertical")]
public class InputGesture_ScribbleVertical : InputGesture_Scribble
{
	public float validThreshold = 0.5f;

	protected override Vector2 scribbleCross
	{
		get
		{
			return new Vector2(0f, 1f);
		}
	}

	protected override float evaluationValidThreshold
	{
		get
		{
			return validThreshold;
		}
	}

	protected override InputEvent GestureCallback(InputGestureStatus gestureStatus)
	{
		Vector2 cursorPosition = gestureStatus.Hand.fingers[0].CursorPosition;
		return new InputEvent(InputEvent.EEventType.OnGestureScribbleVertical, cursorPosition, 0);
	}
}
