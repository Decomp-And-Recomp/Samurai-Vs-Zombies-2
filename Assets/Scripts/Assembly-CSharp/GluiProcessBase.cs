using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class GluiProcessBase : MonoBehaviour
{
	public delegate void ProcessDoneHandler(GluiProcessBase process);

	[method: MethodImpl(32)]
	public event ProcessDoneHandler Event_ProcessDone;

	public abstract bool AnyPhaseHandled();

	public abstract bool IsPhaseHandled(GluiStatePhase phase);

	public abstract GluiStatePhase[] PhasesHandled();

	public abstract bool ProcessStart(GluiStatePhase phase);

	public virtual bool ProcessStartReversed(GluiStatePhase phase)
	{
		return ProcessStart(phase);
	}

	public virtual void ProcessInterrupt()
	{
		this.Event_ProcessDone = null;
	}

	public virtual void ProcessDone()
	{
		OnProcessDone();
	}

	public virtual void ProcessPause(bool pause)
	{
	}

	protected void OnProcessDone()
	{
		if (this.Event_ProcessDone != null)
		{
			this.Event_ProcessDone(this);
			this.Event_ProcessDone = null;
		}
	}
}
