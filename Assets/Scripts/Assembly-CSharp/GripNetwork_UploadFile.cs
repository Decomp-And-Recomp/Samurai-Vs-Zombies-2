using System;
using Gamespy.CloudStorage;
using Gamespy.Common;

public class GripNetwork_UploadFile : DisposableMonoBehaviour
{
	private string stackTrace;

	private CloudFile cloudFile;

	private RequestState uploadFileState;

	private Action<GripNetwork.Result, string> mUploadFileCallback;

	public void UploadFile(byte[] fileData, Action<GripNetwork.Result, string> UploadFileCallback)
	{
		mUploadFileCallback = UploadFileCallback;
		stackTrace = GenericUtils.StackTrace();
		try
		{
			cloudFile = new CloudFile(GripNetwork.GameSpyAccountManager.SecurityToken, fileData);
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed, null);
		}
	}

	private void Update()
	{
		try
		{
			if (uploadFileState != RequestState.Complete)
			{
				uploadFileState = cloudFile.Upload();
			}
			else if (cloudFile.Result != 0)
			{
				WhenDone(GripNetwork.Result.Failed, null);
			}
			else
			{
				WhenDone(GripNetwork.Result.Success, cloudFile.Upload_FileId);
			}
		}
		catch (Exception)
		{
			WhenDone(GripNetwork.Result.Failed, null);
			ObjectUtils.DestroyImmediate(base.gameObject);
		}
	}

	private void WhenDone(GripNetwork.Result result, string fileID)
	{
		Action<GripNetwork.Result, string> action = mUploadFileCallback;
		if (result != 0)
		{
		}
		if (action != null)
		{
			action(result, fileID);
		}
		ObjectUtils.DestroyImmediate(base.gameObject);
	}
}
