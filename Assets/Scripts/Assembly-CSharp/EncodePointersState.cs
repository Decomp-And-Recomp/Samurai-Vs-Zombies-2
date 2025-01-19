public class EncodePointersState : DoNothingState
{
	private UpdateCheckData objData;

	public override void Init(FSM fsm)
	{
	}

	public override void OnEnter(FSM fsm, int prevState)
	{
		objData = (UpdateCheckData)fsm.GetOwnerObject();
		string lastDBUpdate = null;
		string lastIncrBuild = null;
		string localDataBundleVersion = BundleUtils.GetLocalDataBundleVersion(UpdateCheckData.MainDB);
		string localIncrementalBuild = BundleUtils.GetLocalIncrementalBuild(UpdateCheckData.MainDB);
		if (ReturnLastReplace(localDataBundleVersion, localIncrementalBuild, objData.MainPtrContents, ref lastDBUpdate, ref lastIncrBuild))
		{
			objData.MainDatabasePointer = lastDBUpdate;
			objData.MainDatabaseBuild = lastIncrBuild;
		}
		else
		{
			objData.MainDatabasePointer = null;
			objData.MainDatabaseBuild = null;
		}
		lastDBUpdate = null;
		lastIncrBuild = null;
		localDataBundleVersion = BundleUtils.GetLocalDataBundleVersion(objData.DeviceLanguage);
		localIncrementalBuild = BundleUtils.GetLocalIncrementalBuild(objData.DeviceLanguage);
		if (ReturnLastReplace(localDataBundleVersion, localIncrementalBuild, objData.LangPtrContents, ref lastDBUpdate, ref lastIncrBuild))
		{
			objData.LangDatabasePointer = lastDBUpdate;
			objData.LangDatabaseBuild = lastIncrBuild;
		}
		else
		{
			objData.LangDatabasePointer = null;
			objData.LangDatabaseBuild = null;
		}
	}

	public override void OnExit(FSM fsm, int nextState)
	{
	}

	public override void OnUpdate(FSM fsm)
	{
		fsm.QueueState(9);
	}

	private bool ReturnLastReplace(string currentDBVersion, string currentIncrBuild, string ptrContents, ref string lastDBUpdate, ref string lastIncrBuild)
	{
		ptrContents = ptrContents.Trim();
		if (string.IsNullOrEmpty(ptrContents))
		{
			return false;
		}
		string[] array = ptrContents.Split('\n');
		if (array.Length <= 0)
		{
			return false;
		}
		int num = -1;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].StartsWith("\"" + currentDBVersion + "\" "))
			{
				num = i;
				break;
			}
		}
		if (num == array.Length - 1)
		{
			return false;
		}
		for (int num2 = array.Length - 1; num2 > num; num2--)
		{
			string[] array2 = array[num2].Split(' ');
			if (array2.Length >= 3)
			{
				string text = array2[0].Trim('"');
				string text2 = array2[1].Trim('"');
				string text3 = array2[2].Trim('"', '\r');
				if (text3 == UpdateCheckData.ReplaceUpdate)
				{
					if (currentDBVersion == text && text2 == currentIncrBuild)
					{
						return false;
					}
					lastDBUpdate = text;
					lastIncrBuild = text2;
					return true;
				}
			}
		}
		return false;
	}
}
