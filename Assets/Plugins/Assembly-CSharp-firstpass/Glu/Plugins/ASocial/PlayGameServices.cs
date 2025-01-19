using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Glu.Plugins.AMiscUtils;
using UnityEngine;

namespace Glu.Plugins.ASocial
{
	public class PlayGameServices : MonoBehaviour
	{
		public enum Participants_Status
		{
			STATUS_DECLINED = 0,
			STATUS_FINISHED = 1,
			STATUS_INVITED = 2,
			STATUS_JOINED = 3,
			STATUS_LEFT = 4,
			STATUS_NOT_INVITED_YET = 5
		}

		public enum Match_Result
		{
			MATCH_RESULT_DISAGREED = 0,
			MATCH_RESULT_DISCONNECT = 1,
			MATCH_RESULT_LOSS = 2,
			MATCH_RESULT_NONE = 3,
			MATCH_RESULT_TIE = 4,
			MATCH_RESULT_UNINITIALIZED = 5,
			MATCH_RESULT_WIN = 6,
			PLACING_UNINITIALIZED = 7
		}

		public enum ConnectionResult
		{
			SUCCESS = 0,
			SERVICE_MISSING = 1,
			SERVICE_VERSION_UPDATE_REQUIRED = 2,
			SERVICE_DISABLED = 3,
			SIGN_IN_REQUIRED = 4,
			INVALID_ACCOUNT = 5,
			RESOLUTION_REQUIRED = 6,
			NETWORK_ERROR = 7,
			INTERNAL_ERROR = 8,
			SERVICE_INVALID = 9,
			DEVELOPER_ERROR = 10,
			LICENSE_CHECK_FAILED = 11
		}

		public struct Friend
		{
			public string ID;

			public string DisplayName;

			public string FullName;

			public bool HasAppInstalled;
		}

		public struct Participant
		{
			public string ParticipantID;

			public string PlayerID;

			public string DisplayName;

			public Uri HiResImageUri;

			public Uri IconImageUri;

			public Participants_Status Status;

			public bool IsConnectedToRoom;

			public bool HasPlacing;

			public int Placing;

			public bool HasResult;

			public Match_Result Result;

			public bool IsNominatedHost;

			public bool IsRoomCreator;

			public int ParticipantIndex;
		}

		public enum Leaderboard_Time_Span
		{
			TIME_SPAN_DAILY = 0,
			TIME_SPAN_WEEKLY = 1,
			TIME_SPAN_ALL_TIME = 2
		}

		public enum LeaderboardCollection
		{
			COLLECTION_PUBLIC = 0,
			COLLECTION_SOCIAL = 1
		}

		public struct RetrievedTopLeaderboard
		{
			public string LeaderboardID;

			public string Leaderboard_DisplayName;

			public int SortOrder;

			public string Rank;

			public string Score;

			public string PlayerID;

			public string PlayerName;
		}

		public const int CLIENT_NONE = 0;

		public const int CLIENT_GAMES = 1;

		public const int CLIENT_PLUS = 2;

		public const int CLIENT_APPSTATE = 4;

		public const int CLIENT_ALL = 7;

		private const string gameObjectName = "PlayGameServicesGO";

		private static bool isInitialized = false;

		private static bool isSignedIn = false;

		private static string participantID = string.Empty;

		public static List<Participant> Participants = null;

		public static List<Friend> FriendsList = null;

		private static Participant localParticipant;

		private static bool foundDataOnCloud;

		private static bool mSuppressSignInWarnings;

		private static AndroidJavaClass _ggs = null;

		public static bool IsSignedIn
		{
			get
			{
				return isSignedIn;
			}
		}

		public static string ParticipantID
		{
			get
			{
				return participantID;
			}
		}

