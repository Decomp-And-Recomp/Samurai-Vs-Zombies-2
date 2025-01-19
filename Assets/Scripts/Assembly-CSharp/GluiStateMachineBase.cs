using UnityEngine;

public abstract class GluiStateMachineBase : GluiListenerBase, IInputContainer
{
	private GluiPotentialStates potentialStates;

	private bool firstState = true;

	public string defaultState = string.Empty;

	public bool defaultSkipsOnStartTransition;

	public bool defaultToPersistedStateName;

	public string PersistCurrentStateName = string.Empty;

	public StateMachineInputExclusivity inputExclusivity;

	protected StateChangeAction nextStateAction;

	protected int nextStatePopCount = 1;

	protected GluiStateHistoryNode nextState;

	protected StateMachinePhase nextStateFirstStatus = StateMachinePhase.None;

	protected GluiReaction reactionOnPhaseDone;

	private StateMachinePhase status = StateMachinePhase.None;

	protected GluiStateHistory history = new GluiStateHistory();

	[DataBundleSchemaFilter(typeof(GluiState_MetadataSchema), false)]
	[HideInInspector]
	public DataBundleRecordKey defaultMetadata;

	public GluiPotentialStates PotentialStates
	{
		get
		{
			if (potentialStates == null)
			{
				ScanForPotentialStates();
			}
			return potentialStates;
		}
	}

	protected StateMachinePhase Phase
	{
		get
		{
			return status;
		}
		set
		{
			status = value;
		}
	}

	public bool IsCurrentDefaultState
	{
		get
		{
			return history.Current == null || history.Current.state.name == defaultState;
		}
	}

	protected GluiState_MetadataSchema DefaultMetadata
	{
		get
		{
			if (defaultMetadata != (DataBundleRecordKey)string.Empty)
			{
				return DataBundleRuntime.Instance.InitializeRecord<GluiState_MetadataSchema>(defaultMetadata);
			}
			return null;
		}
	}

	protected abstract StateChangeAction DefaultChangeStateAction { get; }

	public void Awake()
	{
		ScanForPotentialStates();
	}

	public void Start()
	{
		DefaultState();
	}

	private void ScanForPotentialStates()
	{
		if (!(potentialStates != null))
		{
			potentialStates = base.gameObject.GetComponent(typeof(GluiPotentialStates)) as GluiPotentialStates;
			if (potentialStates == null)
			{
				potentialStates = base.gameObject.AddComponent(typeof(GluiPotentialStates)) as GluiPotentialStates;
			}
			potentialStates.ScanForStates();
		}
	}

	private void DefaultState()
	{
		if (!firstState)
		{
			return;
		}
		if (defaultToPersistedStateName && !string.IsNullOrEmpty(PersistCurrentStateName))
		{
			string text = SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData(PersistCurrentStateName) as string;
			if (!string.IsNullOrEmpty(text))
			{
				ChangeState(new GluiStateHistoryNode(potentialStates.StateData(text)), defaultSkipsOnStartTransition);
				return;
			}
		}
		if (!string.IsNullOrEmpty(defaultState))
		{
			ChangeState(new GluiStateHistoryNode(potentialStates.StateData(defaultState)), defaultSkipsOnStartTransition);
		}
	}

	public void ChangeState(string newState)
	{
		ChangeState(new GluiStateHistoryNode(potentialStates.StateData(newState)), false);
	}

	public GluiStateBase GetCurrentState()
	{
		GluiStateHistoryNode current = history.Current;
		return current.state;
	}

	protected virtual void ChangeState(GluiStateHistoryNode newState, bool skipOnStart)
	{
		if (Phase == StateMachinePhase.Exiting || newState == null)
		{
			return;
		}
		firstState = false;
		GluiStateHistoryNode current = history.Current;
		if (current != null && !(current.state != newState.state))
		{
			return;
		}
		nextState = newState;
		nextStateAction = DefaultChangeStateAction;
		if (history.Current != null)
		{
			history.Current.reverseTransition = newState.reverseTransition;
		}
		if (skipOnStart)
		{
			nextStateFirstStatus = StateMachinePhase.Starting;
		}
		else
		{
			nextStateFirstStatus = StateMachinePhase.BeforeStart;
		}
		if (current == null)
		{
			Phase = StateMachinePhase.SwitchingState;
			if (DataBundleRuntime.Instance != null)
			{
				NextPhase();
			}
		}
		else if (Phase != StateMachinePhase.SwitchingState && Phase != StateMachinePhase.Exiting)
		{
			Phase = StateMachinePhase.Running;
			NextPhase();
		}
	}

