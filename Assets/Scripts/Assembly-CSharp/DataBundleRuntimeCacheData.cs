using System;
using System.Collections.Generic;
using System.Reflection;

public class DataBundleRuntimeCacheData
{
	public class DataBundleFieldData
	{
		public FieldInfo fInfo;

		public string fieldID;

		public bool staticResource;

		public DataBundleResourceGroup group;
	}

	private static Dictionary<string, List<DataBundleFieldData>> cachedFieldData = new Dictionary<string, List<DataBundleFieldData>>();

	public static List<DataBundleFieldData> GetFieldData(Type type)
	{
		CacheFieldData(type.Name, type);
		return cachedFieldData[type.Name];
	}

	public static List<DataBundleFieldData> GetFieldData(string schema)
	{
		if (cachedFieldData.ContainsKey(schema))
		{
			return cachedFieldData[schema];
		}
		return null;
	}

	public static void CacheFieldData(string typeName, Type objectDataType)
	{
		if (cachedFieldData.ContainsKey(typeName))
		{
			return;
		}
		cachedFieldData.Add(typeName, new List<DataBundleFieldData>());
		FieldInfo[] fields = objectDataType.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!DataBundleUtils.SupportedType(fieldInfo.FieldType) || fieldInfo.IsStatic || fieldInfo.IsLiteral || fieldInfo.IsInitOnly)
			{
				continue;
			}
			DataBundleFieldData dataBundleFieldData = new DataBundleFieldData();
			dataBundleFieldData.fInfo = fieldInfo;
			dataBundleFieldData.fieldID = fieldInfo.Name;
			dataBundleFieldData.staticResource = false;
			object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(DataBundleFieldAttribute), false);
			if (customAttributes != null && customAttributes.Length > 0)
			{
				DataBundleFieldAttribute dataBundleFieldAttribute = (DataBundleFieldAttribute)customAttributes[0];
				int identifier = dataBundleFieldAttribute.Identifier;
				if (identifier >= 0)
				{
					dataBundleFieldData.fieldID = identifier.ToString();
				}
				dataBundleFieldData.staticResource = dataBundleFieldAttribute.StaticResource;
				dataBundleFieldData.group = dataBundleFieldAttribute.Group;
			}
			cachedFieldData[typeName].Add(dataBundleFieldData);
		}
	}
}
