using System;
using UnityEngine;

public class GluiScrollSticky : MonoBehaviour
{
	public float stickyStrength = 5f;

	private GluiScrollList scrollList;

	private void Awake()
	{
		scrollList = GetComponent<GluiScrollList>();
		if (!(scrollList == null))
		{
			GluiScrollList gluiScrollList = scrollList;
			gluiScrollList.FlingOverride = (GluiScrollList.FlingOverrideHandler)Delegate.Combine(gluiScrollList.FlingOverride, new GluiScrollList.FlingOverrideHandler(StickyFling));
		}
	}

	private void StickyFling(GluiCursorState state, ref Vector2 fling)
	{
		if (state == GluiCursorState.Held && !(fling == Vector2.zero) && fling.magnitude < stickyStrength)
		{
			fling = Vector2.zero;
		}
	}
}
