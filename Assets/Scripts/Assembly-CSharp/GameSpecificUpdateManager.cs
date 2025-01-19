public class GameSpecificUpdateManager : UpdateManager.GameSpecificUpdateManager
{
	private bool launchUpdate;

	private WaitForResponse<bool> shouldDownloadUpdates;

	private WaitForResponse<bool> shouldRetryDownload;

	private WaitForResponse<bool> shouldRetryEntireUpdate;

	private WaitForResponse<bool> shouldExitApp;

	private bool updateDownloaded;

	private bool forcedUpdateRequired;

	public static bool UpdateActive { get; private set; }

	public GameSpecificUpdateManager(bool launchUpdate)
	{
		this.launchUpdate = launchUpdate;
		UpdateActive = true;
		forcedUpdateRequired = false;
		updateDownloaded = false;
	}

	public void OnCheckForUpdateBegin()
	{
		if (launchUpdate)
		{
		}
	}

	public void OnCheckForUpdateProgress(float progress)
	{
		if (!launchUpdate)
		{
		}
	}

	public void OnCheckForUpdateComplete(bool updatesAvailable, UpdateSystem.UpdateError error)
	{
		if (launchUpdate)
		{
		}
		shouldDownloadUpdates = new WaitForResponse<bool>();
		if (error != null)
		{
			forcedUpdateRequired = error.type == UpdateSystem.UpdateError.ErrorType.AppUpdateRequired;
			shouldDownloadUpdates.Response = false;
		}
		else if (updatesAvailable)
		{
			if (!launchUpdate)
			{
				shouldDownloadUpdates.Response = true;
			}
			else
			{
				shouldDownloadUpdates.Response = true;
			}
		}
		else
		{
			shouldDownloadUpdates.Response = false;
		}
	}

	public WaitForResponse<bool> ShouldDownloadUpdate()
	{
		return shouldDownloadUpdates;
	}

	public void OnDownloadUpdatesBegin()
	{
		if (launchUpdate)
		{
		}
	}

	public void OnDownloadUpdatesProgress(float progress)
	{
		if (!launchUpdate)
		{
		}
	}

	public void OnDownloadUpdatesComplete(UpdateSystem.UpdateError error)
	{
		shouldRetryDownload = new WaitForResponse<bool>();
		if (launchUpdate)
		{
		}
		if (error != null)
		{
			if (!launchUpdate)
			{
				shouldRetryDownload.Response = false;
			}
			else
			{
				shouldRetryDownload.Response = false;
			}
		}
		else
		{
			shouldRetryDownload.Response = false;
			updateDownloaded = true;
		}
	}

	public WaitForResponse<bool> ShouldRetryDownloadUpdate()
	{
		return shouldRetryDownload;
	}

	public void OnVerifyDataBegin()
	{
	}

	public void OnVerifyDataComplete(UpdateSystem.UpdateError error)
	{
		shouldRetryEntireUpdate = new WaitForResponse<bool>();
		if (error != null)
		{
			if (!launchUpdate)
			{
				shouldRetryEntireUpdate.Response = false;
			}
			else
			{
				shouldRetryEntireUpdate.Response = false;
			}
		}
		else
		{
			shouldRetryEntireUpdate.Response = false;
		}
	}

	public WaitForResponse<bool> ShouldRetryEntireUpdateAttempt()
	{
		return shouldRetryEntireUpdate;
	}

	public void OnComplete()
	{
		if (forcedUpdateRequired)
		{
			ShowForcedUpdatePopup();
		}
		else if (!launchUpdate && updateDownloaded)
		{
			shouldExitApp = new WaitForResponse<bool>();
		}
		else
		{
			UpdateActive = false;
		}
	}

	public WaitForResponse<bool> ShouldExitApp()
	{
		return shouldExitApp;
	}

	public void PreExitApp()
	{
	}

	public void ShowForcedUpdatePopup()
	{
	}
}
