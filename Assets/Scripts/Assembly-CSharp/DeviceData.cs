using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class DeviceData : Dictionary<string, DeviceDataEntry>
{
	public DeviceDataEntry Current
	{
		get
		{
			if (!ContainsKey(ApplicationUtilities.UserID))
			{
				DeviceDataEntry deviceDataEntry = new DeviceDataEntry();
				deviceDataEntry.DeviceName = SystemInfo.deviceName;
				deviceDataEntry.ID = ApplicationUtilities.UserID;
				deviceDataEntry.SaveTime = DateTime.MinValue;
				this[ApplicationUtilities.UserID] = deviceDataEntry;
			}
			return this[ApplicationUtilities.UserID];
		}
	}

	public DeviceDataEntry Latest
	{
		get
		{
			DeviceDataEntry deviceDataEntry = null;
			DateTime dateTime = DateTime.MinValue;
			foreach (DeviceDataEntry value in base.Values)
			{
				DateTime saveTime = value.SaveTime;
				if (deviceDataEntry == null || saveTime > dateTime)
				{
					deviceDataEntry = value;
					dateTime = saveTime;
				}
			}
			return deviceDataEntry;
		}
	}

	public DeviceData()
		: base(1)
	{
		Reset();
	}

	public DeviceData(IDictionary<string, DeviceDataEntry> other)
		: base(other)
	{
	}

	public void Update()
	{
		DeviceDataEntry current = Current;
		current.DeviceName = SystemInfo.deviceName;
		current.SaveTime = ApplicationUtilities.Now;
	}

	public void SetIfNewer(DeviceDataEntry entry)
	{
		if (!ContainsKey(entry.ID) || entry.SaveTime > this[entry.ID].SaveTime)
		{
			this[entry.ID] = entry;
		}
	}

	public void Reset()
	{
		Clear();
		DeviceDataEntry deviceDataEntry = new DeviceDataEntry();
		deviceDataEntry.DeviceName = SystemInfo.deviceName;
		deviceDataEntry.ID = ApplicationUtilities.UserID;
		deviceDataEntry.SaveTime = DateTime.MinValue;
		this[ApplicationUtilities.UserID] = deviceDataEntry;
	}

	public void CopyFrom(DeviceData source)
	{
		if (object.ReferenceEquals(this, source))
		{
			return;
		}
		Clear();
		foreach (KeyValuePair<string, DeviceDataEntry> item in source)
		{
			this[item.Key] = item.Value;
		}
	}

	public void SerializeToStream(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		try
		{
			binaryWriter.Write(Count);
			foreach (DeviceDataEntry value in base.Values)
			{
				binaryFormatter.Serialize(binaryWriter.BaseStream, value);
			}
		}
		catch (SerializationException)
		{
		}
		binaryWriter.Flush();
	}

	public void DeserializeFromStream(Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		try
		{
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				Hashtable hashtable = (Hashtable)binaryFormatter.Deserialize(binaryReader.BaseStream);
				if (hashtable != null)
				{
					DeviceDataEntry ifNewer = new DeviceDataEntry(hashtable);
					SetIfNewer(ifNewer);
				}
			}
		}
		catch (SerializationException)
		{
		}
	}
}
