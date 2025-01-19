using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ionic.Crc;
using Ionic.Zlib;
using UnityEngine;

public abstract class SaveProvider : IDisposable
{
	public enum Result
	{
		None = 0,
		Success = 1,
		Failure = 2,
		NotFound = 3,
		Invalid = 4
	}

	public class SaveHeader
	{
		private enum Layout
		{
			Start0 = 0,
			Start1 = 1,
			Start2 = 2,
			Start3 = 3,
			Length = 4,
			Version = 5,
			CRC0 = 6,
			CRC1 = 7,
			CRC2 = 8,
			CRC3 = 9,
			UseEncoding = 10,
			UseDeviceData = 11,
			End0 = 12,
			End1 = 13,
			_COUNT = 14
		}

		private const byte kVersion = 0;

		private static readonly byte[] kCrcSalt = new byte[4] { 250, 87, 240, 13 };

		private byte[] data = new byte[14];

		public bool UseEncoding
		{
			get
			{
				return data[10] == 1;
			}
			set
			{
				data[10] = (byte)(value ? 1u : 0u);
			}
		}

		public bool UseDeviceData
		{
			get
			{
				return data[11] == 1;
			}
			set
			{
				data[11] = (byte)(value ? 1u : 0u);
			}
		}

		public int Length
		{
			get
			{
				return 14;
			}
		}

		public uint CRC
		{
			get
			{
				return (uint)((data[6] << 24) | (data[7] << 16) | (data[8] << 8) | data[9]);
			}
			set
			{
				data[6] = (byte)((value & 0xFF000000u) >> 24);
				data[7] = (byte)((value & 0xFF0000) >> 16);
				data[8] = (byte)((value & 0xFF00) >> 8);
				data[9] = (byte)(value & 0xFFu);
			}
		}

		public SaveHeader()
		{
			data[0] = 83;
			data[1] = 65;
			data[2] = 86;
			data[3] = 69;
			data[5] = 0;
			data[4] = 14;
			data[12] = 13;
			data[13] = 10;
			RevertCRC();
		}

		public SaveHeader(byte[] headerData)
			: this()
		{
			Array.Copy(headerData, data, 14L);
		}

		public byte[] GetBytes()
		{
			return data;
		}

		public void RevertCRC()
		{
			RevertCRC(data);
		}

		public static void RevertCRC(byte[] headerData)
		{
			Array.Copy(kCrcSalt, 0L, headerData, 6L, kCrcSalt.Length);
		}
	}

	private SaveHeader header = new SaveHeader();

	private Dictionary<string, SaveTarget> targets = new Dictionary<string, SaveTarget>();

	private Dictionary<string, DeviceData> deviceDataForTarget = new Dictionary<string, DeviceData>();

	private CRC32 crc = new CRC32();

	private TimeSpan? autoSaveInterval;

	private DateTime? autoSaveTime;

	private bool autoSaveEnabled;

	private bool alreadyDisposed;

	public SaveHeader Header
	{
		get
		{
			return header;
		}
		private set
		{
			header = value;
		}
	}

	public string Name { get; private set; }

	public bool SaveOnExit { get; set; }

	public bool RequireCRCMatch { get; set; }

	public TimeSpan? AutoSaveInterval
	{
		get
		{
			return autoSaveInterval;
		}
		set
		{
			TimeSpan? timeSpan = autoSaveInterval;
			if (timeSpan.GetValueOrDefault() != value.GetValueOrDefault() || (timeSpan.HasValue ^ value.HasValue))
			{
				autoSaveInterval = value;
				ScheduleNextAutoSave();
			}
		}
	}

	public DateTime? AutoSaveTime
	{
		get
		{
			return autoSaveTime;
		}
		private set
		{
			autoSaveTime = value;
		}
	}

	public bool AutoSaveEnabled
	{
		get
		{
			return autoSaveEnabled;
		}
		set
		{
			if (autoSaveEnabled != value)
			{
				autoSaveEnabled = value;
				ScheduleNextAutoSave();
			}
		}
	}

	public Action<SaveTarget, byte[]> OnSaveComplete { get; set; }

	public static T Create<T>(string name) where T : SaveProvider, new()
	{
		UnityThreadHelper.Activate();
		T val = new T
		{
			Name = name
		};
		if (SingletonSpawningMonoBehaviour<SaveManager>.Instance != null)
		{
			SingletonSpawningMonoBehaviour<SaveManager>.Instance.AddSave(val);
		}
		return val;
	}

