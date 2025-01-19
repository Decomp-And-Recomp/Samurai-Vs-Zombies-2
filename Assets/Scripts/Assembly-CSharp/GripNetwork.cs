using System;
using System.Collections.Generic;
using Gamespy.Authentication;
using Gamespy.Matchmaking;
using UnityEngine;

public class GripNetwork
{
	public enum Result
	{
		Success = 0,
		Failed = 1,
		RecordNotFound = 2,
		InUse = 3,
		RecordLimitReached = 4
	}

	public const int kBadRecordID = -1;

	public static Account GameSpyAccountManager
	{
		get
		{
			if (GripNetwork_Login.Instance == null)
			{
				return null;
			}
			return GripNetwork_Login.Instance.Manager;
		}
	}

	public static bool Ready
	{
		get
		{
			return GripNetwork_Login.Instance != null && GripNetwork_Login.Instance.Ready;
		}
	}

	public static bool Busy
	{
		get
		{
			return GripNetwork_Login.Instance != null && GripNetwork_Login.Instance.Busy;
		}
	}

	public static void CreateAccount(string nickName, string password, string email, Action<Result> loginCallback)
	{
		UnityThreadHelper.Activate();
		GripNetwork_Login.Instance.Create(nickName, password, email, loginCallback);
	}

	public static void Login(string nickName, string password, Action<Result> loginCallback)
	{
		UnityThreadHelper.Activate();
		GripNetwork_Login.Instance.Login(nickName, password, loginCallback);
	}

	public static void Logout()
	{
		GripNetwork_Login.Reset();
	}

	public static DisposableMonoBehaviour SendMessageToHost(string ip, int port, string message, Action<Result> sendMessageCallback)
	{
		GameObject gameObject = new GameObject("GripNetwork_SendMessageToHost");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		GripNetwork_SendMessageToHost gripNetwork_SendMessageToHost = gameObject.AddComponent<GripNetwork_SendMessageToHost>();
		gripNetwork_SendMessageToHost.SendMessageToHost(ip, port, message, sendMessageCallback);
		return gripNetwork_SendMessageToHost;
	}

	public static DisposableMonoBehaviour CreateRecord(string tableID, GripField[] fields, Action<Result, int> recordCreatedCallback)
	{
		GameObject gameObject = new GameObject("GripNetwork_CreateRecord");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		GripNetwork_CreateRecord gripNetwork_CreateRecord = gameObject.AddComponent<GripNetwork_CreateRecord>();
		gripNetwork_CreateRecord.CreateRecord(tableID, fields, recordCreatedCallback);
		return gripNetwork_CreateRecord;
	}

	public static DisposableMonoBehaviour RemoveRecord(string tableID, int recordID, Action<Result, int> recordRemovedCallback)
	{
		GameObject gameObject = new GameObject("GripNetwork_RemoveRecord");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		GripNetwork_RemoveRecord gripNetwork_RemoveRecord = gameObject.AddComponent<GripNetwork_RemoveRecord>();
		gripNetwork_RemoveRecord.RemoveRecord(tableID, recordID, recordRemovedCallback);
		return gripNetwork_RemoveRecord;
	}

	public static DisposableMonoBehaviour SearchRecordsOrGetMyRecords(string tableName, string[] fieldNames, int profileId, int numRecords, Action<Result, GripField[,]> searchUpdateCallback)
	{
		if (profileId == GameSpyAccountManager.SecurityToken.ProfileId)
		{
			return GetMyRecords(tableName, fieldNames, searchUpdateCallback);
		}
		return SearchRecords(tableName, fieldNames, string.Empty, string.Empty, new int[1] { profileId }, numRecords, 0, searchUpdateCallback);
	}

	public static DisposableMonoBehaviour SearchFirstRecord(string tableName, string[] fieldNames, int profileId, Action<Result, GripField[,]> searchUpdateCallback)
	{
		return SearchRecords(tableName, fieldNames, string.Empty, string.Empty, new int[1] { profileId }, 1, 0, searchUpdateCallback);
	}

	public static DisposableMonoBehaviour GetMyRecords(string tableName, string[] fieldNames, Action<Result, GripField[,]> searchUpdateCallback)
	{
		GameObject gameObject = new GameObject("GripNetwork_GetMyRecords");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		GripNetwork_GetMyRecords gripNetwork_GetMyRecords = gameObject.AddComponent<GripNetwork_GetMyRecords>();
		gripNetwork_GetMyRecords.GetMyRecords(tableName, fieldNames, searchUpdateCallback);
		return gripNetwork_GetMyRecords;
	}

	public static DisposableMonoBehaviour CountRecords(string tableName, string sqlStyleFilter, Action<Result, int> countUpdateCallback)
	{
		GameObject gameObject = new GameObject("GripNetwork_CountRecords");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		GripNetwork_CountRecords gripNetwork_CountRecords = gameObject.AddComponent<GripNetwork_CountRecords>();
		gripNetwork_CountRecords.CountRecords(tableName, sqlStyleFilter, countUpdateCallback);
		return gripNetwork_CountRecords;
	}

	public static DisposableMonoBehaviour SearchRecords(string tableName, string[] fieldNames, string sqlStyleFilter, string sqlStyleSort, int[] profileIds, int numRecordsPerPage, int pageNumber, Action<Result, GripField[,]> searchUpdateCallback)
	{
		GameObject gameObject = new GameObject("GripNetwork_SearchRecords");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		GripNetwork_SearchRecords gripNetwork_SearchRecords = gameObject.AddComponent<GripNetwork_SearchRecords>();
		gripNetwork_SearchRecords.SearchRecords(tableName, fieldNames, sqlStyleFilter, sqlStyleSort, profileIds, numRecordsPerPage, pageNumber, searchUpdateCallback);
		return gripNetwork_SearchRecords;
	}