	protected void Update()
	{
		if (Phase == StateMachinePhase.SwitchingState)
		{
			NextPhase();
		}
	}

	protected void NextPhase()
	{
		bool flag = false;
		do
		{
			switch (Phase)
			{
			case StateMachinePhase.Exiting:
				Phase = StateMachinePhase.SwitchingState;
				flag = true;
				break;
			case StateMachinePhase.SwitchingState:
				if (history.Current == null || StartPhase(GluiStatePhase.Destroy, history.Current.state.processes, null, history.Current.state, history.Current.reverseTransition))
				{
				}
				DoStateChangeAction();
				flag = true;
				break;
			case StateMachinePhase.BeforeStart:
				flag = StartPhase(GluiStatePhase.Init, history.Current.state.processes, history.Current.state.onStart, history.Current.state, history.Current.reverseTransition);
				Phase = StateMachinePhase.Starting;
				break;
			case StateMachinePhase.Starting:
				flag = true;
				Phase = StateMachinePhase.Running;
				StartPhase(GluiStatePhase.Running, history.Current.state.processes, history.Current.state.onReady, history.Current.state, history.Current.reverseTransition);
				break;
			case StateMachinePhase.Running:
				Phase = StateMachinePhase.Exiting;
				flag = StartPhase(GluiStatePhase.Exit, history.Current.state.processes, history.Current.state.onExit, history.Current.state, history.Current.reverseTransition);
				break;
			}
		}
		while (!flag);
	}

	private void DoStateChangeAction()
	{
		GluiStateHistoryNode gluiStateHistoryNode = null;
		if (history.Current != null)
		{
			history.Current.state.processes.Clear();
		}
		switch (nextStateAction)
		{
		default:
			return;
		case StateChangeAction.Replace:
			history.Pop();
			gluiStateHistoryNode = history.Push(nextState);
			break;
		case StateChangeAction.Push:
			if (history.Current != null)
			{
				history.Current.state.DestroyState();
			}
			gluiStateHistoryNode = history.Push(nextState);
			break;
		case StateChangeAction.Pop:
		{
			for (int i = 0; i < nextStatePopCount; i++)
			{
				history.Pop();
			}
			gluiStateHistoryNode = CurrentOrDefault();
			break;
		}
		case StateChangeAction.PopToState:
			while (history.Count > 0 && history.Current.state != nextState.state)
			{
				history.Pop();
			}
			gluiStateHistoryNode = CurrentOrDefault();
			break;
		}
		nextStateAction = StateChangeAction.None;
		Phase = StateMachinePhase.BeforeLoad;
		if (gluiStateHistoryNode != null)
		{
			InitStateNode(gluiStateHistoryNode);
		}
	}

