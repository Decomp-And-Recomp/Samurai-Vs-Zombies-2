using UnityEngine;

[AddComponentMenu("Glui Action/Listener - Send Agent Order")]
public class GluiActionListener_SendAgentOrder : GluiListenerSingle
{
	public string agencyFilter;

	public string agentID;

	public GluiAgentBase.Order orderToSend;

	protected override void OnTrigger(GameObject sender, object data)
	{
		if (agencyFilter != string.Empty)
		{
			SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder(agencyFilter, agentID, orderToSend, base.gameObject, null);
		}
		else
		{
			SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.SendSimpleOrder(agentID, orderToSend, base.gameObject, null);
		}
	}
}
