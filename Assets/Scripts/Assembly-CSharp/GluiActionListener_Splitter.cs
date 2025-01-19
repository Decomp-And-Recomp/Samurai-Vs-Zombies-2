using UnityEngine;

[AddComponentMenu("Glui Action/Listener - Splitter")]
public class GluiActionListener_Splitter : GluiListenerSingle
{
	public string[] actionsToSend;

	protected override void OnTrigger(GameObject sender, object data)
	{
		string[] array = actionsToSend;
		foreach (string text in array)
		{
			if (text != string.Empty)
			{
				GluiActionSender.SendGluiAction(text, sender, data);
			}
		}
	}
}
