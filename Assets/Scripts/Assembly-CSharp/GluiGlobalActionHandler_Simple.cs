using UnityEngine;

[AddComponentMenu("Glui Action/Global Handler - Simple")]
public class GluiGlobalActionHandler_Simple : GluiGlobalActionHandler
{
	public override bool HandleGlobalAction(string action, GameObject sender, object data)
	{
		return false;
	}
}
