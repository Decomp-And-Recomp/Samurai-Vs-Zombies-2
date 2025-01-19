using System;
using System.Security.Cryptography;
using System.Text;
using Gamespy.Common;
using UnityEngine;

public class GripAccount
{
	public enum LoginStatus
	{
		LoggedOut = 0,
		InProgress = 1,
		Complete = 2,
		Failed = 3
	}

	public static readonly string kRecordName = "tavkeeper_account";

	public static readonly string kAccountKey = "GRIPACCOUNT_ID";

	public static readonly string kVersionKey = "GRIPACCOUNT_VERSION";

	public static readonly float kVersion = 1f;

	public string ID { get; set; }

	public string Password { get; set; }

	public string Name { get; set; }

	public string Email { get; set; }

	public LoginStatus Status { get; private set; }

	public bool New { get; private set; }

	public float Version { get; private set; }

	private GripAccount()
	{
	}

	private GripAccount(string id)
	{
		ID = id;
		Password = GeneratePassword(id);
		Name = string.Empty;
		Email = id + "@example.com";
	}

	public static void Login(bool allowCreateNewAccount, Action<GripAccount, int> onComplete)
	{
		int retryAttempts = 0;
		RetrieveCurrent(delegate(GripAccount account)
		{
			if (account != null)
			{
				account.TryToLogin(delegate
				{
					if (onComplete != null)
					{
						onComplete(account, retryAttempts);
					}
				});
			}
			else if (allowCreateNewAccount)
			{
				account = CreateNew(ref retryAttempts);
				account.TryToCreate(delegate(GripNetwork.Result createResult)
				{
					if (createResult == GripNetwork.Result.Success)
					{
						account.Save();
					}
					if (onComplete != null)
					{
						onComplete(account, retryAttempts);
					}
				});
			}
		});
	}

	public static GripAccount CreateNew(ref int retryAttempt)
	{
		string userID = ApplicationUtilities.UserID;
		string s = GeneralConfig.GameSpyName + userID;
		SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider();
		byte[] bytes = Encoding.ASCII.GetBytes(s);
		byte[] bytes2 = sHA1CryptoServiceProvider.ComputeHash(bytes);
		string id = TypeConverters.ByteArrayToHexString(bytes2);
		retryAttempt++;
		GripAccount gripAccount = new GripAccount(id);
		gripAccount.New = true;
		return gripAccount;
	}

	public static void RetrieveCurrent(Action<GripAccount> onComplete)
	{
		RetrieveDeviceAccount(onComplete);
	}

	private static void RetrieveDeviceAccount(Action<GripAccount> onComplete)
	{
		string text = null;
		text = ((!Singleton<Profile>.Exists || string.IsNullOrEmpty(Singleton<Profile>.Instance.GameSpyUserID)) ? PlayerPrefs.GetString(kAccountKey) : Singleton<Profile>.Instance.GameSpyUserID);
		GripAccount gripAccount = null;
		if (!string.IsNullOrEmpty(text))
		{
			gripAccount = new GripAccount(text);
			if (Singleton<Profile>.Exists && Singleton<Profile>.Instance.GameSpyUserAccountVersion != 0f)
			{
				gripAccount.Version = Singleton<Profile>.Instance.GameSpyUserAccountVersion;
			}
			else
			{
				gripAccount.Version = PlayerPrefs.GetFloat(kVersionKey, 0f);
			}
		}
		if (onComplete != null)
		{
			onComplete(gripAccount);
		}
	}

	public static string GeneratePassword(string source)
	{
		int hashCode = source.GetHashCode();
		return (hashCode ^ 0x7FFFFFFF).ToString();
	}

	public void Save()
	{
		if (Singleton<Profile>.Exists)
		{
			Singleton<Profile>.Instance.GameSpyUserID = ID;
			Singleton<Profile>.Instance.GameSpyUserAccountVersion = Version;
		}
		PlayerPrefs.SetString(kAccountKey, ID);
		PlayerPrefs.SetFloat(kVersionKey, Version);
	}

	private void Create(Action<GripNetwork.Result> onComplete)
	{
		Status = LoginStatus.InProgress;
		GripNetwork.CreateAccount(ID, Password, Email, delegate(GripNetwork.Result createResult)
		{
			switch (createResult)
			{
			case GripNetwork.Result.Success:
				Status = LoginStatus.Complete;
				if (onComplete != null)
				{
					onComplete(GripNetwork.Result.Success);
				}
				break;
			case GripNetwork.Result.InUse:
				Login(onComplete);
				break;
			default:
				Status = LoginStatus.Failed;
				if (onComplete != null)
				{
					onComplete(GripNetwork.Result.Failed);
				}
				break;
			}
		});
	}

	private void Login(Action<GripNetwork.Result> onComplete)
	{
		Status = LoginStatus.InProgress;
		GripNetwork.Login(ID, Password, delegate(GripNetwork.Result loginResult)
		{
			switch (loginResult)
			{
			case GripNetwork.Result.Success:
				Status = LoginStatus.Complete;
				if (onComplete != null)
				{
					onComplete(GripNetwork.Result.Success);
				}
				break;
			case GripNetwork.Result.Failed:
				Create(onComplete);
				break;
			default:
				Status = LoginStatus.Failed;
				if (onComplete != null)
				{
					onComplete(GripNetwork.Result.Failed);
				}
				break;
			}
		});
	}

	public void TryToLogin(Action<GripNetwork.Result> onComplete)
	{
		if (Status != LoginStatus.InProgress)
		{
			Status = LoginStatus.LoggedOut;
			Login(onComplete);
		}
	}

	public void TryToCreate(Action<GripNetwork.Result> onComplete)
	{
		if (Status != LoginStatus.InProgress)
		{
			Status = LoginStatus.LoggedOut;
			Create(onComplete);
		}
	}

	public void ClearNewFlag()
	{
		New = false;
	}
}
