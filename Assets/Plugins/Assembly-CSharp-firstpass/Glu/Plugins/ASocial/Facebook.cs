using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Glu.Plugins.AMiscUtils;
using UnityEngine;

namespace Glu.Plugins.ASocial
{
	public class Facebook : MonoBehaviour
	{
		public enum FeedType
		{
			Name = 0,
			Caption = 1,
			Description = 2,
			Link = 3,
			Picture = 4
		}

		public enum AppRequestType
		{
			message = 0,
			suggestedFriends = 1,
			data = 2
		}

		private const string gameObjectName = "FacebookGO";

		private static GameObject go;

		private static List<Dictionary<string, string>> mFriends;

		private static AndroidJavaClass _facebook;

		public static AndroidJavaClass facebook
		{
			get
			{
				if (_facebook == null)
				{
					_facebook = new AndroidJavaClass("com.glu.plugins.FacebookGlu");
				}
				return _facebook;
			}
		}

		[method: MethodImpl(32)]
		public static event EventHandler<FacebookEventArgs> FacebookLoginHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<FacebookFQLQueryEventArgs> FacebookFQLQueryCompleteHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<FacebookRequestEventArgs> FacebookRequestCompleteHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<FacebookFeedEventArgs> FacebookFeedPostCompleteHandler;

		private void OnDestroy()
		{
			API_OnDestroy();
		}

		private void OnApplicationPause(bool paused)
		{
			if (paused)
			{
				API_OnPause();
			}
			else
			{
				API_OnResume();
			}
		}

		public static void Init()
		{
			go = AJavaTools.GetPluginsGameObject();
			if (go == null)
			{
				go = new GameObject("FacebookGO");
			}
			UnityEngine.Object.DontDestroyOnLoad(go);
			go.AddComponent<FacebookComponent>();
			ASocial.Init();
			API_Init();
		}

		public static void Login()
		{
			API_Login();
		}

		public static bool IsLoggedIn()
		{
			return API_IsLoggedIn();
		}

		public static void Post(Dictionary<FeedType, string> parameters, bool showDialog = true)
		{
			API_Post(parameters, showDialog);
		}

		public static void PostPhoto(string path, string linkURL, string caption = "", bool showDialog = true)
		{
			Dictionary<FeedType, string> dictionary = new Dictionary<FeedType, string>();
			string packageName = AJavaTools.GameInfo.GetPackageName();
			string value = packageName.Substring(packageName.LastIndexOf('.') + 1);
			dictionary.Add(FeedType.Name, value);
			dictionary.Add(FeedType.Caption, caption);
			dictionary.Add(FeedType.Link, linkURL);
			dictionary.Add(FeedType.Picture, path);
			API_Post(dictionary, showDialog);
		}

		public static string GetID()
		{
			return GetUserInfo("id");
		}

		public static string GetName()
		{
			return GetUserInfo("name");
		}

		public static string GetUsername()
		{
			return GetUserInfo("username");
		}

		public static string GetAppId()
		{
			return API_GetAppId();
		}

		public static string GetUserInfo(string key)
		{
			return API_GetUserInfo(key);
		}

		public static void Request(Dictionary<AppRequestType, string> parameters)
		{
			API_Request(parameters);
		}

		public static List<Dictionary<string, string>> GetFriendsWithApp()
		{
			return mFriends;
		}

		public static void FQLQuery(string query)
		{
			API_FQLQuery(query);
		}

		public static void Logout()
		{
			API_Logout();
		}

