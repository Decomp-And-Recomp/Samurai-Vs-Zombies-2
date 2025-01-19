using System;
using UnityEngine;

[Serializable]
public class DataAdaptor_StoreHeroPanel : DataAdaptorBase
{
	public GluiStandardButtonContainer button;

	public GameObject text_Name;

	public GameObject scrollListObject;

	public GameObject sprite_Portrait;

	public GluiText requiredWaveText;

	public GameObject requiredNextObject;

	public GameObject requiredFutureObject;

	private string id;

	public override void SetData(object data)
	{
		DataBundleRecordHandle<HeroSchema> dataBundleRecordHandle = data as DataBundleRecordHandle<HeroSchema>;
		if (requiredNextObject != null)
		{
			requiredNextObject.SetActive(false);
		}
		if (requiredFutureObject != null)
		{
			requiredFutureObject.SetActive(false);
		}
		if (dataBundleRecordHandle != null)
		{
			HeroSchema hero = dataBundleRecordHandle.Data;
			id = hero.id;
			SetGluiTextInChild(text_Name, StringUtils.GetStringFromStringRef(hero.displayName));
			SetGluiSpriteInChild(sprite_Portrait, hero.storePortrait);
			string text = "\\n";
			string text2 = StringUtils.GetStringFromStringRef("LocalizedStrings", "chapter_wave_num");
			do
			{
				text2 = text2.Replace(text, " ");
			}
			while (text2.Contains(text));
			for (int i = Singleton<Profile>.Instance.wave_SinglePlayerGame; i < Mathf.Min(999, Singleton<Profile>.Instance.wave_SinglePlayerGame + 10); i++)
			{
				WaveSchema waveData = WaveManager.GetWaveData(i, WaveManager.WaveType.Wave_SinglePlayer);
				if (!waveData.recommendedHeroIsRequired || !(DataBundleRuntime.RecordKey(waveData.recommendedHero) == id))
				{
					continue;
				}
				if (i != Singleton<Profile>.Instance.wave_SinglePlayerGame)
				{
					if (requiredFutureObject != null)
					{
						requiredFutureObject.SetActive(true);
					}
					if (requiredWaveText != null)
					{
						requiredWaveText.Text = string.Format(text2, i);
					}
				}
				else if (requiredNextObject != null)
				{
					requiredNextObject.SetActive(true);
				}
				break;
			}
			button.gameObject.SetActive(true);
			button.GetActionData = () => hero;
		}
		else
		{
			button.gameObject.SetActive(false);
			if (data is string)
			{
				string strA = (string)data;
				Texture2D texture2D = null;
				texture2D = ((string.Compare(strA, "Allies", true) == 0) ? (ResourceCache.GetCachedResource("UI/Textures/DynamicIcons/Misc/Store_PortraitAllies", 1).Resource as Texture2D) : ((string.Compare(strA, "Champions", true) == 0) ? (ResourceCache.GetCachedResource("UI/Textures/DynamicIcons/Misc/Store_PortraitChampions", 1).Resource as Texture2D) : ((string.Compare(strA, "Consumables", true) == 0) ? (ResourceCache.GetCachedResource("UI/Textures/DynamicIcons/Misc/Store_PortraitExtras", 1).Resource as Texture2D) : ((string.Compare(strA, "Upgrades", true) == 0) ? (ResourceCache.GetCachedResource("UI/Textures/DynamicIcons/Misc/Store_PortraitUpgrades", 1).Resource as Texture2D) : ((string.Compare(strA, "Charms", true) != 0) ? (ResourceCache.GetCachedResource("UI/Textures/DynamicIcons/Misc/Store_PortraitGlobal", 1).Resource as Texture2D) : (ResourceCache.GetCachedResource("UI/Textures/DynamicIcons/Misc/Store_PortraitCharms", 1).Resource as Texture2D))))));
				if (texture2D != null)
				{
					SetGluiSpriteInChild(sprite_Portrait, texture2D);
				}
			}
		}
		string text3 = data as string;
		if (text3 != null)
		{
			id = text3;
			SetGluiTextInChild(text_Name, text3);
		}
		GluiBouncyScrollList component = scrollListObject.GetComponent<GluiBouncyScrollList>();
		component.Redraw(id);
	}
}
