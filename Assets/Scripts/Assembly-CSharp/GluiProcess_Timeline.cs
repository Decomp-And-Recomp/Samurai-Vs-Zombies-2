using UnityEngine;

[AddComponentMenu("Glui Process/Process Action Timeline")]
[RequireComponent(typeof(GluiTimeline))]
public class GluiProcess_Timeline : GluiProcessSingleState
{
	public override bool ProcessStart(GluiStatePhase phase)
	{
		return GetComponent<GluiTimeline>().StartTimeline(ProcessDone);
	}
}
