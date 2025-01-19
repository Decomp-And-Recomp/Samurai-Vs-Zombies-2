using UnityEngine;

[AddComponentMenu("Glui Action/Action Log")]
public class GluiActionLog : GluiActionLog_Base
{
	public override void Add_Handled(string action, GameObject sender, string senderName, IGluiActionHandler handler)
	{
	}

	public override void Add_Unhandled(string action, GameObject sender, string senderName)
	{
	}
}
