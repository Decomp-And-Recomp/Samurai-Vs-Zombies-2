using UnityEngine;

[AddComponentMenu("Glui/Hotspot")]
[ExecuteInEditMode]
public class GluiHotspot : GluiBase
{
	public string onPress = string.Empty;

	public string onRelease = string.Empty;

	protected bool state;

	private void ChangeState(bool newState)
	{
		if (newState != state)
		{
			state = newState;
			Trigger(state);
		}
	}

	protected virtual void Trigger(bool pressed)
	{
		string text = ((!pressed) ? onRelease : onPress);
		if (text != string.Empty)
		{
			GluiWidget component = GetComponent<GluiWidget>();
			if (!(component == null) && component.Handler != null)
			{
				component.Handler.SendMessage(text);
			}
		}
	}

	public virtual void HandleInput(InputEvent inputEvent, out InputRouter.InputResponse response)
	{
		if (!base.Enabled)
		{
			response = InputRouter.InputResponse.Passthrough;
			return;
		}
		switch (inputEvent.EventType)
		{
		case InputEvent.EEventType.OnCursorDown:
			OnCursorDown(inputEvent);
			break;
		case InputEvent.EEventType.OnCursorUp:
			OnCursorUp(inputEvent);
			break;
		}
		response = InputRouter.InputResponse.Handled;
	}

	private void OnCursorDown(InputEvent e)
	{
		ChangeState(true);
	}

	private void OnCursorUp(InputEvent e)
	{
		ChangeState(false);
	}
}
