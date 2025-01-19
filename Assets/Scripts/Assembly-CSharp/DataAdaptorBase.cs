using UnityEngine;

[AddComponentMenu("")]
public class DataAdaptorBase
{
	public virtual void SetData(object data)
	{
	}

	public virtual void SetDefaultData()
	{
	}

	protected void SetGluiTextInChild(string objectName, GameObject self, string text)
	{
		Transform transform = self.transform.Find(objectName);
		SetGluiTextInChild(transform.gameObject, text);
	}

	protected GluiText SetGluiTextTagInChild(GameObject child, string tag)
	{
		if (child != null && tag != null)
		{
			GluiText component = child.GetComponent<GluiText>();
			component.TaggedStringReference = tag;
			return component;
		}
		return null;
	}

	protected GluiText SetGluiTextInChild(GameObject child, string text)
	{
		if (child != null && text != null)
		{
			GluiText component = child.GetComponent<GluiText>();
			component.Text = text;
			return component;
		}
		return null;
	}

	protected void SetGluiTextInChild(GameObject child, string text, Color color)
	{
		GluiText gluiText = SetGluiTextInChild(child, text);
		if (gluiText != null)
		{
			gluiText.enabled = true;
			gluiText.Color = color;
		}
	}

	protected void SetGluiTextFormatInChild(GameObject child, params object[] args)
	{
		if (child != null)
		{
			GluiText component = child.GetComponent<GluiText>();
			component.Text = string.Format(StringUtils.GetStringFromStringRef(component.TaggedStringReference), args);
		}
	}

	protected void SetGluiEnabledInChild(GameObject child, bool enabled)
	{
		if (child != null)
		{
			GluiWidget component = child.GetComponent<GluiWidget>();
			if (component != null)
			{
				component.Enabled = enabled;
			}
		}
	}

	protected void SetGluiTagInChild(GameObject child, string text)
	{
		if (child != null && !string.IsNullOrEmpty(text))
		{
			child.tag = text;
		}
	}

	protected void SetGluiMeterInChild(GameObject child, float newValue)
	{
		if (child != null)
		{
			GluiMeter component = child.GetComponent<GluiMeter>();
			component.Value = newValue;
		}
	}

	protected void SetGluiSpriteInChild(GameObject child, Texture texture)
	{
		if (child != null && texture != null)
		{
			GluiSprite component = child.GetComponent<GluiSprite>();
			if (component != null)
			{
				component.Texture = (Texture2D)texture;
				component.Refresh_Full();
			}
		}
	}

	protected void ClearGluiSpriteInChild(GameObject child)
	{
		if (child != null)
		{
			GluiSprite component = child.GetComponent<GluiSprite>();
			if (component != null)
			{
				component.Texture = null;
			}
		}
	}

	protected void HideObjectWidgets(GameObject child)
	{
		object[] components = child.GetComponents(typeof(GluiWidget));
		object[] array = components;
		for (int i = 0; i < array.Length; i++)
		{
			GluiWidget gluiWidget = (GluiWidget)array[i];
			gluiWidget.Visible = false;
			gluiWidget.Enabled = false;
		}
	}

	protected void HideObjectWidgets(GameObject[] children)
	{
		foreach (GameObject child in children)
		{
			HideObjectWidgets(child);
		}
	}

	protected void SetGluiFlipbookAnimInChild(GameObject child, Texture2D[] textureArray)
	{
		if (child != null)
		{
			GluiFlipbook component = child.GetComponent<GluiFlipbook>();
			if (component != null)
			{
				component.SetTextureArray(textureArray);
			}
		}
	}

	protected void SetGluiFlipbookAnimInChild(GameObject child, Texture texture)
	{
		if (child != null)
		{
			GluiFlipbook component = child.GetComponent<GluiFlipbook>();
			if (component != null)
			{
				component.SetTextureArray(texture);
			}
		}
	}

	protected void SendGluiEnableByValue(GameObject child, string valueToSend)
	{
		if (child != null)
		{
			Component[] components = child.GetComponents(typeof(IGluiElement_BaseSwitch));
			Component[] array = components;
			for (int i = 0; i < array.Length; i++)
			{
				IGluiElement_DataAdaptor gluiElement_DataAdaptor = (IGluiElement_DataAdaptor)array[i];
				gluiElement_DataAdaptor.SetGluiCustomElementData(valueToSend);
			}
		}
	}

	protected void SetGluiListDataInChild(GameObject child, object[] records)
	{
		if (child != null)
		{
			GluiList_Base gluiList_Base = child.GetComponent(typeof(GluiList_Base)) as GluiList_Base;
			if (gluiList_Base != null)
			{
				gluiList_Base.UpdateFromData(records);
			}
		}
	}

	protected void SetGluiButtonStateInChild(GameObject child, bool? locked, bool? selected)
	{
		if (!(child != null))
		{
			return;
		}
		GluiStandardButtonContainer gluiStandardButtonContainer = child.GetComponent(typeof(GluiStandardButtonContainer)) as GluiStandardButtonContainer;
		if (gluiStandardButtonContainer != null)
		{
			if (locked.HasValue)
			{
				gluiStandardButtonContainer.Locked = locked.Value;
			}
			if (selected.HasValue)
			{
				gluiStandardButtonContainer.Selected = selected.Value;
			}
		}
	}

	protected void SetGluiButtonOnReleaseInChild(GameObject child, string[] actions)
	{
		if (child != null)
		{
			GluiStandardButtonContainer gluiStandardButtonContainer = child.GetComponent(typeof(GluiStandardButtonContainer)) as GluiStandardButtonContainer;
			if (gluiStandardButtonContainer != null)
			{
				gluiStandardButtonContainer.onReleaseActions = actions;
			}
		}
	}
}
