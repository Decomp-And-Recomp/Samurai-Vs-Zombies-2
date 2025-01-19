using UnityEngine;

public class MouseInputDriver : IInputDriver
{
	private enum MouseButtons
	{
		LEFT = 0,
		RIGHT = 1,
		MIDDLE = 2,
		TOTAL = 3
	}

	private Vector2 prevPinchDist;

	private bool bFirstPinch;

	public override void Initialize()
	{
		base.Initialize();
		bFirstPinch = true;
	}

	public override int GetTouchCount()
	{
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			if (Input.GetMouseButton(i))
			{
				num++;
			}
		}
		return num;
	}

	public override void UpdatePinchGesture(InputGestureStatus gestureStatus, InputGesture_Pinch pinchy)
	{
		HandInfo hand = gestureStatus.Hand;
		if (GetTouchCount() == 2)
		{
			Vector2 cursorPosition = hand.fingers[0].CursorPosition;
			Vector2 cursorStartPosition = hand.fingers[0].CursorStartPosition;
			Vector2 vector = cursorPosition - cursorStartPosition;
			if (bFirstPinch)
			{
				prevPinchDist = vector;
				bFirstPinch = false;
			}
			pinchy.PinchDeltaScalar = vector.magnitude - prevPinchDist.magnitude;
			prevPinchDist = vector;
		}
		else
		{
			pinchy.PinchDeltaScalar = 0f;
			bFirstPinch = false;
		}
	}

	public override void UpdateInputProcessing(ref InputGestureStatus gestureStatus)
	{
		HandInfo hand = gestureStatus.Hand;
		if (hand == null)
		{
			return;
		}
		hand.fingers[0].IsFingerDown = Input.GetMouseButton(0);
		hand.fingers[1].IsFingerDown = Input.GetMouseButton(1);
		hand.fingers[2].IsFingerDown = Input.GetMouseButton(2);
		for (int i = 0; i < hand.fingers.Length; i++)
		{
			hand.fingers[i].WasActive = hand.fingers[i].IsActive;
			if (hand.fingers[i].IsActive)
			{
				hand.fingers[i].DurationActive += Time.deltaTime;
			}
			if (!hand.fingers[i].WasFingerDown && hand.fingers[i].IsFingerDown)
			{
				hand.fingers[i].IsActive = true;
				hand.fingers[i].CursorStartPosition = Input.mousePosition;
				hand.fingers[i].CursorPosition = hand.fingers[i].CursorStartPosition;
				hand.fingers[i].CursorReleasedPosition = new Vector2(-1f, -1f);
				inputManager.OnCursorDown(i);
				hand.fingers[i].WasFingerDown = true;
			}
			else if (hand.fingers[i].WasFingerDown && !hand.fingers[i].IsFingerDown)
			{
				inputManager.OnCursorUp(i);
				hand.fingers[i].DurationActive = 0f;
				hand.fingers[i].CursorStartPosition = new Vector2(-1f, -1f);
				hand.fingers[i].WasFingerDown = false;
				hand.fingers[i].IsActive = false;
			}
			if (hand.fingers[i].IsFingerDown)
			{
				hand.fingers[i].OldCursorPosition = hand.fingers[i].CursorPosition;
				hand.fingers[i].CursorPosition = Input.mousePosition;
				hand.fingers[i].CursorReleasedPosition = hand.fingers[i].CursorPosition;
			}
		}
	}
}
