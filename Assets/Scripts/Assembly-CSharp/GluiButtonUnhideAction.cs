using System;
using UnityEngine;

[Serializable]
public class GluiButtonUnhideAction : GluiButtonAction
{
	[SerializeField]
	public GluiWidget Target;

	private bool OriginalValue;

	public override string GetActionName()
	{
		return "Unhide";
	}

	public override void OnEnterState()
	{
		if (Target != null)
		{
			OriginalValue = Target.gameObject.activeSelf;
			Target.gameObject.SetActive(true);
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