		public static AndroidJavaClass ggs
		{
			get
			{
				if (_ggs == null)
				{
					_ggs = new AndroidJavaClass("com.glu.plugins.PlayGameServicesGlu");
				}
				return _ggs;
			}
		}

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onConnectedToPGSHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onConnectionToPGSFailed;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onDisconnectedFromPGSHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onRoomConnectedHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onLeftRoomHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onConnectedToRoomHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onDisconnectedFromRoomHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onPeersConnectedHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onPeersDisconnectedHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onJoinedRoomHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onRoomCreatedHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onPeerLeftHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<MessageArgs> onMPMessageReceivedHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onUserCancelledInvitingFriendsHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onUserCancelledAccpetingInviteHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onLoadedFriendsListHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onCloudLoadSuccessHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onCloudLoadNoDataHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onCloudLoadNetworkErrorNoDataHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onCloudLoadNetworkErrorHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onCloudConflictHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<RetrievedTopScoresLeaderboardArgs> onLeaderboardScoresRetrievalCompleteHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<RetrievedTopScoresLeaderboardArgs> onLeaderboardScoresRetrievalCompleteNoDataHandler;

		[method: MethodImpl(32)]
		public static event EventHandler<EventArgs> onLeaderboardScoresRetrievalFailedHandler;

		public static void Init(bool showWaitDialog = false, bool suppressSignInWarnings = true, int clientsToUse = 7)
		{
			ASocial.Init();
			if (IsPlayServicesSupportedOnDevice())
			{
				GameObject gameObject = new GameObject("PlayGameServicesGO");
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				gameObject.AddComponent<PGSComponent>();
				API_Init(clientsToUse);
				SetWaitDialogDisplay(showWaitDialog);
				mSuppressSignInWarnings = suppressSignInWarnings;
				string path = AJavaTools.GameInfo.GetFilesPath() + "/mpdata";
				DirectoryInfo directoryInfo = new DirectoryInfo(path);
				if (directoryInfo.Exists)
				{
					directoryInfo.Delete(true);
				}
				directoryInfo = Directory.CreateDirectory(path);
				SetMessagesDataPath(directoryInfo.FullName + "/");
				isInitialized = true;
			}
		}

		private static void SetMessagesDataPath(string path)
		{
			API_SetMPMessageDataPath(path);
		}

		public static string GetPlayerID()
		{
			return API_GetPlayerID();
		}

		public static void SetWaitDialogDisplay(bool showDialog)
		{
			API_SetWaitDialogDisplay(showDialog);
		}

		public static void AutoMatch(int minAutoMatchPlayers, int maxAutoMatchPlayers)
		{
			if (IsPlayServicesSupportedOnDevice())
			{
				if (isSignedIn)
				{
					API_AutoMatch(minAutoMatchPlayers, maxAutoMatchPlayers);
				}
				else if (!mSuppressSignInWarnings)
				{
					API_DisplaySignInMessage();
				}
				else if (Debug.isDebugBuild)
				{
					Debug.LogWarning("Can't use PGS feature without signing in!");
				}
			}
		}

		public static void SendInvites(int minOpponents, int maxOpponents)
		{
			if (IsPlayServicesSupportedOnDevice())
			{
				if (isSignedIn)
				{
					API_SendInvites(minOpponents, maxOpponents);
				}
				else if (!mSuppressSignInWarnings)
				{
					API_DisplaySignInMessage();
				}
				else if (Debug.isDebugBuild)
				{
					Debug.LogWarning("Can't use PGS feature without signing in!");
				}
			}
		}

		public static void SendMessage(byte[] msg, bool reliable)
		{
			API_SendMessage(msg, reliable);
		}

		public static void ViewInvites()
		{
			if (IsPlayServicesSupportedOnDevice())
			{
				if (isSignedIn)
				{
					API_ViewInvites();
				}
				else if (!mSuppressSignInWarnings)
				{
					API_DisplaySignInMessage();
				}
				else if (Debug.isDebugBuild)
				{
					Debug.LogWarning("Can't use PGS feature without signing in!");
				}
			}
		}

		public static void LeaveMatch()
		{
			API_LeaveMatch();
		}

