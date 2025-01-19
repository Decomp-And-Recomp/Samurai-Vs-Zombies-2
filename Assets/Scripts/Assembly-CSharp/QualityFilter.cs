using System;
using UnityEngine;

[AddComponentMenu("Quality Settings/Quality Filter")]
public class QualityFilter : MonoBehaviour
{
	[Serializable]
	public class FilterSetting
	{
		public EPortableQualitySetting minimumQualitySetting;

		public EPortableQualitySetting maximumQualitySetting;

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
		EPortableQualitySetting quality = PortableQualitySettings.GetQuality();
		FilterSetting[] array = settings;
		foreach (FilterSetting filterSetting in array)
		{
			if ((filterSetting.minimumQualitySetting == EPortableQualitySetting.None || quality >= filterSetting.minimumQualitySetting) && (filterSetting.maximumQualitySetting == EPortableQualitySetting.None || quality <= filterSetting.maximumQualitySetting))
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
