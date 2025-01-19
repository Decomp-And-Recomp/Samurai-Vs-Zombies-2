using UnityEngine;

[AddComponentMenu("Glui Agent/Agent Timeline")]
[RequireComponent(typeof(GluiTimeline))]
public class GluiAgent_Timeline : GluiAgentBase
{
	private Order order_For_Enable;

	private Order order_For_Disable = Order.Disable;

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

	public void Activity_Enable()
	{
		GetComponent<GluiTimeline>().StartTimeline(null);
	}

	public void Activity_Disable()
	{
		GetComponent<GluiTimeline>().StopTimeline();
	}
}