		public static void SignIn()
		{
			API_SignIn();
		}

		public static void SignOut()
		{
			if (isSignedIn)
			{
				API_SignOut();
				isSignedIn = false;
			}
		}

		public static void LoadFriends()
		{
			API_LoadFriends();
		}

		public static bool IsGamesClientConnected()
		{
			return API_IsGamesClientConnected();
		}

		public static bool IsAppStateClientConnected()
		{
			return API_IsAppStateClientConnected();
		}

		public static bool IsPlusClientConnected()
		{
			return API_IsGooglePlusClientConnected();
		}

		public static bool IsPlayServicesSupportedOnDevice()
		{
			Debug.Log("Supported: " + API_IsPlayServicesSupportedOnDevice());
			return API_IsPlayServicesSupportedOnDevice() == 0;
		}

		public static string GetDisplayName()
		{
			return API_GetDisplayName();
		}

		public static string GetRoomCreatorID()
		{
			return API_GetRoomCreatorID();
		}

		public static void UpdateProgress(string achievementID, float progress)
		{
			API_UpdateAchievement(achievementID, progress, mSuppressSignInWarnings);
		}

		public static void UnlockAchievement(string achievementID)
		{
			API_UnlockAchievement(achievementID, mSuppressSignInWarnings);
		}

		public static void SubmitScore(string leaderboardID, int score)
		{
			API_SubmitScore(leaderboardID, score, mSuppressSignInWarnings);
		}

		public static void ShowAchievements()
		{
			API_ShowAchievements();
		}

		public static void ShowLeaderboards()
		{
			API_ShowLeaderboards();
		}

		public static void LoadTopScores(string leaderboardID, Leaderboard_Time_Span timeSpan = Leaderboard_Time_Span.TIME_SPAN_ALL_TIME, LeaderboardCollection collection = LeaderboardCollection.COLLECTION_SOCIAL, int maxResults = 25)
		{
			if ((maxResults <= 0 || maxResults > 25) && Debug.isDebugBuild)
			{
				Debug.LogError("MaxResult can only be 25! current maxResults = " + maxResults);
			}
			else
			{
				API_GetTopLeaderboards(leaderboardID, (int)timeSpan, (int)collection, maxResults);
			}
		}

		public static bool IsGooglePlusAppInstalled()
		{
			return API_IsGooglePlusAppInstalled();
		}

		public static void ShareAPost(string message, string url)
		{
			API_ShareAPost(message, url);
		}

		public static void InviteToInstallGame(string message, string marketurl, string weburl)
		{
			API_InviteToInstallApp(message, marketurl, weburl);
		}

		public static void LoadFromCloud()
		{
			foundDataOnCloud = false;
			API_LoadFromCloud();
		}

		public static void SaveToCloud(byte[] data)
		{
			API_SaveToCloud(data);
		}

		public static byte[] GetcloudSave()
		{
			if (foundDataOnCloud)
			{
				return API_GetcloudSave();
			}
			return null;
		}

		public static byte[] GetCloudSaveServerData()
		{
			return API_GetcloudSaveServerData();
		}

		public static void ResolveConflict(bool useServerData)
		{
			API_ResolveConflict(useServerData);
		}

		public static string GetParticipantsDisplayName(string participantID)
		{
			if (Participants != null && Participants.Count > 0)
			{
				foreach (Participant participant in Participants)
				{
					if (participant.ParticipantID == participantID)
					{
						return participant.DisplayName;
					}
				}
			}
			return "Guest";
		}

		public static Participant GetLocalParticipant()
		{
			return localParticipant;
		}

		public static void SetRoomVariant(int roomVariant)
		{
			API_SetRoomVariant(roomVariant);
		}

		private void OnDestroy()
		{
			API_OnDestroy();
			isInitialized = false;
		}

