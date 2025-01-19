using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GluiButtonShaderAction : GluiButtonAction
{
	[SerializeField]
	public GameObject Target;

	[SerializeField]
	public GluiWidget.ShaderType ShaderType = GluiWidget.ShaderType.Auto_GluiAlphaBlend_Desaturate;

	private Dictionary<int, GluiWidget.ShaderType> OriginalValues = new Dictionary<int, GluiWidget.ShaderType>();

	public override string GetActionName()
	{
		return "Shader";
	}

	public override void OnEnterState()
	{
		OriginalValues.Clear();
		ShadeWidget(Target, ShaderType);
	}

	public override void OnLeaveState()
	{
		UnshadeWidget(Target);
	}

	private void ShadeWidget(GameObject target, GluiWidget.ShaderType shader)
	{
		if (!(target == null))
		{
			GluiWidget component = target.GetComponent<GluiWidget>();
			if (component != null)
			{
				int instanceID = component.gameObject.GetInstanceID();
				OriginalValues.Add(instanceID, component.AutoShader);
				component.AutoShader = shader;
			}
			for (int i = 0; i < target.gameObject.transform.childCount; i++)
			{
				ShadeWidget(target.gameObject.transform.GetChild(i).gameObject, shader);
			}
		}
	}

	private void UnshadeWidget(GameObject target)
	{
		if (!(target == null))
		{
			GluiWidget component = target.GetComponent<GluiWidget>();
			if (component != null)
			{
				int instanceID = component.gameObject.GetInstanceID();
				component.AutoShader = OriginalValues[instanceID];
			}
			for (int i = 0; i < target.gameObject.transform.childCount; i++)
			{
				UnshadeWidget(target.gameObject.transform.GetChild(i).gameObject);
			}
		}
	}
}