	private void InitStateNode(GluiStateHistoryNode nextNode)
	{
		if (PersistCurrentStateName != string.Empty)
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save(PersistCurrentStateName, nextNode.state.name);
		}
		nextNode.state.InitState(delegate(GameObject objPrefab)
		{
			if (objPrefab != null)
			{
				IMenuContextBase menuContextBase = (IMenuContextBase)ObjectUtils.FindComponent<IMenuContextBase>(objPrefab);
				if (menuContextBase != null)
				{
					GluiStateHistoryNode gluiStateHistoryNode = history.FindHistoryNodeByState(nextNode.state);
					if (gluiStateHistoryNode.context != null)
					{
						menuContextBase.SetMenuContext(gluiStateHistoryNode.context);
					}
					else
					{
						menuContextBase.SetMenuContext(history.PopCurrentContext());
					}
				}
			}
			Phase = nextStateFirstStatus;
			NextPhase();
		});
		ChangeInputExclusivity(nextNode);
	}

	protected override void ActionDone()
	{
		if (Phase == StateMachinePhase.Starting || Phase == StateMachinePhase.Exiting)
		{
			NextPhase();
		}
	}

	private bool StartPhase(GluiStatePhase phase, GluiStateProcesses processes, GluiReaction reaction, GluiStateBase state, bool reverseTransition)
	{
		bool flag = processes.Start(phase, state.name, reverseTransition);
		if (flag)
		{
			processes.Event_PhaseDone += HandleProcessesEvent_PhaseDone;
		}
		else
		{
			DoReaction(reaction);
		}
		return flag;
	}

	private void HandleProcessesEvent_PhaseDone(GluiStatePhase phase)
	{
		DoReaction(reactionOnPhaseDone);
		ActionDone();
	}

	private void DoReaction(GluiReaction reaction)
	{
		if (reaction != null)
		{
			GluiActionSender.SendGluiAction(reaction.actionOnDone, base.gameObject, null);
		}
	}

	protected virtual GluiStateHistoryNode PushDefaultState()
	{
		return history.Push(new GluiStateHistoryNode(potentialStates.StateData(defaultState)));
	}

	protected GluiStateHistoryNode CurrentOrDefault()
	{
		if (history.Count == 0)
		{
			if (defaultState == string.Empty)
			{
				return null;
			}
			return PushDefaultState();
		}
		return history.Current;
	}

	public bool IsCurrent(string action)
	{
		if (history.Count == 0)
		{
			return false;
		}
		return history.Current.state.HandlesAction(action);
	}

	public virtual void FilterInput(InputCrawl inputCrawl, GameObject objectToFilter, out InputRouter.InputResponse response)
	{
		if (status != StateMachinePhase.Running)
		{
			response = InputRouter.InputResponse.Blocked;
		}
		else
		{
			response = InputRouter.InputResponse.Unhandled;
		}
	}

	protected void DefaultExclusiveValues(out int layer, out InputLayerType layerType)
	{
		GluiState_MetadataSchema gluiState_MetadataSchema = DefaultMetadata;
		if (gluiState_MetadataSchema == null)
		{
			layer = 0;
			layerType = InputLayerType.Exclusive;
		}
		else
		{
			layer = DynamicEnum.ToIndex(gluiState_MetadataSchema.exclusiveLayer);
			layerType = gluiState_MetadataSchema.inputLayerType;
		}
	}

	private void ChangeInputExclusivity(GluiStateHistoryNode node)
	{
		int exclusiveLayer;
		InputLayerType inputLayerType;
		switch (inputExclusivity)
		{
		case StateMachineInputExclusivity.Never:
			break;
		case StateMachineInputExclusivity.Always:
			DefaultExclusiveValues(out exclusiveLayer, out inputLayerType);
			SingletonMonoBehaviour<InputManager>.Instance.AddExclusiveInputContainer(base.gameObject, exclusiveLayer, inputLayerType);
			break;
		case StateMachineInputExclusivity.IfNotDefaultState:
			DefaultExclusiveValues(out exclusiveLayer, out inputLayerType);
			if (node.state.name == defaultState)
			{
				SingletonMonoBehaviour<InputManager>.Instance.RemoveExclusiveInputContainer(base.gameObject, exclusiveLayer);
			}
			else
			{
				SingletonMonoBehaviour<InputManager>.Instance.AddExclusiveInputContainer(base.gameObject, exclusiveLayer, inputLayerType);
			}
			break;
		case StateMachineInputExclusivity.UseStateMetadata:
			node.state.GetInputExclusivity(defaultMetadata, out exclusiveLayer, out inputLayerType);
			if (inputLayerType == InputLayerType.Normal)
			{
				SingletonMonoBehaviour<InputManager>.Instance.RemoveExclusiveInputContainer(base.gameObject, -1);
			}
			else
			{
				SingletonMonoBehaviour<InputManager>.Instance.AddExclusiveInputContainer(base.gameObject, exclusiveLayer, inputLayerType);
			}
			break;
		}
	}
}
