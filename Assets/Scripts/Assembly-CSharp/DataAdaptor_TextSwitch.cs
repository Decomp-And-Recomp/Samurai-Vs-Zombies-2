using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("")]
public class DataAdaptor_TextSwitch : DataAdaptor_MatchValueBase
{
	public enum Matched
	{
		Value = 0,
		Null = 1,
		None = 2
	}

	[Serializable]
	public class SwitchEntry
	{
		public string[] matches;

		public string textToSet;
	}

	public GameObject targetObject;

	public SwitchEntry[] switchTable;

	public string IfNullOrNoValue;

	public string IfNoMatch;

	protected override void SetData_Internal(object data)
	{
		if (data == null)
		{
			EnableOnlyMatchingEntry(null, Matched.Null);
			return;
		}
		if (!(data is string))
		{
			EnableOnlyMatchingEntry(null, Matched.None);
			return;
		}
		string text = data.ToString();
		if (string.IsNullOrEmpty(text))
		{
			EnableOnlyMatchingEntry(null, Matched.Null);
			return;
		}
		SwitchEntry match = GetMatch(text);
		EnableOnlyMatchingEntry(match, Matched.Value);
	}

	public override void SetDefaultData()
	{
		EnableOnlyMatchingEntry(null, Matched.Null);
	}

	private void EnableOnlyMatchingEntry(SwitchEntry match, Matched matched)
	{
		Action<GameObject, string> action = MatchAction();
		switch (matched)
		{
		case Matched.None:
			action(targetObject, IfNoMatch);
			break;
		case Matched.Null:
			action(targetObject, IfNullOrNoValue);
			break;
		case Matched.Value:
			if (match != null)
			{
				action(targetObject, match.textToSet);
			}
			break;
		}
	}

	private Action<GameObject, string> MatchAction()
	{
		return MatchAction_SetText;
	}

	private void MatchAction_SetText(GameObject thisGameObject, string text)
	{
		SetGluiTextInChild(thisGameObject, text);
	}

	private SwitchEntry GetMatch(string textToMatch)
	{
		SwitchEntry[] array = switchTable;
		foreach (SwitchEntry switchEntry in array)
		{
			string[] matches = switchEntry.matches;
			foreach (string text in matches)
			{
				if (text == textToMatch)
				{
					return switchEntry;
				}
			}
		}
		return null;
	}
}