		private void onCloudLoadSuccess(string status)
		{
			Debug.Log("onCloudLoadSuccess status = " + status);
			if (status == "0")
			{
				foundDataOnCloud = true;
				PlayGameServices.onCloudLoadSuccessHandler.Raise(this, null);
			}
			else
			{
				PlayGameServices.onCloudLoadNoDataHandler.Raise(this, null);
			}
		}

		private void onUserCancelledInvitingFriends(string status)
		{
			Debug.Log("onUserCancelledInvitingFriends");
			PlayGameServices.onUserCancelledInvitingFriendsHandler.Raise(this, null);
		}

		private void onUserCancelledAcceptingInvites(string status)
		{
			Debug.Log("onUserCancelledAcceptingInvites");
			PlayGameServices.onUserCancelledAccpetingInviteHandler.Raise(this, null);
		}

		private void onCloudConflict(string status)
		{
			Debug.Log("onCloudConflict");
			PlayGameServices.onCloudConflictHandler.Raise(this, null);
		}

		private void onCloudLoadNoData(string status)
		{
			Debug.Log("onCloudLoadNoData");
			PlayGameServices.onCloudLoadNetworkErrorNoDataHandler.Raise(this, null);
		}

		private void onCloudLoadNetworkError(string status)
		{
			Debug.Log("onCloudLoadNetworkError");
			PlayGameServices.onCloudLoadNetworkErrorHandler.Raise(this, null);
		}

		private void onConnectedToPGS(string status)
		{
			Debug.Log("onConnectedToPGS");
			isSignedIn = true;
			PlayGameServices.onConnectedToPGSHandler.Raise(this, null);
		}

		private void onDisconnectedFromPGS(string status)
		{
			Debug.Log("onDisconnectedFromPGS");
			isSignedIn = false;
			PlayGameServices.onDisconnectedFromPGSHandler.Raise(this, null);
		}

		private void onConnectionFailedToPGS(string status)
		{
			Debug.Log("onConnectionFailedToPGS");
			isSignedIn = false;
			PlayGameServices.onConnectionToPGSFailed.Raise(this, null);
		}

		private void onRoomConnected(string status)
		{
			Debug.Log("onRoomConnected");
			participantID = API_GetParticipantID();
			BuildParticipantsList();
			PlayGameServices.onRoomConnectedHandler.Raise(this, null);
		}

		private void onLeftRoom(string status)
		{
			Debug.Log("onLeftRoom");
			PlayGameServices.onLeftRoomHandler.Raise(this, null);
		}

		private void onConnectedToRoom(string status)
		{
			Debug.Log("onConnectedToRoom");
			PlayGameServices.onConnectedToRoomHandler.Raise(this, null);
		}

		private void onDisconnectedFromRoom(string status)
		{
			Debug.Log("onDisconnectedFromRoom");
			PlayGameServices.onDisconnectedFromRoomHandler.Raise(this, null);
		}

		private void onPeersConnected(string status)
		{
			Debug.Log("onPeersConnected");
			PlayGameServices.onPeersConnectedHandler.Raise(this, null);
		}

		private void onPeersDisconnected(string status)
		{
			Debug.Log("onPeersDisconnected");
			PlayGameServices.onPeersDisconnectedHandler.Raise(this, null);
		}

		private void onPeerLeft(string status)
		{
			Debug.Log("onPeerLeft");
			PlayGameServices.onPeerLeftHandler.Raise(this, null);
		}

		private void onJoinedRoom(string status)
		{
			Debug.Log("onJoinedRoom");
			PlayGameServices.onJoinedRoomHandler.Raise(this, null);
		}

		private void onRoomCreated(string roomCreatorID)
		{
			Debug.Log("onRoomCreated - roomCreatorID " + roomCreatorID);
			PlayGameServices.onRoomCreatedHandler.Raise(this, null);
		}

		private void onPeopleLoaded(string status)
		{
			Debug.Log("onPeopleLoaded(" + status + ")");
			if (status == "success")
			{
				BuildFriendsList();
			}
			else if (PlayGameServices.onLoadedFriendsListHandler != null)
			{
				PlayGameServices.onLoadedFriendsListHandler.Raise(this, null);
			}
		}

