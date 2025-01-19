using UnityEngine;

public class CheckBundlesExistState : DoNothingState
{
	private bool fileExists;

	private UpdateCheckData objData;

	public override void Init(FSM fsm)
	{
	}

	public override void OnEnter(FSM fsm, int prevState)
	{
		objData = (UpdateCheckData)fsm.GetOwnerObject();
	}

	public override void OnExit(FSM fsm, int nextState)
	{
	}

	public override void OnUpdate(FSM fsm)
	{
		if (!fileExists || objData.ForceUpdate || !PlayerPrefs.HasKey("CachedVersion") || PlayerPrefs.GetString("CachedVersion") != GeneralConfig.Version)
		{
			fsm.QueueState(3);
		}
		else
		{
			fsm.QueueState(2);
		}
	}
}
