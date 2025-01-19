using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class BundleManifest
{
	public class BundleData
	{
		public int version;

		public long size;

		public string name;

		public string md5Hash;

		public EPortableQualitySetting minQualitySetting;

		public EPortableQualitySetting maxQualitySetting;

		public string locLanguage;

		public string FolderName()
		{
			if (!string.IsNullOrEmpty(locLanguage) && locLanguage != DataBundleRuntime.notLocalized)
			{
				return locLanguage;
			}
			if (minQualitySetting == EPortableQualitySetting.None && maxQualitySetting == EPortableQualitySetting.None)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (minQualitySetting != 0)
			{
				stringBuilder.Append(minQualitySetting.ToString());
			}
			stringBuilder.Append('_');
			if (maxQualitySetting != 0)
			{
				stringBuilder.Append(maxQualitySetting.ToString());
			}
			return stringBuilder.ToString();
		}

		public bool HasSameHash(BundleData data)
		{
			return string.Compare(data.name, name) == 0 && data.minQualitySetting == minQualitySetting && data.maxQualitySetting == maxQualitySetting && string.Compare(data.md5Hash, md5Hash) == 0;
		}

		public bool IsOlderThan(BundleData data)
		{
			return data.version > version && string.Compare(data.name, name) == 0;
		}

		public bool HasSameQuallitySetting(BundleData data)
		{
			return string.Compare(data.name, name) == 0 && data.minQualitySetting == minQualitySetting && data.maxQualitySetting == maxQualitySetting;
		}

		public bool HasSameName(BundleData data)
		{
			return string.Compare(data.name, name) == 0;
		}
	}

	public string startTag;

	public List<BundleData> bundleList;

	public string ManifestHash;

	public BundleManifest()
	{
		startTag = AssetBundleConfig.ManifestStartTag;
		bundleList = new List<BundleData>();
	}

	public BundleManifest(BundleManifest otherManifest)
	{
		startTag = otherManifest.startTag;
		foreach (BundleData bundle in otherManifest.bundleList)
		{
			bundleList.Add(bundle);
		}
	}

	public static BundleManifest ManifestFromString(string manifestText)
	{
		BundleManifest bundleManifest = new BundleManifest();
		bundleManifest.bundleList = new List<BundleData>();
		bundleManifest.ParceFromText(manifestText);
		return bundleManifest;
	}

	public static BundleManifest ManifestFromFile(string filePath)
	{
		BundleManifest bundleManifest = new BundleManifest();
		bundleManifest.ReadManifestFile(filePath);
		return bundleManifest;
	}

	public void ClearJustBuiltFlags()
	{
	}

	public void AddToManifest(BundleData bundle)
	{
		bundleList.Add(bundle);
	}

	public void InsertBundleData(BundleData data, int insAt)
	{
		if (insAt >= 0 && insAt < bundleList.Count)
		{
			bundleList.Insert(insAt, data);
		}
		else
		{
			AddToManifest(data);
		}
	}

	public int GetBundleVersion(string bundleName)
	{
		BundleData bundleData = FindBundleByName(bundleName);
		if (bundleData == null)
		{
			return -1;
		}
		return bundleData.version;
	}

	public BundleData FindBundleByName(string name)
	{
		foreach (BundleData bundle in bundleList)
		{
			if (string.Compare(bundle.name, name) == 0)
			{
				return bundle;
			}
		}
		return null;
	}

	public List<BundleData> FindAllByName(string name)
	{
		List<BundleData> list = new List<BundleData>();
		foreach (BundleData bundle in bundleList)
		{
			if (string.Compare(bundle.name, name) == 0)
			{
				list.Add(bundle);
			}
		}
		return list;
	}

	public BundleData FindBundleByNameAndQualityAndLoc(string name, EPortableQualitySetting minQuality, EPortableQualitySetting maxQuality, string locLanguage)
	{
		foreach (BundleData bundle in bundleList)
		{
			if (string.Compare(bundle.name, name) == 0 && bundle.minQualitySetting == minQuality && bundle.maxQualitySetting == maxQuality && bundle.locLanguage == locLanguage)
			{
				return bundle;
			}
		}
		return null;
	}

	public bool DoesFilesystemContainAllManifestFiles(string bundlePath)
	{
		foreach (BundleData bundle in bundleList)
		{
			string path = bundlePath + bundle.name;
			if (!File.Exists(path))
			{
				return false;
			}
		}
		return true;
	}

	public void AddToManifest(string name, string md5Hash, long size, int version, EPortableQualitySetting minQuality, EPortableQualitySetting maxQuality, string locLanguage)
	{
		BundleData bundleData = new BundleData();
		bundleData.name = name;
		bundleData.version = version;
		bundleData.size = size;
		bundleData.md5Hash = md5Hash;
		bundleData.minQualitySetting = minQuality;
		bundleData.maxQualitySetting = maxQuality;
		bundleData.locLanguage = locLanguage;
		AddToManifest(bundleData);
	}

	public void AddToManifest(string name, string md5Hash, long size, int version, EPortableQualitySetting minQuality, EPortableQualitySetting maxQuality)
	{
		AddToManifest(name, md5Hash, size, version, minQuality, maxQuality, DataBundleRuntime.notLocalized);
	}

	public int RemoveFromManifest(string name, EPortableQualitySetting minQuality, EPortableQualitySetting maxQuality, string locLanguage)
	{
		for (int i = 0; i < bundleList.Count; i++)
		{
			BundleData bundleData = bundleList[i];
			if (string.Compare(bundleData.name, name) == 0 && bundleData.minQualitySetting == minQuality && bundleData.maxQualitySetting == maxQuality && string.Compare(bundleData.locLanguage, locLanguage) == 0)
			{
				bundleList.RemoveAt(i);
				return i;
			}
		}
		return -1;
	}

	public void ReadManifestFile(string path)
	{
		if (File.Exists(path))
		{
			FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8);
			ParceFromText(streamReader.ReadToEnd());
			streamReader.Close();
			fileStream.Close();
		}
	}

	public void ReadOrCreateManifestFile(string path)
	{
		if (File.Exists(path))
		{
			File.SetAttributes(path, FileAttributes.Normal);
			FileStream fileStream = new FileStream(path, FileMode.Open);
			StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8);
			ParceFromText(streamReader.ReadToEnd());
			streamReader.Close();
			fileStream.Close();
		}
		else
		{
			ReplaceOrCreateManifestFile(path);
		}
	}

	public void ParceFromText(string manifestText)
	{
		Regex regex = new Regex("[\n\r]+");
		string[] array = regex.Split(manifestText);
		startTag = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			string input = array[i];
			Regex regex2 = new Regex(AssetBundleConfig.ManifestLineValueSeperator);
			string[] array2 = regex2.Split(input);
			if (array2.Length != 7)
			{
				break;
			}
			BundleData bundleData = new BundleData();
			bundleData.name = array2[0];
			bundleData.version = int.Parse(array2[1]);
			bundleData.size = int.Parse(array2[2]);
			bundleData.md5Hash = array2[3];
			bundleData.minQualitySetting = (EPortableQualitySetting)(int)Enum.Parse(typeof(EPortableQualitySetting), array2[4]);
			bundleData.maxQualitySetting = (EPortableQualitySetting)(int)Enum.Parse(typeof(EPortableQualitySetting), array2[5]);
			bundleData.locLanguage = array2[6].Trim();
			bundleList.Add(bundleData);
		}
		ManifestHash = BundleUtils.Md5HashFromString(manifestText);
	}

	public void ReplaceOrCreateManifestFile(string manifestPath)
	{
		ReplaceOrCreateManifestFile(manifestPath, null);
	}

	public void ReplaceOrCreateManifestFile(string manifestPath, string language)
	{
		BundleUtils.DeleteFileIfExists(manifestPath);
		string directoryName = Path.GetDirectoryName(manifestPath);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		StreamWriter streamWriter = File.CreateText(manifestPath);
		streamWriter.WriteLine(startTag);
		foreach (BundleData bundle in bundleList)
		{
			if (string.IsNullOrEmpty(language) || bundle.locLanguage == DataBundleRuntime.notLocalized || bundle.locLanguage == language)
			{
				string value = string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}", AssetBundleConfig.ManifestLineValueSeperator, bundle.name, bundle.version.ToString(), bundle.size.ToString(), bundle.md5Hash, bundle.minQualitySetting.ToString(), bundle.maxQualitySetting.ToString(), bundle.locLanguage);
				streamWriter.WriteLine(value);
			}
		}
		streamWriter.Close();
	}

	public static void AppendToManifestFile(BundleData bundle, string filePath)
	{
		if (!File.Exists(filePath))
		{
		}
		StreamWriter streamWriter = File.AppendText(filePath);
		string value = string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}", AssetBundleConfig.ManifestLineValueSeperator, bundle.name, bundle.version.ToString(), bundle.size.ToString(), bundle.md5Hash, bundle.minQualitySetting.ToString(), bundle.maxQualitySetting.ToString(), bundle.locLanguage);
		streamWriter.WriteLine(value);
		streamWriter.Close();
	}

	public static bool DoesFilesystemContainAllFilesInManifest(string manifestPath, string bundlePath)
	{
		if (!File.Exists(manifestPath))
		{
			return false;
		}
		BundleManifest bundleManifest = new BundleManifest();
		bundleManifest.ReadOrCreateManifestFile(manifestPath);
		return bundleManifest.DoesFilesystemContainAllManifestFiles(bundlePath);
	}

	public bool HasBundleForQuality(string bundleName, EPortableQualitySetting quality)
	{
		foreach (BundleData bundle in bundleList)
		{
			if (bundle.name != bundleName || (bundle.minQualitySetting > quality && bundle.minQualitySetting != 0) || (bundle.maxQualitySetting < quality && bundle.maxQualitySetting != 0))
			{
				continue;
			}
			return true;
		}
		return false;
	}

	public string GetBundleOverrideFolder(string bundleName, EPortableQualitySetting quality)
	{
		foreach (BundleData bundle in bundleList)
		{
			if (bundle.name != bundleName || (bundle.minQualitySetting == EPortableQualitySetting.None && bundle.maxQualitySetting == EPortableQualitySetting.None) || (bundle.minQualitySetting > quality && bundle.minQualitySetting != 0) || (bundle.maxQualitySetting < quality && bundle.maxQualitySetting != 0))
			{
				continue;
			}
			return bundle.FolderName();
		}
		return string.Empty;
	}

	public List<BundleData> BundlesForQuality(EPortableQualitySetting quality, bool getQualityUpdates)
	{
		string systemLanguage = BundleUtils.GetSystemLanguage();
		List<BundleData> list = new List<BundleData>();
		List<BundleData> list2 = null;
		if (!getQualityUpdates)
		{
			list2 = BundlesForQuality(EPortableQualitySetting.Low, true);
		}
		BundleData bd;
		foreach (BundleData bundle in bundleList)
		{
			bd = bundle;
			if ((bd.locLanguage != DataBundleRuntime.notLocalized && bd.locLanguage != systemLanguage) || (bd.minQualitySetting > quality && bd.minQualitySetting != 0) || (bd.maxQualitySetting < quality && bd.maxQualitySetting != 0))
			{
				continue;
			}
			BundleData bundleData = list.Find((BundleData b) => b.HasSameName(bd));
			bool flag = true;
			if (!getQualityUpdates && list2 != null)
			{
				flag = list2.Find((BundleData b) => b.HasSameName(bd)) != null;
			}
			if (flag)
			{
				if (bundleData == null)
				{
					list.Add(bd);
				}
				else if ((bd.minQualitySetting != 0 || bd.maxQualitySetting != 0) && ((bd.maxQualitySetting <= bundleData.maxQualitySetting && bd.maxQualitySetting != 0) || bundleData.maxQualitySetting == EPortableQualitySetting.None) && ((bd.minQualitySetting >= bundleData.minQualitySetting && bd.minQualitySetting != 0) || bundleData.minQualitySetting == EPortableQualitySetting.None))
				{
					list.Remove(bundleData);
					list.Add(bd);
				}
			}
		}
		return list;
	}
}
