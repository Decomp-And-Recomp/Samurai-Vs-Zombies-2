using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[AddComponentMenu("")]
public class DataAdaptor_EnableByValue : DataAdaptor_MatchValueBase
{
	public enum EnableType
	{
		Enable_GameObjects = 0,
		Enable_GluiWidgets = 1,
		Enable_GluiWidgets_InputOnly = 2,
		Select_GluiWidgets = 3
	}

	[Serializable]
	public class MatchEntry
	{
		public string[] values;

		public GameObject[] objectsToEnable;
	}

	public MatchEntry[] matchTable;

	public GameObject[] objectsToEnableIfNullOrNoValue;

	public GameObject[] objectsToDisableIfNullOrNoValue;

	public GameObject[] objectsToEnableIfNoMatch;

	public bool modifyChildren = true;

	public EnableType enableType;

	public GluiForcedUpdateList updateAfterOnTargets;

	protected override void SetData_Internal(object data)
	{
		if (data == null)
		{
			EnableOnlyMatchingEntry(null, true);
			return;
		}
		if (!(data is string))
		{
			EnableOnlyMatchingEntry(null, false);
			return;
		}
		string text = data.ToString();
		if (string.IsNullOrEmpty(text))
		{
			EnableOnlyMatchingEntry(null, true);
			return;
		}
		MatchEntry match = GetMatch(text);
		EnableOnlyMatchingEntry(match, false);
	}

	public override void SetDefaultData()
	{
		EnableOnlyMatchingEntry(null, true);
	}

	private void EnableOnlyMatchingEntry(MatchEntry match, bool isNullSet)
	{
		Action<GameObject, bool> action = MatchAction();
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		MatchEntry[] array = matchTable;
		foreach (MatchEntry matchEntry in array)
		{
			if (matchEntry == match)
			{
				list.AddRange(matchEntry.objectsToEnable);
			}
			else
			{
				list2.AddRange(matchEntry.objectsToEnable);
			}
		}
		if (match == null && isNullSet)
		{
			list.AddRange(objectsToEnableIfNullOrNoValue);
			list2.AddRange(objectsToDisableIfNullOrNoValue);
		}
		else
		{
			list2.AddRange(objectsToEnableIfNullOrNoValue);
			list.AddRange(objectsToDisableIfNullOrNoValue);
		}
		if (match == null && !isNullSet)
		{
			list.AddRange(objectsToEnableIfNoMatch);
		}
		else
		{
			list2.AddRange(objectsToEnableIfNoMatch);
		}
		foreach (GameObject item in list2)
		{
			if (item != null && !list.Contains(item))
			{
				action(item, false);
				updateAfterOnTargets.TriggerOnTarget(item, modifyChildren);
			}
		}
		foreach (GameObject item2 in list)
		{
			if (item2 != null)
			{
				action(item2, true);
				updateAfterOnTargets.TriggerOnTarget(item2, modifyChildren);
			}
		}
		updateAfterOnTargets.TriggerAdditionalObjects();
	}

	private Action<GameObject, bool> MatchAction()
	{
		switch (enableType)
		{
		case EnableType.Enable_GameObjects:
			if (modifyChildren)
			{
				return MatchAction_EnableRecursive;
			}
			return MatchAction_Enable;
		case EnableType.Enable_GluiWidgets:
		case EnableType.Enable_GluiWidgets_InputOnly:
		case EnableType.Select_GluiWidgets:
			if (modifyChildren)
			{
				return MatchAction_Widget_EnableRecursive;
			}
			return MatchAction_Widget_Enable;
		default:
			return null;
		}
	}

	private void MatchAction_Enable(GameObject thisGameObject, bool enable)
	{
		thisGameObject.SetActive(enable);
	}

	private void MatchAction_EnableRecursive(GameObject thisGameObject, bool enable)
	{
		thisGameObject.SetActive(enable);
	}

	private void MatchAction_Widget_Enable(GameObject thisGameObject, bool enable)
	{
		GluiWidget gluiWidget = (GluiWidget)thisGameObject.GetComponent(typeof(GluiWidget));
		switch (enableType)
		{
		case EnableType.Enable_GluiWidgets_InputOnly:
			gluiWidget.AllowInput = enable;
			break;
		case EnableType.Select_GluiWidgets:
		{
			GluiButtonContainerBase component = gluiWidget.GetComponent<GluiButtonContainerBase>();
			if (component != null)
			{
				component.Selected = true;
			}
			break;
		}
		default:
			gluiWidget.Enabled = enable;
			break;
		}
	}

	private void MatchAction_Widget_EnableRecursive(GameObject thisGameObject, bool enable)
	{
		Component[] componentsInChildren = thisGameObject.GetComponentsInChildren(typeof(GluiWidget), true);
		Component[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			GluiWidget gluiWidget = (GluiWidget)array[i];
			switch (enableType)
			{
			case EnableType.Enable_GluiWidgets_InputOnly:
				gluiWidget.AllowInput = enable;
				break;
			case EnableType.Select_GluiWidgets:
			{
				GluiButtonContainerBase component = gluiWidget.GetComponent<GluiButtonContainerBase>();
				if (component != null)
				{
					component.Selected = true;
				}
				break;
			}
			default:
				gluiWidget.Enabled = enable;
				break;
			}
		}
	}

	private MatchEntry GetMatch(string textToMatch)
	{
		MatchEntry[] array = matchTable;
		foreach (MatchEntry matchEntry in array)
		{
			string[] values = matchEntry.values;
			foreach (string text in values)
			{
				if (text == textToMatch)
				{
					return matchEntry;
				}
			}
		}
		return null;
	}
}
