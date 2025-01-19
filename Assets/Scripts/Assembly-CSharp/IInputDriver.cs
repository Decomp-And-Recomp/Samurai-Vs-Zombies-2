using UnityEngine;

[AddComponentMenu("")]
public class IInputDriver : MonoBehaviour
{
	protected InputManager inputManager;

	public virtual void Initialize()
	{
		inputManager = SingletonMonoBehaviour<InputManager>.Instance;
	}

	public virtual void UpdateInputProcessing(ref InputGestureStatus gestureStatus)
	{
	}

	public virtual void UpdatePinchGesture(InputGestureStatus gestureStatus, InputGesture_Pinch pinchy)
	{
		pinchy.PinchDelta = Vector2.zero;
	}

	public virtual int GetTouchCount()
	{
		return 0;
	}
}
