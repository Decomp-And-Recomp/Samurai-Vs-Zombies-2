using UnityEngine;

[AddComponentMenu("Glui Action/Listener - Method")]
public class GluiActionListener_Method : GluiListenerSingle
{
	public string methodToCall;

	protected override void OnTrigger(GameObject sender, object data)
	{
		CallMethod(methodToCall, sender, data);
	}
}