		private void onReceivedMessage(string msg)
		{
			if (!string.IsNullOrEmpty(msg))
			{
				StartCoroutine(getReceivedMessage(msg));
			}
			else
			{
				StartCoroutine(getReceivedMessage());
			}
		}

		private void onLeaderboardScoresRetrievalComplete(string leaderboardID)
		{
			Debug.Log("onLeaderboardScoresRetrievalComplete(" + leaderboardID + ")");
			GenerateLeaderboards(leaderboardID);
		}

		private void onLeaderboardScoresRetrievalFailed(string errorCode)
		{
			PlayGameServices.onLeaderboardScoresRetrievalFailedHandler.Raise(this, null);
		}

		private void GenerateLeaderboards(string leaderboardID)
		{
			int num = API_GetCountForRetrievedLeaderboards(leaderboardID);
			if (num > 0)
			{
				List<RetrievedTopLeaderboard> list = new List<RetrievedTopLeaderboard>();
				for (int i = 0; i < num; i++)
				{
					string text = API_GetLeaderboardInfoForIndex(leaderboardID, i);
					Debug.Log("TopLeaderboard " + text);
					if (string.IsNullOrEmpty(text))
					{
						continue;
					}
					string[] array = text.Split('|');
					if (array.Length < 7)
					{
						Debug.LogError("GenerateLeaderboards() LBInfo doesn't match req!!!");
						continue;
					}
					if (Debug.isDebugBuild)
					{
						Debug.Log("LBID:" + array[0] + " LBNAME:" + array[1] + " LBST:" + array[2] + " LBScore:" + array[3] + " LBRank:" + array[4] + " LBPlayerName:" + array[5] + " LBPlayerID:" + array[6]);
					}
					RetrievedTopLeaderboard item = default(RetrievedTopLeaderboard);
					item.LeaderboardID = array[0];
					item.Leaderboard_DisplayName = array[1];
					item.SortOrder = Convert.ToInt16(array[2]);
					item.Score = array[3];
					item.Rank = array[4];
					item.PlayerName = array[5];
					item.PlayerID = array[6];
					list.Add(item);
				}
				RetrievedTopScoresLeaderboardArgs eventArgs = new RetrievedTopScoresLeaderboardArgs(leaderboardID, RetrievedTopScoresLeaderboardArgs.Status.Success, list);
				PlayGameServices.onLeaderboardScoresRetrievalCompleteHandler.Raise(this, eventArgs);
			}
			else
			{
				RetrievedTopScoresLeaderboardArgs eventArgs2 = new RetrievedTopScoresLeaderboardArgs(leaderboardID, RetrievedTopScoresLeaderboardArgs.Status.No_Data);
				PlayGameServices.onLeaderboardScoresRetrievalCompleteNoDataHandler.Raise(this, eventArgs2);
			}
		}

		private IEnumerator getReceivedMessage(string path)
		{
			EventUtils.Raise(eventArgs: new MessageArgs(MessageArgs.MessageType.Non_Reliable, File.ReadAllBytes(path)), eventToTrigger: PlayGameServices.onMPMessageReceivedHandler, sender: this);
			File.Delete(path);
			yield break;
		}

		private IEnumerator getReceivedMessage()
		{
			byte[] msg = API_GetReceivedMessage();
			if (msg != null)
			{
				EventUtils.Raise(eventArgs: new MessageArgs(MessageArgs.MessageType.Non_Reliable, msg), eventToTrigger: PlayGameServices.onMPMessageReceivedHandler, sender: this);
			}
			yield return null;
		}

