using System;

public abstract class SaveTarget
{
	private string name;

	public virtual string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public virtual bool UseBackup { get; set; }

	public abstract void Save(byte[] data);

	public abstract void Load(bool loadBackup, Action<byte[]> onComplete);

	public abstract void Delete();

	public void SaveValue<T>(string key, T value)
	{
		if (value != null && !string.IsNullOrEmpty(key))
		{
			switch (GetStorageType(typeof(T)))
			{
			case ValueStorageType.Int:
				SaveValueInt(key, Convert.ToInt32(value));
				break;
			case ValueStorageType.Float:
				SaveValueFloat(key, Convert.ToSingle(value));
				break;
			case ValueStorageType.String:
				SaveValueString(key, value.ToString());
				break;
			}
		}
	}

	protected virtual void SaveValueInt(string key, int value)
	{
	}

	protected virtual void SaveValueFloat(string key, float value)
	{
	}

	protected virtual void SaveValueString(string key, string value)
	{
	}

	public virtual void LoadValue(string key, float defaultValue, Action<float> onComplete)
	{
		if (onComplete != null)
		{
			onComplete(defaultValue);
		}
	}

	public virtual void LoadValue(string key, int defaultValue, Action<int> onComplete)
	{
		if (onComplete != null)
		{
			onComplete(defaultValue);
		}
	}

	public virtual void LoadValue(string key, string defaultValue, Action<string> onComplete)
	{
		if (onComplete != null)
		{
			onComplete(defaultValue);
		}
	}

	public virtual void DeleteValue(string key)
	{
	}

	public static ValueStorageType GetStorageType(Type t)
	{
		switch (Type.GetTypeCode(t))
		{
		case TypeCode.Boolean:
		case TypeCode.SByte:
		case TypeCode.Byte:
		case TypeCode.Int16:
		case TypeCode.UInt16:
		case TypeCode.Int32:
		case TypeCode.UInt32:
		case TypeCode.Int64:
		case TypeCode.UInt64:
			return ValueStorageType.Int;
		case TypeCode.Single:
		case TypeCode.Double:
			return ValueStorageType.Float;
		default:
			return ValueStorageType.String;
		}
	}
}
