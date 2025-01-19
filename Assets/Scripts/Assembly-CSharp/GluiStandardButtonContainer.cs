using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Glui/Standard Button Container")]
[ExecuteInEditMode]
public class GluiStandardButtonContainer : GluiButtonContainerBase
{
	public enum GluiButton4State
	{
		Default = 0,
		Pressed = 1,
		Selected = 2,
		Locked = 3
	}

	public enum MouseState
	{
		Up = 0,
		Down = 1
	}

	private static string[] states = new string[4]
	{
		GluiButton4State.Default.ToString(),
		GluiButton4State.Pressed.ToString(),
		GluiButton4State.Selected.ToString(),
		GluiButton4State.Locked.ToString()
	};

	public Action action;

	public string[] onPressActions;

	public string[] onReleaseActions;

	public string soundOnPress = "SOUND_GLUI_BUTTON_PRESS";

	public string soundOnLockedPress = string.Empty;

	public bool allowMultitouch;

	public GluiText Text;

	private GluiButton4State buttonState;

	private MouseState mouseState;

	private bool isLocked;

	public Func<object> GetActionData { get; set; }

	public bool Locked
	{
		get
		{
			return isLocked;
		}
		set
		{
			isLocked = value;
			UpdateButtonState();
		}
	}

	public override string[] GetStates()
	{
		return states;
	}

	public override string GetCurrentState()
	{
		return buttonState.ToString();
	}

	protected override void OnCreate()
	{
		if ((base.name.Equals("Button_Close") || base.name.Equals("Button_Back") || base.name.Equals("Button_CancelLogin")) && !string.IsNullOrEmpty(onReleaseActions[0]))
		{
			SingletonMonoBehaviour<InputManager>.Instance.backStack.Push(this);
		}
	}

	protected void ChangeMouseState(MouseState newState)
	{
		if (newState != mouseState)
		{
			mouseState = newState;
			UpdateButtonState();
		}
	}

	protected void UpdateButtonState()
	{
		if (Locked)
		{
			ChangeButtonState(GluiButton4State.Locked);
		}
		else if (mouseState == MouseState.Down)
		{
			ChangeButtonState(GluiButton4State.Pressed);
		}
		else if (Selected)
		{
			ChangeButtonState(GluiButton4State.Selected);
		}
		else
		{
			ChangeButtonState(GluiButton4State.Default);
		}
	}

	protected void ChangeButtonState(GluiButton4State newState)
	{
		if (newState != buttonState)
		{
			buttonState = newState;
			if (onButtonStateChanged != null)
			{
				onButtonStateChanged(buttonState.ToString());
			}
		}
	}

	public override void HandleInput(InputCrawl crawl, out InputRouter.InputResponse response)
	{
		if (!allowMultitouch && crawl.inputEvent.CursorIndex != 0)
		{
			response = InputRouter.InputResponse.Blocked;
			return;
		}
		if (!base.Usable)
		{
			response = InputRouter.InputResponse.Blocked;
			return;
		}
		switch (crawl.inputEvent.EventType)
		{
		case InputEvent.EEventType.OnCursorDown:
			ChangeMouseState(MouseState.Down);
			SendActions(onPressActions);
			break;
		case InputEvent.EEventType.OnCursorUp:
			if (mouseState == MouseState.Down || (onReleaseActions.Length > 0 && onReleaseActions[0].Equals("BUTTON_ATTACK")))
			{
				ChangeMouseState(MouseState.Up);
				if (buttonState == GluiButton4State.Locked)
				{
					GluiSoundSender.SendGluiSound(soundOnLockedPress, base.gameObject);
				}
				else
				{
					GluiSoundSender.SendGluiSound(soundOnPress, base.gameObject);
				}
				SendActions(onReleaseActions);
				if (action != null)
				{
					action();
				}
			}
			break;
		case InputEvent.EEventType.OnCursorExit:
			ChangeMouseState(MouseState.Up);
			break;
		}
		response = InputRouter.InputResponse.Handled;
	}

	protected override void OnSelectedChanged()
	{
		base.OnSelectedChanged();
		UpdateButtonState();
	}

	protected void SendActions(string[] actions)
	{
		if (actions == null)
		{
			return;
		}
		object data = null;
		if (GetActionData != null)
		{
			data = GetActionData();
		}
		for (int i = 0; i < actions.Length; i++)
		{
			if (!string.IsNullOrEmpty(actions[i]))
			{
				GluiActionSender.SendGluiAction(actions[i], base.gameObject, data);
			}
		}
	}

	public override List<GluiTexture> GetGluiTextures()
	{
		return new List<GluiTexture>();
	}
}