	public SaveTarget GetTarget(string targetName)
	{
		if (string.IsNullOrEmpty(targetName))
		{
			return null;
		}
		SaveTarget value;
		if (!targets.TryGetValue(targetName, out value))
		{
		}
		return value;
	}

	public DeviceData GetDeviceData(string targetName)
	{
		if (string.IsNullOrEmpty(targetName))
		{
			return null;
		}
		DeviceData value = null;
		if (Header.UseDeviceData && !deviceDataForTarget.TryGetValue(targetName, out value))
		{
			value = new DeviceData();
			deviceDataForTarget[targetName] = value;
		}
		return value;
	}

	public T AddTarget<T>(string targetName) where T : SaveTarget, new()
	{
		SaveTarget saveTarget = new T
		{
			Name = targetName
		};
		targets[targetName] = saveTarget;
		return saveTarget as T;
	}

	public void Save()
	{
		if (targets.Count > 0)
		{
			DeviceData deviceData = null;
			byte[] array = null;
			foreach (KeyValuePair<string, SaveTarget> target in targets)
			{
				if (array == null)
				{
					if (Header.UseDeviceData)
					{
						deviceData = GetDeviceData(target.Key);
						deviceData.Update();
					}
					array = Encode(targets.Values, deviceData);
				}
				SaveInternal(target.Value, array);
				if (deviceData != null)
				{
					GetDeviceData(target.Key).CopyFrom(deviceData);
				}
			}
		}
		ScheduleNextAutoSave();
	}

	public void Save(string targetName)
	{
		SaveTarget value;
		if (!string.IsNullOrEmpty(targetName) && targets.TryGetValue(targetName, out value))
		{
			DeviceData deviceData = null;
			if (Header.UseDeviceData)
			{
				deviceData = GetDeviceData(targetName);
				deviceData.Update();
			}
			SaveInternal(value, Encode(new SaveTarget[1] { value }, deviceData));
		}
		ScheduleNextAutoSave();
	}

	public void Load(string targetName, Action<Result> onComplete)
	{
		SaveTarget value;
		if (!string.IsNullOrEmpty(targetName) && targets.TryGetValue(targetName, out value))
		{
			LoadInternal(value, false, onComplete);
		}
		else if (onComplete != null)
		{
			onComplete(Result.Invalid);
		}
	}

	public void ChooseSaveTarget(string targetNameA, string targetNameB, Action<SaveTarget, DeviceData> onResult, Action<DeviceData, DeviceData, Action<bool>> onReconcile)
	{
		SingletonSpawningMonoBehaviour<SaveManager>.Instance.StartCoroutine(ChooseSaveTargetInternal(targetNameA, targetNameB, onResult, onReconcile));
	}

	protected abstract bool Write(Stream dataStream, IEnumerable<SaveTarget> targets, DeviceData deviceData);

	protected abstract bool Read(SaveHeader dataHeader, Stream dataStream, SaveTarget target, DeviceData deviceData);

