using System;
using Glu.Plugins.ASocial;
using UnityEngine;

public class MultiplayerLoginSequence
{
	private static GameObject sender;

	private static DateTime? lastTimeLoaded;

	private static int timeToUseLastLoad_Minutes = 15;

	public static void LoginStart(GameObject sender, bool createNewAccount)
	{
		MultiplayerLoginSequence.sender = sender;
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MULTIPLAYER_LOGIN_DONE", "False");
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MULTIPLAYER_LOGIN_TITLE", "MenuFixedStrings.Menu_MPLoggingIn");
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MULTIPLAYER_LOGIN_TEXT", "MenuFixedStrings.Menu_MPLoggingInDescription");
		Singleton<Profile>.Instance.MultiplayerData.Login(LoginComplete, createNewAccount);
		ApplicationUtilities.MakePlayHavenContentRequest("multiplayer_login");
	}

	private static void LoginComplete()
	{
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MULTIPLAYER_LOGIN_DONE", "True");
		if (Singleton<Profile>.Instance.MultiplayerData.Account.Status == GripAccount.LoginStatus.Complete)
		{
			DateTime? dateTime = lastTimeLoaded;
			if (!dateTime.HasValue || DateTime.Now.Subtract(lastTimeLoaded.Value).TotalMinutes > (double)timeToUseLastLoad_Minutes || Singleton<Profile>.Instance.MultiplayerData.CollectionData.Count == 0)
			{
				Singleton<Profile>.Instance.MultiplayerData.RetrieveMyCollection(LoginLoadPlayerCollectionComplete);
			}
			else
			{
				LoginSuccess(true);
			}
		}
		else
		{
			LoginFail();
		}
	}

	private static void LoginLoadPlayerCollectionComplete(GripNetwork.Result result, bool setCompleted)
	{
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MULTIPLAYER_LOGIN_DONE", "True");
		if (result == GripNetwork.Result.Success)
		{
			LoginSuccess(false);
		}
		else
		{
			LoginFail();
		}
	}

	private static void LoginSuccess(bool accountLoadSkipped)
	{
		if (PlayGameServices.IsSignedIn)
		{
			try
			{
				Singleton<Profile>.Instance.MultiplayerData.SetAndroidID(PlayGameServices.GetPlayerID());
				SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.AndroidGoogleFriendsRead();
			}
			catch (Exception)
			{
			}
		}
		if ((Singleton<Profile>.Instance.MultiplayerData.Account.New && !accountLoadSkipped) || string.IsNullOrEmpty(Singleton<Profile>.Instance.MultiplayerData.UserName))
		{
			GluiActionSender.SendGluiAction("MULTIPLAYER_LOGIN_CREATED", sender, null);
		}
		else
		{
			GluiDelayedAction.Create("MULTIPLAYER_LOGIN_SUCCESS", 0.15f, sender, true);
		}
		sender = null;
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MULTIPLAYER_LOGIN_DONE", "True");
		lastTimeLoaded = DateTime.Now;
		Singleton<Profile>.Instance.MultiplayerData.Account.ClearNewFlag();
	}

	private static void LoginFail()
	{
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MULTIPLAYER_LOGIN_DONE", "True");
		GluiActionSender.SendGluiAction("MULTIPLAYER_LOGIN_FAILURE", sender, null);
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MULTIPLAYER_LOGIN_TITLE", "MenuFixedStrings.Menu_MPLogInFail");
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("MULTIPLAYER_LOGIN_TEXT", "MenuFixedStrings.Menu_MPLogInFailDescription");
		GlobalActions.ClearMultiplayerGameSession();
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("GAME_MODE", "Button_Story");
		if (ModeSelectImpl.Instance != null)
		{
			ModeSelectImpl.Instance.UpdateModeButtons();
		}
		sender = null;
		lastTimeLoaded = null;
	}

	public static void AssureDataReloadOnNextLogin()
	{
		lastTimeLoaded = null;
	}
}
