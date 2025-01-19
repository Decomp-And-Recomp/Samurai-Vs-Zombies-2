using UnityEngine;

[AddComponentMenu("Glui State/State Machine")]
public class GluiStateMachine : GluiStateMachineBase, IGluiActionHandler
{
	protected override StateChangeAction DefaultChangeStateAction
	{
		get
		{
			return StateChangeAction.Replace;
		}
	}

	bool IGluiActionHandler.HandleAction(string action, GameObject sender, object data)
	{
		bool result = false;
		GluiStateBase gluiStateBase = base.PotentialStates.StateForAction(action);
		if (gluiStateBase != null)
		{
			ChangeState(new GluiStateHistoryNode(gluiStateBase, 0, data, string.Equals(action, gluiStateBase.actionToHandleReverse)), false);
			result = true;
		}
		return result;
	}
}
