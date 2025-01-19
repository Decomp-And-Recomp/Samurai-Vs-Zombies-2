using System.Collections.Generic;
using UnityEngine;

public class GluiAgency
{
	public enum AgentFilter
	{
		AllAgentsInAgency = 0,
		Random_OneAgentInAgency = 1
	}

	private List<GluiAgentBase> agents = new List<GluiAgentBase>();

	private GluiAgencyPersistentOrder persistentOrder;

	public List<GluiAgentBase> Agents
	{
		get
		{
			return agents;
		}
	}

	public void PersistentOrder_Set(GluiAgencyPersistentOrder persistentOrder, GameObject sender)
	{
		this.persistentOrder = persistentOrder;
		foreach (GluiAgentBase agent in agents)
		{
			persistentOrder.UpdateAgent(agent, sender);
		}
	}

	public void PersistentOrder_Clear(GameObject sender)
	{
		if (persistentOrder != null)
		{
			persistentOrder.ClearOrder(agents, sender);
			persistentOrder = null;
		}
	}

	public void Add(GluiAgentBase agent, GameObject sender)
	{
		agents.Add(agent);
		if (persistentOrder != null)
		{
			persistentOrder.UpdateAgent(agent, sender);
		}
	}

	public void Remove(GluiAgentBase agent)
	{
		agents.Remove(agent);
	}

	public List<GluiAgentBase> Find(string agentID)
	{
		List<GluiAgentBase> list = agents.FindAll((GluiAgentBase agent) => agentID == agent.ID);
		if (list == null)
		{
		}
		return list;
	}

	public List<GluiAgentBase> Find(List<AgentKey> agentKeys)
	{
		return agents.FindAll((GluiAgentBase agent) => agentKeys.Find((AgentKey key) => key.hash == agent.ID) != null);
	}

	public List<GluiAgentBase> Find(List<AgentKey> agentKeys, out List<GluiAgentBase> agentsNotMatching)
	{
		List<GluiAgentBase> agents = new List<GluiAgentBase>();
		List<GluiAgentBase> agentsFalse = new List<GluiAgentBase>();
		agents.ForEach(delegate(GluiAgentBase agent)
		{
			if (agentKeys.Find((AgentKey key) => key.hash == agent.ID) != null)
			{
				agents.Add(agent);
			}
			else
			{
				agentsFalse.Add(agent);
			}
		});
		agentsNotMatching = agentsFalse;
		return agents;
	}

	public List<GluiAgentBase> Find(List<string> agentIDList)
	{
		return agents.FindAll((GluiAgentBase agent) => agentIDList.Contains(agent.ID));
	}

	public List<GluiAgentBase> Find(List<string> agentIDList, out List<GluiAgentBase> agentsNotMatching)
	{
		List<GluiAgentBase> agents = new List<GluiAgentBase>();
		List<GluiAgentBase> agentsFalse = new List<GluiAgentBase>();
		agents.ForEach(delegate(GluiAgentBase agent)
		{
			if (agentIDList.Contains(agent.ID))
			{
				agents.Add(agent);
			}
			else
			{
				agentsFalse.Add(agent);
			}
		});
		agentsNotMatching = agentsFalse;
		return agents;
	}

	public List<GluiAgentBase> Find(AgentFilter filter)
	{
		List<GluiAgentBase> list = new List<GluiAgentBase>();
		switch (filter)
		{
		case AgentFilter.AllAgentsInAgency:
			return agents;
		case AgentFilter.Random_OneAgentInAgency:
			if (agents.Count > 0)
			{
				int index = Random.Range(0, agents.Count);
				list.Add(agents[index]);
			}
			break;
		}
		return list;
	}

	public List<GluiAgentBase> FindNot(List<GluiAgentBase> agentList)
	{
		return agents.FindAll((GluiAgentBase agent) => !agentList.Contains(agent));
	}
}
