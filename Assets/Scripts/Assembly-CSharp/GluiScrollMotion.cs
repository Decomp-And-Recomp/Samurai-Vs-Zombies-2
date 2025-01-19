using UnityEngine;

public class GluiScrollMotion
{
	public float flingDecay;

	private Vector2 fling = new Vector2(0f, 0f);

	private float holdTimer;

	public float dragStartDistance = 10f;

	private float dragDist;

	private Vector2 dragAnchor;

	private bool dragged;

	private bool autoScroll;

	private float autoScrollVertical;

	private float autoScrollHorizontal;

	private float timeTillAutoScroll = 3f;

	private float timeSinceTouched;

	public bool Moving
	{
		get
		{
			return fling != Vector2.zero;
		}
	}

	public bool Dragging
	{
		get
		{
			return dragged;
		}
	}

	public GluiScrollMotion(float flingDecay, bool autoScroll = false, float autoScrollVertical = 0f, float autoScrollHorizontal = 0f, float timeTillAutoScroll = 3f)
	{
		this.flingDecay = flingDecay;
		this.autoScroll = autoScroll;
		this.autoScrollVertical = autoScrollVertical;
		this.autoScrollHorizontal = autoScrollHorizontal;
		this.timeTillAutoScroll = timeTillAutoScroll;
	}

	public void Clear()
	{
		dragDist = 0f;
		fling = Vector2.zero;
	}

	public void ResetDrag(Vector2 position)
	{
		dragAnchor = position;
		dragged = false;
	}

	public void UpdateMotion(bool touchFocus, ref Vector2 offset, out bool offsetChanged)
	{
		if (dragged && fling.magnitude > 0f)
		{
			offset += fling;
			offsetChanged = true;
		}
		else
		{
			offsetChanged = false;
		}
		if (touchFocus)
		{
			if (holdTimer > 0f)
			{
				holdTimer -= Time.deltaTime;
				if (holdTimer <= 0f)
				{
					fling = new Vector2(0f, 0f);
				}
			}
			timeSinceTouched = 0f;
			return;
		}
		fling.Scale(new Vector2(flingDecay, flingDecay));
		if (fling.magnitude < 1f)
		{
			fling = new Vector2(0f, 0f);
		}
		if (fling == Vector2.zero)
		{
			dragged = false;
		}
		if (autoScroll)
		{
			if (timeSinceTouched > timeTillAutoScroll)
			{
				offset.x += autoScrollHorizontal * Time.deltaTime;
				offset.y += autoScrollVertical * Time.deltaTime;
				offsetChanged = true;
			}
			else
			{
				timeSinceTouched += Time.deltaTime;
			}
		}
	}

	public void Touching(Vector2 touchPosition, GluiScrollList.Direction direction, out bool dragStarted)
	{
		Vector2 vector = touchPosition - dragAnchor;
		dragStarted = false;
		switch (direction)
		{
		case GluiScrollList.Direction.Horizontal:
			vector.y = 0f;
			break;
		case GluiScrollList.Direction.Vertical:
			vector.x = 0f;
			break;
		}
		float magnitude = vector.magnitude;
		if (magnitude > 0f && magnitude > fling.magnitude)
		{
			fling = vector;
			holdTimer = 0.1f;
		}
		if (magnitude > 0f)
		{
			dragDist += magnitude;
			if (dragDist >= dragStartDistance && !dragged)
			{
				dragStarted = true;
				dragged = true;
			}
			dragAnchor = touchPosition;
		}
	}
}
