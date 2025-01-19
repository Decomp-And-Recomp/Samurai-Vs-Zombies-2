using System.Collections.Generic;
using UnityEngine;

public class EquipMenuImpl : UIHandler<EquipMenuImpl>, IGluiActionHandler
{
	private class PageInfo
	{
		public GameObject uiPrefab;

		public ComponentCreator componentCreator;

		public bool allowEnemyInfo;

		public PageInfo(GameObject prefab, ComponentCreator creator, bool allowEnemyInfo)
		{
			uiPrefab = prefab;
			componentCreator = creator;
			this.allowEnemyInfo = allowEnemyInfo;
		}
	}

	private delegate EquipPage ComponentCreator(GameObject uiParent);

	public GameObject WavePanel;

	public GameObject HeroPanel;

	public GameObject HelperPanel;

	public GameObject AbilityPanel;

	public GameObject CharmPanel;

	public GameObject DailyChallengeWavePanel;

	public GameObject DefenseModeActivateObject;

	public GameObject[] DefenseModeDeactivateObject;

	public GluiText nextButtonText;

	private bool mPlayHavenRequestsDone;

	private List<PageInfo> mPagesInfo = new List<PageInfo>();

	private GluiText mCommonInfoText;

	private object mCommonInfoObject;

	private GameObject mActivePageUI;

	private int mActivePageIndex = -1;

	private EquipPage mActivePageComponent;

	private GameObject mQueuedForDeletion;

	private bool mRequestDeleteQueue;

	private EnemiesShowCase mEnemiesShowCase;

	private GameObject mEnemyInfoPanel;

	public object commonInfoDisplay
	{
		get
		{
			return mCommonInfoObject;
		}
		set
		{
			mCommonInfoObject = value;
			if (value == null)
			{
				mCommonInfoText.Text = string.Empty;
			}
			else if (value is string)
			{
				mCommonInfoText.Text = (string)value;
			}
			else if (value is HelperSchema)
			{
				mCommonInfoText.Text = StringUtils.GetStringFromStringRef(((HelperSchema)value).desc);
			}
			else if (value is AbilitySchema)
			{
				mCommonInfoText.Text = StringUtils.GetStringFromStringRef(((AbilitySchema)value).description);
			}
			else if (value is CharmSchema)
			{
				mCommonInfoText.Text = StringUtils.GetStringFromStringRef(((CharmSchema)value).storeDesc);
			}
			else if (value is HeroSchema)
			{
				HeroSchema heroSchema = (HeroSchema)value;
				mCommonInfoText.Text = StringUtils.GetStringFromStringRef(heroSchema.displayName) + ": " + StringUtils.GetStringFromStringRef(heroSchema.desc);
			}
			else
			{
				mCommonInfoText.Text = "** ERROR **";
			}
		}
	}

	public override void Awake()
	{
		base.Awake();
		if (!Singleton<Profile>.Exists)
		{
			StartCoroutine(Singleton<Profile>.Instance.Init());
		}
	}

	public override void Start()
	{
		base.Start();
		SingletonMonoBehaviour<FrontEnd>.Instance.OnEquipScreen = true;
		mCommonInfoText = base.gameObject.FindChildComponent<GluiText>("Text_Info");
		if (!Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive() && !Singleton<Profile>.Instance.ChangingDefenseLoadout)
		{
			if (Singleton<Profile>.Instance.inDailyChallenge)
			{
				mPagesInfo.Add(new PageInfo(DailyChallengeWavePanel, (GameObject ui) => new EquipPageWaves(ui), false));
			}
			else
			{
				mPagesInfo.Add(new PageInfo(WavePanel, (GameObject ui) => new EquipPageWaves(ui), true));
			}
		}
		mPagesInfo.Add(new PageInfo(HeroPanel, (GameObject ui) => new EquipPageHeroes(ui), false));
		mPagesInfo.Add(new PageInfo(HelperPanel, (GameObject ui) => new EquipPageAllies(ui), false));
		mPagesInfo.Add(new PageInfo(AbilityPanel, (GameObject ui) => new EquipPageAbilities(ui), false));
		if (!Singleton<Profile>.Instance.ChangingDefenseLoadout)
		{
			mPagesInfo.Add(new PageInfo(CharmPanel, (GameObject ui) => new EquipPageCharms(ui), false));
			SetupEnemiesShowCase();
		}
		TransitionToPage(0);
		if (!Singleton<Profile>.Instance.ChangingDefenseLoadout)
		{
			return;
		}
		if (DefenseModeDeactivateObject != null)
		{
			GameObject[] defenseModeDeactivateObject = DefenseModeDeactivateObject;
			foreach (GameObject gameObject in defenseModeDeactivateObject)
			{
				if ((bool)gameObject)
				{
					gameObject.SetActive(false);
				}
			}
		}
		if (DefenseModeActivateObject != null)
		{
			DefenseModeActivateObject.SetActive(true);
		}
	}

