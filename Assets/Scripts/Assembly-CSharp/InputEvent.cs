using System;
using UnityEngine;

public class InputEvent
{
	[Flags]
	public enum EEventType
	{
		OnCursorDown = 2,
		OnCursorUp = 4,
		OnCursorMove = 8,
		OnSingleTap = 0x10,
		OnDoubleTap = 0x20,
		OnLongPress = 0x40,
		OnPinchGesture = 0x80,
		OnPinchEndGesture = 0x100,
		OnCursorExit = 0x400,
		OnGestureScribbleHorizontal = 0x800,
		OnGestureScribbleVertical = 0x1000,
		OnGestureScribbleCircle = 0x2000,
		OnGestureFlickDown = 0x4000,
		OnGestureFlickUp = 0x8000,
		OnGestureFlickLeft = 0x10000,
		OnGestureFlickRight = 0x20000,
		OnGestureFlickUpLeft = 0x40000,
		OnGestureFlickUpRight = 0x80000,
		OnGestureFlickDownLeft = 0x100000,
		OnGestureFlickDownRight = 0x200000
	}

	public EEventType EventType { get; internal set; }

	public Vector2 Position { get; internal set; }

	public int CursorIndex { get; internal set; }

	public GameObject Target { get; internal set; }

	internal InputEvent(EEventType type, Vector2 position, int cursorIndex)
	{
		EventType = type;
		Position = position;
		CursorIndex = cursorIndex;
	}

	public Vector3 Position_In_Camera(Camera camera, float Z)
	{
		return camera.ScreenToWorldPoint(new Vector3(Position.x, Position.y, Z));
	}
}
