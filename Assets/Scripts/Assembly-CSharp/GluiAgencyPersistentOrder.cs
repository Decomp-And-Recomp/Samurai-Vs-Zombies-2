using System.Collections.Generic;
using UnityEngine;

public class GluiAgencyPersistentOrder
{
	public List<string> agents = new List<string>();

	public GluiAgentBase.Order order;

	public GluiAgentBase.Order order_Mismatch;

	public bool reverseOnClear;

	private string dataKey;

	public GluiAgencyPersistentOrder(GluiAgentBase.Order order, GluiAgentBase.Order order_Mismatch, bool reverseOnClear = false)
	{
		this.order = order;
		this.order_Mismatch = order_Mismatch;
		this.reverseOnClear = reverseOnClear;
	}

	public void UpdateAgent(GluiAgentBase agent, GameObject sender)
	{
		if (agents.Contains(agent.ID))
		{
			DoOrder(agent, sender, order);
		}
		else
		{
			DoOrder(agent, sender, order_Mismatch);
		}
	}

	public void ClearOrder(List<GluiAgentBase> agentsAffected, GameObject sender)
	{
		if (!reverseOnClear)
		{
			return;
		}
		foreach (GluiAgentBase item in agentsAffected)
		{
			if (agents.Contains(item.ID))
			{
				DoOrder(item, sender, order_Mismatch);
			}
			else
			{
				DoOrder(item, sender, order);
			}
		}
	}

	private void DoOrder(GluiAgentBase agent, GameObject sender, GluiAgentBase.Order order)
	{
		agent.HandleOrder(new GluiOrderPacket(order, sender, null));
	}
}
