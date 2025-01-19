using System.Collections;

public class FrontEnd : SingletonMonoBehaviour<FrontEnd>
{
	private bool resourcesLoaded;

	public bool OnEquipScreen { get; set; }

	protected override void Awake()
	{
		base.Awake();
		if (!Singleton<Profile>.Exists)
		{
			LoadingScreen.LogBegin();
			StartCoroutine(Singleton<Profile>.Instance.Init());
		}
		else
		{
			LoadResources();
		}
	}

	private IEnumerator Start()
	{
		while (!Singleton<Profile>.Instance.Initialized)
		{
			yield return null;
		}
		Singleton<Profile>.Instance.StartDecaySystem();
		LoadResources();
		SingletonSpawningMonoBehaviour<GluIap>.Instance.UpdateProductData();
	}

	private void LoadResources()
	{
		if (!resourcesLoaded)
		{
			resourcesLoaded = true;
			MemoryWarningHandler.Instance.unloadOnMemoryWarning = true;
			Singleton<AbilitiesDatabase>.Instance.LoadFrontEndData();
			LoadingScreen.LogStep("AbilitiesDatabase.Instance.LoadFrontEndData");
			Singleton<CharmsDatabase>.Instance.LoadFrontEndData();
			LoadingScreen.LogStep("CharmsDatabase.Instance.LoadFrontEndData");
			Singleton<HelpersDatabase>.Instance.LoadFrontEndData();
			LoadingScreen.LogStep("HelpersDatabase.Instance.LoadFrontEndData");
			Singleton<EnemiesDatabase>.Instance.LoadFrontEndData();
			LoadingScreen.LogStep("EnemiesDatabase.Instance.LoadFrontEndData");
			Singleton<HeroesDatabase>.Instance.LoadFrontEndData();
			LoadingScreen.LogStep("HeroesDatabase.Instance.LoadFrontEndData");
			Singleton<PotionsDatabase>.Instance.LoadFrontEndData();
			LoadingScreen.LogStep("PotionsDatabase.Instance.LoadFrontEndData");
			Singleton<Profile>.Instance.LoadFrontEndData();
			LoadingScreen.LogStep("Profile.Instance.LoadFrontEndData");
			Singleton<Achievements>.Instance.LoadFrontEndData();
			MultiplayerLoginSequence.LoginStart(base.gameObject, false);
			LoadingScreen.LogStep("MultiplayerLoginSequence.LoginStart");
		}
	}

	protected override void OnDestroy()
	{
		if (!ApplicationUtilities.HasShutdown)
		{
			MemoryWarningHandler.Instance.unloadOnMemoryWarning = false;
			Singleton<AbilitiesDatabase>.Instance.UnloadData();
			Singleton<CharmsDatabase>.Instance.UnloadData();
			Singleton<HelpersDatabase>.Instance.UnloadData();
			Singleton<EnemiesDatabase>.Instance.UnloadData();
			Singleton<HeroesDatabase>.Instance.UnloadData();
			Singleton<PotionsDatabase>.Instance.UnloadData();
			Singleton<Profile>.Instance.UnloadData();
			Singleton<Achievements>.Instance.UnloadData();
			GluiCore.sMaterialDictionary.Clear();
		}
		base.OnDestroy();
	}

	private void OnApplicationPause(bool paused)
	{
		if (Singleton<Profile>.Exists && Singleton<Profile>.Instance.Initialized)
		{
			Singleton<Profile>.Instance.Save(false);
		}
	}

	private void OnApplicationQuit()
	{
		if (Singleton<Profile>.Exists && Singleton<Profile>.Instance.Initialized)
		{
			Singleton<Profile>.Instance.Save();
		}
	}
}
