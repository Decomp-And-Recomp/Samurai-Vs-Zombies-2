using System;
using UnityEngine;

public class GluiModulator : MonoBehaviour
{
	private GluiText textWidget;

	private GluiMeter valueWidget;

	private bool isModulating;

	public virtual void Awake()
	{
		textWidget = GetComponent(typeof(GluiText)) as GluiText;
		if (textWidget != null)
		{
			GluiText gluiText = textWidget;
			gluiText.onTextChanged = (GluiText.TextChangedAction)Delegate.Combine(gluiText.onTextChanged, new GluiText.TextChangedAction(OnTextChanged_First));
		}
		valueWidget = GetComponent(typeof(GluiMeter)) as GluiMeter;
		if (valueWidget != null)
		{
			GluiMeter gluiMeter = valueWidget;
			gluiMeter.onValueChanged = (GluiMeter.ValueChangedAction)Delegate.Combine(gluiMeter.onValueChanged, new GluiMeter.ValueChangedAction(OnValueChanged_First));
		}
	}

	public void OnDestroy()
	{
		if (textWidget != null)
		{
			GluiText gluiText = textWidget;
			gluiText.onTextChanged = (GluiText.TextChangedAction)Delegate.Remove(gluiText.onTextChanged, new GluiText.TextChangedAction(OnTextChanged_First));
		}
		if (valueWidget != null)
		{
			GluiMeter gluiMeter = valueWidget;
			gluiMeter.onValueChanged = (GluiMeter.ValueChangedAction)Delegate.Remove(gluiMeter.onValueChanged, new GluiMeter.ValueChangedAction(OnValueChanged_First));
		}
	}

	protected void OnTextChanged_First(ref string textToModify, string originalText)
	{
		if (!isModulating)
		{
			OnTextChanged(ref textToModify, originalText);
		}
	}

	protected virtual void SetText(string text)
	{
		if (!(textWidget == null))
		{
			isModulating = true;
			textWidget.Text = text;
			isModulating = false;
		}
	}

	protected string GetText()
	{
		if (textWidget == null)
		{
			return string.Empty;
		}
		return textWidget.Text;
	}

	protected virtual void OnTextChanged(ref string text, string originalText)
	{
	}

	protected void OnValueChanged_First(ref float valueToModify, float originalValue)
	{
		if (!isModulating)
		{
			OnValueChanged(ref valueToModify, originalValue);
		}
	}

	protected void SetValue(float value)
	{
		if (valueWidget == null)
		{
			SetText(value.ToString());
			return;
		}
		isModulating = true;
		valueWidget.Value = value;
		isModulating = false;
	}

	protected float GetValue()
	{
		if (valueWidget == null)
		{
			float result;
			float.TryParse(GetText(), out result);
			return result;
		}
		return valueWidget.Value;
	}

	protected virtual void OnValueChanged(ref float value, float originalValue)
	{
	}
}
