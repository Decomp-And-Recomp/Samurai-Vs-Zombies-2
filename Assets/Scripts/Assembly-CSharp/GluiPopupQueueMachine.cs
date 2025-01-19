using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Glui State/Popup Queue Machine")]
public class GluiPopupQueueMachine : GluiStackMachine, IGluiActionHandler
{
	public bool Add(string action, int priority = 0, object context = null)
	{
		GluiStateBase gluiStateBase = base.PotentialStates.StateForAction(action);
		if (gluiStateBase == null)
		{
			return false;
		}
		GluiStateHistoryNode newNode = new GluiStateHistoryNode(gluiStateBase, gluiStateBase.GetPriority(defaultMetadata), context, string.Equals(action, gluiStateBase.actionToHandleReverse));
		Add(newNode);
		return true;
	}

	public void Add(GluiStateHistoryNode newNode)
	{
		if (newNode.state == null)
		{
		}
		if (history.AttemptInsert(newNode))
		{
			return;
		}
		if (base.Phase == StateMachinePhase.Exiting || base.Phase == StateMachinePhase.SwitchingState)
		{
			if (nextState.priority > newNode.priority)
			{
				history.Push(newNode);
			}
			else
			{
				history.Push(nextState);
			}
		}
		ChangeState(newNode, false);
	}

	public void Remove(string name, bool removeAllInstances = false)
	{
		if (history.Count != 0)
		{
			List<GluiStateHistoryNode> list = history.Remove(name, removeAllInstances);
			if (history.Current.state.HandlesAction(name) && (removeAllInstances || list.Count == 0))
			{
				PopState();
			}
		}
	}

	public bool IsQueued(string action)
	{
		return history.Find(action) != null;
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		bool flag = false;
		if (action == ActionToPop)
		{
			flag = true;
			PopState();
		}
		else if (action == ActionToPopToTop)
		{
			flag = true;
			PopToTopState();
		}
		else
		{
			flag = Add(action, 0, data);
		}
		return flag;
	}
}
