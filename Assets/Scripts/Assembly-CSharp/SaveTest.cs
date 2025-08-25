using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveTest : MonoBehaviour
{
	[Serializable]
	public class MySaveData : BinaryStreamProvider.SaveHandler
	{
		public string aString;

		public int aInt;

		public float aFloat;

		public string bString;

		public int bInt;

		public float bFloat;

		public MySaveData()
		{
			base.Version = 1f;
			base.Identifier = typeof(MySaveData).ToString();
		}

		public override void Save(BinaryStreamProvider sp, IEnumerable<SaveTarget> targets)
		{
			sp.WriteData(aString);
			sp.WriteData(aInt);
			sp.WriteData(aFloat);
			foreach (SaveTarget target in targets)
			{
				target.SaveValue("bString", bString);
				target.SaveValue("bInt", bInt);
				target.SaveValue("bFloat", bFloat);
			}
		}

		public override void Load(BinaryStreamProvider sp, float handlerVersion, SaveTarget target)
		{
			aString = sp.ReadData_String();
			aInt = sp.ReadData_Int();
			aFloat = sp.ReadData_Float();
			target.LoadValue("bString", string.Empty, delegate(string val)
			{
				bString = val;
			});
			target.LoadValue("bInt", 0, delegate(int val)
			{
				bInt = val;
			});
			target.LoadValue("bFloat", 0f, delegate(float val)
			{
				bFloat = val;
			});
		}

		public override string ToString()
		{
			return string.Format("[MySaveData: Version={0}, Identifier={1}] aString={2}, aInt={3}, aFloat={4}, bString={5}, bInt={6}, bFloat={7},", base.Version, base.Identifier, aString, aInt, aFloat, bString, bInt, bFloat);
		}
	}

	public bool doSaveLocal;

	public bool doLoadLocal;

	public bool doSaveNetwork;

	public bool doLoadNetwork;

	public bool doSaveCloud;

	public bool doLoadCloud;

	public bool doTextSave;

	public bool doTextLoad;

	public MySaveData saveData = new MySaveData();

	public GripAccount CurrentAccount;

	public int createAccountRetryAttempt;

	public bool AccountLoginComplete;

	public bool ConnectionAlertIsShown;

	public bool ShowConnectionErrors;

	private BinaryStreamProvider binarySave;

	private SimpleTextProvider textSave;

	private void Awake()
	{
		SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.Init();
	}

	private void Start()
	{
		binarySave = SaveProvider.Create<BinaryStreamProvider>("save");
		binarySave.SaveOnExit = false;
		binarySave.Header.UseEncoding = true;
		binarySave.Header.UseDeviceData = true;
		binarySave.AddHandler(saveData);
		textSave = SaveProvider.Create<SimpleTextProvider>("textSave");
		textSave.Header.UseEncoding = false;
		textSave.Header.UseDeviceData = true;
		FileSaveTarget fileSaveTarget = textSave.AddTarget<FileSaveTarget>("localText");
		fileSaveTarget.UseBackup = true;
		textSave.Strings["aInt"] = saveData.aInt.ToString();
		textSave.Strings["aString"] = saveData.aString.ToString();
		textSave.Strings["aFloat"] = saveData.aFloat.ToString();
	}

	private void Update()
	{
		if (doSaveLocal)
		{
			StopCoroutine("DoSaveLocal");
			StartCoroutine("DoSaveLocal");
			doSaveLocal = false;
		}
		if (doLoadLocal)
		{
			StopCoroutine("DoLoadLocal");
			StartCoroutine("DoLoadLocal");
			doLoadLocal = false;
		}
		if (doSaveNetwork)
		{
			StopCoroutine("DoSaveNetwork");
			StartCoroutine("DoSaveNetwork");
			doSaveNetwork = false;
		}
		if (doLoadNetwork)
		{
			StopCoroutine("DoLoadNetwork");
			StartCoroutine("DoLoadNetwork");
			doLoadNetwork = false;
		}
		if (doSaveCloud || Input.touchCount == 1)
		{
			StopCoroutine("DoSaveCloud");
			StartCoroutine("DoSaveCloud");
			doSaveCloud = false;
		}
		if (doLoadCloud)
		{
			StopCoroutine("DoLoadCloud");
			StartCoroutine("DoLoadCloud");
			doLoadCloud = false;
		}
		if (doTextSave)
		{
			textSave.Save();
			doTextSave = false;
		}
		if (doTextLoad)
		{
			textSave.Load("localText", delegate
			{
			});
			doTextLoad = false;
		}
	}

	private IEnumerator DoSaveLocal()
	{
		string targetName = "local";
		FileSaveTarget target2 = binarySave.GetTarget(targetName) as FileSaveTarget;
		if (target2 == null)
		{
			target2 = binarySave.AddTarget<FileSaveTarget>(targetName);
			target2.UseBackup = true;
		}
		binarySave.Save(targetName);
		yield break;
	}

	private IEnumerator DoLoadLocal()
	{
		string targetName = "local";
		FileSaveTarget target2 = binarySave.GetTarget(targetName) as FileSaveTarget;
		if (target2 == null)
		{
			target2 = binarySave.AddTarget<FileSaveTarget>(targetName);
			target2.UseBackup = true;
		}
		bool loadComplete = false;
		binarySave.Load(targetName, delegate
		{
			loadComplete = true;
		});
		while (!loadComplete)
		{
			yield return null;
		}
	}

	private IEnumerator DoSaveNetwork()
	{
		string targetName = "saveOne";
		GripNetworkSaveTarget target = binarySave.GetTarget(targetName) as GripNetworkSaveTarget;
		if (target == null)
		{
			target = binarySave.AddTarget<GripNetworkSaveTarget>(targetName);
			target.UseBackup = true;
			target.Table = "SaveTest";
		}
		Login();
		while (target.CurrentStatus == GripNetworkSaveTarget.Status.Uninitialized)
		{
			yield return null;
		}
		binarySave.Save(targetName);
		while (target.CurrentStatus == GripNetworkSaveTarget.Status.Saving)
		{
			yield return null;
		}
	}

	private IEnumerator DoLoadNetwork()
	{
		string targetName = "saveOne";
		GripNetworkSaveTarget target = binarySave.GetTarget(targetName) as GripNetworkSaveTarget;
		if (target == null)
		{
			target = binarySave.AddTarget<GripNetworkSaveTarget>(targetName);
			target.UseBackup = true;
			target.Table = "SaveTest";
		}
		Login();
		while (target.CurrentStatus == GripNetworkSaveTarget.Status.Uninitialized)
		{
			yield return null;
		}
		bool loadComplete = false;
		binarySave.Load(targetName, delegate
		{
			loadComplete = true;
		});
		while (!loadComplete)
		{
			yield return null;
		}
	}

	private void Login()
	{
		if (CurrentAccount == null)
		{
			GripAccount.RetrieveCurrent(delegate(GripAccount account)
			{
				if (account == null)
				{
					CurrentAccount = GripAccount.CreateNew(ref createAccountRetryAttempt);
					CurrentAccount.TryToCreate(delegate(GripNetwork.Result createResult)
					{
						if (createResult == GripNetwork.Result.Success)
						{
							AccountLoginComplete = true;
							CurrentAccount.Save();
						}
						else
						{
							TestForCurrentAccountLogin();
						}
					});
				}
				else
				{
					CurrentAccount = account;
					TestForCurrentAccountLogin();
				}
			});
		}
		else
		{
			TestForCurrentAccountLogin();
		}
	}

	private IEnumerator DoSaveCloud()
	{
		yield return null;
		string targetName = "cloud";
		iCloudSaveTarget target2 = binarySave.GetTarget(targetName) as iCloudSaveTarget;
		if (target2 == null)
		{
			target2 = binarySave.AddTarget<iCloudSaveTarget>(targetName);
			target2.UseBackup = true;
		}
		binarySave.Save(targetName);
		doLoadCloud = true;
	}

	private IEnumerator DoLoadCloud()
	{
		string targetName = "cloud";
		iCloudSaveTarget target2 = binarySave.GetTarget(targetName) as iCloudSaveTarget;
		if (target2 == null)
		{
			target2 = binarySave.AddTarget<iCloudSaveTarget>(targetName);
			target2.UseBackup = true;
		}
		bool loadComplete = false;
		binarySave.Load(targetName, delegate
		{
			loadComplete = true;
		});
		while (!loadComplete)
		{
			yield return null;
		}
	}

	public bool TestForCurrentAccountLogin()
	{
		if (CurrentAccount != null && CurrentAccount.Status != GripAccount.LoginStatus.Complete)
		{
			if ((CurrentAccount.Status == GripAccount.LoginStatus.LoggedOut || CurrentAccount.Status == GripAccount.LoginStatus.Failed) && !ConnectionAlertIsShown)
			{
				CurrentAccount.TryToLogin(delegate(GripNetwork.Result result)
				{
					switch (result)
					{
					case GripNetwork.Result.Success:
						AccountLoginComplete = true;
						CurrentAccount.Save();
						break;
					case GripNetwork.Result.InUse:
						CurrentAccount = GripAccount.CreateNew(ref createAccountRetryAttempt);
						TestForCurrentAccountLogin();
						break;
					default:
						if (!ConnectionAlertIsShown && ShowConnectionErrors)
						{
							ConnectionAlertIsShown = true;
							NUF.PopUpAlert(StringUtils.GetStringFromStringRef("LocalizedStrings", "Code_ServerUnavailable"), StringUtils.GetStringFromStringRef("LocalizedStrings", "Code_ServerUnavailableTryLater"), StringUtils.GetStringFromStringRef("LocalizedStrings", "Code_Retry"), null, delegate
							{
								ConnectionAlertIsShown = false;
							});
						}
						break;
					}
				});
			}
			return false;
		}
		return true;
	}
}
