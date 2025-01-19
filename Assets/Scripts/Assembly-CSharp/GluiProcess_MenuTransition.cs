using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Glui Process/Process MenuTransition")]
public class GluiProcess_MenuTransition : GluiProcessStartAndStop
{
	public float delayBeforeTransition;

	public float delayBeforeExitTransition;

	private List<GluiTransition> transitions;

	private int running;

	private GluiStatePhase phaseRunning;

	public string actionInitDone = string.Empty;

	public string actionExitDone = string.Empty;

	public bool runOnChildren;

	public GameObject[] childrenToIgnore;

	private List<GluiTransition> Transitions
	{
		get
		{
			if (transitions == null)
			{
				transitions = new List<GluiTransition>();
				if (runOnChildren)
				{
					transitions.AddRange(GetComponentsInChildren<GluiTransition>(true));
					if (childrenToIgnore != null && childrenToIgnore.Length > 0)
					{
						transitions.RemoveAll((GluiTransition t) => Array.Find(childrenToIgnore, (GameObject go) => go == t.gameObject) != null);
					}
				}
				else
				{
					transitions.AddRange(GetComponents<GluiTransition>());
				}
				transitions.ForEach(delegate(GluiTransition thisSequence)
				{
					thisSequence.onDoneCallback = (Action<GluiTransition.Position>)Delegate.Combine(thisSequence.onDoneCallback, new Action<GluiTransition.Position>(OnDoneCallback));
				});
			}
			return transitions;
		}
	}

	public override bool ProcessStart(GluiStatePhase phase)
	{
		bool flag = false;
		switch (phase)
		{
		case GluiStatePhase.Init:
			flag = DoPhase(DoPhaseInit, "DoPhaseInit", delayBeforeTransition);
			phaseRunning = GluiStatePhase.Init;
			break;
		case GluiStatePhase.Exit:
			flag = DoPhase(DoPhaseExit, "DoPhaseExit", delayBeforeExitTransition);
			phaseRunning = GluiStatePhase.Exit;
			break;
		}
		return running > 0 || flag;
	}

	public override bool ProcessStartReversed(GluiStatePhase phase)
	{
		bool flag = false;
		switch (phase)
		{
		case GluiStatePhase.Init:
			flag = DoPhase(DoPhaseInit_Reverse, "DoPhaseInit_Reverse", delayBeforeTransition);
			phaseRunning = GluiStatePhase.Init;
			break;
		case GluiStatePhase.Exit:
			flag = DoPhase(DoPhaseExit_Reverse, "DoPhaseExit_Reverse", delayBeforeExitTransition);
			phaseRunning = GluiStatePhase.Exit;
			break;
		}
		return running > 0 || flag;
	}

	private bool DoPhase(Action phaseMethod, string methodName, float delayBeforeTransition)
	{
		if (delayBeforeTransition > 0f)
		{
			if (Transitions.Count > 0)
			{
				Invoke(methodName, delayBeforeTransition);
				return true;
			}
		}
		else
		{
			phaseMethod();
		}
		return false;
	}

	private void DoPhaseInit()
	{
		List<GluiTransition> list = Transitions;
		running = 0;
		list.ForEach(delegate(GluiTransition thisTransition)
		{
			if (thisTransition.gameObject.activeInHierarchy && thisTransition.Transition_Forward(false))
			{
				running++;
			}
		});
	}

	private void DoPhaseInit_Reverse()
	{
		List<GluiTransition> list = Transitions;
		running = 0;
		list.ForEach(delegate(GluiTransition thisTransition)
		{
			if (thisTransition.gameObject.activeInHierarchy && thisTransition.Transition_Back(true))
			{
				running++;
			}
		});
	}

	private void DoPhaseExit()
	{
		List<GluiTransition> list = Transitions;
		running = 0;
		list.ForEach(delegate(GluiTransition thisTransition)
		{
			if (thisTransition.gameObject.activeInHierarchy && thisTransition.Transition_Back(false))
			{
				running++;
			}
		});
	}

	private void DoPhaseExit_Reverse()
	{
		List<GluiTransition> list = Transitions;
		running = 0;
		list.ForEach(delegate(GluiTransition thisTransition)
		{
			if (thisTransition.gameObject.activeInHierarchy && thisTransition.Transition_Forward(true))
			{
				running++;
			}
		});
	}

	public override void ProcessInterrupt()
	{
		base.ProcessInterrupt();
		DoPhaseExit();
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

	private void OnDoneCallback(GluiTransition.Position position)
	{
		running--;
		if (running == 0)
		{
			SendDoneAction();
			ProcessDone();
		}
	}
}
