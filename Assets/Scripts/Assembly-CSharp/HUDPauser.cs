using UnityEngine;

public class HUDPauser : MonoBehaviour, IGluiActionHandler
{
	public GameObject RestartButton;

	private bool mClosed;

	private bool mQuitAllowed = true;

	private float mBackupMusicVolume;

	private void Start()
	{
		base.gameObject.transform.position = new Vector3(0f, 0f, 0f);
		if (WeakGlobalMonoBehavior<InGameImpl>.Exists)
		{
			WeakGlobalMonoBehavior<InGameImpl>.Instance.gamePaused = true;
		}
		mBackupMusicVolume = AudioUtils.MusicVolumePlayer;
		AudioUtils.MusicVolumePlayer = 0f;
		if (Singleton<Profile>.Instance.wave_SinglePlayerGame == 1 && Singleton<Profile>.Instance.GetWaveLevel(1) == 1 && !Singleton<Profile>.Instance.inMultiplayerWave)
		{
			mQuitAllowed = false;
			base.gameObject.FindChildComponent<GluiStandardButtonContainer>("Button_Quit").Locked = true;
			base.gameObject.FindChild("Button_Quit").SetActive(false);
		}
		if (Singleton<Profile>.Instance.inMultiplayerWave && RestartButton != null)
		{
			RestartButton.SetActive(false);
		}
	}

	private void Update()
	{
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		if (!mClosed)
		{
			switch (action)
			{
			case "BUTTON_PAUSEMENU_RESUME":
				Close();
				return true;
			case "BUTTON_PAUSEMENU_QUIT":
				Quit();
				return true;
			case "BUTTON_PAUSEMENU_RESTART":
				Restart();
				return true;
			}
		}
		return false;
	}

	private void Quit()
	{
		if (mQuitAllowed)
		{
			Close();
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MainMenuScreen", "Menu_Store");
			LoadingScreen.LoadLevel("AllMenus");
		}
	}

	private void Restart()
	{
		Close();
		if (WeakGlobalInstance<WaveManager>.Instance != null)
		{
			WeakGlobalMonoBehavior<InGameImpl>.Instance.OnHudRestart();
			WaveSchema waveData = WeakGlobalInstance<WaveManager>.Instance.GetWaveData();
			string scene = waveData.scene;
			LoadingScreen.LoadLevel(scene);
		}
	}

	private void Close()
	{
		AudioUtils.MusicVolumePlayer = mBackupMusicVolume;
		mClosed = true;
		GluiActionSender.SendGluiAction("POPUP_EMPTY", base.gameObject, null);
		WeakGlobalMonoBehavior<InGameImpl>.Instance.gamePaused = false;
	}
}
