using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class DataBundleUtils
{
	public static bool SupportedType(Type t)
	{
		return t == typeof(int) || t == typeof(float) || t == typeof(string) || t == typeof(bool) || t == typeof(DataBundleRecordKey) || t == typeof(DataBundleRecordTable) || t.IsEnum || t.IsSubclassOf(typeof(UnityEngine.Object));
	}

	public static bool IsAssetRelativeToResourcesFolder(string assetPath, out string relativePath)
	{
		relativePath = null;
		int num = assetPath.IndexOf(DataBundleRuntime.resourceFolder);
		if (num >= 0)
		{
			relativePath = assetPath.Substring(num + DataBundleRuntime.resourceFolder.Length);
			int num2 = relativePath.LastIndexOf('.');
			if (num2 >= 0)
			{
				relativePath = relativePath.Substring(0, num2);
			}
			return true;
		}
		return false;
	}

	public static object ByteArrayToObject(byte[] arrBytes)
	{
		if (arrBytes == null || arrBytes.Length == 0)
		{
			return null;
		}
		MemoryStream memoryStream = new MemoryStream();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		memoryStream.Write(arrBytes, 0, arrBytes.Length);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return binaryFormatter.Deserialize(memoryStream);
	}

	public static byte[] ObjectToByteArray(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize(memoryStream, obj);
		return memoryStream.ToArray();
	}

	public static object ConvertStringToObjectOfType(string objValue, Type typeInfo, bool convertEnum)
	{
		object result = objValue;
		if (typeInfo != typeof(string) && !typeInfo.IsSubclassOf(typeof(UnityEngine.Object)) && (!typeInfo.IsEnum || convertEnum) && typeInfo != typeof(DataBundleRecordTable) && typeInfo != typeof(DataBundleRecordKey))
		{
			if (typeInfo.IsEnum)
			{
				try
				{
					result = Enum.Parse(typeInfo, objValue, true);
				}
				catch (Exception)
				{
					string[] names = Enum.GetNames(typeInfo);
					if (names != null && names.Length > 0)
					{
						result = Enum.Parse(typeInfo, names[0]);
					}
				}
			}
			else if (typeInfo.IsPrimitive)
			{
				if (string.IsNullOrEmpty(objValue))
				{
					result = typeof(DataBundleUtils).GetMethod("CreateDefault", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(typeInfo).Invoke(null, null);
				}
				else
				{
					try
					{
						result = Convert.ChangeType(objValue, typeInfo);
					}
					catch (Exception)
					{
						result = typeof(DataBundleUtils).GetMethod("CreateDefault", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(typeInfo).Invoke(null, null);
					}
				}
			}
		}
		else if (typeInfo == typeof(DataBundleRecordTable))
		{
			result = (DataBundleRecordTable)objValue;
		}
		else if (typeInfo == typeof(DataBundleRecordKey))
		{
			result = (DataBundleRecordKey)objValue;
		}
		return result;
	}

	public static T ConvertToType<T>(string strValue, T defaultValue)
	{
		T result = defaultValue;
		if (strValue != null)
		{
			object obj = ConvertStringToObjectOfType(strValue, typeof(T), false);
			try
			{
				result = (T)obj;
			}
			catch (Exception)
			{
				try
				{
					result = (T)Convert.ChangeType(obj, typeof(T));
				}
				catch (Exception)
				{
				}
			}
			return result;
		}
		return result;
	}

	public static T CreateDefault<T>()
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle == typeof(string))
		{
			return (T)(object)string.Empty;
		}
		if (typeFromHandle.IsValueType || typeFromHandle == typeof(UnityEngine.Object) || typeFromHandle.IsSubclassOf(typeof(UnityEngine.Object)) || typeFromHandle == typeof(object))
		{
			return default(T);
		}
		return Activator.CreateInstance<T>();
	}

	public static object DefaultValueRuntime(FieldInfo fieldInfo)
	{
		object obj = GetDefaultValue(fieldInfo);
		if (obj == null)
		{
			if (fieldInfo.FieldType == typeof(string))
			{
				obj = string.Empty;
			}
			else if (fieldInfo.FieldType.IsEnum)
			{
				string[] names = Enum.GetNames(fieldInfo.FieldType);
				if (names != null && names.Length > 0)
				{
					obj = Enum.Parse(fieldInfo.FieldType, names[0]);
				}
			}
			else if (fieldInfo.FieldType.IsValueType || fieldInfo.FieldType == typeof(DataBundleRecordTable) || fieldInfo.FieldType == typeof(DataBundleRecordKey))
			{
				obj = Activator.CreateInstance(fieldInfo.FieldType);
			}
		}
		return obj;
	}

	public static T GetDefaultValue<T>(Type schema, string fieldID)
	{
		FieldInfo[] fields = schema.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(DataBundleFieldAttribute), false);
			string text = null;
			if (customAttributes != null && customAttributes.Length > 0)
			{
				text = ((DataBundleFieldAttribute)customAttributes[0]).Identifier.ToString();
			}
			if (fieldInfo.Name == fieldID || text == fieldID)
			{
				object defaultValue = GetDefaultValue(fieldInfo);
				if (defaultValue != null)
				{
					return (T)defaultValue;
				}
			}
		}
		return CreateDefault<T>();
	}

	public static object GetDefaultValue(FieldInfo fieldInfo)
	{
		object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(DataBundleDefaultValueAttribute), false);
		if (customAttributes != null && customAttributes.Length > 0)
		{
			object value = ((DataBundleDefaultValueAttribute)customAttributes[0]).Value;
			if (value != null)
			{
				Type type = value.GetType();
				if (type == fieldInfo.FieldType || (type == typeof(string) && (fieldInfo.FieldType == typeof(DataBundleRecordKey) || fieldInfo.FieldType == typeof(DataBundleRecordTable) || fieldInfo.FieldType.IsEnum)))
				{
					return value;
				}
			}
		}
		return null;
	}

	public static FieldInfo KeyField(Type schema)
	{
		FieldInfo[] fields = schema.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(DataBundleKeyAttribute), false);
			if (customAttributes != null && customAttributes.Length > 0)
			{
				return fieldInfo;
			}
		}
		return null;
	}

	public static string FieldName(FieldInfo field)
	{
		object[] customAttributes = field.GetCustomAttributes(typeof(DataBundleFieldAttribute), false);
		if (customAttributes != null && customAttributes.Length > 0)
		{
			int identifier = ((DataBundleFieldAttribute)customAttributes[0]).Identifier;
			if (identifier >= 0)
			{
				return identifier.ToString();
			}
		}
		return field.Name;
	}

	public static bool IsTypeNumeric(Type type)
	{
		return type == typeof(int) || type == typeof(float);
	}

	public static bool IsSchemaLocalizable(Type schema)
	{
		object[] customAttributes = schema.GetCustomAttributes(typeof(DataBundleClassAttribute), false);
		if (customAttributes != null && customAttributes.Length > 0)
		{
			return ((DataBundleClassAttribute)customAttributes[0]).Localizable;
		}
		return false;
	}

	public static T InitializeRecord<T>(DataBundleRecordKey record)
	{
		if (DataBundleRuntime.Instance == null || !DataBundleRuntime.Instance.Initialized)
		{
			return default(T);
		}
		return DataBundleRuntime.Instance.InitializeRecord<T>(record);
	}

	public static T[] InitializeRecords<T>(string table)
	{
		if (DataBundleRuntime.Instance == null || !DataBundleRuntime.Instance.Initialized)
		{
			return null;
		}
		return DataBundleRuntime.Instance.InitializeRecords<T>(table);
	}
}
