using System.Runtime.CompilerServices;

public class GluiStateProcesses : GluiStateProcessesBase
{
	public delegate void PhaseDoneHandler(GluiStatePhase phase);

	private GluiStatePhase phaseRunning;

	[method: MethodImpl(32)]
	public event PhaseDoneHandler Event_PhaseDone;

	public void Interrupt()
	{
		foreach (ProcessPhaseList phaseList in phaseLists)
		{
			phaseList.Interrupt();
		}
	}

	public void Stop()
	{
		foreach (ProcessPhaseList phaseList in phaseLists)
		{
			phaseList.Stop();
		}
	}

	public bool Start(GluiStatePhase phase, string stateName, bool reversed = false)
	{
		Interrupt();
		ProcessPhaseList processPhaseList = Find(phase);
		phaseRunning = phase;
		if (processPhaseList == null || processPhaseList.Processes.Count == 0)
		{
			return false;
		}
		foreach (GluiProcessBase process in processPhaseList.Processes)
		{
			if ((!reversed && process.ProcessStart(phase)) || (reversed && process.ProcessStartReversed(phase)))
			{
				processPhaseList.processesRunning++;
				process.Event_ProcessDone += HandleThisProcessEvent_ProcessDone;
			}
		}
		if (processPhaseList.processesRunning > 0)
		{
			return true;
		}
		return false;
	}

	public void Pause(bool pause)
	{
		foreach (ProcessPhaseList phaseList in phaseLists)
		{
			phaseList.Pause(pause);
		}
	}

	private void HandleThisProcessEvent_ProcessDone(GluiProcessBase process)
	{
		ProcessPhaseList processPhaseList = Find(phaseRunning);
		if (!processPhaseList.Processes.Contains(process))
		{
		}
		processPhaseList.processesRunning--;
		if (processPhaseList.processesRunning == 0)
		{
			OnPhaseDone(phaseRunning);
		}
		else if (processPhaseList.processesRunning >= 0)
		{
		}
	}

	protected void OnPhaseDone(GluiStatePhase phase)
	{
		if (this.Event_PhaseDone != null)
		{
			this.Event_PhaseDone(phase);
		}
	}

	public override void Clear()
	{
		base.Clear();
		this.Event_PhaseDone = null;
	}
}
