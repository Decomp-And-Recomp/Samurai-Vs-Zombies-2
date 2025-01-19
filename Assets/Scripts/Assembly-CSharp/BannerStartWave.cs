using UnityEngine;

public class BannerStartWave : Banner
{
	private string mWaveText;

	protected override string uiPrefabPath
	{
		get
		{
			return "UI/Prefabs/HUD/Banner_StartWave";
		}
	}

	public BannerStartWave(float fTimeBeforeFade, int waveNumber)
		: base(fTimeBeforeFade)
	{
		if (Singleton<Profile>.Instance.inDailyChallenge)
		{
			mWaveText = StringUtils.GetStringFromStringRef(Singleton<Profile>.Instance.dailyChallengeProceduralWaveSchema.waveDisplayName);
		}
		else
		{
			mWaveText = string.Format(StringUtils.GetStringFromStringRef("MenuFixedStrings", "add_wave"), waveNumber);
		}
	}

	protected override void InitText()
	{
		GluiText component = ObjectUtils.FindTransformInChildren(base.bannerInstance.transform, "SwapText_Wave").gameObject.GetComponent<GluiText>();
		GluiText component2 = ObjectUtils.FindTransformInChildren(base.bannerInstance.transform, "SwapText_TopLine").gameObject.GetComponent<GluiText>();
		GluiText component3 = ObjectUtils.FindTransformInChildren(base.bannerInstance.transform, "SwapText_BottomLine").gameObject.GetComponent<GluiText>();
		if (Singleton<Profile>.Instance.inMultiplayerWave)
		{
			component.gameObject.SetActive(false);
			if (Singleton<PlayModesManager>.Instance.Attacking)
			{
				component2.TaggedStringReference = "MenuFixedStrings.Banner_Attack1";
				component3.TaggedStringReference = "MenuFixedStrings.Banner_Attack2";
			}
			else
			{
				component3.TaggedStringReference = "MenuFixedStrings.Banner_Defend2";
			}
			return;
		}
		Transform transform = ObjectUtils.FindTransformInChildren(base.bannerInstance.transform, "SwapText_Wave_Shadow");
		if (transform != null)
		{
			GluiText component4 = transform.gameObject.GetComponent<GluiText>();
			if (component4 != null)
			{
				component4.Text = mWaveText;
			}
		}
		component.Text = mWaveText;
	}
}
