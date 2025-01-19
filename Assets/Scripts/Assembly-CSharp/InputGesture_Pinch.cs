using UnityEngine;

[AddComponentMenu("Input Gesture/Input Gesture Pinch")]
public class InputGesture_Pinch : InputGestureBase
{
	public Vector2 PinchDelta;

	public Vector2 OldPinchDelta;

	public Vector2 TruePinchDelta;

	public Vector2 PinchCurrentDist;

	public Vector2 PinchPrevDist;

	public float PinchDeltaScalar;

	public bool IsPinching;

	public bool WasPinching;

	public void Start()
	{
		PinchDelta = Vector2.zero;
		PinchCurrentDist = (PinchPrevDist = (TruePinchDelta = Vector2.zero));
		PinchDeltaScalar = 0f;
		IsPinching = false;
		WasPinching = false;
	}

	public override InputEvent UpdateGesture(InputGestureStatus gestureStatus, InputManager inputManager)
	{
		IInputDriver inputDevice = inputManager.InputDevice;
		inputDevice.UpdatePinchGesture(gestureStatus, this);
		if (inputDevice.GetTouchCount() == 2)
		{
			WasPinching = true;
			return OnPinchGesture(gestureStatus, 0);
		}
		if (WasPinching && inputDevice.GetTouchCount() != 2)
		{
			WasPinching = false;
			return OnPinchEndGesture(gestureStatus, 0);
		}
		return null;
	}

	public InputEvent OnPinchGesture(InputGestureStatus gestureStatus, int iFingerIndex)
	{
		Vector2 cursorPosition = gestureStatus.Hand.fingers[iFingerIndex].CursorPosition;
		return new InputEvent(InputEvent.EEventType.OnPinchGesture, cursorPosition, iFingerIndex);
	}

	public InputEvent OnPinchEndGesture(InputGestureStatus gestureStatus, int iFingerIndex)
	{
		Vector2 cursorPosition = gestureStatus.Hand.fingers[iFingerIndex].CursorPosition;
		return new InputEvent(InputEvent.EEventType.OnPinchEndGesture, cursorPosition, iFingerIndex);
	}
}
