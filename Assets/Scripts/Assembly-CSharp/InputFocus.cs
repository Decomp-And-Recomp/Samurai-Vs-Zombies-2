using UnityEngine;

public class InputFocus
{
	private GameObject focusedObject;

	public bool HasFocusedObject
	{
		get
		{
			return focusedObject != null;
		}
	}

	public void SetFocusedObject(GameObject focusObject)
	{
		focusedObject = focusObject;
	}

	public void ClearFocusedObject(GameObject focusObject)
	{
		if (focusedObject == focusObject)
		{
			focusedObject = null;
		}
	}

	public InputTrace.HitInfo CreateFocusedHit()
	{
		InputTrace.HitInfo hitInfo = new InputTrace.HitInfo();
		hitInfo.camera = focusedObject.GetComponent<Camera>();
		hitInfo.target = focusedObject;
		return hitInfo;
	}
}
