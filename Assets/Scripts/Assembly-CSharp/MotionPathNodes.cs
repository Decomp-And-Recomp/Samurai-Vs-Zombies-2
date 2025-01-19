using System;
using UnityEngine;

[Serializable]
public class MotionPathNodes
{
	public MotionPathNode startPosition;

	public MotionPathNode endPosition;

	public void SetStartPosition(Vector2 point)
	{
		startPosition.point = point;
		startPosition.source = MotionPathNode.PositionSource.PointValue;
		startPosition.UpdateDefaultPosition(null);
	}

	public void SetEndPosition(Vector2 point)
	{
		endPosition.point = point;
		endPosition.source = MotionPathNode.PositionSource.PointValue;
		endPosition.UpdateDefaultPosition(null);
	}

	public void UpdateDefaultPosition(GameObject owner)
	{
		startPosition.UpdateDefaultPosition(owner);
		endPosition.UpdateDefaultPosition(owner);
	}
}
