using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Glui Process/Process Oscillate")]
public class GluiProcess_OscillateTransition : GluiProcessSingleState
{
	private List<GluiTransition> transitions;

	private List<GluiTransition> Transitions
	{
		get
		{
			if (transitions == null)
			{
				transitions = new List<GluiTransition>();
				transitions.AddRange(GetComponents<GluiTransition>());
				transitions.ForEach(delegate(GluiTransition thisSequence)
				{
					GluiTransition gluiTransition = thisSequence;
					gluiTransition.onDoneCallback = (Action<GluiTransition.Position>)Delegate.Combine(gluiTransition.onDoneCallback, (Action<GluiTransition.Position>)delegate(GluiTransition.Position position)
					{
						switch (position)
						{
						case GluiTransition.Position.End:
							thisSequence.Transition_Back();
							break;
						case GluiTransition.Position.Start:
							thisSequence.Transition_Forward();
							break;
						}
					});
				});
			}
			return transitions;
		}
	}

	public override bool ProcessStart(GluiStatePhase phase)
	{
		if (phase == phaseOfStateToAutomaticallyImplement)
		{
			DoPhaseInit();
		}
		return false;
	}

	public override void ProcessPause(bool pause)
	{
		List<GluiTransition> list = Transitions;
		list.ForEach(delegate(GluiTransition thisTransition)
		{
			thisTransition.Enabled = !pause;
		});
	}

	private void DoPhaseInit()
	{
		List<GluiTransition> list = Transitions;
		list.ForEach(delegate(GluiTransition thisTransition)
		{
			thisTransition.Transition_Forward();
		});
	}
}
