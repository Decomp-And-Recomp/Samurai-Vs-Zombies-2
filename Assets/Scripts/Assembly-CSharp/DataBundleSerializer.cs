using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public class DataBundleSerializer
{
	public static string firstChar = "!";

	private static void SerializeUniqueStrings(string language, List<string> stringList)
	{
		using (FileStream fileStream = new FileStream(AssetBundleConfig.BundleDirectory + language + "/" + AssetBundleConfig.DataBundleStringList, FileMode.Create))
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			try
			{
				binaryFormatter.Serialize(fileStream, stringList);
			}
			catch (SerializationException)
			{
			}
			finally
			{
				fileStream.Close();
			}
		}
	}

	public static List<string> GenerateUniqueStrings(IList<DataBundleHashObject> hashValues, Dictionary<string, Dictionary<string, Type>> fieldTypeLookup)
	{
		List<string> list = new List<string>();
		if (hashValues.Count > 0)
		{
			foreach (KeyValuePair<string, Dictionary<string, Type>> item in fieldTypeLookup)
			{
				if (!list.Contains(item.Key))
				{
					list.Add(item.Key);
				}
				foreach (string key in item.Value.Keys)
				{
					if (!list.Contains(key))
					{
						list.Add(key);
					}
				}
			}
			foreach (DataBundleHashObject hashValue in hashValues)
			{
				if (!list.Contains(hashValue.Table))
				{
					list.Add(hashValue.Table);
				}
				if (!list.Contains(hashValue.RecordKey))
				{
					list.Add(hashValue.RecordKey);
				}
			}
			if (list.Count > 32767)
			{
			}
			list.Sort();
			list.Insert(0, firstChar + Application.unityVersion);
		}
		return list;
	}

	public static bool SerializeHashTable(string language, Hashtable htToSerialize, IList<DataBundleHashObject> hashValues, List<string> stringList, Dictionary<string, Dictionary<string, Type>> fieldTypeLookup)
	{
		if (!Directory.Exists(AssetBundleConfig.BundleDirectory))
		{
			Directory.CreateDirectory(AssetBundleConfig.BundleDirectory);
		}
		if (!Directory.Exists(AssetBundleConfig.BundleDirectory + language + "/"))
		{
			Directory.CreateDirectory(AssetBundleConfig.BundleDirectory + language + "/");
		}
		SerializeUniqueStrings(language, stringList);
		if (!GenerateUdamanHashtable(htToSerialize, hashValues, fieldTypeLookup, stringList))
		{
			return false;
		}
		htToSerialize.Add(AssetBundleConfig.VersionKey, Application.unityVersion);
		using (FileStream fileStream = new FileStream(AssetBundleConfig.BundleDirectory + language + "/" + AssetBundleConfig.DataBundleName, FileMode.Create))
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			try
			{
				binaryFormatter.Serialize(fileStream, htToSerialize);
			}
			catch (SerializationException)
			{
			}
			finally
			{
				fileStream.Close();
			}
		}
		return true;
	}

	public static bool GenerateUdamanHashtable(Hashtable htToSerialize, IList<DataBundleHashObject> hashValues, Dictionary<string, Dictionary<string, Type>> fieldTypeLookup, List<string> stringList)
	{
		DataBundleRuntimeLegacyUdaman dataBundleRuntimeLegacyUdaman = new DataBundleRuntimeLegacyUdaman();
		StringBuilder stringBuilder = new StringBuilder();
		Dictionary<long, List<long>> dictionary = new Dictionary<long, List<long>>();
		foreach (DataBundleHashObject hashValue in hashValues)
		{
			Type typeFromHandle = typeof(string);
			try
			{
				typeFromHandle = fieldTypeLookup[hashValue.Schema][hashValue.FieldID];
			}
			catch (KeyNotFoundException)
			{
				if (!(hashValue.Schema == typeof(TaggedString).ToString()) || !(hashValue.FieldID == DataBundleEditorData.locCommentField))
				{
					stringBuilder.AppendLine("This data will not be added to the serialized hashtable. Orphaned data: " + hashValue.Key);
				}
				continue;
			}
			try
			{
				if (typeFromHandle.IsSubclassOf(typeof(UnityEngine.Object)))
				{
					htToSerialize.Add(dataBundleRuntimeLegacyUdaman.GetHashCode(hashValue.Key, stringList), BundleAssetInfo.Instance.RegisterAsset(hashValue.ValueAsString));
				}
				else if (typeFromHandle == typeof(DataBundleRecordKey) || typeFromHandle == typeof(DataBundleRecordTable))
				{
					htToSerialize.Add(dataBundleRuntimeLegacyUdaman.GetHashCode(hashValue.Key, stringList), dataBundleRuntimeLegacyUdaman.GetHashCode(hashValue.ValueAsString, stringList));
				}
				else
				{
					htToSerialize.Add(dataBundleRuntimeLegacyUdaman.GetHashCode(hashValue.Key, stringList), DataBundleUtils.ConvertStringToObjectOfType(hashValue.ValueAsString, typeFromHandle, true));
				}
			}
			catch (IndexOutOfRangeException ex2)
			{
				stringBuilder.AppendLine(ex2.Message + ": " + hashValue.Key);
				continue;
			}
			catch (ArgumentException)
			{
				return false;
			}
			long hashCode = dataBundleRuntimeLegacyUdaman.GetHashCode(hashValue.Schema, hashValue.Table, null, null, stringList);
			if (!dictionary.ContainsKey(hashCode))
			{
				dictionary.Add(hashCode, new List<long>());
			}
			long hashCode2 = dataBundleRuntimeLegacyUdaman.GetHashCode(null, null, hashValue.RecordKey, null, stringList);
			if (!dictionary[hashCode].Contains(hashCode2))
			{
				dictionary[hashCode].Add(hashCode2);
			}
			long hashCode3 = dataBundleRuntimeLegacyUdaman.GetHashCode(hashValue.Schema, null, null, null, stringList);
			if (!dictionary.ContainsKey(hashCode3))
			{
				dictionary.Add(hashCode3, new List<long>());
			}
			long hashCode4 = dataBundleRuntimeLegacyUdaman.GetHashCode(null, hashValue.Table, null, null, stringList);
			if (!dictionary[hashCode3].Contains(hashCode4))
			{
				dictionary[hashCode3].Add(hashCode4);
			}
		}
		foreach (KeyValuePair<long, List<long>> item in dictionary)
		{
			try
			{
				htToSerialize.Add(item.Key, item.Value);
			}
			catch (ArgumentException)
			{
				return false;
			}
		}
		if (stringBuilder.Length > 0)
		{
		}
		return true;
	}

	public static Hashtable DeserializeHashTable()
	{
		Hashtable hashtable = null;
		string systemLanguage = BundleUtils.GetSystemLanguage();
		using (FileStream fileStream = new FileStream(AssetBundleConfig.BundleDataPath + "/" + systemLanguage + "/" + AssetBundleConfig.DataBundleName, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				hashtable = (Hashtable)binaryFormatter.Deserialize(fileStream);
			}
			catch (SerializationException)
			{
			}
			finally
			{
				fileStream.Close();
			}
		}
		if (hashtable.ContainsKey(AssetBundleConfig.VersionKey))
		{
			if (!hashtable[AssetBundleConfig.VersionKey].ToString().Equals(Application.unityVersion))
			{
				throw new Exception(AssetBundleConfig.DataBundleName + " was built with a different verion of Unity. Expected:" + Application.unityVersion + " Found:" + hashtable[AssetBundleConfig.VersionKey].ToString());
			}
			return hashtable;
		}
		throw new Exception(AssetBundleConfig.DataBundleName + ": No version information was found. Expected:" + Application.unityVersion);
	}

	public static List<string> DeserializeStringList()
	{
		string systemLanguage = BundleUtils.GetSystemLanguage();
		List<string> list = null;
		using (FileStream fileStream = new FileStream(AssetBundleConfig.BundleDataPath + "/" + systemLanguage + "/" + AssetBundleConfig.DataBundleStringList, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				list = (List<string>)binaryFormatter.Deserialize(fileStream);
			}
			catch (SerializationException)
			{
			}
			finally
			{
				fileStream.Close();
			}
		}
		if (list.Count > 0)
		{
			if (!list[0].Equals(firstChar + Application.unityVersion))
			{
				throw new Exception(AssetBundleConfig.DataBundleStringList + " was built with a different verion of Unity. Expected:" + Application.unityVersion + " Found:" + list[0]);
			}
			return list;
		}
		throw new Exception(AssetBundleConfig.DataBundleStringList + ": No version information was found. Expected:" + Application.unityVersion);
	}
}