		private void BuildFriendsList()
		{
			if (FriendsList == null)
			{
				FriendsList = new List<Friend>();
			}
			FriendsList.Clear();
			for (int i = 0; i < API_GetFriendsCount(); i++)
			{
				Friend item = default(Friend);
				item.DisplayName = API_GetFriendsDisplayNameAtIndex(i);
				item.FullName = API_GetFriendsNameAtIndex(i);
				item.HasAppInstalled = API_GetFriendsisHasAppAtIndex(i);
				item.ID = API_GetFriendsIDAtIndex(i);
				if (Debug.isDebugBuild)
				{
					Debug.Log("DN " + item.DisplayName + " FN = " + item.FullName + " HasAppInstalled " + item.HasAppInstalled + " ID " + item.ID);
				}
				FriendsList.Add(item);
			}
			API_ClosePersonBuffer();
			if (PlayGameServices.onLoadedFriendsListHandler != null)
			{
				PlayGameServices.onLoadedFriendsListHandler.Raise(this, null);
			}
		}

		private void BuildParticipantsList()
		{
			if (Participants == null)
			{
				Participants = new List<Participant>();
			}
			Participants.Clear();
			int num = 0;
			int index = 0;
			for (int i = 0; i < API_GetParticipantsCount(); i++)
			{
				Participant item = default(Participant);
				item.ParticipantID = API_GetParticipantsIdAtIndex(i);
				item.DisplayName = API_GetParticipantsDisplayNameAtIndex(i);
				item.ParticipantIndex = Participants.Count;
				int hashCode = item.ParticipantID.GetHashCode();
				if (hashCode > num || hashCode == 0)
				{
					num = item.ParticipantID.GetHashCode();
					index = i;
				}
				string text = API_GetParticipantsHiResUriAtIndex(i);
				if (!string.IsNullOrEmpty(text))
				{
					item.HiResImageUri = new Uri(text);
				}
				text = API_GetParticipantsIconImageUriAtIndex(i);
				if (!string.IsNullOrEmpty(text))
				{
					item.IconImageUri = new Uri(text);
				}
				if (participantID == item.ParticipantID)
				{
					localParticipant = item;
				}
				Participants.Add(item);
			}
			Participant value = Participants[index];
			value.IsRoomCreator = true;
			Participants[index] = value;
			if (value.ParticipantID == localParticipant.ParticipantID)
			{
				localParticipant = value;
			}
		}

		private static int API_IsPlayServicesSupportedOnDevice()
		{
			return ggs.CallStatic<int>("IsPlayGameServicesAvailable", new object[0]);
		}

		private static void API_SetMPMessageDataPath(string path)
		{
			ggs.CallStatic("SetMPMessageDataPath", path);
		}

		private static void API_Init(int clientsToUse)
		{
			ggs.CallStatic("Init", "PlayGameServicesGO", clientsToUse, Debug.isDebugBuild);
		}

		private static void API_SetWaitDialogDisplay(bool showDialog)
		{
			ggs.CallStatic("SetWaitDialogDisplay", showDialog);
		}

		private static void API_AutoMatch(int minAutoMatchPlayers, int maxAutoMatchPlayers)
		{
			ggs.CallStatic("AutoMatch", minAutoMatchPlayers, maxAutoMatchPlayers);
		}

		private static void API_SendInvites(int minOpponents, int maxOpponents)
		{
			ggs.CallStatic("MPInviteStart", minOpponents, maxOpponents);
		}

		private static void API_ViewInvites()
		{
			ggs.CallStatic("ViewMPInvites");
		}

		private static void API_LeaveMatch()
		{
			ggs.CallStatic("LeaveMatch");
		}

		private static void API_SignIn()
		{
			ggs.CallStatic("SignIn");
		}

		private static void API_SignOut()
		{
			ggs.CallStatic("SignOut");
		}

		private static void API_ClosePersonBuffer()
		{
			ggs.CallStatic("ClosePersonBuffer");
		}

		private static void API_SetRoomVariant(int roomVariant)
		{
			ggs.CallStatic("SetRoomVariant", roomVariant);
		}

