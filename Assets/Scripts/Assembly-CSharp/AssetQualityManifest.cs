using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public static class AssetQualityManifest
{
	private static SerializableDictionary<string, string>[] assetQualityOverrides;

	public static string AssetQualityManifestSavePath
	{
		get
		{
			return "AssetQualityManifest";
		}
	}

	public static SerializableDictionary<string, string>[] AssetQualityOverrides
	{
		get
		{
			if (assetQualityOverrides == null)
			{
				InitializeManifest();
			}
			return assetQualityOverrides;
		}
	}

	private static void InitializeManifest()
	{
		LoadQualityManifest();
		if (assetQualityOverrides == null)
		{
			assetQualityOverrides = new SerializableDictionary<string, string>[5];
		}
	}

	public static void AddToManifest(string file, string fileOverride, EPortableQualitySetting qualityLevel)
	{
		if (assetQualityOverrides == null)
		{
			InitializeManifest();
		}
		if (assetQualityOverrides[(int)qualityLevel] == null)
		{
			assetQualityOverrides[(int)qualityLevel] = new SerializableDictionary<string, string>();
		}
		if (assetQualityOverrides[(int)qualityLevel].ContainsKey(file))
		{
			assetQualityOverrides[(int)qualityLevel][file] = fileOverride;
		}
		else
		{
			assetQualityOverrides[(int)qualityLevel].Add(file, fileOverride);
		}
	}

	public static void ClearQualityManifest()
	{
		assetQualityOverrides = new SerializableDictionary<string, string>[5];
	}

	public static void SaveQualityManifest()
	{
		if (assetQualityOverrides == null)
		{
			InitializeManifest();
		}
		if (!Directory.Exists(Path.GetDirectoryName(AssetQualityManifestSavePath)))
		{
			Directory.CreateDirectory(Path.GetDirectoryName(AssetQualityManifestSavePath));
		}
		using (FileStream stream = new FileStream(AssetQualityManifestSavePath, FileMode.Create, FileAccess.Write))
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(SerializableDictionary<string, string>[]));
			xmlSerializer.Serialize(stream, assetQualityOverrides);
		}
	}

	private static void LoadQualityManifest()
	{
		TextAsset textAsset = Resources.Load(AssetQualityManifestSavePath) as TextAsset;
		if (textAsset != null)
		{
			MemoryStream stream = new MemoryStream(textAsset.bytes);
			XmlReader xmlReader = XmlReader.Create(stream);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(SerializableDictionary<string, string>[]));
			assetQualityOverrides = xmlSerializer.Deserialize(xmlReader) as SerializableDictionary<string, string>[];
		}
	}
}
