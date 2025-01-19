using System;
using UnityEngine;

[Serializable]
public class GluiButtonHideAction : GluiButtonAction
{
	[SerializeField]
	public GluiWidget Target;

	private bool OriginalValue;

	public override string GetActionName()
	{
		return "Hide";
	}

	public override void OnEnterState()
	{
		if (Target != null)
		{
			OriginalValue = Target.gameObject.activeSelf;
			Target.gameObject.SetActive(false);
		}
	}

	public override void OnLeaveState()
	{
		if (Target != null)
		{
			Target.gameObject.SetActive(OriginalValue);
		}
	}
}