		private void onFacebookLoginComplete(string status)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("onFacebookLoginComplete( " + status + " )");
			}
			if (status.Equals("OPENED") || status.Equals("OPENED_TOKEN_UPDATED"))
			{
				FacebookEventArgs eventArgs = new FacebookEventArgs(FacebookEventArgs.FaceBookLoginStatus.Success, string.Empty);
				Facebook.FacebookLoginHandler.Raise(this, eventArgs);
			}
			else if (status.Equals("CLOSED_LOGIN_FAILED") || status.Equals("CLOSED"))
			{
				FacebookEventArgs eventArgs2 = new FacebookEventArgs(FacebookEventArgs.FaceBookLoginStatus.Failed, string.Empty);
				Facebook.FacebookLoginHandler.Raise(this, eventArgs2);
			}
		}

		private void onFacebookFriendsListRetreived(string response)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("onFacebookFriendsListRetreived( " + response + " )");
			}
			if (string.IsNullOrEmpty(response))
			{
				return;
			}
			response = response.Trim("[{}]".ToCharArray());
			string[] array = Regex.Split(response, "},{");
			if (mFriends == null)
			{
				mFriends = new List<Dictionary<string, string>>();
			}
			mFriends.Clear();
			for (int i = 0; i < array.Length; i++)
			{
				Match match = Regex.Match(array[i], "\"name\"\\s*:\\s*\"(.+?)\"");
				Match match2 = Regex.Match(array[i], "\"uid\"\\s*:\\s*([0-9]+)");
				if (match.Success && match2.Success)
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					dictionary.Add("name", match.Groups[1].Value);
					dictionary.Add("uid", match2.Groups[1].Value);
					if (Debug.isDebugBuild)
					{
						Debug.Log("AddingFriend name:" + match.Groups[1].Value + " , ID:" + match2.Groups[1].Value);
					}
					mFriends.Add(dictionary);
				}
			}
		}

		private void onFQLQueryComplete(string response)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("onFQLQueryComplete( " + response + " )");
			}
			if (!string.IsNullOrEmpty(response))
			{
				FacebookFQLQueryEventArgs eventArgs = new FacebookFQLQueryEventArgs(response);
				Facebook.FacebookFQLQueryCompleteHandler.Raise(this, eventArgs);
			}
		}

		private void onFacebookRequestComplete(string status)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("onFacebookRequestComplete(" + status + ")");
			}
			string[] array = status.Split('|');
			if (array[0] == "success")
			{
				FacebookRequestEventArgs eventArgs = new FacebookRequestEventArgs(FacebookRequestEventArgs.FacebookRequestStatus.Success, string.Empty);
				Facebook.FacebookRequestCompleteHandler.Raise(this, eventArgs);
			}
			else if (array[0] == "cancelled")
			{
				if (array.Length == 2)
				{
					FacebookRequestEventArgs eventArgs2 = new FacebookRequestEventArgs(FacebookRequestEventArgs.FacebookRequestStatus.Cancelled, string.Empty);
					Facebook.FacebookRequestCompleteHandler.Raise(this, eventArgs2);
				}
			}
			else if (array[0] == "failed")
			{
				FacebookRequestEventArgs facebookRequestEventArgs = null;
				EventUtils.Raise(eventArgs: (array.Length != 2) ? new FacebookRequestEventArgs(FacebookRequestEventArgs.FacebookRequestStatus.Failed, string.Empty) : new FacebookRequestEventArgs(FacebookRequestEventArgs.FacebookRequestStatus.Failed, array[1]), eventToTrigger: Facebook.FacebookRequestCompleteHandler, sender: this);
			}
		}

		private void onFacebookPostFeedComplete(string status)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("onFacebookPostFeedComplete(" + status + ")");
			}
			string[] array = status.Split('|');
			if (array[0] == "success")
			{
				FacebookFeedEventArgs eventArgs = new FacebookFeedEventArgs(FacebookFeedEventArgs.FacebookFeedStatus.Success, string.Empty);
				Facebook.FacebookFeedPostCompleteHandler.Raise(this, eventArgs);
			}
			else if (array[0] == "cancelled")
			{
				if (array.Length == 2)
				{
					FacebookFeedEventArgs eventArgs2 = new FacebookFeedEventArgs(FacebookFeedEventArgs.FacebookFeedStatus.Cancelled, string.Empty);
					Facebook.FacebookFeedPostCompleteHandler.Raise(this, eventArgs2);
				}
			}
			else if (array[0] == "failed")
			{
				FacebookFeedEventArgs facebookFeedEventArgs = null;
				EventUtils.Raise(eventArgs: (array.Length != 2) ? new FacebookFeedEventArgs(FacebookFeedEventArgs.FacebookFeedStatus.Failed, string.Empty) : new FacebookFeedEventArgs(FacebookFeedEventArgs.FacebookFeedStatus.Failed, array[1]), eventToTrigger: Facebook.FacebookFeedPostCompleteHandler, sender: this);
			}
		}

		private static void API_Init()
		{
			facebook.CallStatic("Init", go.name);
		}

		private static void API_OnPause()
		{
			facebook.CallStatic("onPause");
		}

		private static void API_OnResume()
		{
			facebook.CallStatic("onResume");
		}

		private static void API_OnDestroy()
		{
			facebook.CallStatic("onDestroy");
		}

		private static void API_Login()
		{
			facebook.CallStatic("Login");
		}

		private static bool API_IsLoggedIn()
		{
			return facebook.CallStatic<bool>("IsLoggedIn", new object[0]);
		}

		private static void API_Post(Dictionary<FeedType, string> parameters, bool showDialog)
		{
			using (AndroidJavaObject androidJavaObject = ASocial.DictionaryToHashMap(parameters))
			{
				facebook.CallStatic("Post", androidJavaObject, showDialog);
			}
		}

		private static void API_PostPhoto(string path, string caption)
		{
			facebook.CallStatic("PostPhoto", path, caption);
		}

		private static void API_Request(Dictionary<AppRequestType, string> parameters)
		{
			using (AndroidJavaObject androidJavaObject = ASocial.DictionaryToHashMap(parameters))
			{
				facebook.CallStatic("Request", false, androidJavaObject);
			}
		}

		private static void API_FQLQuery(string query)
		{
			facebook.CallStatic("FQLQuery", query);
		}

		private static string API_GetUserInfo(string key)
		{
			return facebook.CallStatic<string>("GetUserInfo", new object[1] { key });
		}

		private static string API_GetAppId()
		{
			return facebook.CallStatic<string>("GetAppID", new object[0]);
		}

		private static void API_Logout()
		{
			facebook.CallStatic("Logout");
		}
	}
}
