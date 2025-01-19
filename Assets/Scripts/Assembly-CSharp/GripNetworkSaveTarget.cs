using System;
using System.Collections.Generic;

public class GripNetworkSaveTarget : SaveTarget
{
	public enum Status
	{
		Uninitialized = 0,
		Ready = 1,
		Saving = 2,
		Loading = 3
	}

	public const int kInvalidID = -1;

	private const string kBackupExtension = "Bak";

	private string table;

	private GripField recordIdField = new GripField("recordid", GripField.GripFieldType.Int);

	private GripField ownerIdField = new GripField("ownerid", GripField.GripFieldType.Int);

	private GripField saveField = new GripField("save", GripField.GripFieldType.BinaryData);

	private GripField saveBackupField;

	private Dictionary<string, GripField> keyValueFields = new Dictionary<string, GripField>();

	private List<GripField> pendingSaves = new List<GripField>();

	private Dictionary<string, List<Action<GripField>>> pendingLoads = new Dictionary<string, List<Action<GripField>>>();

	public Status CurrentStatus { get; private set; }

	public int RecordID
	{
		get
		{
			return (recordIdField == null || !recordIdField.mInt.HasValue) ? (-1) : recordIdField.mInt.Value;
		}
	}

	public int OwnerID
	{
		get
		{
			return (ownerIdField == null || !ownerIdField.mInt.HasValue) ? (-1) : ownerIdField.mInt.Value;
		}
	}

	public string Table
	{
		get
		{
			return table;
		}
		set
		{
			if (!string.Equals(table, value))
			{
				table = value;
				CurrentStatus = Status.Uninitialized;
				recordIdField.mInt = null;
				ownerIdField.mInt = null;
				SingletonSpawningMonoBehaviour<SaveManager>.Instance.EnqueueUpdateTask(FindUserRecord);
			}
		}
	}

	public byte[] SaveData
	{
		get
		{
			return (saveField == null) ? null : saveField.mBinaryData;
		}
		private set
		{
			saveField.mBinaryData = value;
			if (saveBackupField != null)
			{
				saveBackupField.mBinaryData = value;
			}
		}
	}

	public Dictionary<string, GripField> Values
	{
		get
		{
			return keyValueFields;
		}
	}

	public bool UseBinarySaveData { get; set; }

	public bool UseKeyValueSaveData { get; set; }

	public override string Name
	{
		get
		{
			return saveField.mName;
		}
		set
		{
			saveField.mName = value;
			if (saveBackupField != null)
			{
				saveBackupField.mName = value + "Bak";
			}
		}
	}

	public override bool UseBackup
	{
		get
		{
			return saveBackupField != null;
		}
		set
		{
			if (value && saveBackupField == null)
			{
				saveBackupField = new GripField(Name + "Bak", GripField.GripFieldType.BinaryData);
			}
			else if (!value)
			{
				saveBackupField = null;
			}
		}
	}

	public override void Save(byte[] data)
	{
		SaveData = data;
		if (UseBinarySaveData && data != null)
		{
			SaveField(saveField);
			if (UseBackup)
			{
				SaveField(saveBackupField);
			}
		}
		if (!UseKeyValueSaveData)
		{
			return;
		}
		foreach (GripField value in keyValueFields.Values)
		{
			SaveField(value);
		}
	}

	public override void Load(bool loadBackup, Action<byte[]> onComplete)
	{
		int fieldsToLoad = 0;
		if (UseBinarySaveData)
		{
			string fieldName = ((!loadBackup || !UseBackup) ? saveField.mName : saveBackupField.mName);
			fieldsToLoad++;
			LoadField(fieldName, delegate(GripField field)
			{
				if (field != null && field.mBinaryData != null && field.mBinaryData.Length > 0)
				{
					SaveData = field.mBinaryData;
				}
				else
				{
					SaveData = null;
				}
				if (--fieldsToLoad == 0 && onComplete != null)
				{
					onComplete(SaveData);
				}
			});
		}
		if (UseKeyValueSaveData)
		{
			foreach (string key in keyValueFields.Keys)
			{
				fieldsToLoad++;
				LoadField(key, delegate(GripField field)
				{
					if (field != null)
					{
						keyValueFields[field.mName] = field;
					}
					if (--fieldsToLoad == 0 && onComplete != null)
					{
						onComplete(SaveData);
					}
				});
			}
		}
		if (fieldsToLoad == 0 && onComplete != null)
		{
			onComplete(null);
		}
	}

