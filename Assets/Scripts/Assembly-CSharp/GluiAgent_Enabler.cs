using UnityEngine;

[AddComponentMenu("Glui Agent/Agent Enabler")]
public class GluiAgent_Enabler : GluiAgentBase
{
	public enum EnableType
	{
		VISIBLE = 0,
		SILENT = 1
	}

	private Order order_For_Enable;

	private Order order_For_Disable = Order.Disable;

	public EnableType visibility;

	public override bool HandleOrder(GluiOrderPacket orderPacket)
	{
		if (orderPacket.order == order_For_Enable)
		{
			Activity_Enable();
			return true;
		}
		if (orderPacket.order == order_For_Disable)
		{
			Activity_Disable();
			return true;
		}
		return false;
	}

	public virtual void Activity_Enable()
	{
		GluiWidget gluiWidget = GetComponent(typeof(GluiWidget)) as GluiWidget;
		if (gluiWidget != null)
		{
			switch (visibility)
			{
			case EnableType.VISIBLE:
				gluiWidget.Enabled = true;
				break;
			case EnableType.SILENT:
				gluiWidget.AllowInput = true;
				break;
			}
		}
	}

	public virtual void Activity_Disable()
	{
		GluiWidget gluiWidget = GetComponent(typeof(GluiWidget)) as GluiWidget;
		switch (visibility)
		{
		case EnableType.VISIBLE:
			gluiWidget.Enabled = false;
			break;
		case EnableType.SILENT:
			gluiWidget.AllowInput = false;
			break;
		}
	}
}
