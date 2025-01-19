using System;
using UnityEngine;

[Serializable]
public class GluiButtonOffsetAction : GluiButtonAction
{
	[SerializeField]
	public GameObject Target;

	[SerializeField]
	public Vector3 OffsetValue;

	private Vector3 OriginalValue;

	public override string GetActionName()
	{
		return "Offset";
	}

	public override void OnEnterState()
	{
		if (Target != null)
		{
			OriginalValue = Target.transform.localPosition;
			Target.transform.localPosition += OffsetValue;
		}
	}

	public override void OnLeaveState()
	{
		if (Target != null)
		{
			Target.transform.localPosition = OriginalValue;
		}
	}
}
