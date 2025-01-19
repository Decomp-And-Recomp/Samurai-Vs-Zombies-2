using UnityEngine;

[AddComponentMenu("Glui State/Stack Machine")]
public class GluiStackMachine : GluiStateMachineBase, IGluiActionHandler
{
	public string ActionToPop = string.Empty;

	public string ActionToPopToTop = string.Empty;

	protected override StateChangeAction DefaultChangeStateAction
	{
		get
		{
			return StateChangeAction.Push;
		}
	}

	bool IGluiActionHandler.HandleAction(string action, GameObject sender, object data)
	{
		bool result = false;
		if (action == ActionToPop)
		{
			result = true;
			PopState();
		}
		else if (action == ActionToPopToTop)
		{
			result = true;
			PopToTopState();
		}
		else
		{
			GluiStateBase gluiStateBase = base.PotentialStates.StateForAction(action);
			if (gluiStateBase != null)
			{
				HandleStateAction(gluiStateBase, data, string.Equals(gluiStateBase.actionToHandleReverse, action));
				result = true;
			}
		}
		return result;
	}

	public void PopState()
	{
		if (nextStateAction != StateChangeAction.PopToState)
		{
			if (nextStateAction == StateChangeAction.Pop)
			{
				nextStatePopCount++;
			}
			else
			{
				nextStateAction = StateChangeAction.Pop;
				nextStatePopCount = 1;
			}
			if (history != null)
			{
				base.Phase = StateMachinePhase.Running;
				NextPhase();
			}
		}
	}

	public void PopToTopState()
	{
		if (history.Count != 0 && history != null && history.Count != 1)
		{
			PopToState(history[0].state);
		}
	}

	public void PopToState(string action)
	{
		GluiStateBase gluiStateBase = base.PotentialStates.StateForAction(action);
		if (gluiStateBase != null)
		{
			PopToState(gluiStateBase);
		}
	}

	public void PopToState(GluiStateBase state, bool reverseTransition = false)
	{
		nextStateAction = StateChangeAction.PopToState;
		nextState = new GluiStateHistoryNode(state, 0, null, reverseTransition);
		base.Phase = StateMachinePhase.Running;
		NextPhase();
	}

	protected virtual void HandleStateAction(GluiStateBase state, object data, bool reverseTransition)
	{
		ChangeState(new GluiStateHistoryNode(state, 0, data, reverseTransition), false);
	}
}
