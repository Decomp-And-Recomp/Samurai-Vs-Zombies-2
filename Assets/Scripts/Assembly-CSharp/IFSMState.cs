using System;

public interface IFSMState
{
	void Init(FSM fsm);

	void OnEnter(FSM fsm, int prevState);

	void OnExit(FSM fsm, int nextState);

	void OnUpdate(FSM fsm);

	void OnResumeFromInterrupt(FSM fsm);

	FSMStateData GetStateData();

	void SetStateData(FSMStateData data);

	Action GetCallBack(FSM.StateCallBackType type);

	void SetCallBack(FSM.StateCallBackType type, Action callback);
}
