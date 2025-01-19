using System;
using System.Collections;
using System.IO;

public class iCloudSaveTarget : FileSaveTarget
{
	private bool saveFileExists;

	private bool saveFileBackupExists;

	public override string SaveDirectory
	{
		get
		{
			return FileManager.GetCloudContainerDirectoryPath();
		}
	}

	public bool SearchingForCloudFile { get; private set; }

	public override void Save(byte[] data)
	{
		if (!FileManager.CheckCloudStorageAvailability())
		{
			return;
		}
		if (string.IsNullOrEmpty(saveFilePath))
		{
			saveFilePath = Path.Combine(FileManager.GetCloudContainerDirectoryPath(), Name);
		}
		FileManager.SaveFile(saveFilePath, data);
		saveFileExists = true;
		if (UseBackup)
		{
			if (string.IsNullOrEmpty(saveFilePath))
			{
				saveFileBackupPath = Path.Combine(FileManager.GetCloudContainerDirectoryPath(), FileSaveTarget.BackupName(Name));
			}
			FileManager.SaveFile(saveFileBackupPath, data);
			saveFileBackupExists = true;
		}
	}

	public override void Load(bool loadBackup, Action<byte[]> onComplete)
	{
		if (onComplete != null)
		{
			if (!FileManager.CheckCloudStorageAvailability())
			{
				onComplete(null);
				return;
			}
			SetFileName(Name);
			SingletonSpawningMonoBehaviour<SaveManager>.Instance.StartCoroutine(LoadFromCloud(loadBackup, onComplete));
		}
	}

	public override void LoadValue(string key, float defaultValue, Action<float> onComplete)
	{
		if (!string.IsNullOrEmpty(key) && onComplete != null)
		{
			onComplete(defaultValue);
		}
	}

	public override void LoadValue(string key, int defaultValue, Action<int> onComplete)
	{
		if (!string.IsNullOrEmpty(key) && onComplete != null)
		{
			onComplete(defaultValue);
		}
	}

	public override void LoadValue(string key, string defaultValue, Action<string> onComplete)
	{
		if (!string.IsNullOrEmpty(key) && onComplete != null)
		{
			onComplete(defaultValue);
		}
	}

	public override void DeleteValue(string key)
	{
		if (!string.IsNullOrEmpty(key))
		{
		}
	}

	protected override void SaveValueInt(string key, int value)
	{
	}

	protected override void SaveValueFloat(string key, float value)
	{
	}

	protected override void SaveValueString(string key, string value)
	{
	}

	protected override void SetFileName(string fileName)
	{
		saveFilePath = null;
		saveFileBackupPath = null;
		if (!FileManager.CheckCloudStorageAvailability())
		{
			return;
		}
		SearchingForCloudFile = true;
		int findCount = 2;
		FileManager.FindFilePathInCloud(Name, delegate(FileData fileData)
		{
			saveFileExists = fileData.Exists;
			if (fileData.Exists)
			{
				saveFilePath = fileData.Path;
			}
			else if (string.IsNullOrEmpty(saveFilePath))
			{
				saveFilePath = Path.Combine(FileManager.GetCloudContainerDirectoryPath(), Name);
			}
			if (--findCount == 0)
			{
				SearchingForCloudFile = false;
			}
		});
		FileManager.FindFilePathInCloud(FileSaveTarget.BackupName(Name), delegate(FileData fileData)
		{
			saveFileBackupExists = fileData.Exists;
			if (fileData.Exists)
			{
				saveFileBackupPath = fileData.Path;
			}
			else if (string.IsNullOrEmpty(saveFileBackupPath))
			{
				saveFileBackupPath = Path.Combine(FileManager.GetCloudContainerDirectoryPath(), FileSaveTarget.BackupName(Name));
			}
			if (--findCount == 0)
			{
				SearchingForCloudFile = false;
			}
		});
	}

	protected IEnumerator LoadFromCloud(bool loadBackup, Action<byte[]> onComplete)
	{
		while (SearchingForCloudFile)
		{
			yield return null;
		}
		if (!loadBackup)
		{
			if (!saveFileExists)
			{
				if (onComplete != null)
				{
					onComplete(null);
				}
			}
			else
			{
				LoadFromFile(saveFilePath, onComplete);
			}
		}
		else if (!saveFileBackupExists)
		{
			if (onComplete != null)
			{
				onComplete(null);
			}
		}
		else
		{
			LoadFromFile(saveFileBackupPath, onComplete);
		}
	}
}
