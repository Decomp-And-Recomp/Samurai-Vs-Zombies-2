using UnityEngine;

[AddComponentMenu("Glui Agent/Agent Redraw GluiList")]
public class GluiAgent_Redraw_GluiList : GluiAgentBase
{
	public Order order_For_Redraw = Order.Redraw;

	public override bool HandleOrder(GluiOrderPacket orderPacket)
	{
		if (orderPacket.order == order_For_Redraw)
		{
			Activity_Redraw();
			return true;
		}
		return false;
	}

	public virtual void Activity_Redraw()
	{
		GluiList_Base[] componentsInChildren = GetComponentsInChildren<GluiList_Base>(false);
		foreach (GluiList_Base gluiList_Base in componentsInChildren)
		{
			gluiList_Base.Redraw();
		}
	}
}
