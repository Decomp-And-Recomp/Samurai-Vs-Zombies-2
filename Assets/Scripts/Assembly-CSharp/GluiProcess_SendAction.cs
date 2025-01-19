using UnityEngine;

[AddComponentMenu("Glui Process/Process Send Action")]
public class GluiProcess_SendAction : GluiProcessSingleState
{
	public string[] actions;

	public override bool ProcessStart(GluiStatePhase phase)
	{
		string[] array = actions;
		foreach (string action in array)
		{
			GluiActionSender.SendGluiAction(action, base.gameObject, null);
		}
		return false;
	}
}
