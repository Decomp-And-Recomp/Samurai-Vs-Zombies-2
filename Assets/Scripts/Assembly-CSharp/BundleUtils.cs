using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class BundleUtils
{
	private static string BaseDirectory()
	{
		return AJavaTools.GameInfo.GetExternalFilesPath();
	}

	public static string DownloadedAssetBundlePath()
	{
		return BaseDirectory() + "/Library/AssetBundles/" + AWSServerConfig.DownloadSubDirectory;
	}

	public static string DownloadStagingAssetBundlePath()
	{
		return BaseDirectory() + "/Library/AssetBundlesStaging/" + AWSServerConfig.DownloadSubDirectory;
	}

	public static string LocalAssetBundlePath()
	{
		return string.Format("{0}/{1}/", Application.streamingAssetsPath, "AssetBundles");
	}

	public static string GetBundlePath(string bundleName, string overrideFolder)
	{
		return GetBundlePath(bundleName, overrideFolder, true);
	}

	public static string GetBundlePath(string bundleName, string overrideFolder, bool prependFileProtocol)
	{
		string text = null;
		string arg = LocalAssetBundlePath();
		if (!string.IsNullOrEmpty(overrideFolder))
		{
			text = string.Format("{0}{1}/{2}", arg, overrideFolder, bundleName);
		}
		if (text == null)
		{
			text = string.Format("{0}{1}", arg, bundleName);
		}
		if (prependFileProtocol && !text.Contains("file://"))
		{
			text = "file://" + text;
		}
		return text;
	}

	public static string GetAssetInBundleKey(string assetName, string bundleName)
	{
		return assetName + "." + bundleName;
	}

	public static void DeleteFileIfExists(string filePath)
	{
		if (File.Exists(filePath))
		{
			File.SetAttributes(filePath, FileAttributes.Normal);
			File.Delete(filePath);
		}
	}

	public static void CopyFile(string srcPath, string destPath)
	{
		File.Copy(srcPath, destPath, true);
	}

	public static bool FileCompare(string file1, string file2)
	{
		if (file1 == file2)
		{
			return true;
		}
		FileStream fileStream = new FileStream(file1, FileMode.Open, FileAccess.Read);
		FileStream fileStream2 = new FileStream(file2, FileMode.Open, FileAccess.Read);
		if (fileStream.Length != fileStream2.Length)
		{
			fileStream.Close();
			fileStream2.Close();
			return false;
		}
		int num;
		int num2;
		do
		{
			num = fileStream.ReadByte();
			num2 = fileStream2.ReadByte();
		}
		while (num == num2 && num != -1);
		fileStream.Close();
		fileStream2.Close();
		return num - num2 == 0;
	}

	public static string Md5HashFromFile(string file)
	{
		StringBuilder stringBuilder = new StringBuilder();
		FileStream fileStream = new FileStream(file, FileMode.Open);
		MD5 mD = new MD5CryptoServiceProvider();
		byte[] array = mD.ComputeHash(fileStream);
		fileStream.Close();
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			stringBuilder.Append(b.ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	public static string Md5HashFromString(string source)
	{
		StringBuilder stringBuilder = new StringBuilder();
		MD5 mD = new MD5CryptoServiceProvider();
		byte[] array = mD.ComputeHash(Encoding.UTF8.GetBytes(source));
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			stringBuilder.Append(b.ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	public static string MakeBundleNameFromPath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return string.Empty;
		}
		return path.Replace('/', '_').Replace('\\', '_').Replace('@', '_')
			.Replace(' ', '_');
	}

	public static Material ValidateMaterial(Material material)
	{
		if (material != null && material.shader != null)
		{
			string name = material.name;
			bool flag = false;
			if (BundleLoader.UseAssetCache())
			{
				Material cached = AssetCache.GetCached(material);
				if (cached != null)
				{
					return cached;
				}
			}
			if (!material.shader.isSupported || (Application.isEditor && !BundleLoader.UseLocalPrefabs()))
			{
				Shader shader = Shader.Find(material.shader.name);
				if (shader != null && shader.GetInstanceID() != material.shader.GetInstanceID())
				{
					material = Object.Instantiate(material) as Material;
					flag = true;
					material.name = name;
					material.shader = shader;
				}
			}
			if (BundleLoader.UseAssetCache())
			{
				Texture2D texture2D = null;
				if (material.HasProperty("_MainTex"))
				{
					texture2D = material.mainTexture as Texture2D;
				}
				if (texture2D != null)
				{
					Texture2D cached2 = AssetCache.GetCached(texture2D);
					if (cached2 != null)
					{
						if (cached2.GetInstanceID() != texture2D.GetInstanceID())
						{
							if (!flag)
							{
								material = Object.Instantiate(material) as Material;
								flag = true;
								material.name = name;
							}
							material.mainTexture = cached2;
						}
					}
					else
					{
						AssetCache.Cache(texture2D);
					}
				}
			}
			if (BundleLoader.UseAssetCache())
			{
				AssetCache.Cache(material);
			}
		}
		return material;
	}

	public static Dictionary<string, Material> ValidateMaterials(GameObject rootObject)
	{
		Dictionary<string, Material> dictionary = new Dictionary<string, Material>();
		if (rootObject != null)
		{
			Component[] componentsInChildren = rootObject.GetComponentsInChildren(typeof(Renderer), true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Renderer renderer = (Renderer)componentsInChildren[i];
				renderer.castShadows = false;
				renderer.receiveShadows = false;
				if (renderer.sharedMaterial != null)
				{
					renderer.sharedMaterial = ValidateMaterial(renderer.sharedMaterial);
					dictionary[renderer.sharedMaterial.name] = renderer.sharedMaterial;
				}
			}
		}
		return dictionary;
	}

	public static string GetSystemLanguage()
	{
		return "English"; // implement later!!!
		string language = NUF.GetLanguage();
		string[] supportedLanguages = GeneralConfig.SupportedLanguages;
		foreach (string text in supportedLanguages)
		{
			if (text == language)
			{
				return language;
			}
		}
		return GeneralConfig.SupportedLanguages[0];
	}

	public static void UpdateLocalDataBundleInfo(string dbVersion, string incrBuild, string dbPath, string dbType)
	{
		PlayerPrefs.SetString(string.Format(UpdateCheckData.DBVer, dbType), dbVersion);
		PlayerPrefs.SetString(string.Format(UpdateCheckData.IncrBuild, dbType), incrBuild);
		PlayerPrefs.SetString(string.Format(UpdateCheckData.DBMD5, dbType), Md5HashFromFile(dbPath));
		PlayerPrefs.Save();
	}

	public static string GetLocalDataBundleVersion(string dbType)
	{
		string key = string.Format(UpdateCheckData.DBVer, dbType);
		if (PlayerPrefs.HasKey(key))
		{
			return PlayerPrefs.GetString(key);
		}
		return string.Empty;
	}

	public static string GetLocalDataBundleMD5(string dbType)
	{
		string key = string.Format(UpdateCheckData.DBMD5, dbType);
		if (PlayerPrefs.HasKey(key))
		{
			return PlayerPrefs.GetString(key);
		}
		return string.Empty;
	}

	public static string GetLocalIncrementalBuild(string dbType)
	{
		string key = string.Format(UpdateCheckData.IncrBuild, dbType);
		if (PlayerPrefs.HasKey(key))
		{
			return PlayerPrefs.GetString(key);
		}
		return string.Empty;
	}
}
