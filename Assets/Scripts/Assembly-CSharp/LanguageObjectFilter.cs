using System;
using UnityEngine;

[AddComponentMenu("Quality Settings/Language Object Filter")]
public class LanguageObjectFilter : MonoBehaviour
{
	[Serializable]
	public class FilterSetting
	{
		public string[] onlyForLanguages;

		public string[] notForLanguages;

		public GameObject[] objects;
	}

	public bool destroyOnDebug;

	public FilterSetting[] settings;

	private void Awake()
	{
		if (settings == null)
		{
			return;
		}
		string language = BundleUtils.GetSystemLanguage();
		FilterSetting[] array = settings;
		foreach (FilterSetting filterSetting in array)
		{
			bool flag = Array.Find(filterSetting.onlyForLanguages, (string str) => language.StartsWith(str, StringComparison.OrdinalIgnoreCase)) == null;
			if (!flag)
			{
				flag = Array.Find(filterSetting.notForLanguages, (string str) => language.StartsWith(str, StringComparison.OrdinalIgnoreCase)) != null;
			}
			if (!flag)
			{
				continue;
			}
			GameObject[] objects = filterSetting.objects;
			foreach (GameObject gameObject in objects)
			{
				if (gameObject != null)
				{
					if (!destroyOnDebug || !Debug.isDebugBuild)
					{
						gameObject.SetActive(false);
					}
					else
					{
						UnityEngine.Object.DestroyImmediate(gameObject);
					}
				}
			}
		}
	}
}
