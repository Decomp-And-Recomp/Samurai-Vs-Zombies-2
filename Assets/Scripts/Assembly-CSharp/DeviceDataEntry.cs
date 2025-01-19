using System;
using System.Collections;
using System.Runtime.Serialization;

[Serializable]
public class DeviceDataEntry : Hashtable
{
	public static readonly string kIdKey = "id";

	public static readonly string kDeviceNameKey = "name";

	public static readonly string kSaveTimeKey = "time";

	public string ID
	{
		get
		{
			return (string)this[kIdKey];
		}
		set
		{
			this[kIdKey] = value;
		}
	}

	public DateTime SaveTime
	{
		get
		{
			return (DateTime)this[kSaveTimeKey];
		}
		set
		{
			this[kSaveTimeKey] = value;
		}
	}

	public string DeviceName
	{
		get
		{
			return (string)this[kDeviceNameKey];
		}
		set
		{
			this[kDeviceNameKey] = value;
		}
	}

	public DeviceDataEntry()
	{
	}

	public DeviceDataEntry(IDictionary other)
		: base(other)
	{
	}

	protected DeviceDataEntry(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
