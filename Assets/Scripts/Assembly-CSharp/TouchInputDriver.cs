using UnityEngine;

public class TouchInputDriver : IInputDriver
{
	private Touch GetTouchByIndex(int iTouchIndex)
	{
		return Input.touches[iTouchIndex];
	}

	public override int GetTouchCount()
	{
		return Input.touchCount;
	}

	public override void UpdatePinchGesture(InputGestureStatus gestureStatus, InputGesture_Pinch pinchy)
	{
		if (GetTouchCount() == 2)
		{
			Vector2 position = Input.GetTouch(0).position;
			Vector2 position2 = Input.GetTouch(1).position;
			Vector2 deltaPosition = Input.GetTouch(0).deltaPosition;
			Vector2 deltaPosition2 = Input.GetTouch(1).deltaPosition;
			Vector2 vector = position - position2;
			Vector2 vector2 = position - deltaPosition - (position2 - deltaPosition2);
			pinchy.PinchDeltaScalar = vector.magnitude - vector2.magnitude;
			pinchy.IsPinching = true;
		}
		else
		{
			pinchy.PinchDeltaScalar = 0f;
			pinchy.IsPinching = false;
		}
	}

	public override void UpdateInputProcessing(ref InputGestureStatus gestureStatus)
	{
		HandInfo hand = gestureStatus.Hand;
		int num = 0;
		for (int i = 0; i < Input.touches.Length; i++)
		{
			if (num < hand.fingers.Length)
			{
				Touch touch = Input.touches[i];
				bool isFingerDown = false;
				if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled && GetTouchCount() > 0)
				{
					isFingerDown = true;
				}
				hand.fingers[num].IsFingerDown = isFingerDown;
				hand.fingers[num].TouchIndex = i;
				num++;
			}
		}
		if (GetTouchCount() == 0)
		{
			for (num = 0; num < hand.fingers.Length; num++)
			{
				FingerInfo fingerInfo = hand.fingers[num];
				if (fingerInfo.IsFingerDown)
				{
					fingerInfo.IsFingerDown = false;
					inputManager.OnCursorUp(num);
					fingerInfo.WasFingerDown = false;
					fingerInfo.IsActive = false;
					fingerInfo.DurationActive = 0f;
					fingerInfo.CursorStartPosition = new Vector2(-1f, -1f);
				}
			}
		}
		for (num = 0; num < hand.fingers.Length; num++)
		{
			hand.fingers[num].WasActive = hand.fingers[num].IsActive;
			if (hand.fingers[num].IsActive)
			{
				hand.fingers[num].DurationActive += Time.deltaTime;
			}
			if (!hand.fingers[num].WasFingerDown && hand.fingers[num].IsFingerDown)
			{
				if (hand.fingers[num].TouchIndex >= Input.touches.Length)
				{
					break;
				}
				hand.fingers[num].IsActive = true;
				hand.fingers[num].CursorStartPosition = GetTouchByIndex(hand.fingers[num].TouchIndex).position;
				hand.fingers[num].CursorPosition = hand.fingers[num].CursorStartPosition;
				hand.fingers[num].CursorReleasedPosition = new Vector2(-1f, -1f);
				inputManager.OnCursorDown(num);
				hand.fingers[num].WasFingerDown = true;
				hand.fingers[num].DurationActive = 0f;
				InputGesture_Pinch component = GetComponent<InputGesture_Pinch>();
				if ((bool)component)
				{
					component.TruePinchDelta = Vector2.zero;
					component.PinchPrevDist = (component.PinchCurrentDist = hand.fingers[num].CursorStartPosition);
				}
			}
			else if (hand.fingers[num].WasFingerDown && !hand.fingers[num].IsFingerDown)
			{
				inputManager.OnCursorUp(num);
				hand.fingers[num].WasFingerDown = false;
				hand.fingers[num].IsActive = false;
				hand.fingers[num].CursorStartPosition = new Vector2(-1f, -1f);
			}
			if (hand.fingers[num].IsFingerDown)
			{
				if (hand.fingers[num].TouchIndex >= Input.touches.Length)
				{
					break;
				}
				hand.fingers[num].OldCursorPosition = hand.fingers[num].CursorPosition;
				hand.fingers[num].CursorPosition = GetTouchByIndex(hand.fingers[num].TouchIndex).position;
				hand.fingers[num].CursorReleasedPosition = hand.fingers[num].CursorPosition;
			}
		}
	}
}
