using System.Collections;
using System.IO;
using UnityEngine;

public class DownloadUpdatesState : DoNothingState
{
	private UpdateCheckData objData;

	private static readonly int DBToDownload = 2;

	private int DBDownloaded { get; set; }

	public override void Init(FSM fsm)
	{
	}

	public override void OnEnter(FSM fsm, int prevState)
	{
		objData = (UpdateCheckData)fsm.GetOwnerObject();
		DBDownloaded = 0;
		if (!string.IsNullOrEmpty(objData.MainDatabasePointer) && !string.IsNullOrEmpty(objData.MainDatabaseBuild))
		{
			string text = AWSServerConfig.DownloadUrl + objData.MainDatabasePointer + "/" + objData.MainDatabaseBuild + "/";
			string assetPath = AssetBundleConfig.BundleDataPath + "/" + AssetBundleConfig.DataBundleName;
			text += AssetBundleConfig.DataBundleName;
			SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.StartCoroutine(DownloadAndSave(objData.MainDatabasePointer, objData.MainDatabaseBuild, text, assetPath, UpdateCheckData.MainDB));
		}
		else
		{
			DownloadSkipped();
		}
		if (!string.IsNullOrEmpty(objData.LangDatabasePointer) && !string.IsNullOrEmpty(objData.LangDatabaseBuild))
		{
			string text2 = AWSServerConfig.DownloadUrl + objData.LangDatabasePointer + "/" + objData.LangDatabaseBuild + "/";
			string assetPath2 = AssetBundleConfig.BundleDataPath + "/" + AssetBundleConfig.DataBundleName;
			text2 += AssetBundleConfig.DataBundleName;
			SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.StartCoroutine(DownloadAndSave(objData.LangDatabasePointer, objData.LangDatabaseBuild, text2, assetPath2, objData.DeviceLanguage));
		}
		else
		{
			DownloadSkipped();
		}
	}

	public override void OnExit(FSM fsm, int nextState)
	{
	}

	public override void OnUpdate(FSM fsm)
	{
		if (DBDownloaded >= DBToDownload)
		{
			if (objData.error != null)
			{
			}
			fsm.QueueState(7);
		}
	}

	private IEnumerator DownloadAndSave(string dbVersion, string build, string downloadFile, string assetPath, string assetType)
	{
		bool yieldForDownloads = false;
		WWW bundle = new WWW(downloadFile);
		while (!bundle.isDone)
		{
			yield return null;
		}
		if (bundle.isDone)
		{
			if (!string.IsNullOrEmpty(bundle.error))
			{
				objData.error = new UpdateSystem.UpdateError(UpdateSystem.UpdateError.ErrorType.DownloadFailed, bundle, downloadFile, string.Empty);
				objData.FileIndex++;
				DBDownloaded++;
				yield break;
			}
			BundleUtils.DeleteFileIfExists(assetPath);
			using (FileStream fs = new FileStream(assetPath, FileMode.Create))
			{
				fs.Write(bundle.bytes, 0, bundle.bytes.Length);
				fs.Close();
			}
			bundle.Dispose();
			if (!string.IsNullOrEmpty(dbVersion))
			{
				BundleUtils.UpdateLocalDataBundleInfo(dbVersion, build, assetPath, assetType);
			}
			objData.FileIndex++;
			DBDownloaded++;
		}
		else
		{
			yieldForDownloads = true;
		}
		if (yieldForDownloads)
		{
			yield return null;
		}
	}

	private void DownloadSkipped()
	{
		DBDownloaded++;
		objData.FileIndex++;
	}
}
