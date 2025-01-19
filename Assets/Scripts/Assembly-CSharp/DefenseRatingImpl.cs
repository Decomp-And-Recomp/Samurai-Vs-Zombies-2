using UnityEngine;

public class DefenseRatingImpl : MonoBehaviour, IGluiActionHandler
{
	public GluiText HeroRatingText;

	public GluiText AbilityRatingText;

	public GluiText HelperRatingText;

	public GluiText GlobalRatingText;

	public GluiText DefenseRatingText;

	public GluiText PlayerNameText;

	public Transform HeroIconLocator;

	public Transform[] HelperIconLocator;

	public Transform[] AbilityIconLocator;

	public Transform[] GlobalIconLocator;

	public GameObject ChangeButton;

	public GameObject IconPrefab;

	public static MultiplayerProfileLoadout loadoutToDisplay;

	private DataBundleRecordHandle<VillageArcherSchema> mArcherHandle;

	private DataBundleRecordHandle<PitSchema> mPitHandle;

	private void Start()
	{
		if (loadoutToDisplay == null)
		{
			loadoutToDisplay = Singleton<Profile>.Instance.MultiplayerData.LocalPlayerLoadout;
		}
		else if (ChangeButton != null)
		{
			ChangeButton.SetActive(false);
		}
		if (DefenseRatingText != null)
		{
			DefenseRatingText.Text = loadoutToDisplay.defenseRating.ToString();
		}
		if (HeroRatingText != null)
		{
			HeroRatingText.Text = loadoutToDisplay.heroRating.ToString();
		}
		if (HelperRatingText != null)
		{
			HelperRatingText.Text = loadoutToDisplay.helperRating.ToString();
		}
		if (AbilityRatingText != null)
		{
			AbilityRatingText.Text = loadoutToDisplay.abilityRating.ToString();
		}
		if (GlobalRatingText != null)
		{
			GlobalRatingText.Text = (loadoutToDisplay.bellRating + loadoutToDisplay.gateRating + loadoutToDisplay.archerRating + loadoutToDisplay.pitRating).ToString();
		}
		if (PlayerNameText != null)
		{
			if (!string.IsNullOrEmpty(loadoutToDisplay.playerName))
			{
				PlayerNameText.Text = loadoutToDisplay.playerName;
			}
			else
			{
				PlayerNameText.Text = Singleton<Profile>.Instance.MultiplayerData.UserName;
			}
		}
		GameObject gameObject = Object.Instantiate(IconPrefab, HeroIconLocator.position, HeroIconLocator.rotation) as GameObject;
		DefenseRatingWidgetImpl component = gameObject.GetComponent<DefenseRatingWidgetImpl>();
		gameObject.transform.parent = HeroIconLocator;
		HeroSchema heroSchema = Singleton<HeroesDatabase>.Instance[loadoutToDisplay.heroId];
		component.SetupWidget(heroSchema.icon, null, loadoutToDisplay.heroLevel);
		int num = Mathf.Min(HelperIconLocator.Length, loadoutToDisplay.selectedHelpers.Length);
		for (int i = 0; i < num; i++)
		{
			HelperSelectionInfo helperSelectionInfo = loadoutToDisplay.selectedHelpers[i];
			if (helperSelectionInfo != null)
			{
				HelperSchema helperSchema = Singleton<HelpersDatabase>.Instance[helperSelectionInfo.helperId];
				if (helperSchema != null)
				{
					gameObject = Object.Instantiate(IconPrefab, HelperIconLocator[i].position, HelperIconLocator[i].rotation) as GameObject;
					component = gameObject.GetComponent<DefenseRatingWidgetImpl>();
					gameObject.transform.parent = HelperIconLocator[i];
					Texture2D goldIcon = ((!helperSelectionInfo.golden) ? null : helperSchema.championIcon);
					component.SetupWidget(helperSchema.HUDIcon, goldIcon, helperSelectionInfo.level);
				}
			}
		}
		int num2 = Mathf.Min(AbilityIconLocator.Length, loadoutToDisplay.abilityIdList.Count);
		for (int j = 0; j < num2; j++)
		{
			AbilitySchema abilitySchema = Singleton<AbilitiesDatabase>.Instance[loadoutToDisplay.abilityIdList[j]];
			if (abilitySchema != null)
			{
				gameObject = Object.Instantiate(IconPrefab, AbilityIconLocator[j].position, AbilityIconLocator[j].rotation) as GameObject;
				component = gameObject.GetComponent<DefenseRatingWidgetImpl>();
				gameObject.transform.parent = AbilityIconLocator[j];
				component.SetupWidget(abilitySchema.icon, null, loadoutToDisplay.abilityLevel[j]);
			}
		}
		gameObject = Object.Instantiate(IconPrefab, GlobalIconLocator[0].position, GlobalIconLocator[0].rotation) as GameObject;
		component = gameObject.GetComponent<DefenseRatingWidgetImpl>();
		gameObject.transform.parent = GlobalIconLocator[0];
		TextDBSchema[] data = DataBundleUtils.InitializeRecords<TextDBSchema>("Gate");
		string @string = data.GetString(TextDBSchema.LevelKey("icon", loadoutToDisplay.baseLevel));
		component.SetupWidget(@string, loadoutToDisplay.baseLevel);
		int num3 = 1;
		if (loadoutToDisplay.bellLevel > 0)
		{
			gameObject = Object.Instantiate(IconPrefab, GlobalIconLocator[num3].position, GlobalIconLocator[num3].rotation) as GameObject;
			component = gameObject.GetComponent<DefenseRatingWidgetImpl>();
			gameObject.transform.parent = GlobalIconLocator[num3];
			data = DataBundleUtils.InitializeRecords<TextDBSchema>("Bell");
			@string = data.GetString(TextDBSchema.LevelKey("icon", loadoutToDisplay.bellLevel));
			component.SetupWidget(@string, loadoutToDisplay.bellLevel);
			num3++;
		}
		if (loadoutToDisplay.archerLevel > 0)
		{
			gameObject = Object.Instantiate(IconPrefab, GlobalIconLocator[num3].position, GlobalIconLocator[num3].rotation) as GameObject;
			component = gameObject.GetComponent<DefenseRatingWidgetImpl>();
			gameObject.transform.parent = GlobalIconLocator[num3];
			mArcherHandle = new DataBundleRecordHandle<VillageArcherSchema>("VillageArchers", loadoutToDisplay.archerLevel.ToString());
			mArcherHandle.Load(DataBundleResourceGroup.FrontEnd, false, delegate
			{
			});
			component.SetupWidget(mArcherHandle.Data.icon, null, loadoutToDisplay.archerLevel);
			num3++;
		}
		if (loadoutToDisplay.pitLevel > 0)
		{
			gameObject = Object.Instantiate(IconPrefab, GlobalIconLocator[num3].position, GlobalIconLocator[num3].rotation) as GameObject;
			component = gameObject.GetComponent<DefenseRatingWidgetImpl>();
			gameObject.transform.parent = GlobalIconLocator[num3];
			mPitHandle = new DataBundleRecordHandle<PitSchema>("Pit", loadoutToDisplay.pitLevel.ToString());
			mPitHandle.Load(DataBundleResourceGroup.FrontEnd, false, delegate
			{
			});
			component.SetupWidget(mPitHandle.Data.icon, null, loadoutToDisplay.pitLevel);
			num3++;
		}
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		if (action == "CHANGE_DEFENSE")
		{
			Singleton<Profile>.Instance.ChangingDefenseLoadout = true;
			GluiActionSender.SendGluiAction("POPUP_POP", sender, null);
			GluiActionSender.SendGluiAction("MENU_HUD_EMPTY", sender, null);
			GluiActionSender.SendGluiAction("MENU_MAIN_EQUIP", sender, null);
			return true;
		}
		return false;
	}
}
