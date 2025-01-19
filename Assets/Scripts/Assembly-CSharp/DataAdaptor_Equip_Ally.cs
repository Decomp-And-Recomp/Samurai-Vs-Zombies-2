using System;
using UnityEngine;

[Serializable]
public class DataAdaptor_Equip_Ally : DataAdaptorBase
{
	public GameObject button;

	public GameObject icon;

	public GameObject text_Name;

	public GameObject text_LeftCounter;

	public GameObject text_RightCounter;

	public GameObject arrow;

	public GameObject recommended;

	public GameObject required;

	public GameObject available;

	public GameObject buyBadge;

	public GameObject removeButton;

	public GameObject goldStar;

	public GameObject newBadge;

	public bool isSlot;

	private GluiStandardButtonContainer mButtonRef;

	private object mData;

	private bool mWasLocked;

	public override void SetData(object data)
	{
		Texture2D texture2D = null;
		bool flag = false;
		string text = "(missing)";
		int num = -1;
		int num2 = -1;
		bool active = false;
		bool flag2 = false;
		bool active2 = false;
		bool active3 = false;
		Texture2D texture2D2 = null;
		bool active4 = false;
		mData = data;
		if (button != null)
		{
			mButtonRef = button.GetComponent<GluiStandardButtonContainer>();
		}
		if (data is HeroSchema)
		{
			HeroSchema heroSchema = (HeroSchema)data;
			text = StringUtils.GetStringFromStringRef(heroSchema.displayName);
			texture2D = heroSchema.icon;
			flag = heroSchema.Locked;
			mWasLocked = heroSchema.Locked;
			if (heroSchema.Locked && heroSchema.purchaseToUnlock)
			{
				active3 = true;
			}
			WaveSchema waveData = WaveManager.GetWaveData(Singleton<Profile>.Instance.waveToPlay, Singleton<Profile>.Instance.waveTypeToPlay);
			if (Singleton<Profile>.Instance.inDailyChallenge)
			{
				flag2 = Singleton<Profile>.Instance.dailyChallengeHeroSchema.id == heroSchema.id;
				flag = !flag2;
			}
			else if (waveData.recommendedHero != null && !Singleton<Profile>.Instance.inMultiplayerWave && !Singleton<Profile>.Instance.ChangingDefenseLoadout)
			{
				HeroSchema heroSchema2 = Singleton<HeroesDatabase>.Instance[waveData.recommendedHero.Key];
				if (heroSchema2 != null)
				{
					string id = heroSchema2.id;
					if (id == heroSchema.id)
					{
						if (waveData.recommendedHeroIsRequired)
						{
							flag2 = true;
						}
						else
						{
							active = true;
						}
					}
					else if (waveData.recommendedHeroIsRequired && !heroSchema.Locked)
					{
						active2 = heroSchema.overrideRequirements;
					}
				}
			}
		}
		else if (data is HelperSchema)
		{
			HelperSchema helperSchema = (HelperSchema)data;
			text = StringUtils.GetStringFromStringRef(helperSchema.displayName);
			num = (int)helperSchema.resourcesCost;
			if (Singleton<Profile>.Instance.GetGoldenHelperUnlocked(helperSchema.id))
			{
				texture2D2 = helperSchema.TryGetChampionIcon();
			}
			if (Singleton<Profile>.Instance.inDailyChallenge)
			{
				if (!Singleton<Profile>.Instance.dailyChallengeHelpers.Contains(helperSchema.id))
				{
					texture2D = helperSchema.TryGetLockedIcon();
					flag = true;
				}
				else
				{
					texture2D = helperSchema.TryGetHUDIcon();
					flag2 = true;
					flag = false;
				}
			}
			else
			{
				if (helperSchema.Locked)
				{
					texture2D = helperSchema.TryGetLockedIcon();
					flag = true;
				}
				else
				{
					texture2D = helperSchema.TryGetHUDIcon();
					flag = false;
				}
				if (helperSchema.id == "Farmer" && Singleton<Profile>.Instance.waveToPlay == 2 && Singleton<Profile>.Instance.GetWaveLevel(2) == 1 && !Singleton<Profile>.Instance.inVSMultiplayerWave && !Singleton<Profile>.Instance.ChangingDefenseLoadout)
				{
					flag2 = true;
				}
			}
		}
		else if (data is AbilitySchema)
		{
			AbilitySchema abilitySchema = (AbilitySchema)data;
			text = StringUtils.GetStringFromStringRef(abilitySchema.displayName);
			texture2D = ((!BundleUtils.GetSystemLanguage().StartsWith("English") && !(abilitySchema.iconNoText == null)) ? abilitySchema.iconNoText : abilitySchema.icon);
			if (Singleton<Profile>.Instance.inDailyChallenge)
			{
				flag = !Singleton<Profile>.Instance.dailyChallengeAbilities.Contains(abilitySchema.id);
				flag2 = !flag;
			}
			else
			{
				flag = abilitySchema.EquipLocked;
			}
		}
		else if (data is CharmSchema)
		{
			CharmSchema charmSchema = (CharmSchema)data;
			text = StringUtils.GetStringFromStringRef(charmSchema.displayName);
			if (!isSlot)
			{
				num2 = Singleton<Profile>.Instance.GetNumCharms(charmSchema.id);
			}
			flag = num2 == 0;
			texture2D = charmSchema.icon;
		}
		if (arrow != null)
		{
			arrow.SetActive(false);
		}
		if (text_Name != null)
		{
			SetGluiTextInChild(text_Name, text);
		}
		if (text_LeftCounter != null)
		{
			if (num < 0)
			{
				text_LeftCounter.SetActive(false);
			}
			else
			{
				SetGluiTextInChild(text_LeftCounter, num.ToString());
			}
		}
		if (text_RightCounter != null)
		{
			if (num2 < 0)
			{
				text_RightCounter.SetActive(false);
			}
			else
			{
				SetGluiTextInChild(text_RightCounter, num2.ToString());
			}
		}
		if (icon != null)
		{
			if (texture2D != null)
			{
				SetGluiSpriteInChild(icon, texture2D);
				if (!icon.activeSelf)
				{
					icon.SetActive(true);
				}
			}
			else
			{
				icon.SetActive(false);
			}
		}
		if (button != null)
		{
			button.GetComponent<GluiStandardButtonContainer>().Locked = flag;
			if (data is CharmSchema)
			{
				Transform transform = ObjectUtils.FindTransformInChildren(button.transform, "Art_Padlock");
				if (transform != null && flag)
				{
					transform.gameObject.SetActive(false);
				}
			}
		}
		if (recommended != null)
		{
			recommended.SetActive(active);
		}
		if (required != null)
		{
			required.SetActive(flag2);
		}
		if (available != null)
		{
			available.SetActive(active2);
		}
		if (buyBadge != null)
		{
			buyBadge.SetActive(active3);
		}
		if (removeButton != null)
		{
			removeButton.SetActive(!flag2);
		}
		if (goldStar != null)
		{
			GluiSprite component = goldStar.GetComponent<GluiSprite>();
			if (texture2D2 != null && component != null)
			{
				goldStar.SetActive(true);
				component.Texture = texture2D2;
			}
			else
			{
				goldStar.SetActive(false);
			}
		}
		if (newBadge != null)
		{
			newBadge.SetActive(active4);
		}
	}

	public void VisualUpdate()
	{
		if (mWasLocked && mData != null && mData is HeroSchema)
		{
			HeroSchema heroSchema = (HeroSchema)mData;
			if (!heroSchema.Locked)
			{
				mWasLocked = false;
				SetData(mData);
			}
		}
		if (arrow == null || isSlot || WeakGlobalMonoBehavior<EquipMenuImpl>.Instance == null)
		{
			return;
		}
		if (WeakGlobalMonoBehavior<EquipMenuImpl>.Instance.commonInfoDisplay == mData && mData != null)
		{
			if (!arrow.activeSelf)
			{
				arrow.SetActive(true);
			}
			if (mButtonRef != null && !mButtonRef.Locked)
			{
				mButtonRef.Selected = true;
			}
		}
		else
		{
			if (arrow.activeSelf)
			{
				arrow.SetActive(false);
			}
			if (mButtonRef != null && !mButtonRef.Locked)
			{
				mButtonRef.Selected = false;
			}
		}
	}
}
