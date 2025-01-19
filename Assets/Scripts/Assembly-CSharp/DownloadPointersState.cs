using System.Collections;
using UnityEngine;

public class DownloadPointersState : DoNothingState
{
	private UpdateCheckData objData;

	public bool Complete { get; private set; }

	public override void Init(FSM fsm)
	{
	}

	public override void OnEnter(FSM fsm, int prevState)
	{
		Complete = false;
		objData = (UpdateCheckData)fsm.GetOwnerObject();
		if (AWSServerConfig.CheckForUpdates)
		{
			SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.StartCoroutine(DownloadFiles());
		}
	}

	public override void OnExit(FSM fsm, int nextState)
	{
	}

	public override void OnUpdate(FSM fsm)
	{
		if (AWSServerConfig.CheckForUpdates)
		{
			if (Complete)
			{
				if (objData.error == null)
				{
					fsm.QueueState(5);
				}
				else if (objData.error.type == UpdateSystem.UpdateError.ErrorType.CouldNotConnect || objData.error.type == UpdateSystem.UpdateError.ErrorType.AppUpdateRequired)
				{
					fsm.QueueState(9);
				}
			}
		}
		else
		{
			fsm.QueueState(7);
		}
	}

	private IEnumerator DownloadFiles()
	{
		objData.error = null;
		string downloadUrl = AWSServerConfig.DownloadUrl;
		WWW www3 = new WWW(UpdateSystem.AppendRandomValueToUrl(downloadUrl + string.Format(AWSServerConfig.PointerFile, UpdateCheckData.MainDB)));
		if (!www3.isDone)
		{
			yield return www3;
		}
		if (!string.IsNullOrEmpty(www3.error))
		{
			objData.error = new UpdateSystem.UpdateError(UpdateSystem.UpdateError.ErrorType.CouldNotConnect, www3, string.Empty, string.Empty);
			Complete = true;
			yield break;
		}
		objData.MainPtrContents = www3.text;
		objData.FileIndex++;
		www3.Dispose();
		www3 = new WWW(UpdateSystem.AppendRandomValueToUrl(downloadUrl + string.Format(AWSServerConfig.PointerFile, objData.DeviceLanguage)));
		if (!www3.isDone)
		{
			yield return www3;
		}
		if (!string.IsNullOrEmpty(www3.error))
		{
			objData.error = new UpdateSystem.UpdateError(UpdateSystem.UpdateError.ErrorType.CouldNotConnect, www3, string.Empty, string.Empty);
			Complete = true;
			yield break;
		}
		objData.LangPtrContents = www3.text;
		objData.FileIndex++;
		www3.Dispose();
		www3 = new WWW(UpdateSystem.AppendRandomValueToUrl(downloadUrl + AWSServerConfig.ForcedUpdateFileName));
		if (!www3.isDone)
		{
			yield return www3;
		}
		if (!string.IsNullOrEmpty(www3.error))
		{
			objData.error = new UpdateSystem.UpdateError(UpdateSystem.UpdateError.ErrorType.CouldNotConnect, www3, string.Empty, string.Empty);
			Complete = true;
		}
		else if (www3.text == "1" || www3.text == "true")
		{
			objData.error = new UpdateSystem.UpdateError(UpdateSystem.UpdateError.ErrorType.AppUpdateRequired, www3, string.Empty, string.Empty);
			Complete = true;
			www3.Dispose();
		}
		else if (www3.text != "0" && www3.text != "false")
		{
			objData.error = new UpdateSystem.UpdateError(UpdateSystem.UpdateError.ErrorType.DownloadFailed, www3, string.Empty, string.Empty);
			Complete = true;
			www3.Dispose();
		}
		else
		{
			www3.Dispose();
			Complete = true;
		}
	}
}
