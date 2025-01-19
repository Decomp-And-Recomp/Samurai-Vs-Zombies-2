using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FacebookListener : MonoBehaviour
{
	private static FBBSession _session;

	private static FBBUserCollection _friends = new FBBUserCollection();

	[method: MethodImpl(32)]
	public static event Action<FBBSession> FacebookInitialized;

	[method: MethodImpl(32)]
	public static event Action<string> FacebookInitializationFailed;

	[method: MethodImpl(32)]
	public static event Action<FBBSession> SessionLoggedIn;

	[method: MethodImpl(32)]
	public static event Action<string> SessionLoginFailed;

	[method: MethodImpl(32)]
	public static event Action<FBBSession> SessionValidated;

	[method: MethodImpl(32)]
	public static event Action<string> SessionValidationFailed;

	[method: MethodImpl(32)]
	public static event Action<string> SessionLoggedOut;

	[method: MethodImpl(32)]
	public static event Action<string> ScorePublished;

	[method: MethodImpl(32)]
	public static event Action<string> NetworkConnectionFailed;

	[method: MethodImpl(32)]
	public static event Action<string> PostFinished;

	[method: MethodImpl(32)]
	public static event Action<FBBUserCollection> FriendsRead;

	[method: MethodImpl(32)]
	public static event Action<FBBUserCollection> InviteFriendsDialogClosed;

	private void Awake()
	{
		FBBDebug.Trace("FacebookListener.Awake");
		base.gameObject.name = GetType().ToString();
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	public void ReceiveMessage_FacebookInitialized(string serializedUser)
	{
		FBBDebug.Trace("FacebookListener.ReceiveMessage_FacebookInitialized");
		if (FacebookListener.FacebookInitialized != null)
		{
			FBBUser user = FBBUser.Deserialize(serializedUser);
			_session = new FBBSession(user, null, (!Facebook.IsLoggedIn()) ? FBBSessionState.Closed : FBBSessionState.Open);
			FacebookListener.FacebookInitialized(_session);
		}
	}

	public void ReceiveMessage_FacebookInitializationFailed(string error)
	{
		FBBDebug.Trace("FacebookListener.ReceiveMessage_FacebookInitializationFailed");
		if (FacebookListener.FacebookInitializationFailed != null)
		{
			FacebookListener.FacebookInitializationFailed(error);
		}
	}

	public void ReceiveMessage_SessionLoggedIn(string serializedUser)
	{
		FBBDebug.Trace("FacebookListener.ReceiveMessage_SessionLoggedIn");
		if (FacebookListener.SessionLoggedIn != null)
		{
			FBBUser user = FBBUser.Deserialize(serializedUser);
			_session = new FBBSession(user, null, (!Facebook.IsLoggedIn()) ? FBBSessionState.Closed : FBBSessionState.Open);
			FacebookListener.SessionLoggedIn(_session);
		}
	}

	public void ReceiveMessage_SessionLoginFailed(string error)
	{
		FBBDebug.Trace("FacebookListener.ReceiveMessage_SessionLoginFailed");
		if (FacebookListener.SessionLoginFailed != null)
		{
			FacebookListener.SessionLoginFailed(error);
		}
	}

	public void ReceiveMessage_SessionValidated(string serializedUser)
	{
		FBBDebug.Trace("FacebookListener.ReceiveMessage_SessionValidated");
		if (FacebookListener.SessionValidated != null)
		{
			FBBUser user = FBBUser.Deserialize(serializedUser);
			_session = new FBBSession(user, null, (!Facebook.IsLoggedIn()) ? FBBSessionState.Closed : FBBSessionState.Open);
			FacebookListener.SessionValidated(_session);
		}
	}

	public void ReceiveMessage_SessionValidationFailed(string error)
	{
		FBBDebug.Trace("FacebookListener.ReceiveMessage_SessionValidationFailed");
		if (FacebookListener.SessionValidationFailed != null)
		{
			FacebookListener.SessionValidationFailed(error);
		}
	}

	public void ReceiveMessage_SessionLoggedOut(string userID)
	{
		FBBDebug.Trace("FacebookListener.ReceiveMessage_SessionLoggedOut");
		if (FacebookListener.SessionLoggedOut != null)
		{
			_session = null;
			FacebookListener.SessionLoggedOut(userID);
		}
	}

	public void ReceiveMessage_ScorePublished(string success)
	{
		FBBDebug.Trace("FacebookListener.ReceiveMessage_ScorePublished");
		if (FacebookListener.ScorePublished != null)
		{
			FacebookListener.ScorePublished(success);
		}
	}

	public void ReceiveMessage_FriendsRead(string data)
	{
		FBBDebug.Trace("FacebookListener.ReceiveMessage_FriendsRead");
		if (FacebookListener.FriendsRead != null)
		{
			_friends = FBBUserCollection.Deserialize(data);
			FacebookListener.FriendsRead(_friends);
		}
	}

	public void ReceiveMessage_NetworkConnectionFailed(string hostname)
	{
		FBBDebug.Trace("FacebookListener.ReceiveMessage_NetworkConnectionFailed");
		if (FacebookListener.NetworkConnectionFailed != null)
		{
			FacebookListener.NetworkConnectionFailed(hostname);
		}
	}

	public void ReceiveMessage_InviteFriendsDialogClosed()
	{
		FBBDebug.Trace("FacebookListener.ReceiveMessage_InviteFriendsDialogClosed");
		if (FacebookListener.InviteFriendsDialogClosed != null)
		{
			FBBUserCollection obj = new FBBUserCollection();
			FacebookListener.InviteFriendsDialogClosed(obj);
		}
	}

	public void ReceiveMessage_PostFinished(string postId)
	{
		FBBDebug.Trace("FacebookListener.ReceiveMessage_PostFinished");
		if (FacebookListener.PostFinished != null)
		{
			FacebookListener.PostFinished(postId);
		}
	}
}
