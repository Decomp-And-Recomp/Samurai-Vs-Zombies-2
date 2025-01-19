using UnityEngine;

[AddComponentMenu("Effect Maestro/Motion Path Curve")]
public class MotionPathCurve : MotionPath
{
	public enum CurveType
	{
		QuadraticCurve = 0,
		CubicCurve = 1
	}

	public enum CurveDirection
	{
		Right = 0,
		Left = 1,
		Random = 2,
		SquiggleCubicOnly = 3
	}

	protected Vector2 midPoint1;

	protected Vector2 midPoint2;

	public CurveType curveType;

	public CurveDirection curveDirection = CurveDirection.Random;

	public float midPoint1PercentMinTangentOffset = 0.2f;

	public float midPoint1PercentMaxTangentOffset = 0.4f;

	public float midPoint1PercentMinNormalOffset = 0.25f;

	public float midPoint1PercentMaxNormalOffset = 0.5f;

	public float midPoint2PercentMinTangentOffsetCubicOnly = 0.6f;

	public float midPoint2PercentMaxTangentOffsetCubicOnly = 0.8f;

	public float midPoint2PercentMinNormalOffsetCubicOnly = 0.25f;

	public float midPoint2PercentMaxNormalOffsetCubicOnly = 0.5f;

	protected override void NewTransition(GameObject owner)
	{
		base.NewTransition(owner);
		Vector2 zero = Vector2.zero;
		zero = ((!forward) ? (nodes.startPosition.Position - nodes.endPosition.Position) : (nodes.endPosition.Position - nodes.startPosition.Position));
		float num = Random.Range(midPoint1PercentMinTangentOffset, midPoint1PercentMaxTangentOffset);
		if (forward)
		{
			midPoint1 = nodes.startPosition.Position + zero * num;
		}
		else
		{
			midPoint1 = nodes.endPosition.Position + zero * num;
		}
		Vector2 vector = new Vector2(zero.y, 0f - zero.x);
		Vector2 vector2 = vector * Random.Range(midPoint1PercentMinNormalOffset, midPoint1PercentMaxNormalOffset);
		CurveDirection curveDirection = CurveDirection.Random;
		switch (this.curveDirection)
		{
		case CurveDirection.Left:
			midPoint1 += vector2;
			break;
		case CurveDirection.Right:
			midPoint1 -= vector2;
			break;
		case CurveDirection.Random:
		case CurveDirection.SquiggleCubicOnly:
			if (Random.Range(0, 2) == 0)
			{
				midPoint1 += vector2;
				curveDirection = CurveDirection.Left;
			}
			else
			{
				midPoint1 -= vector2;
				curveDirection = CurveDirection.Right;
			}
			break;
		}
		if (curveType != CurveType.CubicCurve)
		{
			return;
		}
		num = Random.Range(midPoint2PercentMinTangentOffsetCubicOnly, midPoint2PercentMaxTangentOffsetCubicOnly);
		if (forward)
		{
			midPoint2 = nodes.startPosition.Position + zero * num;
		}
		else
		{
			midPoint2 = nodes.endPosition.Position + zero * num;
		}
		vector2 = vector * Random.Range(midPoint2PercentMinNormalOffsetCubicOnly, midPoint2PercentMaxNormalOffsetCubicOnly);
		switch (this.curveDirection)
		{
		case CurveDirection.Left:
			midPoint2 += vector2;
			break;
		case CurveDirection.Right:
			midPoint2 -= vector2;
			break;
		case CurveDirection.Random:
			if (Random.Range(0, 2) == 0)
			{
				midPoint2 += vector2;
			}
			else
			{
				midPoint2 -= vector2;
			}
			break;
		case CurveDirection.SquiggleCubicOnly:
			if (curveDirection == CurveDirection.Right)
			{
				midPoint2 += vector2;
			}
			else
			{
				midPoint2 -= vector2;
			}
			break;
		}
	}

	protected override Vector2 GetPosition()
	{
		if (forward)
		{
			switch (curveType)
			{
			case CurveType.QuadraticCurve:
				return MathHelper.GetPointOnCurve(nodes.startPosition.Position, midPoint1, nodes.endPosition.Position, GetPercentProgress());
			case CurveType.CubicCurve:
				return MathHelper.GetPointOnCurve(nodes.startPosition.Position, midPoint1, midPoint2, nodes.endPosition.Position, GetPercentProgress());
			}
		}
		else
		{
			switch (curveType)
			{
			case CurveType.QuadraticCurve:
				return MathHelper.GetPointOnCurve(nodes.endPosition.Position, midPoint1, nodes.startPosition.Position, GetPercentProgress());
			case CurveType.CubicCurve:
				return MathHelper.GetPointOnCurve(nodes.endPosition.Position, midPoint1, midPoint2, nodes.startPosition.Position, GetPercentProgress());
			}
		}
		return Vector2.zero;
	}
}
