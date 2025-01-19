using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GripTech
{
	public static class ResourceLoader
	{
		private static Regex resourceLoaderRegex = new Regex(".*/Resources/");

		public static List<UnityEngine.Object> Load(List<string> paths)
		{
			List<UnityEngine.Object> list = new List<UnityEngine.Object>(paths.Count);
			foreach (string path in paths)
			{
				list.Add(Load(path));
			}
			return list;
		}

		public static UnityEngine.Object Load(string path)
		{
			return Resources.Load(QualityOverridePath(path));
		}

		public static UnityEngine.Object Load(string path, Type type)
		{
			return Resources.Load(QualityOverridePath(path), type);
		}

		public static void Unload(UnityEngine.Object obj)
		{
			if (obj != null && !(obj is GameObject))
			{
				Resources.UnloadAsset(obj);
			}
		}

		private static string QualityOverridePath(string path)
		{
			SerializableDictionary<string, string> serializableDictionary = AssetQualityManifest.AssetQualityOverrides[(int)PortableQualitySettings.GetQuality()];
			string text = path;
			if (serializableDictionary != null && serializableDictionary.ContainsKey(path))
			{
				text = serializableDictionary[path];
			}
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}
			text = resourceLoaderRegex.Replace(text, string.Empty);
			if (Path.HasExtension(text))
			{
				text = Path.GetDirectoryName(text) + "/" + Path.GetFileNameWithoutExtension(text);
			}
			return text;
		}
	}
}
