using System;

public class DoNothingState : IFSMState
{
	protected Action[] callbacks = new Action[4];

	public FSMStateData StateData = new FSMStateData();

	public virtual void Init(FSM fsm)
	{
	}

	public virtual void OnEnter(FSM fsm, int prevState)
	{
	}

	public virtual void OnExit(FSM fsm, int nextState)
	{
	}

	public virtual void OnUpdate(FSM fsm)
	{
	}

	public virtual void OnResumeFromInterrupt(FSM fsm)
	{
	}

	public virtual Action GetCallBack(FSM.StateCallBackType type)
	{
		return callbacks[(int)type];
	}

	public virtual void SetCallBack(FSM.StateCallBackType type, Action callback)
	{
		callbacks[(int)type] = callback;
	}

	public virtual FSMStateData GetStateData()
	{
		return StateData;
	}

	public virtual void SetStateData(FSMStateData data)
	{
		StateData = data;
	}
}