		private static void API_DisplaySignInMessage(string title = "Google+ Sign-in required!", string message = "Please sign-in with Google+ in the game to use this feature.")
		{
			ggs.CallStatic("DisplaySignInMessage", title, message);
		}

		private static void API_SendMessage(byte[] msg, bool reliable)
		{
			ggs.CallStaticSafe("SendMessage", msg, reliable);
		}

		private static void API_SendMessage(byte[] msg, string participantsId, bool reliable)
		{
			ggs.CallStaticSafe("SendMessage", msg, participantsId, reliable);
		}

		private static string API_GetPlayerID()
		{
			return ggs.CallStatic<string>("GetCurrentPlayerID", new object[0]);
		}

		private static byte[] API_GetReceivedMessage()
		{
			return ggs.CallStatic<byte[]>("GetReceivedMessage", new object[0]);
		}

		private static string API_GetParticipantID()
		{
			return ggs.CallStatic<string>("GetParticipantID", new object[0]);
		}

		private static string API_GetRoomCreatorID()
		{
			return ggs.CallStatic<string>("GetRoomCreatorID", new object[0]);
		}

		private static string API_GetDisplayName()
		{
			return ggs.CallStatic<string>("GetDisplayName", new object[0]);
		}

		private static string API_GetParticipantsIdFromPlayerId(string playerID)
		{
			return ggs.CallStatic<string>("GetParticipantsIdFromPlayerId", new object[1] { playerID });
		}

		private static int API_GetParticipantsCount()
		{
			return ggs.CallStatic<int>("GetParticipantsCount", new object[0]);
		}

		private static string API_GetParticipantsIdAtIndex(int idx)
		{
			return ggs.CallStatic<string>("GetParticipantsIdAtIndex", new object[1] { idx });
		}

		private static string API_GetParticipantsDisplayNameAtIndex(int idx)
		{
			return ggs.CallStatic<string>("GetParticipantsDisplayNameAtIndex", new object[1] { idx });
		}

		private static string API_GetParticipantsHiResUriAtIndex(int idx)
		{
			return ggs.CallStatic<string>("GetParticipantsHiResUriAtIndex", new object[1] { idx });
		}

		private static string API_GetParticipantsIconImageUriAtIndex(int idx)
		{
			return ggs.CallStatic<string>("GetParticipantsIconImageUriAtIndex", new object[1] { idx });
		}

		private static int API_GetParticipantsStatusAtIndex(int idx)
		{
			return ggs.CallStatic<int>("GetParticipantsStatusAtIndex", new object[1] { idx });
		}

		private static int API_GetParticipantsPlacingAtIndex(int idx)
		{
			return ggs.CallStatic<int>("GetParticipantsPlacingAtIndex", new object[1] { idx });
		}

		private static bool API_GetParticipantsHasPlacingAtIndex(int idx)
		{
			return ggs.CallStatic<bool>("GetParticipantsHasPlacingAtIndex", new object[1] { idx });
		}

		private static int API_GetParticipantsResultTypeAtIndex(int idx)
		{
			return ggs.CallStatic<int>("GetParticipantsResultTypeAtIndex", new object[1] { idx });
		}

		private static bool API_GetParticipantsHasResultAtIndex(int idx)
		{
			return ggs.CallStatic<bool>("GetParticipantsHasResultAtIndex", new object[1] { idx });
		}

		private static bool API_IsParticipantAtIndexConnectedToRoom(int idx)
		{
			return ggs.CallStatic<bool>("IsParticipantAtIndexConnectedToRoom", new object[1] { idx });
		}

		private static bool API_IsParticipantAtIndexTheRoomCreator(int idx)
		{
			return ggs.CallStatic<bool>("IsParticipantAtIndexTheRoomCreator", new object[1] { idx });
		}

		private static void API_OnDestroy()
		{
			ggs.CallStatic("OnDestroy");
		}

		private static void API_UpdateAchievement(string achievementID, float progress, bool dontShowSignDialog)
		{
			ggs.CallStatic("UpdateAchievement", achievementID, progress, dontShowSignDialog);
		}

