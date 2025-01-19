using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GluiButtonContainerBase))]
[AddComponentMenu("Glui/Button Container State Tracker")]
public class GluiButtonContainer_StateTracker : MonoBehaviour
{
	private List<GluiButtonAction> buttonActions;

	private GluiButtonContainerBase trackingButton;

	private string currentState;

	private bool firstUpdate;

	private void Awake()
	{
		FindTrackingButton();
	}

	private void Update()
	{
		if (!firstUpdate)
		{
			firstUpdate = true;
			if (trackingButton != null)
			{
				ButtonStateChanged(trackingButton.GetCurrentState());
			}
		}
	}

	public List<GluiButtonAction> GetButtonActions()
	{
		List<GluiButtonAction> list = new List<GluiButtonAction>();
		Component[] components = base.gameObject.GetComponents(typeof(GluiButtonAction));
		for (int i = 0; i < components.Length; i++)
		{
			GluiButtonAction item = (GluiButtonAction)components[i];
			list.Add(item);
		}
		return list;
	}

	private void FindTrackingButton()
	{
		if (trackingButton == null || buttonActions == null)
		{
			buttonActions = GetButtonActions();
			trackingButton = base.gameObject.GetComponent<GluiButtonContainerBase>();
			if (trackingButton != null)
			{
				GluiButtonContainerBase gluiButtonContainerBase = trackingButton;
				gluiButtonContainerBase.onButtonStateChanged = (GluiButtonContainerBase.OnButtonStateChanged)Delegate.Combine(gluiButtonContainerBase.onButtonStateChanged, new GluiButtonContainerBase.OnButtonStateChanged(ButtonStateChanged));
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (trackingButton != null)
		{
			GluiButtonContainerBase gluiButtonContainerBase = trackingButton;
			gluiButtonContainerBase.onButtonStateChanged = (GluiButtonContainerBase.OnButtonStateChanged)Delegate.Remove(gluiButtonContainerBase.onButtonStateChanged, new GluiButtonContainerBase.OnButtonStateChanged(ButtonStateChanged));
		}
	}

	public void ButtonStateChanged(string newStateName)
	{
		if (!(newStateName == currentState))
		{
			ReverseState(currentState);
			ApplyState(newStateName);
			currentState = newStateName;
		}
	}

	private void ApplyState(string state)
	{
		if (string.IsNullOrEmpty(state) || buttonActions == null)
		{
			return;
		}
		for (int i = 0; i < buttonActions.Count; i++)
		{
			if (buttonActions[i].State == state)
			{
				buttonActions[i].OnEnterState();
			}
		}
	}

	private void ReverseState(string state)
	{
		if (string.IsNullOrEmpty(state) || buttonActions == null)
		{
			return;
		}
		for (int num = buttonActions.Count - 1; num >= 0; num--)
		{
			if (buttonActions[num].State == state)
			{
				buttonActions[num].OnLeaveState();
			}
		}
	}
}
