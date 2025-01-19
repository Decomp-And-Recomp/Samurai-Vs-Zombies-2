using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Glui Action/Listener - Send Agency Order")]
public class GluiActionListener_SendAgencyOrder : GluiListenerSingle
{
	public string agency;

	public GluiAgency.AgentFilter agentFilter;

	public GluiAgentBase.Order orderToSend;

	public GluiAgentBase.Order orderToSend_NotMatching = GluiAgentBase.Order.None;

	protected override void OnTrigger(GameObject sender, object data)
	{
		if (!(agency == string.Empty))
		{
			GluiAgency gluiAgency = SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.GetAgency(agency);
			if (gluiAgency != null)
			{
				List<GluiAgentBase> agentList = gluiAgency.Find(agentFilter);
				SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendOrder(agency, agentList, orderToSend, orderToSend_NotMatching, base.gameObject, null);
			}
		}
	}
}
