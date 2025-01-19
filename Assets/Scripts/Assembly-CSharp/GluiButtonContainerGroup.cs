using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Glui/Button Container Group")]
public class GluiButtonContainerGroup : MonoBehaviour
{
	public enum AutoSelect
	{
		None = 0,
		First = 1,
		Manual = 2,
		Persistent = 3
	}

	public string actionOnChange = string.Empty;

	public bool allowDeselect;

	public AutoSelect autoSelect = AutoSelect.First;

	public string autoSelectName;

	private List<GameObject> addedButtons = new List<GameObject>();

	private GameObject selectedButton;

	private bool isEnabled = true;

	public bool Enabled
	{
		get
		{
			return isEnabled;
		}
		set
		{
			if (value != isEnabled)
			{
				isEnabled = value;
				GluiStandardButtonContainer gluiStandardButtonContainer = SelectedButton();
				if (gluiStandardButtonContainer != null)
				{
					gluiStandardButtonContainer.Selected = isEnabled;
				}
			}
		}
	}

	public void Add(GluiStandardButtonContainer button)
	{
		if (!addedButtons.Contains(button.gameObject))
		{
			button.action = delegate
			{
				SelectButton(button);
			};
			addedButtons.Add(button.gameObject);
			if (autoSelect == AutoSelect.First && !allowDeselect && SelectedButton() == null)
			{
				SelectButton(button);
			}
		}
	}

	public void ScanForButtonsOnChildren()
	{
		GluiStandardButtonContainer[] componentsInChildren = GetComponentsInChildren<GluiStandardButtonContainer>();
		Array.ForEach(componentsInChildren, Add);
		string selectedButtonName = null;
		if (autoSelect == AutoSelect.Manual)
		{
			selectedButtonName = autoSelectName;
		}
		else if (autoSelect == AutoSelect.Persistent)
		{
			selectedButtonName = SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData(autoSelectName) as string;
		}
		if (!string.IsNullOrEmpty(selectedButtonName))
		{
			GluiStandardButtonContainer gluiStandardButtonContainer = Array.Find(componentsInChildren, (GluiStandardButtonContainer btn) => string.Equals(btn.name, selectedButtonName));
			if (gluiStandardButtonContainer != null)
			{
				SelectButton(gluiStandardButtonContainer);
			}
		}
	}

	public void SelectButton(GluiStandardButtonContainer newSelection)
	{
		GluiStandardButtonContainer gluiStandardButtonContainer = SelectedButton();
		if (newSelection == null || newSelection == gluiStandardButtonContainer)
		{
			if (allowDeselect && gluiStandardButtonContainer != null)
			{
				gluiStandardButtonContainer.Selected = false;
				selectedButton = null;
				GluiActionSender.SendGluiAction(actionOnChange, base.gameObject, null);
				if (autoSelect == AutoSelect.Persistent && !string.IsNullOrEmpty(autoSelectName))
				{
					SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save(autoSelectName, string.Empty);
				}
			}
			return;
		}
		Add(newSelection);
		if (gluiStandardButtonContainer != null)
		{
			gluiStandardButtonContainer.Selected = false;
		}
		newSelection.Selected = isEnabled;
		selectedButton = newSelection.gameObject;
		if (autoSelect == AutoSelect.Persistent && !string.IsNullOrEmpty(autoSelectName))
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save(autoSelectName, newSelection.name);
		}
		GluiActionSender.SendGluiAction(actionOnChange, base.gameObject, newSelection.gameObject);
	}

	public GluiStandardButtonContainer SelectedButton()
	{
		if (selectedButton != null)
		{
			return selectedButton.GetComponent<GluiStandardButtonContainer>();
		}
		return null;
	}

	private void Start()
	{
		ScanForButtonsOnChildren();
	}
}
