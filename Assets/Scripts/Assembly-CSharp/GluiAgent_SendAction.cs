using UnityEngine;

[AddComponentMenu("Glui Agent/Agent Send Action")]
public class GluiAgent_SendAction : GluiAgentBase
{
	public string[] actionsToSend;

	public override bool HandleOrder(GluiOrderPacket orderPacket)
	{
		if (orderPacket.order == Order.Enable)
		{
			string[] array = actionsToSend;
			foreach (string text in array)
			{
				if (text != string.Empty)
				{
					GluiActionSender.SendGluiAction(text, base.gameObject, null);
				}
			}
			return true;
		}
		return false;
	}
}
