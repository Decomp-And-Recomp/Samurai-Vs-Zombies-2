using System;
using UnityEngine;

[Serializable]
public class GluiOrderToSend
{
	public string agencyFilter;

	public string agentID;

	public GluiAgentBase.Order order;

	public void Send(GameObject sender)
	{
		if (agencyFilter != string.Empty)
		{
			SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder(agencyFilter, agentID, order, sender, null);
		}
		else
		{
			SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder(agentID, order, sender, null);
		}
	}
}
