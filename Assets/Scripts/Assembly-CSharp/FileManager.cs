using System;
using System.Collections.Generic;
using System.IO;

public static class FileManager
{
	private const int kMaxFilePathLength = 256;

	private static Dictionary<string, FileData> findFilePathInCloudActions;

	private static Dictionary<string, FileData> saveFileActions;

	private static Dictionary<string, FileData> loadFileActions;

	private static bool useNativeFileIO;

	public static bool CloudStorageAvailable { get; private set; }

	static FileManager()
	{
	}

	public static bool CheckCloudStorageAvailability()
	{
		CloudStorageAvailable = !string.IsNullOrEmpty(GetCloudContainerDirectoryPath());
		return CloudStorageAvailable;
	}

	public static string GetCloudContainerDirectoryPath()
	{
		return GetCloudContainerDirectoryPath(null);
	}

	public static string GetCloudContainerDirectoryPath(string containerID)
	{
		return null;
	}

	public static void FindFilePathInCloud(string path, Action<FileData> onComplete)
	{
		FindFilePathInCloud(path, null, onComplete);
	}

	public static void FindFilePathInCloud(string fileName, string containerID, Action<FileData> onComplete)
	{
		FileData fileData = new FileData(fileName, onComplete);
		fileData.ContainerID = containerID;
		if (onComplete != null)
		{
			onComplete(fileData);
		}
	}

	public static string ConvertLocalPathToCloudPath(string localPath)
	{
		return ConvertLocalPathToCloudPath(localPath, null);
	}

	public static string ConvertLocalPathToCloudPath(string localPath, string containerID)
	{
		return null;
	}

	public static void SaveFile(string path, byte[] data)
	{
		SaveFile(path, data, null);
	}

	public static void SaveFile(string path, byte[] data, Action<FileData> onComplete)
	{
		FileData fileData = new FileData(path, onComplete);
		fileData.Data = data;
		using (FileStream output = new FileStream(path, FileMode.Create, FileAccess.Write))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(data);
			}
		}
		if (onComplete != null)
		{
			fileData.Exists = true;
			onComplete(fileData);
		}
	}

	public static void LoadFile(string path, Action<FileData> onComplete)
	{
		FileData fileData = new FileData(path, onComplete);
		using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
		{
			using (BinaryReader binaryReader = new BinaryReader(fileStream))
			{
				fileData.Data = binaryReader.ReadBytes((int)fileStream.Length);
			}
		}
		if (onComplete != null)
		{
			fileData.Exists = true;
			onComplete(fileData);
		}
	}

	private static bool AddAction(Dictionary<string, FileData> actions, FileData data)
	{
		if (data != null && !string.IsNullOrEmpty(data.Path))
		{
			if (!actions.ContainsKey(data.Path))
			{
				actions.Add(data.Path, data);
				return true;
			}
			actions[data.Path].OnCompleteActions.AddRange(data.OnCompleteActions);
		}
		return false;
	}

	private static FileData RemoveAction(Dictionary<string, FileData> actions, string path)
	{
		FileData value = null;
		if (!string.IsNullOrEmpty(path) && actions.TryGetValue(path, out value))
		{
			actions.Remove(path);
		}
		return value;
	}
}
