using UnityEngine;

public class InputGestureStatus
{
	public const float fDragDistanceThreshold = 5f;

	protected HandInfo hand;

	public Vector2 lastSingleTouchPosition;

	public bool DataGestureDone;

	public bool IsDragging;

	public HandInfo Hand
	{
		get
		{
			return hand;
		}
	}

	public void Reset()
	{
		hand = new HandInfo(3);
		DataGestureDone = false;
	}

	public void ClearOnNoTouch()
	{
		DataGestureDone = false;
	}
}
