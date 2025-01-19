public class ReconnectAllState : DoNothingState
{
	public override void Init(FSM fsm)
	{
	}

	public override void OnEnter(FSM fsm, int prevState)
	{
	}

	public override void OnExit(FSM fsm, int nextState)
	{
	}

	public override void OnUpdate(FSM fsm)
	{
		fsm.QueueState(9);
	}
}
