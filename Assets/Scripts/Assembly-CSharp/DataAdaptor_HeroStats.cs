using System;
using UnityEngine;

[Serializable]
public class DataAdaptor_HeroStats : DataAdaptorBase
{
	public object context;

	public GluiText heroName;

	public GluiText heroDescription;

	public GluiSprite heroPortrait;

	public GameObject statAbility;

	public GameObject statAllies;

	public GameObject statHealth;

	public GameObject statMelee;

	public GameObject statRanged;

	public GameObject statSpeed;

	public override void SetData(object data)
	{
		context = data;
		if (data is HeroSchema)
		{
			HeroSchema heroSchema = (HeroSchema)data;
			if (heroSchema != null)
			{
				heroName.Text = StringUtils.GetStringFromStringRef(heroSchema.displayName);
				heroDescription.Text = StringUtils.GetStringFromStringRef(heroSchema.desc);
				heroPortrait.Texture = heroSchema.storePortrait;
				HeroStarsSchema heroStarsSchema = heroSchema.HeroStarsSchema;
				SetStars(statSpeed, heroStarsSchema.speed);
				SetStars(statRanged, heroStarsSchema.ranged);
				SetStars(statMelee, heroStarsSchema.melee);
				SetStars(statHealth, heroStarsSchema.health);
				SetStars(statAllies, heroStarsSchema.allySlots);
				SetStars(statAbility, heroStarsSchema.ability);
			}
		}
	}

	private void SetStars(GameObject starsParent, int numStars)
	{
		for (int i = 1; i <= 5; i++)
		{
			GameObject gameObject = starsParent.FindChild(string.Format("Art_Star{0}", i));
			if (i > numStars)
			{
				gameObject.SetActive(false);
			}
		}
	}
}
