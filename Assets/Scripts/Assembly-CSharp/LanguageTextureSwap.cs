using System;
using UnityEngine;

[AddComponentMenu("Localization/Language Texture Swap")]
public class LanguageTextureSwap : MonoBehaviour
{
	[Serializable]
	public class SwapSetting
	{
		public string language;

		public string swapTexturePath;
	}

	public SwapSetting[] settings;

	private void Awake()
	{
		if (settings == null)
		{
			return;
		}
		string systemLanguage = BundleUtils.GetSystemLanguage();
		SwapSetting swapSetting = Array.Find(settings, (SwapSetting setting) => systemLanguage.StartsWith(setting.language, StringComparison.OrdinalIgnoreCase));
		if (swapSetting != null)
		{
			SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource(swapSetting.swapTexturePath, 1);
			if (cachedResource != null)
			{
				base.GetComponent<Renderer>().material.mainTexture = cachedResource.Resource as Texture2D;
			}
		}
	}
}
