using System;
using UnityEngine;

[Serializable]
public class GluiButtonScaleAction : GluiButtonAction
{
	[SerializeField]
	public GameObject Target;

	[SerializeField]
	public Vector3 ScaleValue = new Vector3(1f, 1f, 1f);

	private Vector3 OriginalValue;

	public override string GetActionName()
	{
		return "Scale";
	}

	public override void OnEnterState()
	{
		if (Target != null)
		{
			OriginalValue = Target.transform.localScale;
			Target.transform.localScale = ScaleValue;
		}
	}

	public override void OnLeaveState()
	{
		if (Target != null)
		{
			Target.transform.localScale = OriginalValue;
		}
	}
}
