using UnityEngine;

public class GluiOrderPacket
{
	public GluiAgentBase.Order order;

	public GameObject sender;

	public Object tag;

	public GluiOrderPacket(GluiAgentBase.Order order, GameObject sender, Object tag)
	{
		this.order = order;
		this.sender = sender;
		this.tag = tag;
	}
}
