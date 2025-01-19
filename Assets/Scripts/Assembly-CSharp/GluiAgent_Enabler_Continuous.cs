using UnityEngine;

[AddComponentMenu("Glui Agent/Agent Enabler Continuous")]
public class GluiAgent_Enabler_Continuous : GluiAgent_Enabler
{
	public bool? isEnabling;

	public bool enforceEnabled = true;

	public bool enforceDisabled = true;

	public override void Activity_Enable()
	{
		base.Activity_Enable();
		isEnabling = true;
	}

	public override void Activity_Disable()
	{
		base.Activity_Disable();
		isEnabling = false;
	}

	public void LateUpdate()
	{
		if (!isEnabling.HasValue)
		{
			return;
		}
		if (isEnabling.Value)
		{
			if (enforceEnabled)
			{
				Activity_Enable();
			}
		}
		else if (enforceDisabled)
		{
			Activity_Disable();
		}
	}
}
