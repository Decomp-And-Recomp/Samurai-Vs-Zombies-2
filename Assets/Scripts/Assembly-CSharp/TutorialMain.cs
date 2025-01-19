using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMain : SingletonMonoBehaviour<TutorialMain>, IGluiActionHandler
{
	private const string passiveTutorial_GroupName = "Passive";

	private const string tutorialTableName = "TutorialData";

	private const string tutorialActionWhenChainDone = "TUTORIAL_CLEAR";

	private DataBundleTableHandle<TutorialSchema> tutorialDataHandle;

	private List<TutorialSchema> tutorialData;

	public IEnumerator Start()
	{
		while (!DataBundleRuntime.Instance)
		{
			yield return null;
		}
		Init();
	}

	public void Init()
	{
		if (tutorialData == null)
		{
			tutorialDataHandle = new DataBundleTableHandle<TutorialSchema>("TutorialData");
			tutorialData = new List<TutorialSchema>(tutorialDataHandle.Data);
		}
	}

	public bool IsTutorialNeeded(string tutorialName)
	{
		return !Singleton<Profile>.Instance.IsTutorialDone("Passive", tutorialName);
	}

	private bool IsTutorialNeeded(TutorialSchema record)
	{
		return !Singleton<Profile>.Instance.IsTutorialDone("Passive", record.name);
	}

	private void SetTutorialDone(TutorialSchema record)
	{
		Singleton<Profile>.Instance.SetTutorialDone("Passive", record.name);
	}

	private void ClearTutorialDone(TutorialSchema record)
	{
		Singleton<Profile>.Instance.ClearTutorialDone("Passive", record.name);
	}

	private TutorialSchema GetTutorialRecord(string name)
	{
		return tutorialData.Find((TutorialSchema record) => record.name == name);
	}

	public string GetCurrentTutorial_Key()
	{
		return SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData("CURRENT_TUTORIAL") as string;
	}

	private TutorialSchema GetCurrentTutorial()
	{
		string text = SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData("CURRENT_TUTORIAL") as string;
		return GetTutorialRecord(text);
	}

	public bool TutorialStartIfNeeded(string name)
	{
		return TutorialStartIfNeeded(GetTutorialRecord(name));
	}

	private bool TutorialStartIfNeeded(TutorialSchema record)
	{
		if (record != null && IsTutorialNeeded(record))
		{
			TutorialStart(record);
			return true;
		}
		return false;
	}

	private void TutorialStart(TutorialSchema record)
	{
		if (record != null)
		{
			bool flag = false;
			if (!string.IsNullOrEmpty(GetCurrentTutorial_Key()))
			{
				TutorialDone();
				flag = true;
			}
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save("CURRENT_TUTORIAL", record.name);
			if (flag)
			{
				GluiDelayedAction.Create(record.actionSendOnStart, 1.5f, base.gameObject, false);
			}
			else
			{
				GluiActionSender.SendGluiAction(record.actionSendOnStart, base.gameObject, null);
			}
		}
	}

	public void TutorialDone()
	{
		TutorialDone(GetCurrentTutorial());
	}

	private void TutorialDone(TutorialSchema record)
	{
		if (record != null)
		{
			SetTutorialDone(record);
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Remove("CURRENT_TUTORIAL");
			GluiActionSender.SendGluiAction(record.actionSendOnDone, base.gameObject, null);
			if (!string.IsNullOrEmpty(record.tutorialToPlayNext.Key))
			{
				TutorialStart(GetTutorialRecord(record.tutorialToPlayNext.Key));
			}
			else
			{
				GluiActionSender.SendGluiAction("TUTORIAL_CLEAR", base.gameObject, null);
			}
		}
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		if (tutorialData == null)
		{
			return false;
		}
		foreach (TutorialSchema tutorialDatum in tutorialData)
		{
			if (tutorialDatum.actionToTrigger == action)
			{
				if (!TutorialStartIfNeeded(tutorialDatum) && action == "TUTORIAL_MP01_TRIGGER")
				{
					TutorialStartIfNeeded("MP04_Defense");
				}
				return true;
			}
		}
		switch (action)
		{
		case "TUTORIAL_DONE":
			TutorialDone();
			return true;
		default:
			return false;
		}
	}

	public void SetAllTutorialDoneFlags(bool isDone)
	{
		foreach (TutorialSchema tutorialDatum in tutorialData)
		{
			if (isDone)
			{
				SetTutorialDone(tutorialDatum);
			}
			else
			{
				ClearTutorialDone(tutorialDatum);
			}
		}
	}
}