		private static void API_UnlockAchievement(string achievementID, bool dontShowSignDialog)
		{
			ggs.CallStatic("UnlockAchievement", achievementID, dontShowSignDialog);
		}

		private static void API_SubmitScore(string leaderboardID, int score, bool dontShowSignDialog)
		{
			ggs.CallStatic("SubmitScore", leaderboardID, score, dontShowSignDialog);
		}

		private static void API_ShowAchievements()
		{
			ggs.CallStatic("onShowAchievementsRequested");
		}

		private static void API_ShowLeaderboards()
		{
			ggs.CallStatic("onShowLeaderboardsRequested");
		}

		private static void API_LoadFromCloud()
		{
			ggs.CallStatic("loadFromCloud");
		}

		private static void API_SaveToCloud(byte[] data)
		{
			ggs.CallStaticSafe("saveToCloud", data);
		}

		private static byte[] API_GetcloudSave()
		{
			return ggs.CallStatic<byte[]>("GetcloudSave", new object[0]);
		}

		private static byte[] API_GetcloudSaveServerData()
		{
			return ggs.CallStatic<byte[]>("GetcloudSaveServerData", new object[0]);
		}

		private static void API_ResolveConflict(bool useServerData)
		{
			ggs.CallStatic("ResolveConflict", useServerData);
		}

		private static void API_LoadFriends()
		{
			ggs.CallStatic("GetPeoplePlayingApp");
		}

		private static bool API_IsGamesClientConnected()
		{
			return ggs.CallStatic<bool>("IsGamesClientConnected", new object[0]);
		}

		private static bool API_IsAppStateClientConnected()
		{
			return ggs.CallStatic<bool>("IsAppStateClientConnected", new object[0]);
		}

		private static bool API_IsGooglePlusClientConnected()
		{
			return ggs.CallStatic<bool>("IsGooglePlusClientConnected", new object[0]);
		}

		private static int API_GetFriendsCount()
		{
			return ggs.CallStatic<int>("GetFriendsCount", new object[0]);
		}

		private static string API_GetFriendsIDAtIndex(int idx)
		{
			return ggs.CallStatic<string>("GetFriendsIDAtIndex", new object[1] { idx });
		}

		private static string API_GetFriendsDisplayNameAtIndex(int idx)
		{
			return ggs.CallStatic<string>("GetFriendsDisplayNameAtIndex", new object[1] { idx });
		}

		private static string API_GetFriendsNameAtIndex(int idx)
		{
			return ggs.CallStatic<string>("GetFriendsNameAtIndex", new object[1] { idx });
		}

		private static bool API_GetFriendsisHasAppAtIndex(int idx)
		{
			return ggs.CallStatic<bool>("GetFriendsisHasAppAtIndex", new object[1] { idx });
		}

		private static void API_GetTopLeaderboards(string leaderboardid, int span, int leaderboardCollection, int maxResults)
		{
			ggs.CallStatic("LoadTopScores", leaderboardid, span, leaderboardCollection, maxResults);
		}

		private static int API_GetCountForRetrievedLeaderboards(string leaderboardID)
		{
			return ggs.CallStatic<int>("GetCountForRetrievedLeaderboards", new object[1] { leaderboardID });
		}

		private static string API_GetLeaderboardInfoForIndex(string leaderboardid, int index)
		{
			return ggs.CallStatic<string>("GetLeaderboardInfoForIndex", new object[2] { leaderboardid, index });
		}

		private static bool API_IsGooglePlusAppInstalled()
		{
			return ggs.CallStatic<bool>("IsGooglePlusAppInstalled", new object[0]);
		}

		private static void API_ShareAPost(string message, string url)
		{
			ggs.CallStatic("ShareAPost", message, url);
		}

		private static void API_InviteToInstallApp(string message, string marketurl, string url)
		{
			ggs.CallStatic("InviteToInstallApp", message, marketurl, url);
		}
	}
}