	protected virtual byte[] Encode(IEnumerable<SaveTarget> targets, DeviceData deviceData)
	{
		MemoryStream memoryStream = new MemoryStream();
		Header.RevertCRC();
		memoryStream.Write(Header.GetBytes(), 0, Header.Length);
		bool flag;
		if (Header.UseEncoding)
		{
			GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);
			flag = Write(gZipStream, targets, deviceData);
			gZipStream.Close();
		}
		else
		{
			flag = Write(memoryStream, targets, deviceData);
		}
		if (flag)
		{
			byte[] array = memoryStream.ToArray();
			crc.Reset();
			crc.SlurpBlock(array, 0, array.Length);
			uint num = (uint)(UnityEngine.Random.seed = crc.Crc32Result);
			uint num2 = (uint)UnityEngine.Random.Range(0, int.MaxValue);
			num ^= num2;
			UnityEngine.Random.seed = (int)DateTime.Now.Ticks;
			Header.CRC = num;
			Array.Copy(Header.GetBytes(), array, Header.Length);
			return array;
		}
		return null;
	}

	protected virtual Stream Decode(byte[] data, out SaveHeader dataHeader)
	{
		if (data == null)
		{
			dataHeader = null;
			return null;
		}
		int num = 0;
		dataHeader = new SaveHeader(data);
		num += dataHeader.Length;
		SaveHeader.RevertCRC(data);
		crc.Reset();
		crc.SlurpBlock(data, 0, data.Length);
		uint num2 = (uint)(UnityEngine.Random.seed = crc.Crc32Result);
		uint num3 = (uint)UnityEngine.Random.Range(0, int.MaxValue);
		num2 ^= num3;
		UnityEngine.Random.seed = (int)DateTime.Now.Ticks;
		if (dataHeader.CRC != num2 && RequireCRCMatch)
		{
			return null;
		}
		MemoryStream memoryStream = new MemoryStream(data, num, data.Length - num, false, true);
		if (dataHeader.UseEncoding)
		{
			return new GZipStream(memoryStream, CompressionMode.Decompress);
		}
		return memoryStream;
	}

	private void SaveInternal(SaveTarget target, byte[] data)
	{
		target.Save(data);
		if (OnSaveComplete != null)
		{
			OnSaveComplete(target, data);
		}
	}

	private void LoadInternal(SaveTarget target, bool useBackup, Action<Result> onComplete)
	{
		target.Load(useBackup, delegate(byte[] data)
		{
			bool flag = true;
			Stream stream = null;
			DeviceData deviceData = null;
			SaveHeader dataHeader = null;
			try
			{
				stream = Decode(data, out dataHeader);
				if (Header.UseDeviceData)
				{
					deviceData = GetDeviceData(target.Name);
				}
				flag = Read(dataHeader, stream, target, deviceData);
			}
			catch (Exception)
			{
				flag = false;
			}
			finally
			{
				if (stream != null)
				{
					stream.Close();
				}
			}
			if (flag)
			{
				if (onComplete != null)
				{
					onComplete(Result.Success);
				}
			}
			else if (target.UseBackup && !useBackup)
			{
				LoadInternal(target, true, onComplete);
			}
			else
			{
				Result obj = ((data != null) ? Result.Failure : Result.NotFound);
				if (onComplete != null)
				{
					onComplete(obj);
				}
			}
		});
	}

	private void ScheduleNextAutoSave()
	{
		if (AutoSaveEnabled && AutoSaveInterval.HasValue)
		{
			AutoSaveTime = DateTime.Now + AutoSaveInterval.Value;
		}
		else
		{
			AutoSaveTime = null;
		}
		if (ApplicationUtilities._autoSave)
		{
			SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.performAutoSave();
		}
	}

	private IEnumerator ChooseSaveTargetInternal(string targetNameA, string targetNameB, Action<SaveTarget, DeviceData> onResult, Action<DeviceData, DeviceData, Action<bool>> onReconcile)
	{
		string userId = ApplicationUtilities.UserID;
		DeviceData combinedDeviceData = new DeviceData();
		DeviceData targetAData = null;
		DeviceData targetBData = null;
		SaveTarget result = null;
		SaveTarget targetA = GetTarget(targetNameA);
		SaveTarget targetB = GetTarget(targetNameB);
		StringBuilder log = null;
		if (!GeneralConfig.IsLive || Debug.isDebugBuild)
		{
			log = new StringBuilder();
		}
		Result targetLoadResultA = Result.None;
		Load(targetNameA, delegate(Result resultA)
		{
			targetLoadResultA = resultA;
		});
		Result targetLoadResultB = Result.None;
		Load(targetNameB, delegate(Result resultB)
		{
			targetLoadResultB = resultB;
		});
		while (targetLoadResultA == Result.None || targetLoadResultB == Result.None)
		{
			yield return null;
		}
		if (targetLoadResultA == Result.Success)
		{
			targetAData = GetDeviceData(targetNameA);
		}
		if (targetLoadResultB == Result.Success)
		{
			targetBData = GetDeviceData(targetNameB);
		}
		if ((targetAData == null || targetAData.Count == 0) && (targetBData == null || targetBData.Count == 0))
		{
			if (log != null)
			{
				log.AppendFormat("result = {0}, neither target exists, defaulting to first (other = {1})\n", targetA, targetB);
			}
			result = targetA;
		}
		else if (targetAData != null && targetAData.Count > 0 && (targetBData == null || targetBData.Count == 0))
		{
			if (log != null)
			{
				log.AppendFormat("result = {0}, other target does not exist = {1}\n", targetA, targetB);
			}
			result = targetA;
		}
		else if (targetBData != null && targetBData.Count > 0 && (targetAData == null || targetAData.Count == 0))
		{
			if (log != null)
			{
				log.AppendFormat("result = {0}, other target does not exist = {1}\n", targetB, targetA);
			}
			result = targetB;
		}
		else
		{
			DeviceDataEntry latestTargetASave = null;
			DeviceDataEntry latestTargetBSave = null;
			foreach (DeviceDataEntry entry in targetAData.Values)
			{
				if (log != null)
				{
					log.AppendFormat("{0} save = {1} @ {2}\n", targetA, entry.DeviceName, entry.SaveTime.ToLocalTime().ToString());
				}
				if (latestTargetASave == null || entry.SaveTime > latestTargetASave.SaveTime)
				{
					latestTargetASave = entry;
				}
				string id2 = entry.ID;
				if (!combinedDeviceData.ContainsKey(id2) || entry.SaveTime > combinedDeviceData[id2].SaveTime)
				{
					combinedDeviceData[id2] = entry;
				}
			}
			foreach (DeviceDataEntry entry2 in targetBData.Values)
			{
				if (log != null)
				{
					log.AppendFormat("{0} save = {1} @ {2}\n", targetB, entry2.DeviceName, entry2.SaveTime.ToLocalTime().ToString());
				}
				if (latestTargetBSave == null || entry2.SaveTime > latestTargetBSave.SaveTime)
				{
					latestTargetBSave = entry2;
				}
				string id = entry2.ID;
				if (!combinedDeviceData.ContainsKey(id) || entry2.SaveTime > combinedDeviceData[id].SaveTime)
				{
					combinedDeviceData[id] = entry2;
				}
			}
			if (log != null)
			{
				log.AppendFormat("{0} LATEST save = {1} @ {2}\n", targetA, latestTargetASave.DeviceName, latestTargetASave.SaveTime.ToLocalTime().ToString());
			}
			if (log != null)
			{
				log.AppendFormat("{0} LATEST save = {1} @ {2}\n", targetB, latestTargetBSave.DeviceName, latestTargetBSave.SaveTime.ToLocalTime().ToString());
			}
			if (latestTargetASave.ID == latestTargetBSave.ID && latestTargetBSave.ID == userId)
			{
				result = targetA;
				if (log != null)
				{
					log.AppendFormat("result = {0}, (option A) most recent device\n", result);
				}
			}
			else if (targetBData.ContainsKey(userId) && targetAData.ContainsKey(userId) && targetAData[userId].SaveTime == targetBData[userId].SaveTime && latestTargetBSave.ID != userId)
			{
				result = targetB;
				if (log != null)
				{
					log.AppendFormat("result = {0}, (option B) auto replace\n", result);
				}
			}
			else if (latestTargetASave.SaveTime > latestTargetBSave.SaveTime && latestTargetBSave.ID == userId)
			{
				result = targetA;
				if (log != null)
				{
					log.AppendFormat("result = {0}, (option C) auto keep\n", result);
				}
			}
			else if (targetAData.ContainsKey(userId) && latestTargetBSave.ID != userId && (!targetBData.ContainsKey(userId) || (targetBData.ContainsKey(userId) && targetAData[userId].SaveTime > targetBData[userId].SaveTime)) && onReconcile != null)
			{
				if (log != null)
				{
				}
				bool reconcileComplete = false;
				onReconcile(targetAData, targetBData, delegate(bool useTargetA)
				{
					result = ((!useTargetA) ? targetB : targetA);
					reconcileComplete = true;
				});
				while (!reconcileComplete)
				{
					yield return null;
				}
				if (log != null)
				{
					log.AppendFormat("result = {0}, (option D) user choice\n", result);
				}
			}
		}
		if (result == null)
		{
			result = targetA;
			if (log != null)
			{
				log.AppendFormat("result = {0}, (ERROR) defaulting to first target\n", result);
			}
		}
		if (targetAData != null)
		{
			targetAData.CopyFrom(combinedDeviceData);
		}
		if (targetBData != null)
		{
			targetBData.CopyFrom(combinedDeviceData);
		}
		if (log != null)
		{
			log.Insert(0, "SaveManager.ChooseSaveTarget:\n");
		}
		if (onResult != null)
		{
			onResult(result, combinedDeviceData);
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool isDisposing)
	{
		if (alreadyDisposed)
		{
			return;
		}
		if (isDisposing)
		{
			targets = null;
			deviceDataForTarget = null;
			header = null;
		}
		if (!ApplicationUtilities.HasShutdown && SingletonSpawningMonoBehaviour<UnityThreadHelper>.Exists && SingletonSpawningMonoBehaviour<SaveManager>.Exists)
		{
			UnityThreadHelper.CallOnMainThread(delegate
			{
				SingletonSpawningMonoBehaviour<SaveManager>.Instance.RemoveSave(this);
			});
		}
		alreadyDisposed = true;
	}

	~SaveProvider()
	{
		Dispose(false);
	}
}
