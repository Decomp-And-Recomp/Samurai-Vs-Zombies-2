using UnityEngine;

public class FingerInfo
{
	public bool IsActive;

	public bool WasActive;

	public bool IsFingerDown;

	public bool WasFingerDown;

	public Vector2 CursorPosition;

	public Vector2 OldCursorPosition;

	public Vector2 CursorDeltaMovement;

	public int TouchIndex;

	public bool WasCanceled;

	public float DurationActive;

	public Vector2 CursorStartPosition;

	public Vector2 CursorReleasedPosition;

	public FingerInfo()
	{
		ResetVariables();
	}

	public float DistanceFromCursorStart()
	{
		if (!IsActive)
		{
			return 0f;
		}
		return Vector2.Distance(CursorStartPosition, CursorPosition);
	}

	public void ResetVariables()
	{
		IsActive = false;
		WasActive = false;
		IsFingerDown = false;
		WasFingerDown = false;
		CursorPosition = new Vector2(0f, 0f);
		OldCursorPosition = new Vector2(0f, 0f);
		CursorDeltaMovement = new Vector2(0f, 0f);
		CursorStartPosition = new Vector2(-1f, -1f);
		CursorReleasedPosition = new Vector2(-1f, -1f);
		DurationActive = 0f;
		TouchIndex = -1;
		WasCanceled = false;
	}
}
