using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class FileSaveTarget : SaveTarget
{
	private static readonly string kBackupFileExtension = ".bak";

	protected string saveFilePath;

	protected string saveFileBackupPath;

	public virtual string SaveDirectory
	{
		get
		{
			return AJavaTools.GameInfo.GetFilesPath();
		}
	}

	public string SaveFilePath
	{
		get
		{
			return saveFilePath;
		}
	}

	public string SaveFileBackupPath
	{
		get
		{
			return saveFileBackupPath;
		}
	}

	public override string Name
	{
		set
		{
			if (!string.Equals(base.Name, value))
			{
				base.Name = value;
				SetFileName(value);
			}
		}
	}

	public static string BackupName(string name)
	{
		return name + kBackupFileExtension;
	}

	public override void Save(byte[] data)
	{
		FileManager.SaveFile(saveFilePath, data);
		if (UseBackup)
		{
			FileManager.SaveFile(saveFileBackupPath, data);
		}
	}

	public override void Load(bool loadBackup, Action<byte[]> onComplete)
	{
		if (onComplete != null)
		{
			string filePath = ((!loadBackup) ? saveFilePath : saveFileBackupPath);
			LoadFromFile(filePath, onComplete);
		}
	}

	public override void Delete()
	{
		File.Delete(saveFilePath);
		File.Delete(saveFileBackupPath);
	}

	public override void LoadValue(string key, float defaultValue, Action<float> onComplete)
	{
		if (!string.IsNullOrEmpty(key) && onComplete != null)
		{
			onComplete(PlayerPrefs.GetFloat(key, defaultValue));
		}
	}

	public override void LoadValue(string key, int defaultValue, Action<int> onComplete)
	{
		if (!string.IsNullOrEmpty(key) && onComplete != null)
		{
			onComplete(PlayerPrefs.GetInt(key, defaultValue));
		}
	}

	public override void LoadValue(string key, string defaultValue, Action<string> onComplete)
	{
		if (!string.IsNullOrEmpty(key) && onComplete != null)
		{
			onComplete(PlayerPrefs.GetString(key, defaultValue));
		}
	}

	public override void DeleteValue(string key)
	{
		if (!string.IsNullOrEmpty(key))
		{
			PlayerPrefs.DeleteKey(key);
		}
	}

	protected virtual void SetFileName(string fileName)
	{
		saveFilePath = Path.Combine(SaveDirectory, fileName);
		saveFileBackupPath = Path.Combine(SaveDirectory, BackupName(fileName));
	}

	protected void LoadFromFile(string filePath, Action<byte[]> onComplete)
	{
		if (filePath.Contains("jar:file://"))
		{
			SingletonSpawningMonoBehaviour<SaveManager>.Instance.StartCoroutine(LoadFromWWW(filePath, delegate(WWW www)
			{
				byte[] obj = null;
				if (www != null && string.IsNullOrEmpty(www.error))
				{
					obj = www.bytes;
					www.Dispose();
				}
				onComplete(obj);
			}));
		}
		else if (File.Exists(filePath))
		{
			FileManager.LoadFile(filePath, delegate(FileData loadData)
			{
				onComplete(loadData.Data);
			});
		}
		else
		{
			onComplete(null);
		}
	}

	protected IEnumerator LoadFromWWW(string filePath, Action<WWW> onComplete)
	{
		WWW www = new WWW(filePath);
		if (!www.isDone)
		{
			yield return www;
		}
		onComplete(www);
	}

	protected override void SaveValueInt(string key, int value)
	{
		PlayerPrefs.SetInt(key, value);
	}

	protected override void SaveValueFloat(string key, float value)
	{
		PlayerPrefs.SetFloat(key, value);
	}

	protected override void SaveValueString(string key, string value)
	{
		PlayerPrefs.SetString(key, value);
	}
}