	public override void Update()
	{
		base.Update();
		if (mEnemiesShowCase != null)
		{
			mEnemiesShowCase.Update();
		}
		if (mRequestDeleteQueue)
		{
			mRequestDeleteQueue = false;
			DeleteObjectInQueue();
		}
	}

	private void OnDestroy()
	{
		if (!ApplicationUtilities.HasShutdown)
		{
			Singleton<Profile>.Instance.ChangingDefenseLoadout = false;
			if (mEnemiesShowCase != null)
			{
				mEnemiesShowCase.Clear();
				mEnemiesShowCase = null;
			}
			mPagesInfo.Clear();
			mPagesInfo = null;
			Singleton<Profile>.Instance.ForceOnboardingStage("OnboardingStep16_LeaveCarousel");
			if (SingletonMonoBehaviour<FrontEnd>.Exists)
			{
				SingletonMonoBehaviour<FrontEnd>.Instance.OnEquipScreen = false;
			}
		}
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		switch (action)
		{
		case "BUTTON_BACK":
			GoBack();
			return true;
		case "BUTTON_NEXT":
			GoNext();
			return true;
		case "BUTTON_STORE":
			GoStore();
			return true;
		case "END_PANEL_TRANSITION":
			mRequestDeleteQueue = true;
			return true;
		case "HIDE_ENEMY_INFO":
			ShowEnemyInfo(false);
			return true;
		default:
			return OnUIEvent(action);
		}
	}

	private void GoBack()
	{
		Save();
		if (mActivePageIndex > 0)
		{
			TransitionToPage(mActivePageIndex - 1);
		}
		else if (Singleton<Profile>.Instance.inMultiplayerWave || Singleton<Profile>.Instance.ChangingDefenseLoadout)
		{
			ClearPage();
			GluiActionSender.SendGluiAction("ALERT_MULTIPLAYER_LOGIN", base.gameObject, null);
		}
		else
		{
			ClearPage();
			GluiActionSender.SendGluiAction("MENU_MAIN_MODESELECT_BACK", base.gameObject, null);
		}
	}

	private void GoNext()
	{
		Save();
		if (mActivePageIndex < mPagesInfo.Count - 1)
		{
			TransitionToPage(mActivePageIndex + 1);
			return;
		}
		if (Singleton<Profile>.Instance.MultiplayerData.Account != null && Singleton<Profile>.Instance.MultiplayerData.Account.Status == GripAccount.LoginStatus.Complete)
		{
			Singleton<Profile>.Instance.MultiplayerData.UpdateLoadout();
		}
		if (Singleton<Profile>.Instance.ChangingDefenseLoadout)
		{
			ClearPage();
			Singleton<Profile>.Instance.HasSetupDefenses = true;
			GluiActionSender.SendGluiAction("ALERT_MULTIPLAYER_LOGIN", base.gameObject, null);
		}
		else
		{
			if (Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive())
			{
				Singleton<Profile>.Instance.MultiplayerData.StartMultiplayerGameSession();
			}
			WaveSchema waveData = WeakGlobalInstance<WaveManager>.Instance.GetWaveData();
			string scene = waveData.scene;
			LoadingScreen.LoadLevel(scene);
		}
		Singleton<Profile>.Instance.Save();
	}

	private void GoStore()
	{
		ClearPage();
		GluiActionSender.SendGluiAction("MENU_MAIN_STORE_BACK", base.gameObject, null);
	}

	private void ClearPage(bool? forward = null)
	{
		ShowEnemyInfo(false);
		if (mActivePageUI != null)
		{
			if (forward.HasValue)
			{
				AnimateTransition(mActivePageUI, false, forward.Value);
			}
			if (mQueuedForDeletion != null)
			{
				Object.DestroyImmediate(mQueuedForDeletion);
			}
			mQueuedForDeletion = mActivePageUI;
			mActivePageUI = null;
		}
		if (mComponents.Count > 0)
		{
			mComponents.RemoveAt(mComponents.Count - 1);
		}
	}

