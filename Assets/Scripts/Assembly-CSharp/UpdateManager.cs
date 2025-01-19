using System;
using System.Collections;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{
	public interface GameSpecificUpdateManager
	{
		void OnCheckForUpdateBegin();

		void OnCheckForUpdateProgress(float progress);

		void OnCheckForUpdateComplete(bool updatesAvailable, UpdateSystem.UpdateError error);

		WaitForResponse<bool> ShouldDownloadUpdate();

		void OnDownloadUpdatesBegin();

		void OnDownloadUpdatesProgress(float progress);

		void OnDownloadUpdatesComplete(UpdateSystem.UpdateError error);

		WaitForResponse<bool> ShouldRetryDownloadUpdate();

		void OnVerifyDataBegin();

		void OnVerifyDataComplete(UpdateSystem.UpdateError error);

		WaitForResponse<bool> ShouldRetryEntireUpdateAttempt();

		void OnComplete();

		WaitForResponse<bool> ShouldExitApp();

		void PreExitApp();
	}

	private bool DownloadsInProgress;

	public IEnumerator StartUpdateCoroutine(GameSpecificUpdateManager gameSpecificManager)
	{
		bool retryUpdate = false;
		do
		{
			Initialize();
			gameSpecificManager.OnCheckForUpdateBegin();
			yield return StartCoroutine(CheckForUpdates(gameSpecificManager.OnCheckForUpdateProgress, gameSpecificManager.OnCheckForUpdateComplete));
			WaitForResponse<bool> shouldDownloadUpdate = gameSpecificManager.ShouldDownloadUpdate();
			yield return StartCoroutine(shouldDownloadUpdate);
			if (!shouldDownloadUpdate.Response)
			{
				gameSpecificManager.OnVerifyDataBegin();
				yield return StartCoroutine(VerifyData(gameSpecificManager.OnVerifyDataComplete));
			}
			else
			{
				bool downloadUpdate = true;
				while (downloadUpdate)
				{
					if (!DownloadsInProgress)
					{
						gameSpecificManager.OnDownloadUpdatesBegin();
						yield return StartCoroutine(DownloadUpdates(gameSpecificManager.OnDownloadUpdatesProgress, gameSpecificManager.OnDownloadUpdatesComplete));
						WaitForResponse<bool> shouldRetryDownload = gameSpecificManager.ShouldRetryDownloadUpdate();
						yield return StartCoroutine(shouldRetryDownload);
						downloadUpdate = shouldRetryDownload.Response;
					}
					else
					{
						yield return null;
					}
				}
			}
			Cleanup();
			WaitForResponse<bool> retryEntireUpdate = gameSpecificManager.ShouldRetryEntireUpdateAttempt();
			if (retryEntireUpdate != null)
			{
				yield return StartCoroutine(retryEntireUpdate);
				retryUpdate = retryEntireUpdate.Response;
			}
		}
		while (retryUpdate);
		gameSpecificManager.OnComplete();
		WaitForResponse<bool> exitApp = gameSpecificManager.ShouldExitApp();
		if (exitApp != null)
		{
			yield return StartCoroutine(exitApp);
			if (exitApp.Response)
			{
				yield return null;
				gameSpecificManager.PreExitApp();
				yield return new WaitForSeconds(0.5f);
				Application.Quit();
			}
		}
	}

	private void Initialize()
	{
		SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.Initialize();
	}

	private void Cleanup()
	{
		UnityEngine.Object.Destroy(SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.gameObject);
	}

	private IEnumerator CheckForUpdates(Action<float> onProgressUpdate, Action<bool, UpdateSystem.UpdateError> onComplete)
	{
		UpdateSystem.UpdateError error = null;
		Action<UpdateSystem.UpdateError> onInternalCheckComplete = delegate(UpdateSystem.UpdateError internalError)
		{
			error = internalError;
		};
		SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.StartUpdateCheck(onInternalCheckComplete);
		float fPreviousProgressPercent = 0f;
		while (!SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.IsDone)
		{
			if (SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.InternalData != null)
			{
				float fProgressPercent = (float)SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.InternalData.FileIndex / (float)SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.InternalData.FileCount;
				if (fProgressPercent != fPreviousProgressPercent)
				{
					onProgressUpdate(fProgressPercent);
					fPreviousProgressPercent = fProgressPercent;
				}
			}
			yield return null;
		}
		onComplete(SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.IsUpdateAvailable(), error);
	}

	private IEnumerator DownloadUpdates(Action<float> onProgressUpdate, Action<UpdateSystem.UpdateError> onComplete)
	{
		DownloadsInProgress = true;
		UpdateSystem.UpdateError error = null;
		Action<UpdateSystem.UpdateError> onInternalUpdateComplete = delegate(UpdateSystem.UpdateError internalError)
		{
			error = internalError;
		};
		SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.DoUpdates(onInternalUpdateComplete);
		float fPreviousProgressPercent = 0f;
		while (!SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.IsDone)
		{
			if (SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.InternalData != null)
			{
				float fProgressPercent = (float)SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.InternalData.FileIndex / (float)SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.InternalData.FileCount;
				if (fProgressPercent != fPreviousProgressPercent)
				{
					onProgressUpdate(fProgressPercent);
					fPreviousProgressPercent = fProgressPercent;
				}
			}
			yield return null;
		}
		DownloadsInProgress = false;
		onComplete(error);
	}

	private IEnumerator VerifyData(Action<UpdateSystem.UpdateError> onComplete)
	{
		UpdateSystem.UpdateError error = null;
		Action<UpdateSystem.UpdateError> onInternalVerificationComplete = delegate(UpdateSystem.UpdateError internalError)
		{
			error = internalError;
		};
		SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.DoFinalChecks(onInternalVerificationComplete);
		while (!SingletonSpawningMonoBehaviour<UpdateCheckMonoBehavior>.Instance.IsDone)
		{
			yield return null;
		}
		onComplete(error);
	}
}
