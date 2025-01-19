using System;

public class UpdateCheckMonoBehavior : SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>
{
	private FSM updateCheckStateSystem;

	private UpdateCheckData updateCheckData;

	private Action<UpdateSystem.UpdateError> onComplete;

	public static bool IsUpdateCheckRunning { get; private set; }

	public bool IsDone { get; private set; }

	public UpdateCheckData InternalData
	{
		get
		{
			return updateCheckData;
		}
	}

	public bool IsUpdateAvailable()
	{
		return (!string.IsNullOrEmpty(updateCheckData.MainDatabasePointer) && !string.IsNullOrEmpty(updateCheckData.MainDatabaseBuild)) || (!string.IsNullOrEmpty(updateCheckData.LangDatabasePointer) && !string.IsNullOrEmpty(updateCheckData.LangDatabaseBuild));
	}

	public void StartUpdateCheck(Action<UpdateSystem.UpdateError> onComplete = null)
	{
		StartState(UpdateCheckState.CheckBundlesExist, onComplete);
	}

	public void DoUpdates(Action<UpdateSystem.UpdateError> onComplete = null)
	{
		StartState(UpdateCheckState.DisconnectAll, onComplete);
	}

	public void DoFinalChecks(Action<UpdateSystem.UpdateError> onComplete = null)
	{
		StartState(UpdateCheckState.VerifyDBValidity, onComplete);
	}

	private void StartState(UpdateCheckState state, Action<UpdateSystem.UpdateError> onComplete = null)
	{
		IsDone = false;
		this.onComplete = onComplete;
		if (updateCheckData != null && updateCheckStateSystem != null)
		{
			updateCheckStateSystem.QueueState((int)state);
		}
		else
		{
			IsDone = true;
		}
	}

	private void Reset()
	{
		updateCheckStateSystem = null;
		updateCheckData = null;
		IsUpdateCheckRunning = false;
	}

	public void Initialize()
	{
		IsUpdateCheckRunning = true;
		updateCheckData = new UpdateCheckData();
		updateCheckStateSystem = new FSM();
		updateCheckData.FileCount = 4;
		updateCheckStateSystem.Init(updateCheckData);
		updateCheckStateSystem.RegisterState(1, new DisconnectAllState());
		updateCheckStateSystem.RegisterState(4, new CheckBundlesExistState());
		updateCheckStateSystem.RegisterState(3, new InitAppBundlesState());
		updateCheckStateSystem.RegisterState(2, new DownloadPointersState());
		updateCheckStateSystem.RegisterState(5, new EncodePointersState());
		updateCheckStateSystem.RegisterState(6, new DownloadUpdatesState());
		updateCheckStateSystem.RegisterState(7, new VerifyDBValidityState());
		updateCheckStateSystem.RegisterState(8, new ReconnectAllState());
		updateCheckStateSystem.RegisterState(9, new UpdateCompleteState());
		updateCheckData.DeviceLanguage = BundleUtils.GetSystemLanguage();
		updateCheckData.ForceUpdate = AssetBundleConfig.ForceUpdateDataBundle;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (updateCheckStateSystem == null)
		{
			return;
		}
		updateCheckStateSystem.UpdateFSM();
		if (updateCheckStateSystem.GetCurrentState() == 9)
		{
			if (onComplete != null)
			{
				onComplete(updateCheckData.error);
				onComplete = null;
				updateCheckData.error = null;
			}
			IsDone = true;
		}
	}

	protected override void OnDestroy()
	{
		Reset();
		base.OnDestroy();
	}
}
