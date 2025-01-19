using System;
using UnityEngine;

[Serializable]
public class MotionPathNode
{
	public enum PositionSource
	{
		TouchPosition = 0,
		OwnerPosition = 1,
		OwnerPosition_OffsetBy_Point = 2,
		PointValue = 3,
		MarkerGameObject = 4
	}

	public GameObject markerGameObject;

	public Vector2 point = Vector2.zero;

	public PositionSource source;

	private Vector2 position = Vector2.zero;

	public Vector2 Position
	{
		get
		{
			return position;
		}
		set
		{
			position = value;
		}
	}

	public void UpdateDefaultPosition(GameObject owner)
	{
		switch (source)
		{
		case PositionSource.TouchPosition:
			position = SingletonMonoBehaviour<InputManager>.Instance.GestureStatus.lastSingleTouchPosition;
			break;
		case PositionSource.OwnerPosition:
			position = ObjectUtils.GetObjectScreenPosition(owner);
			break;
		case PositionSource.OwnerPosition_OffsetBy_Point:
			position = ObjectUtils.GetObjectScreenPosition(owner);
			position += point;
			break;
		case PositionSource.PointValue:
			position = point;
			break;
		case PositionSource.MarkerGameObject:
			if ((bool)markerGameObject)
			{
				position = ObjectUtils.GetObjectScreenPosition(markerGameObject);
			}
			break;
		}
	}
}
