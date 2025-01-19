using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Glu.Plugins.ASocial;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class MultiplayerData : SaveProvider
{
	public class FriendData
	{
		public int gamespyID;

		public string gameCenterID;

		public string facebookID;

		public string friendName;

		public bool isGameCenterFriend;

		public bool isFacebookFriend;
	}

	public class MultiplayerUserDataQueryResponse
	{
		public int userID;

		public string userName;

		public string pushToken;

		public string facebookId;

		public int attackRating;

		public int defensiveBuff1;

		public int defensiveBuff2;

		public int[] ratingCategory = new int[14];

		public MultiplayerProfileLoadout loadout;

		public string language;
	}

	public const string kUserDataTarget = "UserData";

	public const string kUserNameField = "username";

	public const string kGameCenterIDField = "gamecenterid";

	public const string kFacebookIDField = "facebookid";

	public const string kAndroidIDField = "androidid";

	private const string kPushIDsField = "pushIDs";

	private const string kLoginTimeField = "loginTime";

	private const string kSeededField = "seeded";

	private const string kDefensiveBuff1Field = "defenseBuff1";

	private const string kDefensiveBuff2Field = "defenseBuff2";

	private const string kLoadoutField = "loadout";

	public const string kAttackRatingField = "attackRating";

	public const string kLanguageField = "language";

	public const string kDefensiveBuff1SetID = "Enemy";

	public const string kDefensiveBuff2SetID = "Amulet";

	public const int kDefensiveBuffCount = 2;

	public const string kCollectibleLootPrefix = "Collectible=";

	public const int kUserID_IsAI = 0;

	public const string kOpponentAITable = "MultiplayerAIOpponentData";

	public const int kOpponentsToAdd = 3;

	public const int kOpponentsToShowAtOneTime = 30;

	public const string kSetCollectedSavePrefix = "CollectionSetCount_";

	public const string kSetDummyRewardSavePrefix = "CollectionSetDummyReward_";

	public const string kPlayerCollectionTable = "PlayerCollection";

	public static string FacebookLink = "https://facebook.com/SamuraiVsZombies";

	public static string FacebookImageLink;

	private GripNetworkSaveTarget userDataSave;

	public readonly string[] kUserDataFields = new string[8] { "username", "pushIDs", "attackRating", "defenseBuff1", "defenseBuff2", "loadout", "language", "facebookid" };

	public readonly string[] kPlayerCollectionFields = new string[13]
	{
		"recordid", "ownerid", "collectionID", "attackerID", "attackTime", "attackData", "defenseBuff1", "defenseBuff2", "username", "version",
		"attackRating", "JailBroken", "ShieldTime"
	};

	private DataBundleTableHandle<CollectionSchema> collectionDataHandle;

	private List<CollectionSchema> collectionData;

	private MultiplayerCollectionStatus collectionStatus = new MultiplayerCollectionStatus();

	private MultiplayerWaveData multiplayerGameSessionData;

	private List<FriendData> mFriendIDs = new List<FriendData>();

	private MultiplayerUserDataQueryResponse mCurrentOpponent;

	private MultiplayerProfileLoadout mLocalPlayerLoadout;

	private MultiplayerTweakSchema mTweakValues;

	public static string CollectionsUdamanTableName
	{
		get
		{
			return "Collections";
		}
	}

	public GripAccount Account { get; private set; }

	public string UserName
	{
		get
		{
			return userDataSave.Values["username"].mString;
		}
		set
		{
			if (userDataSave.Values["username"].mString != value)
			{
				userDataSave.Values["username"].mString = value;
				UpdateSingleField("username");
			}
		}
	}

	public string GameCenterID
	{
		get
		{
			return userDataSave.Values["gamecenterid"].mString;
		}
		set
		{
			userDataSave.Values["gamecenterid"].mString = value;
		}
	}

	public string FacebookID
	{
		get
		{
			return userDataSave.Values["facebookid"].mString;
		}
		set
		{
			userDataSave.Values["facebookid"].mString = value;
		}
	}

	public string AndroidID
	{
		get
		{
			return userDataSave.Values["androidid"].mString;
		}
		set
		{
			userDataSave.Values["androidid"].mString = value;
		}
	}

	public string PushNotificationIDs
	{
		get
		{
			return userDataSave.Values["pushIDs"].mString;
		}
		set
		{
			userDataSave.Values["pushIDs"].mString = value;
		}
	}

	public int OwnerID
	{
		get
		{
			return userDataSave.OwnerID;
		}
	}

	public int DefenseBuff1
	{
		get
		{
			return userDataSave.Values["defenseBuff1"].mInt.GetValueOrDefault();
		}
		set
		{
			userDataSave.Values["defenseBuff1"].mInt = value;
		}
	}

	public int DefenseBuff2
	{
		get
		{
			return userDataSave.Values["defenseBuff2"].mInt.GetValueOrDefault();
		}
		set
		{
			userDataSave.Values["defenseBuff2"].mInt = value;
		}
	}

	public int AttackRating
	{
		get
		{
			return userDataSave.Values["attackRating"].mInt.GetValueOrDefault();
		}
		set
		{
			userDataSave.Values["attackRating"].mInt = value;
		}
	}

	public IEnumerable<GripField> AllUserDataFields
	{
		get
		{
			return userDataSave.Values.Values;
		}
	}

	public List<CollectionSchema> CollectionData
	{
		get
		{
			return collectionData;
		}
	}

	public MultiplayerCollectionStatus CollectionStatus
	{
		get
		{
			return collectionStatus;
		}
	}

	public MultiplayerWaveData MultiplayerGameSessionData
	{
		get
		{
			return multiplayerGameSessionData;
		}
		set
		{
			multiplayerGameSessionData = value;
			if (value == null)
			{
				return;
			}
			int soulsToAttack = value.collectionItem_InConflict.soulsToAttack;
			string text = string.Empty;
			int num = 0;
			foreach (string item in DataBundleRuntime.Instance.EnumerateRecordKeys<MultiplayerTweakSchema>("TweakValues"))
			{
				int num2 = int.Parse(item);
				if (num2 == soulsToAttack || (num2 > num && num2 < soulsToAttack))
				{
					num = num2;
					text = item;
				}
			}
			if (num > 0)
			{
				mTweakValues = DataBundleRuntime.Instance.InitializeRecord<MultiplayerTweakSchema>("TweakValues." + text);
			}
			else
			{
				mTweakValues = DataBundleRuntime.Instance.InitializeRecord<MultiplayerTweakSchema>("TweakValues.50");
			}
		}
	}

	public List<FriendData> Friends
	{
		get
		{
			return mFriendIDs;
		}
	}

	public MultiplayerUserDataQueryResponse CurrentOpponent
	{
		get
		{
			return mCurrentOpponent;
		}
		set
		{
			mCurrentOpponent = value;
		}
	}

	public MultiplayerProfileLoadout LocalPlayerLoadout
	{
		get
		{
			return mLocalPlayerLoadout;
		}
	}

	public MultiplayerTweakSchema TweakValues
	{
		get
		{
			return mTweakValues;
		}
		set
		{
			mTweakValues = value;
		}
	}

	public FriendData LastOpponentFriendData { get; set; }

	public static void NetworkRequiredDialog()
	{
		SingletonMonoBehaviour<InputManager>.Instance.InputEnabled = true;
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC", "LocalizedStrings.no_internet_notification_message_text");
		SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("TEXT_POPUP_GENERIC_BTN", "MenuFixedStrings.ok");
		GluiActionSender.SendGluiAction("ALERT_BLOCK_INPUT", null, null);
	}

	public void Init()
	{
		userDataSave = AddTarget<GripNetworkSaveTarget>("UserData");
		userDataSave.Table = "UserData";
		userDataSave.UseBackup = false;
		userDataSave.UseKeyValueSaveData = true;
		userDataSave.UseBinarySaveData = true;
		userDataSave.Name = "profile";
		mLocalPlayerLoadout = new MultiplayerProfileLoadout();
		mLocalPlayerLoadout.UpdateLocalProfile();
		mLocalPlayerLoadout.isDirty = false;
		userDataSave.Values["username"] = new GripField("username", GripField.GripFieldType.UnicodeString);
		userDataSave.Values["gamecenterid"] = new GripField("gamecenterid", GripField.GripFieldType.UnicodeString);
		userDataSave.Values["facebookid"] = new GripField("facebookid", GripField.GripFieldType.UnicodeString);
		userDataSave.Values["androidid"] = new GripField("androidid", GripField.GripFieldType.UnicodeString);
		userDataSave.Values["loginTime"] = new GripField("loginTime", GripField.GripFieldType.DateAndTime);
		userDataSave.Values["pushIDs"] = new GripField("pushIDs", GripField.GripFieldType.UnicodeString);
		userDataSave.Values["seeded"] = new GripField("seeded", GripField.GripFieldType.Int);
		userDataSave.Values["defenseBuff1"] = new GripField("defenseBuff1", GripField.GripFieldType.Int);
		userDataSave.Values["defenseBuff2"] = new GripField("defenseBuff2", GripField.GripFieldType.Int);
		userDataSave.Values["loadout"] = new GripField("loadout", GripField.GripFieldType.BinaryData);
		userDataSave.Values["attackRating"] = new GripField("attackRating", GripField.GripFieldType.Int);
		userDataSave.Values["language"] = new GripField("language", GripField.GripFieldType.UnicodeString);
		base.AutoSaveEnabled = false;
		base.RequireCRCMatch = false;
		base.SaveOnExit = false;
		base.Header.UseDeviceData = false;
		base.Header.UseEncoding = false;
		InitDefaults();
		collectionDataHandle = new DataBundleTableHandle<CollectionSchema>(CollectionsUdamanTableName);
		collectionData = new List<CollectionSchema>(collectionDataHandle.Data);
		foreach (CollectionSchema collectionDatum in collectionData)
		{
			collectionDatum.Initialize();
		}
		FacebookImageLink = ConfigSchema.Entry("FacebookImageURL");
	}

	private void InitDefaults()
	{
		UserName = string.Empty;
		GameCenterID = ((Social.localUser == null) ? string.Empty : Social.localUser.id);
		FacebookID = string.Empty;
		AndroidID = string.Empty;
		PushNotificationIDs = string.Empty;
		userDataSave.Values["loginTime"].mDateAndTime = SntpTime.UniversalTime;
		userDataSave.Values["defenseBuff1"].mInt = 0;
		userDataSave.Values["defenseBuff2"].mInt = 0;
		userDataSave.Values["seeded"].mInt = 0;
		userDataSave.Values["loadout"].mBinaryData = mLocalPlayerLoadout.Pack();
		userDataSave.Values["attackRating"].mInt = 0;
		userDataSave.Values["language"].mString = string.Empty;
	}

	public void LoadFrontEndResourceData()
	{
		collectionDataHandle.Load(DataBundleResourceGroup.FrontEnd, true, null);
	}

	public void UpdateCollection()
	{
		RetrieveMyCollection(delegate
		{
		});
		if (WeakGlobalMonoBehavior<InGameImpl>.Instance != null)
		{
			GluiActionSender.SendGluiAction("POPUP_PAUSEMENU", null, null);
		}
	}

	public void UnloadResourceData()
	{
		collectionDataHandle.Unload();
	}

	public void LoadUserData(Action<Result> onComplete)
	{
		Load("UserData", delegate(Result result)
		{
			if (onComplete != null)
			{
				onComplete(result);
			}
			userDataSave.Values["loginTime"].mDateAndTime = SntpTime.UniversalTime;
		});
	}

	public void Logout()
	{
		GripNetwork.Logout();
		Account = null;
	}

	public void Login(Action onComplete, bool allowCreateNewAccount)
	{
		if (Account == null)
		{
			GripAccount.Login(allowCreateNewAccount, delegate(GripAccount account, int retryAttempts)
			{
				Account = account;
				if (Account.Status == GripAccount.LoginStatus.Complete)
				{
					OnLogin(onComplete);
				}
				else if (onComplete != null)
				{
					onComplete();
				}
			});
		}
		else if (Account.Status == GripAccount.LoginStatus.LoggedOut || Account.Status == GripAccount.LoginStatus.Failed)
		{
			Account.TryToLogin(delegate(GripNetwork.Result result)
			{
				if (result == GripNetwork.Result.Success)
				{
					OnLogin(onComplete);
				}
				else if (onComplete != null)
				{
					onComplete();
				}
			});
		}
		else if (onComplete != null)
		{
			onComplete();
		}
	}

	private void OnLogin(Action onComplete)
	{
		LoadUserData(delegate(Result loadResult)
		{
			if (loadResult == Result.Failure)
			{
				onComplete();
			}
			else
			{
				GenericUtils.InvokeInParallel(onComplete, delegate(Action findFriendsComplete)
				{
					if (Social.localUser.authenticated && Social.localUser.friends.GetLength(0) > 0)
					{
						string text = "gamecenterid IN (";
						int num = 0;
						IUserProfile[] friends = Social.localUser.friends;
						foreach (IUserProfile userProfile in friends)
						{
							if (!string.IsNullOrEmpty(userProfile.id))
							{
								if (num > 0)
								{
									text += ", ";
								}
								text = text + "'" + userProfile.id.ToString() + "'";
								num++;
							}
						}
						text += ")";
						string[] fieldNames = new string[3] { "ownerid", "gamecenterid", "facebookid" };
						GripNetwork.SearchRecords("UserData", fieldNames, text, string.Empty, null, num, 1, delegate(GripNetwork.Result result, GripField[,] data)
						{
							if (result == GripNetwork.Result.Success && data != null)
							{
								for (int j = 0; j < data.GetLength(0); j++)
								{
									if (data[j, 0].mInt.HasValue)
									{
										int valueOrDefault = data[j, 0].mInt.GetValueOrDefault();
										string mString = data[j, 1].mString;
										string mString2 = data[j, 2].mString;
										string friendName = string.Empty;
										IUserProfile[] friends2 = Social.localUser.friends;
										foreach (IUserProfile userProfile2 in friends2)
										{
											if (userProfile2.id == mString)
											{
												friendName = userProfile2.userName;
												break;
											}
										}
										AddFriend(valueOrDefault, mString, mString2, friendName, false);
									}
								}
							}
							findFriendsComplete();
						});
					}
					else
					{
						findFriendsComplete();
					}
				}, delegate(Action savePushTokenComplete)
				{
					string deviceToken = NUF.GetDeviceToken();
					List<GripField> list = new List<GripField>();
					if (!string.IsNullOrEmpty(deviceToken) && deviceToken != PushNotificationIDs)
					{
						PushNotificationIDs = deviceToken;
						list.Add(userDataSave.Values["pushIDs"]);
					}
					if (Social.localUser.authenticated && GameCenterID != Social.localUser.id)
					{
						GameCenterID = Social.localUser.id;
						list.Add(userDataSave.Values["gamecenterid"]);
					}
					if (BundleUtils.GetSystemLanguage() != userDataSave.Values["language"].mString)
					{
						userDataSave.Values["language"].mString = BundleUtils.GetSystemLanguage();
						list.Add(userDataSave.Values["language"]);
					}
					PushNotification.RegisterCallback(UpdateCollection);
					if (list.Count > 0)
					{
						GripNetwork.UpdateRecord("UserData", userDataSave.RecordID, list.ToArray(), delegate
						{
							savePushTokenComplete();
						});
					}
					else
					{
						savePushTokenComplete();
					}
				});
			}
		});
	}

	public CollectionSchema GetCollectionSetData(string setID)
	{
		return CollectionData.Find((CollectionSchema record) => record.id == setID);
	}

	public CollectionItemSchema GetCollectionItemData(int collectionID, out CollectionSchema collectionSet)
	{
		CollectionItemSchema result = null;
		CollectionSchema resultSet = null;
		collectionSet = null;
		collectionData.ForEach(delegate(CollectionSchema record)
		{
			for (int i = 0; i < record.Items.Length; i++)
			{
				if (record.Items[i].CollectionID == collectionID)
				{
					result = record.Items[i];
					resultSet = record;
					break;
				}
			}
		});
		collectionSet = resultSet;
		return result;
	}

	protected override bool Write(Stream dataStream, IEnumerable<SaveTarget> targets, DeviceData deviceData)
	{
		return false;
	}

	protected override bool Read(SaveHeader dataHeader, Stream dataStream, SaveTarget target, DeviceData deviceData)
	{
		return true;
	}

	public void SetFacebookID(string id)
	{
		if (!(FacebookID != id))
		{
			return;
		}
		FacebookID = id;
		List<GripField> list = new List<GripField>();
		list.Add(userDataSave.Values["facebookid"]);
		GripNetwork.UpdateRecord("UserData", userDataSave.RecordID, list.ToArray(), delegate(GripNetwork.Result result)
		{
			if (result != 0)
			{
				PlayerPrefs.SetInt("DELAY_SET_FACEBOOK_ID", 1);
				SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.isDelaySetFacebookID = true;
			}
		});
	}

	public void SetAndroidID(string id)
	{
		if (!(AndroidID != id))
		{
			return;
		}
		AndroidID = id;
		List<GripField> list = new List<GripField>();
		list.Add(userDataSave.Values["androidid"]);
		GripNetwork.UpdateRecord("UserData", userDataSave.RecordID, list.ToArray(), delegate(GripNetwork.Result result)
		{
			if (result != 0)
			{
			}
		});
	}

	public void DelayedGamespyUpdate()
	{
		List<GripField> list = new List<GripField>();
		list.Add(userDataSave.Values["facebookid"]);
		GripNetwork.UpdateRecord("UserData", userDataSave.RecordID, list.ToArray(), delegate(GripNetwork.Result result)
		{
			if (result != 0)
			{
			}
		});
	}

	public void GetOtherUserData(int userID, Action<MultiplayerUserDataQueryResponse> onComplete)
	{
		if (Singleton<Profile>.Instance.MultiplayerData.Account == null || Singleton<Profile>.Instance.MultiplayerData.Account.Status != GripAccount.LoginStatus.Complete)
		{
			if (onComplete != null)
			{
				onComplete(null);
			}
			return;
		}
		if (userID == 0 || onComplete == null)
		{
			if (onComplete != null)
			{
				onComplete(null);
			}
			return;
		}
		string sqlStyleFilter = string.Format("ownerid={0}", userID);
		GripNetwork.SearchRecords("UserData", Singleton<Profile>.Instance.MultiplayerData.kUserDataFields, sqlStyleFilter, null, null, 0, 0, delegate(GripNetwork.Result result, GripField[,] fields)
		{
			if (result != 0)
			{
				if (onComplete != null)
				{
					onComplete(null);
				}
			}
			else
			{
				MultiplayerUserDataQueryResponse multiplayerUserDataQueryResponse = new MultiplayerUserDataQueryResponse
				{
					userID = userID
				};
				if (fields != null)
				{
					multiplayerUserDataQueryResponse.userName = fields[0, 0].mString;
					multiplayerUserDataQueryResponse.pushToken = fields[0, 1].mString;
					multiplayerUserDataQueryResponse.attackRating = fields[0, 2].mInt.GetValueOrDefault();
					multiplayerUserDataQueryResponse.defensiveBuff1 = fields[0, 3].mInt.GetValueOrDefault();
					multiplayerUserDataQueryResponse.defensiveBuff2 = fields[0, 4].mInt.GetValueOrDefault();
					multiplayerUserDataQueryResponse.loadout = new MultiplayerProfileLoadout(fields[0, 5].mBinaryData);
					multiplayerUserDataQueryResponse.loadout.playerName = multiplayerUserDataQueryResponse.userName;
					multiplayerUserDataQueryResponse.language = fields[0, 6].mString;
					multiplayerUserDataQueryResponse.facebookId = fields[0, 7].mString;
				}
				onComplete(multiplayerUserDataQueryResponse);
			}
		});
	}

	public void GetOwnedItem(int ownerId, int collectionID, Action<CollectionStatusRecord> onDone)
	{
		CollectionStatusRecord record = null;
		string sqlStyleFilter = string.Format("collectionID={0} AND ownerId={1} AND attackerID={2} AND version={3}", collectionID, ownerId, 0, CollectionStatusRecord.kCollectionVersion);
		GripNetwork.SearchRecords("PlayerCollection", Singleton<Profile>.Instance.MultiplayerData.kPlayerCollectionFields, sqlStyleFilter, null, null, 1, 0, delegate(GripNetwork.Result result, GripField[,] fields)
		{
			if (fields != null && fields.GetLength(0) > 0)
			{
				record = CollectionStatusRecord.FromFields(fields, 0);
			}
			onDone(record);
		});
	}

	public void RemoveCollectionItem(CollectionStatusRecord record)
	{
		collectionStatus.Remove(record);
	}

	public void ReportChallengeSuccess(CollectionStatusRecord attackedRecord)
	{
		if (Account == null || Account.Status != GripAccount.LoginStatus.Complete || attackedRecord == null)
		{
			return;
		}
		attackedRecord.AttackTime = SntpTime.UniversalTime;
		attackedRecord.AttackerID = userDataSave.OwnerID;
		attackedRecord.AttackData = Singleton<PlayerWaveEventData>.Instance.Pack();
		string[] fieldNames = new string[1] { "attackerID" };
		string sqlQuery = "recordid = '" + attackedRecord.RecordID + "'";
		int[] profileIds = new int[1] { attackedRecord.OwnerID };
		GripNetwork.SearchRecords("PlayerCollection", fieldNames, sqlQuery, null, profileIds, 1, 1, delegate(GripNetwork.Result result, GripField[,] data)
		{
			if (result == GripNetwork.Result.Success && data != null && data.GetLength(0) > 0 && data[0, 0].mInt.GetValueOrDefault(0) == 0)
			{
				GripField[] fields = new GripField[3]
				{
					attackedRecord.Fields[2],
					attackedRecord.Fields[1],
					attackedRecord.Fields[3]
				};
				GripNetwork.UpdateRecord("PlayerCollection", attackedRecord.RecordID, fields, delegate
				{
					if (mCurrentOpponent != null && !string.IsNullOrEmpty(mCurrentOpponent.pushToken))
					{
						sqlQuery = string.Format("ownerid = {0} AND attackerID > 0", attackedRecord.OwnerID);
						GripNetwork.CountRecords("PlayerCollection", sqlQuery, delegate(GripNetwork.Result countResult, int count)
						{
							if (countResult != 0)
							{
								count = 1;
							}
							string text2;
							if (attackedRecord.IsGameCenterFriend || attackedRecord.IsFacebookFriend)
							{
								string text = Glu.Plugins.ASocial.Facebook.GetName();
								if (string.IsNullOrEmpty(text))
								{
									text = Social.localUser.userName;
								}
								if (!string.IsNullOrEmpty(text))
								{
									string stringRef = "notification_attackFriend_" + mCurrentOpponent.language;
									text2 = string.Format(StringUtils.GetStringFromStringRef("PushNotifications", stringRef), text);
								}
								else
								{
									string stringRef2 = "notification_attack_" + mCurrentOpponent.language;
									text2 = StringUtils.GetStringFromStringRef("PushNotifications", stringRef2);
								}
							}
							else
							{
								string stringRef3 = "notification_attack_" + mCurrentOpponent.language;
								text2 = StringUtils.GetStringFromStringRef("PushNotifications", stringRef3);
							}
							if (string.IsNullOrEmpty(text2))
							{
								text2 = StringUtils.GetStringFromStringRef("PushNotifications", "notification_attack_English");
							}
							PushNotification.SendPushNotification(mCurrentOpponent.pushToken, text2, count, "SvZ2_Custom_Notification.aif");
						});
					}
				});
				if (mCurrentOpponent != null && !string.IsNullOrEmpty(mCurrentOpponent.facebookId))
				{
					foreach (FriendData mFriendID in mFriendIDs)
					{
						if (mFriendID.facebookID == mCurrentOpponent.facebookId)
						{
							LastOpponentFriendData = mFriendID;
							break;
						}
					}
				}
			}
		});
	}

	public void ReportDefenseSuccess(CollectionStatusRecord attackedRecord)
	{
		if (Account != null && Account.Status == GripAccount.LoginStatus.Complete && attackedRecord != null)
		{
			attackedRecord.AttackTime = SntpTime.UniversalTime;
			attackedRecord.AttackerID = 0;
			attackedRecord.AttackData = Singleton<PlayerWaveEventData>.Instance.Pack();
			GripField[] fields = new GripField[3]
			{
				attackedRecord.Fields[2],
				attackedRecord.Fields[1],
				attackedRecord.Fields[3]
			};
			GripNetwork.UpdateRecord("PlayerCollection", attackedRecord.RecordID, fields, delegate
			{
				GluiActionSender.SendGluiAction("QUERY_HAS_COLLECTIBLE_CONFLICT", null, null);
			});
		}
	}

	public IEnumerator SeedRandomCollectionIfNeeded(Action<bool> onDone)
	{
		if (Account == null || Account.Status != GripAccount.LoginStatus.Complete)
		{
			if (onDone != null)
			{
				onDone(false);
			}
			yield break;
		}
		int? mInt = userDataSave.Values["seeded"].mInt;
		if (mInt.GetValueOrDefault() != 0 || !mInt.HasValue)
		{
			if (onDone != null)
			{
				onDone(false);
			}
			yield break;
		}
		if (userDataSave.OwnerID == -1)
		{
			Save();
			while (userDataSave.OwnerID == -1)
			{
				yield return null;
			}
		}
		CollectionStarterSchema record = CollectionStarterSchema.GetRandomRecord("StarterItems");
		int setIndex3 = CollectionSchema.ToIndex(record.item1Set.Table, record.item1Set.Key);
		AddCollectionItem(setIndex3, record.item1Index);
		setIndex3 = CollectionSchema.ToIndex(record.item2Set.Table, record.item2Set.Key);
		AddCollectionItem(setIndex3, record.item2Index);
		setIndex3 = CollectionSchema.ToIndex(record.item3Set.Table, record.item3Set.Key);
		AddCollectionItem(setIndex3, record.item3Index);
		userDataSave.Values["seeded"].mInt = 1;
		Save();
		if (onDone != null)
		{
			onDone(true);
		}
	}

	public void AddCollectionItem(int collectionIndex, int itemIndex)
	{
		collectionStatus.Add_ItemOwned(collectionData[collectionIndex].Items[itemIndex], userDataSave.OwnerID);
	}

	public void RetrieveMyCollection(Action<GripNetwork.Result, bool> onComplete)
	{
		if (Account == null || Account.Status != GripAccount.LoginStatus.Complete)
		{
			if (onComplete != null)
			{
				onComplete(GripNetwork.Result.Failed, false);
			}
			return;
		}
		GripNetwork.GetMyRecords("PlayerCollection", kPlayerCollectionFields, delegate(GripNetwork.Result result, GripField[,] fields)
		{
			bool arg = false;
			if (result == GripNetwork.Result.Success && fields != null)
			{
				collectionStatus.Clear();
				bool flag = false;
				sbyte b = (sbyte)(Integrity.IsJailbroken() ? 1 : 0);
				mLocalPlayerLoadout.UpdateLocalProfile();
				for (int i = 0; i < fields.GetLength(0); i++)
				{
					CollectionStatusRecord collectionStatusRecord = CollectionStatusRecord.FromFields(fields, i);
					if (collectionStatus.FindItem(collectionStatusRecord.CollectionID) != null)
					{
						collectionStatus.Remove(collectionStatusRecord);
					}
					else
					{
						collectionStatus.Add(collectionStatusRecord);
						List<GripField> list = new List<GripField>();
						if (!string.IsNullOrEmpty(UserName) && collectionStatusRecord.UserName != UserName)
						{
							collectionStatusRecord.UserName = UserName;
							list.Add(collectionStatusRecord.Fields[6]);
						}
						if (collectionStatusRecord.DefenseBuff1 != DefenseBuff1)
						{
							collectionStatusRecord.DefenseBuff1 = DefenseBuff1;
							list.Add(collectionStatusRecord.Fields[4]);
						}
						if (collectionStatusRecord.DefenseBuff2 != DefenseBuff2)
						{
							collectionStatusRecord.DefenseBuff2 = DefenseBuff2;
							list.Add(collectionStatusRecord.Fields[5]);
						}
						if (collectionStatusRecord.Version != CollectionStatusRecord.kCollectionVersion)
						{
							flag = true;
							collectionStatusRecord.Version = CollectionStatusRecord.kCollectionVersion;
							list.Add(collectionStatusRecord.Fields[7]);
						}
						if (collectionStatusRecord.AttackRating != mLocalPlayerLoadout.defenseRating)
						{
							collectionStatusRecord.AttackRating = mLocalPlayerLoadout.defenseRating;
							list.Add(collectionStatusRecord.Fields[8]);
						}
						if (collectionStatusRecord.JailBroken != b)
						{
							collectionStatusRecord.JailBroken = b;
							list.Add(collectionStatusRecord.Fields[9]);
						}
						if (collectionStatusRecord.ShieldTime != Singleton<Profile>.Instance.MultiplayerShieldExpireTime)
						{
							collectionStatusRecord.ShieldTime = Singleton<Profile>.Instance.MultiplayerShieldExpireTime;
							list.Add(collectionStatusRecord.Fields[10]);
						}
						if (list.Count > 0)
						{
							GripNetwork.UpdateRecord("PlayerCollection", collectionStatusRecord.RecordID, list.ToArray(), delegate
							{
							});
						}
					}
				}
				if (flag)
				{
					mLocalPlayerLoadout.isDirty = true;
					UpdateLoadout();
				}
				int valueInt = Singleton<Profile>.Instance.saveData.GetValueInt("CollectionItemToAdd");
				if (valueInt > 0)
				{
					valueInt--;
					CollectionSchema collectionSet;
					CollectionItemSchema collectionItemData = GetCollectionItemData(valueInt, out collectionSet);
					MultiplayerCollectionStatusQueryResponse multiplayerCollectionStatusQueryResponse = collectionStatus.Find(valueInt);
					if (collectionItemData != null && multiplayerCollectionStatusQueryResponse.records.Count == 0)
					{
						collectionStatus.Add_ItemOwned(collectionItemData, userDataSave.OwnerID);
					}
					else
					{
						Singleton<Profile>.Instance.saveData.SetValueInt("CollectionItemToAdd", 0);
					}
				}
				arg = AwardAnyCompletedSets();
			}
			else
			{
				Logout();
			}
			CollectionStatus.CheckForUndefendedItems();
			CollectionStatus.Clamp_AttackedItemTimes();
			CollectionStatus.ClearQueryCache();
			GluiActionSender.SendGluiAction("QUERY_HAS_COLLECTIBLE_CONFLICT", null, null);
			if (onComplete != null)
			{
				onComplete(result, arg);
			}
		});
	}

	private bool AwardAnyCompletedSets()
	{
		bool result = false;
		foreach (CollectionSchema collectionDatum in collectionData)
		{
			bool flag = true;
			CollectionItemSchema[] items = collectionDatum.Items;
			foreach (CollectionItemSchema collectionItemSchema in items)
			{
				MultiplayerCollectionStatusQueryResponse multiplayerCollectionStatusQueryResponse = CollectionStatus.Find(collectionItemSchema.CollectionID);
				MultiplayerCollectionStatusQueryResponse.MultiplayerCollectionStatusAggregate aggregate = null;
				if (multiplayerCollectionStatusQueryResponse != null)
				{
					multiplayerCollectionStatusQueryResponse.Aggregate(Singleton<Profile>.Instance.MultiplayerData.OwnerID, out aggregate);
				}
				if (multiplayerCollectionStatusQueryResponse == null || multiplayerCollectionStatusQueryResponse.records.Count == 0 || aggregate.attackerCount > 0)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				Singleton<Profile>.Instance.MultiplayerData.ClearSetItems(collectionDatum);
				Singleton<Profile>.Instance.MultiplayerData.AddCompletedSet(collectionDatum.id, 1);
				Singleton<Profile>.Instance.Save();
				result = true;
			}
		}
		return result;
	}

	public void ClearSetItems(CollectionSchema setData)
	{
		setData.Items.ForEachWithIndex(delegate(CollectionItemSchema item, int index)
		{
			CollectionStatus.Remove(item);
		});
	}

	public int CollectionLevel(string setName)
	{
		int b = TotalTimesCompletedSet(setName);
		return Mathf.Min(3, b);
	}

	public int TotalTimesCompletedSet(string setName)
	{
		return Singleton<Profile>.Instance.saveData.GetValueInt("CollectionSetCount_" + setName);
	}

	public int TotalCollectionItemsOwned()
	{
		if (CollectionStatus != null && CollectionStatus.Collection != null)
		{
			return CollectionStatus.Collection.Count;
		}
		return 0;
	}

	public int TotalCompleteCollections()
	{
		int num = 0;
		if (collectionData != null)
		{
			foreach (CollectionSchema collectionDatum in collectionData)
			{
				num += TotalTimesCompletedSet(collectionDatum.id);
			}
		}
		return num;
	}

	public void AddCompletedSet(string setName, int countToAdd)
	{
		int num = TotalTimesCompletedSet(setName);
		CollectionSchema collectionSetData = GetCollectionSetData(setName);
		CollectionDummyRewardsSchema xtraReward = collectionSetData.GetXtraReward(num);
		if (xtraReward != null)
		{
			CashIn.From(xtraReward.rewardSaveInt, xtraReward.rewardAmount, "CollectionSetComplete");
		}
		num += countToAdd;
		string recordTable = collectionSetData.dummyRewards.RecordTable;
		if (num < 0)
		{
			num = 0;
		}
		if (num > 3)
		{
			CollectionDummyRewardsSchema dummyReward = GetDummyReward(recordTable, setName);
			CashIn.From(dummyReward.rewardSaveInt, dummyReward.rewardAmount, "CollectionSetComplete");
			GluiElement_CollectionSet.LastDummyAwarded = dummyReward;
		}
		if (num >= 3)
		{
			if (num == 3)
			{
				Singleton<Achievements>.Instance.IncrementAchievement("CompleteCollection3x", 1);
			}
			Singleton<Profile>.Instance.saveData.SetValue("CollectionSetDummyReward_" + setName, CollectionDummyRewardsSchema.GetRandomRecord(recordTable).id);
		}
		Singleton<Profile>.Instance.saveData.SetValueInt("CollectionSetCount_" + setName, num);
		UpdateUserDefensiveBuffs();
		Singleton<Achievements>.Instance.IncrementAchievement("CompleteCollection", 1);
		if (num == 1)
		{
			Singleton<Achievements>.Instance.IncrementAchievement("CompleteAllArtifacts", 1);
		}
	}

	public CollectionDummyRewardsSchema GetDummyReward(string rewardTable, string setName)
	{
		string value = Singleton<Profile>.Instance.saveData.GetValue("CollectionSetDummyReward_" + setName);
		return CollectionDummyRewardsSchema.GetRecord(rewardTable, value);
	}

	public void UpdateShieldTime()
	{
		foreach (CollectionStatusRecord item in collectionStatus.Collection)
		{
			item.ShieldTime = Singleton<Profile>.Instance.MultiplayerShieldExpireTime;
			GripField[] fields = new GripField[1] { item.Fields[10] };
			GripNetwork.UpdateRecord("PlayerCollection", item.RecordID, fields, delegate
			{
			});
		}
	}

	public void UpdateUserDefensiveBuffs()
	{
		bool flag = false;
		int num = CollectionLevel("Enemy");
		if (num != userDataSave.Values["defenseBuff1"].mInt)
		{
			userDataSave.Values["defenseBuff1"].mInt = num;
			flag = true;
		}
		num = CollectionLevel("Amulet");
		if (num != userDataSave.Values["defenseBuff2"].mInt)
		{
			userDataSave.Values["defenseBuff2"].mInt = num;
			flag = true;
		}
		if (flag)
		{
			foreach (CollectionStatusRecord item in collectionStatus.Collection)
			{
				item.DefenseBuff1 = userDataSave.Values["defenseBuff1"].mInt.Value;
				item.DefenseBuff2 = userDataSave.Values["defenseBuff2"].mInt.Value;
				GripField[] fields = new GripField[2]
				{
					item.Fields[4],
					item.Fields[5]
				};
				GripNetwork.UpdateRecord("PlayerCollection", item.RecordID, fields, delegate
				{
				});
			}
		}
		Save();
	}

	public bool IsMultiplayerGameSessionActive()
	{
		return MultiplayerGameSessionData != null;
	}

	public void ClearMultiplayerGameSession()
	{
		Singleton<PlayerWaveEventData>.Instance.Reset();
		multiplayerGameSessionData = null;
	}

	public void StartMultiplayerGameSession()
	{
		Singleton<Profile>.Instance.souls -= MultiplayerGameSessionData.potentialConflictForAttack.SoulCostToAttack;
		Singleton<Profile>.Instance.FlurrySession.MP_SoulsUsed += MultiplayerGameSessionData.potentialConflictForAttack.SoulCostToAttack;
		Singleton<Profile>.Instance.Save();
	}

	public static int AmuletExtraCost(int amuletLevel)
	{
		switch (amuletLevel)
		{
		case 1:
			return 25;
		case 2:
			return 50;
		case 3:
			return 100;
		default:
			return 0;
		}
	}

	public void FinishMultiplayerGameSession(bool success)
	{
		if (MultiplayerGameSessionData == null)
		{
			return;
		}
		LastOpponentFriendData = null;
		if (MultiplayerGameSessionData.gameMode != EMultiplayerMode.kDefending)
		{
			Singleton<Profile>.Instance.FlurrySession.MP_AttacksPlayed++;
			if (success)
			{
				Singleton<Profile>.Instance.multiplayerWinRating += 10;
				Singleton<Profile>.Instance.FlurrySession.MP_AttacksWon++;
				UpdateMPLeaderboard();
				ReportChallengeSuccess(MultiplayerGameSessionData.potentialConflictForAttack);
				if (SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariable("LegendaryStrikesHelpDefendObjects", 1) > 0)
				{
					collectionStatus.Add_ItemOwned(MultiplayerGameSessionData.collectionItem_InConflict, userDataSave.OwnerID, Singleton<PlayerWaveEventData>.Instance.Pack());
				}
				else
				{
					collectionStatus.Add_ItemOwned(MultiplayerGameSessionData.collectionItem_InConflict, userDataSave.OwnerID);
				}
			}
			else
			{
				Singleton<Profile>.Instance.multiplayerLossRating += 5;
				MultiplayerCollectionStatusQueryResponse multiplayerCollectionStatusQueryResponse = collectionStatus.Find(MultiplayerGameSessionData.collectionItem_InConflict.CollectionID);
				multiplayerCollectionStatusQueryResponse.records.ForEach(delegate(CollectionStatusRecord record)
				{
					CollectionStatus.Remove(record);
				});
			}
			return;
		}
		MultiplayerCollectionStatusQueryResponse multiplayerCollectionStatusQueryResponse2 = collectionStatus.Find(MultiplayerGameSessionData.collectionItem_InConflict.CollectionID);
		Singleton<Profile>.Instance.FlurrySession.MP_DefensesPlayed++;
		if (success)
		{
			Singleton<Profile>.Instance.multiplayerWinRating += 10;
			Singleton<Profile>.Instance.FlurrySession.MP_DefensesWon++;
			UpdateMPLeaderboard();
			multiplayerCollectionStatusQueryResponse2.records.ForEach(delegate(CollectionStatusRecord record)
			{
				ReportDefenseSuccess(record);
			});
			Singleton<Achievements>.Instance.IncrementAchievement("DefendArtifact1", 1);
			Singleton<Achievements>.Instance.IncrementAchievement("DefendArtifact2", 1);
		}
		else
		{
			Singleton<Profile>.Instance.multiplayerLossRating += 5;
			multiplayerCollectionStatusQueryResponse2.records.ForEach(delegate(CollectionStatusRecord record)
			{
				CollectionStatus.Remove(record);
			});
		}
	}

	private void UpdateMPLeaderboard()
	{
		if (!AJavaTools.Properties.IsBuildAmazon() && PlayGameServices.IsSignedIn)
		{
			int score = Singleton<Profile>.Instance.multiplayerWinRating / 10;
			PlayGameServices.SubmitScore(Profile.kMultiplayerLeaderboard, score);
		}
	}

	public void UpdateLoadout()
	{
		mLocalPlayerLoadout.UpdateLocalProfile();
		if (!mLocalPlayerLoadout.isDirty)
		{
			return;
		}
		userDataSave.Values["loadout"].mType = GripField.GripFieldType.BinaryData;
		userDataSave.Values["loadout"].mBinaryData = mLocalPlayerLoadout.Pack();
		bool ratingChanged = AttackRating != mLocalPlayerLoadout.defenseRating;
		AttackRating = mLocalPlayerLoadout.defenseRating;
		List<GripField> list = new List<GripField>();
		list.Add(userDataSave.Values["loadout"]);
		list.Add(userDataSave.Values["attackRating"]);
		GripNetwork.UpdateRecord("UserData", userDataSave.RecordID, list.ToArray(), delegate(GripNetwork.Result result)
		{
			if (result == GripNetwork.Result.Success)
			{
				mLocalPlayerLoadout.isDirty = false;
			}
			if (ratingChanged)
			{
				foreach (CollectionStatusRecord item in collectionStatus.Collection)
				{
					item.AttackRating = mLocalPlayerLoadout.defenseRating;
					GripField[] fields = new GripField[1] { item.Fields[8] };
					GripNetwork.UpdateRecord("PlayerCollection", item.RecordID, fields, delegate
					{
					});
				}
			}
		});
	}

	public void UpdateSingleField(string field)
	{
		GripField[] fields = new GripField[1] { userDataSave.Values[field] };
		GripNetwork.UpdateRecord("UserData", userDataSave.RecordID, fields, delegate(GripNetwork.Result result)
		{
			if (result == GripNetwork.Result.Success)
			{
			}
			if (field == "username")
			{
				foreach (CollectionStatusRecord item in collectionStatus.Collection)
				{
					item.UserName = UserName;
					GripField[] fields2 = new GripField[1] { item.Fields[6] };
					GripNetwork.UpdateRecord("PlayerCollection", item.RecordID, fields2, delegate
					{
					});
				}
			}
		});
	}

	public bool GetLootCollectibleID(string lootID, out int collectibleID)
	{
		collectibleID = 0;
		if (!string.IsNullOrEmpty(lootID) && lootID.IndexOf("Collectible=") == 0)
		{
			lootID = lootID.Remove(0, "Collectible=".Length);
			return int.TryParse(lootID, out collectibleID);
		}
		return false;
	}

	public void AddFriend(int gamespyID, string gameCenterID, string facebookID, string friendName, bool nameHasPriority)
	{
		foreach (FriendData mFriendID in mFriendIDs)
		{
			if (mFriendID.gamespyID == gamespyID)
			{
				if (!string.IsNullOrEmpty(gameCenterID))
				{
					mFriendID.gameCenterID = gameCenterID;
					mFriendID.isGameCenterFriend = true;
				}
				if (!string.IsNullOrEmpty(facebookID))
				{
					mFriendID.facebookID = facebookID;
					mFriendID.isFacebookFriend = true;
				}
				if (!string.IsNullOrEmpty(friendName) && (nameHasPriority || string.IsNullOrEmpty(mFriendID.friendName)))
				{
					mFriendID.friendName = friendName;
				}
				return;
			}
		}
		FriendData friendData = new FriendData();
		friendData.gamespyID = gamespyID;
		friendData.facebookID = facebookID;
		friendData.isFacebookFriend = !string.IsNullOrEmpty(facebookID);
		friendData.isGameCenterFriend = !string.IsNullOrEmpty(gameCenterID);
		friendData.friendName = friendName;
		mFriendIDs.Add(friendData);
	}

	public void Reset()
	{
		CollectionStatus.RemoveAll();
		if (userDataSave.RecordID != -1)
		{
			GripNetwork.RemoveRecord("UserData", userDataSave.RecordID, delegate
			{
			});
		}
		InitDefaults();
		Logout();
	}
}