	public static DisposableMonoBehaviour UpdateRecord(string tableID, int recordID, GripField[] fields, Action<Result> recordUpdatedCallback)
	{
		GameObject gameObject = new GameObject("GripNetwork_UpdateRecord");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		GripNetwork_UpdateRecord gripNetwork_UpdateRecord = gameObject.AddComponent<GripNetwork_UpdateRecord>();
		gripNetwork_UpdateRecord.UpdateRecord(tableID, recordID, fields, recordUpdatedCallback);
		return gripNetwork_UpdateRecord;
	}

	public static DisposableMonoBehaviour ReadAndLockRecord(string tableName, int ownerId, Action<Result, GripField[]> readAndLockFinishedCallback)
	{
		GameObject gameObject = new GameObject("GripNetwork_ReadAndLockRecord");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		GripNetwork_ReadAndLockRecord gripNetwork_ReadAndLockRecord = gameObject.AddComponent<GripNetwork_ReadAndLockRecord>();
		gripNetwork_ReadAndLockRecord.ReadAndLockRecord(tableName, ownerId, readAndLockFinishedCallback);
		return gripNetwork_ReadAndLockRecord;
	}

	public static DisposableMonoBehaviour UpdateAndUnlockRecord(string tableID, int recordID, GripField[] fields, Action<Result> recordUpdatedCallback)
	{
		GameObject gameObject = new GameObject("GripNetwork_UpdateAndUnlockRecord");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		GripNetwork_UpdateAndUnlockRecord gripNetwork_UpdateAndUnlockRecord = gameObject.AddComponent<GripNetwork_UpdateAndUnlockRecord>();
		gripNetwork_UpdateAndUnlockRecord.UpdateAndUnlockRecord(tableID, recordID, fields, recordUpdatedCallback);
		return gripNetwork_UpdateAndUnlockRecord;
	}

	public static DisposableMonoBehaviour UnlockRecord(string tableID, int recordID, Action<Result> recordUpdatedCallback)
	{
		GameObject gameObject = new GameObject("GripNetwork_UnlockRecord");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		GripNetwork_UpdateAndUnlockRecord gripNetwork_UpdateAndUnlockRecord = gameObject.AddComponent<GripNetwork_UpdateAndUnlockRecord>();
		gripNetwork_UpdateAndUnlockRecord.UnlockRecord(tableID, recordID, recordUpdatedCallback);
		return gripNetwork_UpdateAndUnlockRecord;
	}

	public static DisposableMonoBehaviour UploadFile(byte[] fileData, Action<Result, string> UploadFileCallback)
	{
		GameObject gameObject = new GameObject("GripNetwork_UploadFile");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		GripNetwork_UploadFile gripNetwork_UploadFile = gameObject.AddComponent<GripNetwork_UploadFile>();
		gripNetwork_UploadFile.UploadFile(fileData, UploadFileCallback);
		return gripNetwork_UploadFile;
	}

	public static DisposableMonoBehaviour SearchHosts(string[] keyNames, int count, string filter, Action<Result, List<GameHost>> searchHostsCallback)
	{
		GameObject gameObject = new GameObject("GripNetwork_SearchHosts");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		GripNetwork_SearchHosts gripNetwork_SearchHosts = gameObject.AddComponent<GripNetwork_SearchHosts>();
		gripNetwork_SearchHosts.SearchHosts(keyNames, count, filter, searchHostsCallback);
		return gripNetwork_SearchHosts;
	}

	public static DisposableMonoBehaviour SubscribeHosts(string[] keyNames, string filter, Action<Result, List<GameHost>> searchHostsCallback)
	{
		GameObject gameObject = new GameObject("GripNetwork_SubscribeHosts");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		GripNetwork_SubscribeHosts gripNetwork_SubscribeHosts = gameObject.AddComponent<GripNetwork_SubscribeHosts>();
		gripNetwork_SubscribeHosts.SubscribeHosts(keyNames, filter, searchHostsCallback);
		return gripNetwork_SubscribeHosts;
	}

	public static DisposableMonoBehaviour HTTPRequest(string method, string uri, byte[] bytes, string username, string password, Action<Result, string> whenDone)
	{
		GameObject gameObject = new GameObject("GripNetwork_HTTPRequest");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		GripNetwork_HTTPRequest gripNetwork_HTTPRequest = gameObject.AddComponent<GripNetwork_HTTPRequest>();
		gripNetwork_HTTPRequest.HTTPRequest(method, uri, bytes, username, password, whenDone);
		return gripNetwork_HTTPRequest;
	}

	public static GripNetwork_HostingSession CreateHostingSession(List<Key> keys, Action<GripNetwork_HostingSession.UpdateType, string> updateHandler)
	{
		GameObject gameObject = new GameObject("GripNetwork_HostingSession");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		GripNetwork_HostingSession gripNetwork_HostingSession = gameObject.AddComponent<GripNetwork_HostingSession>();
		gripNetwork_HostingSession.HostingSession(keys, updateHandler);
		return gripNetwork_HostingSession;
	}

	private static void LogSakeRequest(string requestName, string tableName, GripField[] fields)
	{
		string empty = string.Empty;
		empty += requestName;
		empty += ":";
		empty += tableName;
		if (fields != null)
		{
			foreach (GripField gripField in fields)
			{
				empty += "\n";
				empty += gripField.mName;
				empty += ",";
				empty += gripField.GetField().ToString();
			}
		}
	}
}