	public override void Delete()
	{
		SaveData = new byte[0];
		SaveField(saveField);
		if (UseBackup)
		{
			SaveField(saveBackupField);
		}
	}

	public void SaveValueField(string key)
	{
		GripField value = null;
		if (keyValueFields.TryGetValue(key, out value))
		{
			SaveField(value);
		}
	}

	public void SaveValueField(GripField field)
	{
		keyValueFields[field.mName] = field;
		SaveField(field);
	}

	protected override void SaveValueInt(string key, int value)
	{
		GripField value2 = null;
		if (!keyValueFields.TryGetValue(key, out value2))
		{
			value2 = new GripField();
			value2.mName = key;
			keyValueFields[key] = value2;
		}
		value2.mType = GripField.GripFieldType.Int;
		value2.mInt = value;
		SaveField(value2);
	}

	protected override void SaveValueFloat(string key, float value)
	{
		GripField value2 = null;
		if (!keyValueFields.TryGetValue(key, out value2))
		{
			value2 = new GripField();
			value2.mName = key;
			keyValueFields[key] = value2;
		}
		value2.mType = GripField.GripFieldType.Float;
		value2.mFloat = value;
		SaveField(value2);
	}

	protected override void SaveValueString(string key, string value)
	{
		GripField value2 = null;
		if (!keyValueFields.TryGetValue(key, out value2))
		{
			value2 = new GripField();
			value2.mName = key;
			keyValueFields[key] = value2;
		}
		value2.mType = GripField.GripFieldType.UnicodeString;
		value2.mString = value;
		SaveField(value2);
	}

	public override void LoadValue(string key, float defaultValue, Action<float> onComplete)
	{
		if (string.IsNullOrEmpty(key))
		{
			return;
		}
		LoadField(key, delegate(GripField field)
		{
			if (field == null)
			{
				field = new GripField();
				field.mName = key;
			}
			if (!field.mFloat.HasValue)
			{
				field.mFloat = defaultValue;
			}
			keyValueFields[key] = field;
			if (onComplete != null)
			{
				onComplete(field.mFloat.Value);
			}
		});
	}

	public override void LoadValue(string key, int defaultValue, Action<int> onComplete)
	{
		if (string.IsNullOrEmpty(key))
		{
			return;
		}
		LoadField(key, delegate(GripField field)
		{
			if (field == null)
			{
				field = new GripField();
				field.mName = key;
			}
			if (!field.mInt.HasValue)
			{
				field.mInt = defaultValue;
			}
			keyValueFields[key] = field;
			if (onComplete != null)
			{
				onComplete(field.mInt.Value);
			}
		});
	}

	public override void LoadValue(string key, string defaultValue, Action<string> onComplete)
	{
		if (string.IsNullOrEmpty(key))
		{
			return;
		}
		LoadField(key, delegate(GripField field)
		{
			if (field == null)
			{
				field = new GripField();
				field.mName = key;
				field.mString = defaultValue;
			}
			keyValueFields[key] = field;
			if (onComplete != null)
			{
				onComplete(field.mString);
			}
		});
	}

	public override void DeleteValue(string key)
	{
		if (!string.IsNullOrEmpty(key))
		{
			GripField gripField = new GripField();
			gripField.mName = key;
			keyValueFields[key] = gripField;
			SaveField(gripField);
		}
	}

