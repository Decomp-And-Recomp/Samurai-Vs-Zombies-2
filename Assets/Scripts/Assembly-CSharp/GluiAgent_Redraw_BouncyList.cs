using UnityEngine;

[AddComponentMenu("Glui Agent/Agent Redraw BouncyList")]
public class GluiAgent_Redraw_BouncyList : GluiAgentBase
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
		GluiBouncyScrollList[] componentsInChildren = GetComponentsInChildren<GluiBouncyScrollList>(false);
		foreach (GluiBouncyScrollList gluiBouncyScrollList in componentsInChildren)
		{
			gluiBouncyScrollList.Redraw(ID);
		}
	}
}
