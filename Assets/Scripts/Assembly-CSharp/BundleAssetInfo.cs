using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class BundleAssetInfo
{
	public Dictionary<short, List<short>> AssetToBundleList;

	public Dictionary<short, List<short>> BundleDependencyList;

	public List<string> AssetList;

	private static BundleAssetInfo instance;

	public static BundleAssetInfo Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new BundleAssetInfo();
			}
			return instance;
		}
	}

	private BundleAssetInfo()
	{
		AssetToBundleList = new Dictionary<short, List<short>>();
		BundleDependencyList = new Dictionary<short, List<short>>();
		AssetList = new List<string>();
	}

	public int RegisterAsset(string assetPath)
	{
		int result = -1;
		if (string.IsNullOrEmpty(assetPath))
		{
			return result;
		}
		try
		{
			if (!AssetList.Contains(assetPath))
			{
				AssetList.Add(assetPath);
			}
			return AssetList.IndexOf(assetPath);
		}
		catch (Exception)
		{
			return -1;
		}
	}

	public List<string> BundlesAssetIsIn(string assetPath)
	{
		short num = (short)AssetList.IndexOf(assetPath);
		if (num >= 0 && AssetToBundleList.ContainsKey(num))
		{
			List<string> list = new List<string>();
			{
				foreach (short item in AssetToBundleList[num])
				{
					if (item >= 0 && item < AssetList.Count)
					{
						list.Add(AssetList[item]);
					}
				}
				return list;
			}
		}
		return new List<string>();
	}

	public List<string> BundlesAssetIsDependentOn(string assetPath, string bundleName)
	{
		short num = (short)AssetList.IndexOf(assetPath);
		List<string> list = new List<string>();
		if (num >= 0 && AssetToBundleList.ContainsKey(num))
		{
			List<short> list2 = AssetToBundleList[num];
			short num2 = (short)AssetList.IndexOf(bundleName);
			if (num2 != -1 && list2.Contains(num2))
			{
				List<short> value = new List<short>();
				if (BundleDependencyList.TryGetValue(num2, out value))
				{
					foreach (short item in value)
					{
						if (item >= 0 && item < AssetList.Count)
						{
							list.Add(AssetList[item]);
						}
					}
				}
			}
		}
		return list;
	}

	public string GetAssetPath(int assetIndex)
	{
		if (assetIndex >= 0 && assetIndex < AssetList.Count)
		{
			return AssetList[assetIndex];
		}
		return string.Empty;
	}

	public void Serialize(string language)
	{
		if (AssetList.Count > 0 && !AssetList[AssetList.Count - 1].Equals(Application.unityVersion))
		{
			AssetList.Add(Application.unityVersion);
		}
		using (FileStream fileStream = new FileStream(AssetBundleConfig.BundleDirectory + language + "/" + AssetBundleConfig.BundleAssetInfoName, FileMode.Create))
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			try
			{
				binaryFormatter.Serialize(fileStream, AssetToBundleList);
				binaryFormatter.Serialize(fileStream, BundleDependencyList);
			}
			catch (SerializationException)
			{
			}
			finally
			{
				fileStream.Close();
			}
		}
		using (FileStream fileStream2 = new FileStream(AssetBundleConfig.BundleDirectory + language + "/" + AssetBundleConfig.BundleAssetList, FileMode.Create))
		{
			BinaryFormatter binaryFormatter2 = new BinaryFormatter();
			try
			{
				binaryFormatter2.Serialize(fileStream2, AssetList);
			}
			catch (SerializationException)
			{
			}
			finally
			{
				fileStream2.Close();
			}
		}
	}

	public static byte[] ReadBundle(string path)
	{
		if (!Application.isMobilePlatform)
		{
			return File.ReadAllBytes(path);
		}
		using (WWW www = new WWW(path))
		{
			while (!www.isDone)
			{
				// I am not implementing a coroutine for this
			}
			return www.bytes;
		}
	}

	public void DeserializeBundleAssetInfo()
	{
		if (AssetList.Count > 0 || AssetToBundleList.Count > 0)
		{
			return;
		}
		string systemLanguage = BundleUtils.GetSystemLanguage();
		byte[] info = ReadBundle(AssetBundleConfig.BundleDataPath + "/" + systemLanguage + "/" + AssetBundleConfig.BundleAssetInfoName);
		using (MemoryStream stream = new MemoryStream(info))
		{
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				AssetToBundleList = (Dictionary<short, List<short>>)binaryFormatter.Deserialize(stream);
				BundleDependencyList = (Dictionary<short, List<short>>)binaryFormatter.Deserialize(stream);
			}
			catch (SerializationException)
			{
			}
		}
		byte[] list = ReadBundle(AssetBundleConfig.BundleDataPath + "/" + systemLanguage + "/" + AssetBundleConfig.BundleAssetList);
		using (MemoryStream stream2 = new MemoryStream(list))
		{
			try
			{
				BinaryFormatter binaryFormatter2 = new BinaryFormatter();
				AssetList = (List<string>)binaryFormatter2.Deserialize(stream2);
			}
			catch (SerializationException)
			{
			}
		}
		if (AssetList.Count > 0)
		{
			//if (!AssetList[AssetList.Count - 1].Equals(Application.unityVersion))
			//{
			//	throw new Exception(AssetBundleConfig.BundleAssetList + " was built with a different verion of Unity. Expected:" + Application.unityVersion + " Found:" + AssetList[AssetList.Count - 1]);
			//}
			return;
		}
		throw new Exception(AssetBundleConfig.BundleAssetList + ": No version information was found. Expected:" + Application.unityVersion);
	}
}