	private void TransitionToPage(int pageIndex)
	{
		if (pageIndex == mActivePageIndex)
		{
			return;
		}
		bool flag = pageIndex > mActivePageIndex;
		if (mActivePageUI != null)
		{
			ClearPage(flag);
		}
		commonInfoDisplay = null;
		mActivePageIndex = pageIndex;
		mActivePageUI = Object.Instantiate(mPagesInfo[mActivePageIndex].uiPrefab) as GameObject;
		mActivePageUI.transform.position = new Vector3(0f, 0f, 0f);
		mActivePageUI.transform.localPosition = new Vector3(0f, 0f, 0f);
		mActivePageUI.transform.parent = base.transform;
		mActivePageComponent = mPagesInfo[mActivePageIndex].componentCreator(mActivePageUI);
		mComponents.Add(mActivePageComponent);
		AnimateTransition(mActivePageUI, true, flag);
		UpdateNextButton();
		if (!mPlayHavenRequestsDone && mPagesInfo[mActivePageIndex].uiPrefab == HelperPanel)
		{
			mPlayHavenRequestsDone = true;
			if (Singleton<Profile>.Instance.GetNumPotions("healthPotion") == 0)
			{
				ApplicationUtilities.MakePlayHavenContentRequest("consumable_sushi_0");
			}
			if (Singleton<Profile>.Instance.GetNumPotions("leadershipPotion") == 0)
			{
				ApplicationUtilities.MakePlayHavenContentRequest("consumable_tea_0");
			}
		}
		if (mPagesInfo[mActivePageIndex].uiPrefab == WavePanel)
		{
			SingletonMonoBehaviour<TutorialMain>.Instance.TutorialStartIfNeeded("Tutorial_TapEnemy");
		}
	}

	private void UpdateNextButton()
	{
		if (!(nextButtonText != null))
		{
			return;
		}
		if (mActivePageIndex == mPagesInfo.Count - 1)
		{
			if (Singleton<Profile>.Instance.ChangingDefenseLoadout)
			{
				nextButtonText.Text = StringUtils.GetStringFromStringRef("MenuFixedStrings", "iCloud_Save");
			}
			else
			{
				nextButtonText.Text = StringUtils.GetStringFromStringRef("MenuFixedStrings", "Menu_Play");
			}
		}
		else
		{
			nextButtonText.Text = StringUtils.GetStringFromStringRef("MenuFixedStrings", "Menu_Next");
		}
	}

	private void Save()
	{
		if (mActivePageComponent != null)
		{
			mActivePageComponent.Save();
		}
	}

	private void SetupEnemiesShowCase()
	{
		Transform centerPos = base.gameObject.FindChild("Locator_Enemies_Middle").transform;
		mEnemiesShowCase = new EnemiesShowCase(centerPos, base.gameObject.FindChild("BG_Dimmer_Enemies"), Singleton<Profile>.Instance.waveTypeToPlay, Singleton<Profile>.Instance.waveToPlay);
		mEnemiesShowCase.onCharacterTouched = OnCharacterTouched;
	}

	private void AnimateTransition(GameObject target, bool entering, bool forward)
	{
		GluiProcess_MenuTransition component = mActivePageUI.GetComponent<GluiProcess_MenuTransition>();
		if (!(component != null))
		{
			return;
		}
		if (forward)
		{
			if (entering)
			{
				component.ProcessStart(GluiStatePhase.Init);
			}
			else
			{
				component.ProcessStart(GluiStatePhase.Exit);
			}
		}
		else if (entering)
		{
			component.ProcessStartReversed(GluiStatePhase.Init);
		}
		else
		{
			component.ProcessStartReversed(GluiStatePhase.Exit);
		}
	}

	private void DeleteObjectInQueue()
	{
		if (mQueuedForDeletion != null)
		{
			Object.DestroyImmediate(mQueuedForDeletion);
			mQueuedForDeletion = null;
		}
	}

	private void OnCharacterTouched(Character c)
	{
		if (mPagesInfo[mActivePageIndex].allowEnemyInfo)
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("ENEMYSHOWROOM_SELECTION", c);
			ShowEnemyInfo(true);
		}
	}

	public void ShowEnemyInfo(bool show)
	{
		if (show)
		{
			if (mEnemyInfoPanel == null)
			{
				mEnemyInfoPanel = Object.Instantiate(Resources.Load("UI/Prefabs/SelectSuite/PopUp_EnemyInfo") as GameObject) as GameObject;
				mEnemyInfoPanel.transform.parent = base.gameObject.transform;
				mEnemyInfoPanel.transform.localPosition = Vector3.zero;
			}
			else
			{
				mEnemyInfoPanel.SetActive(true);
			}
		}
		else if (mEnemyInfoPanel != null)
		{
			mEnemyInfoPanel.SetActive(false);
		}
	}
}
