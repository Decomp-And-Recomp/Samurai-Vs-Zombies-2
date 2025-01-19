using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GluiButtonTintAction : GluiButtonAction
{
	[SerializeField]
	public GameObject Target;

	[SerializeField]
	public Color TintColor = new Color(1f, 1f, 1f, 1f);

	[SerializeField]
	public bool IgnoreChildren;

	private Dictionary<int, Color> OriginalValues = new Dictionary<int, Color>();

	public override string GetActionName()
	{
		return "Tint";
	}

	public override void OnEnterState()
	{
		OriginalValues.Clear();
		TintWidget(Target, TintColor);
	}

	public override void OnLeaveState()
	{
		UntintWidget(Target);
	}

	private void TintWidget(GameObject target, Color color)
	{
		if (target == null)
		{
			return;
		}
		GluiWidget component = target.GetComponent<GluiWidget>();
		if (component != null)
		{
			int instanceID = component.gameObject.GetInstanceID();
			OriginalValues.Add(instanceID, component.Color);
			component.Color = color;
		}
		if (!IgnoreChildren)
		{
			for (int i = 0; i < target.gameObject.transform.childCount; i++)
			{
				TintWidget(target.gameObject.transform.GetChild(i).gameObject, color);
			}
		}
	}

	private void UntintWidget(GameObject target)
	{
		if (target == null)
		{
			return;
		}
		GluiWidget component = target.GetComponent<GluiWidget>();
		if (component != null)
		{
			int instanceID = component.gameObject.GetInstanceID();
			component.Color = OriginalValues[instanceID];
		}
		if (!IgnoreChildren)
		{
			for (int i = 0; i < target.gameObject.transform.childCount; i++)
			{
				UntintWidget(target.gameObject.transform.GetChild(i).gameObject);
			}
		}
	}
}
