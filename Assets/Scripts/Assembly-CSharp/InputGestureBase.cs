using UnityEngine;

public abstract class InputGestureBase : MonoBehaviour
{
	public abstract InputEvent UpdateGesture(InputGestureStatus gestureStatus, InputManager inputManager);
}
