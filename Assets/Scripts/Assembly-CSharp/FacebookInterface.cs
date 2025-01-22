using System;
using System.Collections.Generic;
using UnityEngine;

public class FacebookInterface : MonoBehaviour
{
	private static bool smShowInterface = true;

	private static FacebookListener smListener;

	private static Action<bool> smOnDoneLogin;

	private static Action<string> smOnFinishedPost;

	public static string LocalFacebookName = string.Empty;

	public static void FacebookLogin(bool showInterface, Action<bool> onDone)
	{
		if (!Facebook.IsBeingModified() && !Facebook.IsLoggedIn())
		{
			smShowInterface = showInterface;
			smOnDoneLogin = onDone;
			if (smListener == null)
			{
				GameObject gameObject = new GameObject("FacebookListener");
				smListener = gameObject.AddComponent<FacebookListener>();
				Facebook.Initialize(string.Empty);
				FacebookListener.FacebookInitialized += FacebookInitialized;
				FacebookListener.SessionLoggedIn += FacebookSuccessfullyLoggedIn;
				FacebookListener.SessionLoginFailed += FacebookLoginError;
				FacebookListener.FriendsRead += FacebookFriendsRead;
				FacebookListener.NetworkConnectionFailed += NetworkConnectionFailed;
			}
			else
			{
				Facebook.Login(smShowInterface);
			}
		}
	}

	private static void Cleanup()
	{
		FacebookListener.FacebookInitialized -= FacebookInitialized;
		FacebookListener.SessionLoggedIn -= FacebookSuccessfullyLoggedIn;
		FacebookListener.SessionLoginFailed -= FacebookLoginError;
		FacebookListener.FriendsRead -= FacebookFriendsRead;
		FacebookListener.NetworkConnectionFailed -= NetworkConnectionFailed;
	}

	private static void FacebookInitialized(FBBSession session)
	{
		if (Facebook.IsLoggedIn())
		{
			if (Singleton<Profile>.Instance.MultiplayerData.FacebookID != session.User.ID)
			{
				Singleton<Profile>.Instance.MultiplayerData.SetFacebookID(session.User.ID);
			}
			LocalFacebookName = session.User.FirstName + " " + session.User.LastName;
			Facebook.ReadFriends();
			if (smOnDoneLogin != null)
			{
				smOnDoneLogin(true);
				smOnDoneLogin = null;
			}
		}
		else
		{
			Facebook.Login(smShowInterface);
		}
	}

	private static void FacebookSuccessfullyLoggedIn(FBBSession session)
	{
		if (Singleton<Profile>.Instance.MultiplayerData.FacebookID != session.User.ID)
		{
			Singleton<Profile>.Instance.MultiplayerData.SetFacebookID(session.User.ID);
		}
		LocalFacebookName = session.User.FirstName + " " + session.User.LastName;
		Facebook.ReadFriends();
		if (smOnDoneLogin != null)
		{
			smOnDoneLogin(true);
			smOnDoneLogin = null;
		}
	}

	private static void NetworkConnectionFailed(string errorMessage)
	{
		Cleanup();
		UnityEngine.Object.Destroy(smListener.gameObject);
		smListener = null;
		MultiplayerData.NetworkRequiredDialog();
	}

	private static void FacebookLoginError(string errorMessage)
	{
		if (smOnDoneLogin != null)
		{
			smOnDoneLogin(false);
			smOnDoneLogin = null;
		}
	}

	private static void FacebookFriendsRead(FBBUserCollection friends)
	{
		string text = "facebookid IN (";
		int num = 0;
		foreach (KeyValuePair<string, FBBUser> friend in friends)
		{
			FBBUser value = friend.Value;
			if (!string.IsNullOrEmpty(value.ID))
			{
				if (num > 0)
				{
					text += ", ";
				}
				text = text + "'" + value.ID + "'";
				num++;
			}
		}
		text += ")";
		string[] fieldNames = new string[3] { "ownerid", "gamecenterid", "facebookid" };
		GripNetwork.SearchRecords("UserData", fieldNames, text, string.Empty, null, num, 1, delegate(GripNetwork.Result result, GripField[,] data)
		{
			if (result == GripNetwork.Result.Success && data != null)
			{
				for (int i = 0; i < data.GetLength(0); i++)
				{
					if (data[i, 0].mInt.HasValue)
					{
						string friendName = string.Empty;
						foreach (KeyValuePair<string, FBBUser> friend2 in friends)
						{
							if (friend2.Value.ID == data[i, 2].mString)
							{
								friendName = friend2.Value.FirstName + " " + friend2.Value.LastName;
								break;
							}
						}
						Singleton<Profile>.Instance.MultiplayerData.AddFriend(data[i, 0].mInt.Value, data[i, 1].mString, data[i, 2].mString, friendName, true);
					}
				}
			}
		});
	}

	public static void PostHighScore(int score)
	{
		Facebook.PublishScore(score);
	}

	public static void FeedDialog(string message, string caption, string description, string toUser, string url, string imageLink, Action<string> onDone)
	{
		//if (PlayHavenManager.instance != null && !PlayHavenManager.instance.isNetworkReachable)
		//{
		//	MultiplayerData.NetworkRequiredDialog();
		//	onDone(string.Empty);
		//	return;
		//}
		smOnFinishedPost = onDone;
		FacebookListener.PostFinished += onPostFinished;
		if (Facebook.IsLoggedIn())
		{
			Facebook.FeedDialog(message, caption, description, toUser, url, imageLink);
			return;
		}
		FacebookLogin(true, delegate(bool result)
		{
			if (result)
			{
				Facebook.FeedDialog(message, caption, description, toUser, url, imageLink);
			}
		});
	}

	public static void onPostFinished(string postId)
	{
		if (smOnFinishedPost != null)
		{
			smOnFinishedPost(postId);
		}
		FacebookListener.PostFinished -= onPostFinished;
	}
}