	private void SaveField(GripField field)
	{
		if (pendingSaves.Count == 0)
		{
			SingletonSpawningMonoBehaviour<SaveManager>.Instance.EnqueueUpdateTask(UpdatePendingSaves);
		}
		if (!pendingSaves.Contains(field))
		{
			pendingSaves.Add(field);
		}
		if (CurrentStatus == Status.Ready)
		{
			CurrentStatus = Status.Saving;
		}
	}

	private void LoadField(string fieldName, Action<GripField> onComplete)
	{
		if (pendingLoads.Count == 0)
		{
			SingletonSpawningMonoBehaviour<SaveManager>.Instance.EnqueueUpdateTask(UpdatePendingLoads);
		}
		List<Action<GripField>> value = null;
		if (!pendingLoads.TryGetValue(fieldName, out value))
		{
			value = new List<Action<GripField>>();
			pendingLoads[fieldName] = value;
		}
		value.Add(onComplete);
		if (CurrentStatus == Status.Ready)
		{
			CurrentStatus = Status.Loading;
		}
	}

	private bool FindUserRecord()
	{
		if (!ApplicationUtilities.HasShutdown && GripNetwork.Ready)
		{
			if (!recordIdField.mInt.HasValue)
			{
				GripNetwork.GetMyRecords(Table, new string[2] { recordIdField.mName, ownerIdField.mName }, delegate(GripNetwork.Result result, GripField[,] fields)
				{
					if (result == GripNetwork.Result.Success && fields.Length > 0)
					{
						recordIdField.mInt = fields[0, 0].mInt;
						ownerIdField.mInt = fields[0, 1].mInt;
					}
					CurrentStatus = Status.Ready;
				});
			}
			return true;
		}
		return false;
	}

	private bool UpdatePendingSaves()
	{
		if (GripNetwork.Ready && (CurrentStatus == Status.Ready || CurrentStatus == Status.Saving))
		{
			if (pendingSaves.Count > 0)
			{
				CurrentStatus = Status.Saving;
				if (RecordID != -1)
				{
					GripNetwork.UpdateRecord(Table, RecordID, pendingSaves.ToArray(), delegate
					{
						CurrentStatus = Status.Ready;
					});
				}
				else
				{
					GripNetwork.CreateRecord(Table, pendingSaves.ToArray(), delegate(GripNetwork.Result result, int id)
					{
						if (result == GripNetwork.Result.Success)
						{
							recordIdField.mInt = id;
							ownerIdField.mInt = GripNetwork.GameSpyAccountManager.SecurityToken.ProfileId;
						}
						CurrentStatus = Status.Ready;
					});
				}
				pendingSaves.Clear();
			}
			return true;
		}
		return false;
	}

	private bool UpdatePendingLoads()
	{
		if (GripNetwork.Ready && (CurrentStatus == Status.Ready || CurrentStatus == Status.Loading))
		{
			if (pendingLoads.Count > 0)
			{
				CurrentStatus = Status.Loading;
				int numFields = pendingLoads.Count;
				string[] array = new string[numFields];
				List<Action<GripField>>[] notifications = new List<Action<GripField>>[numFields];
				pendingLoads.Keys.CopyTo(array, 0);
				pendingLoads.Values.CopyTo(notifications, 0);
				pendingLoads.Clear();
				GripNetwork.SearchFirstRecord(Table, array, OwnerID, delegate(GripNetwork.Result result, GripField[,] fields)
				{
					for (int i = 0; i < numFields; i++)
					{
						foreach (Action<GripField> item in notifications[i])
						{
							if (item != null)
							{
								if (result == GripNetwork.Result.Success && i < fields.GetLength(1))
								{
									item(fields[0, i]);
								}
								else
								{
									item(null);
								}
							}
						}
					}
					CurrentStatus = Status.Ready;
				});
			}
			return true;
		}
		return false;
	}
}
