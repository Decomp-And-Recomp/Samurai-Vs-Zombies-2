using System;
using UnityEngine;

[AddComponentMenu("Effect Maestro/Motion Path")]
public class MotionPath : MonoBehaviour
{
	public enum MotionStyle
	{
		Linear = 0,
		EaseIn = 1,
		EaseOut = 2,
		EaseInAndOut = 3
	}

	public MotionPathNodes nodes;

	public float durationForward = 1f;

	public float durationBackward = 1f;

	public MotionStyle motionStyle;

	public bool useScale;

	public float minScale = 1f;

	public float maxScale = 1f;

	protected float duration;

	protected float elapsedTime;

	protected bool forward;

	protected bool pathActive;

	public bool PathActive
	{
		get
		{
			return pathActive;
		}
	}

	public void Awake()
	{
		base.enabled = false;
	}

	public Vector2 GoForward(GameObject owner)
	{
		forward = true;
		NewTransition(owner);
		duration = durationForward;
		return nodes.startPosition.Position;
	}

	public Vector2 GoBackward(GameObject owner)
	{
		forward = false;
		NewTransition(owner);
		duration = durationBackward;
		return nodes.endPosition.Position;
	}

	public void Stop()
	{
		pathActive = false;
	}

	protected virtual void NewTransition(GameObject owner)
	{
		pathActive = true;
		elapsedTime = 0f;
		nodes.UpdateDefaultPosition(owner);
	}

	public virtual Vector2 UpdatePosition()
	{
		if (!pathActive)
		{
			return Vector2.zero;
		}
		elapsedTime += Time.deltaTime;
		if (elapsedTime > duration)
		{
			pathActive = false;
			if (forward)
			{
				return nodes.endPosition.Position;
			}
			return nodes.startPosition.Position;
		}
		return GetPosition();
	}

	public virtual float UpdateScale()
	{
		if (!pathActive)
		{
			return 1f;
		}
		if (minScale == maxScale)
		{
			return minScale;
		}
		float num = elapsedTime / duration;
		return minScale + Mathf.Sin(num * (float)Math.PI) * (maxScale - minScale);
	}

	protected float GetPercentProgress()
	{
		float result = elapsedTime / duration;
		switch (motionStyle)
		{
		case MotionStyle.EaseIn:
			result = Mathfx.Coserp(0f, 1f, elapsedTime / duration);
			break;
		case MotionStyle.EaseOut:
			result = Mathfx.Sinerp(0f, 1f, elapsedTime / duration);
			break;
		case MotionStyle.EaseInAndOut:
			result = Mathfx.Hermite(0f, 1f, elapsedTime / duration);
			break;
		}
		return result;
	}

	protected virtual Vector2 GetPosition()
	{
		if (forward)
		{
			return Vector2.Lerp(nodes.startPosition.Position, nodes.endPosition.Position, GetPercentProgress());
		}
		return Vector2.Lerp(nodes.endPosition.Position, nodes.startPosition.Position, GetPercentProgress());
	}
}
