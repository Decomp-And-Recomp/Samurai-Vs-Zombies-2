using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Glui Agent/Central Dispatch")]
public class GluiAgent_CentralDispatch : SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>
{
	private Dictionary<string, GluiAgency> agencies = new Dictionary<string, GluiAgency>();

	public GluiAgency GetAgency(string key)
	{
		if (agencies.ContainsKey(key))
		{
			return agencies[key];
		}
		return null;
	}

	public void Add(GluiAgentBase agentToAdd)
	{
		GluiAgency gluiAgency = GetAgency(agentToAdd.Agency);
		if (gluiAgency == null)
		{
			gluiAgency = new GluiAgency();
			agencies.Add(agentToAdd.Agency, gluiAgency);
		}
		gluiAgency.Add(agentToAdd, base.gameObject);
	}

	public void Remove(GluiAgentBase agentToRemove)
	{
		GluiAgency agency = GetAgency(agentToRemove.Agency);
		if (agency != null)
		{
			agency.Remove(agentToRemove);
		}
	}

	public void SendOrder(string agencyName, List<GluiAgentBase> agentList, GluiAgentBase.Order order, GluiAgentBase.Order orderToNonMatching, GameObject sender, Object tag)
	{
		GluiAgency agency = GetAgency(agencyName);
		if (agency != null)
		{
			SendOrderPacket(agentList, new GluiOrderPacket(order, sender, tag));
			SendOrderPacket(agency.FindNot(agentList), new GluiOrderPacket(orderToNonMatching, sender, tag));
		}
	}

	public void SendOrder(string agencyName, List<AgentKey> agentIDList, GluiAgentBase.Order order, GluiAgentBase.Order orderToNonMatching, GameObject sender, Object tag)
	{
		GluiAgency agency = GetAgency(agencyName);
		if (agency != null)
		{
			List<GluiAgentBase> agentsNotMatching;
			SendOrderPacket(agency.Find(agentIDList, out agentsNotMatching), new GluiOrderPacket(order, sender, tag));
			SendOrderPacket(agentsNotMatching, new GluiOrderPacket(orderToNonMatching, sender, tag));
		}
	}

	public void SendOrder(string agencyName, List<AgentKey> agentIDList, GluiAgentBase.Order order, GameObject sender, Object tag)
	{
		GluiAgency agency = GetAgency(agencyName);
		if (agency != null)
		{
			SendOrderPacket(agency.Find(agentIDList), new GluiOrderPacket(order, sender, tag));
		}
	}

	public void SendSimpleOrder(string agencyName, string agentID, GluiAgentBase.Order order, GameObject sender, Object tag)
	{
		if (!string.IsNullOrEmpty(agentID))
		{
			GluiAgency agency = GetAgency(agencyName);
			if (agency != null)
			{
				SendOrderPacket(agency.Find(agentID), new GluiOrderPacket(order, sender, tag));
			}
		}
	}

	public void SendSimpleOrder(string agentID, GluiAgentBase.Order order, GameObject sender, Object tag)
	{
		if (string.IsNullOrEmpty(agentID))
		{
			return;
		}
		agencies.ForEachWithIndex(delegate(KeyValuePair<string, GluiAgency> entry, int index)
		{
			if (agentID == string.Empty)
			{
				SendOrderPacket(entry.Value.Agents, new GluiOrderPacket(order, sender, tag));
			}
			else
			{
				SendOrderPacket(entry.Value.Find(agentID), new GluiOrderPacket(order, sender, tag));
			}
		});
	}

	private void SendOrderPacket(List<GluiAgentBase> agents, GluiOrderPacket orderPacket)
	{
		if (orderPacket.order != GluiAgentBase.Order.None)
		{
			agents.ForEach(delegate(GluiAgentBase agent)
			{
				SendOrderPacket(agent, orderPacket);
			});
		}
	}

	private void SendOrderPacket(GluiAgentBase agent, GluiOrderPacket orderPacket)
	{
		if (!(agent == null) && orderPacket.order != GluiAgentBase.Order.None)
		{
			bool flag = agent.HandleOrder(orderPacket);
		}
	}
}
