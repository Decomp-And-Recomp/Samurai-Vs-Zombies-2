using System;
using UnityEngine;

public static class SchemaFieldAdapter
{
	public static T Deserialize<T>(T fieldValue)
	{
		if (fieldValue is GameObject)
		{
			return (T)(object)UnityEngine.Object.Instantiate(fieldValue as GameObject, Vector3.zero, Quaternion.identity);
		}
		return fieldValue;
	}

	public static string GetAssetPath(Type schemaType, string table, string key, string fieldName)
	{
		return DataBundleRuntime.Instance.GetValue<string>(schemaType, table, key, fieldName, true);
	}
}
