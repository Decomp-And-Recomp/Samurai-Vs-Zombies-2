using UnityEngine;

[AddComponentMenu("Glui Process/Process Delay")]
public class GluiProcess_Delay : GluiProcessStartAndStop
{
	public float secondsToWaitInit;

	public float secondsToWaitExit;

	private GluiStatePhase phaseRunning;

	public string actionInitDone = string.Empty;

	public string actionExitDone = string.Empty;

	public override bool ProcessStart(GluiStatePhase phase)
	{
		CancelInvoke();
		bool result = false;
		switch (phase)
		{
		case GluiStatePhase.Init:
			Invoke("Done", secondsToWaitInit);
			phaseRunning = GluiStatePhase.Init;
			result = true;
			break;
		case GluiStatePhase.Exit:
			Invoke("Done", secondsToWaitExit);
			phaseRunning = GluiStatePhase.Exit;
			result = true;
			break;
		}
		return result;
	}

	public override void ProcessInterrupt()
	{
		CancelInvoke();
		base.ProcessInterrupt();
	}

	private void SendDoneAction()
	{
		switch (phaseRunning)
		{
		case GluiStatePhase.Init:
			GluiActionSender.SendGluiAction(actionInitDone, base.gameObject, null);
			break;
		case GluiStatePhase.Exit:
			GluiActionSender.SendGluiAction(actionExitDone, base.gameObject, null);
			break;
		case GluiStatePhase.Running:
			break;
		}
	}

	private void Done()
	{
		SendDoneAction();
		ProcessDone();
	}
}
