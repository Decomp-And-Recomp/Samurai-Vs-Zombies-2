using System;
using UnityEngine;

[AddComponentMenu("Glui Data/Persistent Adapter Generic")]
public class GluiPersistent_GenericAdapter : SafeEnable_Monobehaviour
{
	[Serializable]
	public class LookupData
	{
		public string PersistentValueToMatch;

		public string text1;

		public string text2;

		public Texture texture;

		public Rect atlasTexture;

		public float value1;

		public float value2;
	}

	public GameObject Widget_Text1;

	public GameObject Widget_Text2;

	public GameObject Widget_Texture;

	public GameObject Widget_AtlasTexture;

	public GameObject Widget_Value1;

	public GameObject Widget_Value2;

	public LookupData[] lookupData;

	public GluiPersistentDataWatcher watcher;

	private string GetPersistentValue()
	{
		return (string)watcher.GetData();
	}

	public override void OnSafeEnable()
	{
		watcher.StartWatching();
		watcher.Event_WatchedDataChanged += HandleWatcherEvent_WatchedDataChanged;
		UpdateOnDataChange();
	}

	private void HandleWatcherEvent_WatchedDataChanged(object data)
	{
		UpdateOnDataChange();
	}

	private void UpdateOnDataChange()
	{
		string lookupValue = (string)watcher.GetData();
		LookupData data = FindDataSet(lookupValue);
		SetData(data);
	}

	private LookupData FindDataSet(string lookupValue)
	{
		if (lookupValue == string.Empty)
		{
			return null;
		}
		LookupData[] array = this.lookupData;
		foreach (LookupData lookupData in array)
		{
			if (lookupData.PersistentValueToMatch == lookupValue)
			{
				return lookupData;
			}
		}
		return null;
	}

	private void SetData(LookupData data)
	{
		if (data != null)
		{
			string stringFromStringRef = StringUtils.GetStringFromStringRef(data.text1);
			if (!string.IsNullOrEmpty(stringFromStringRef))
			{
				SetTextInChild(Widget_Text1, stringFromStringRef);
			}
			else
			{
				SetTextInChild(Widget_Text1, data.text1);
			}
			stringFromStringRef = StringUtils.GetStringFromStringRef(data.text2);
			if (!string.IsNullOrEmpty(stringFromStringRef))
			{
				SetTextInChild(Widget_Text1, stringFromStringRef);
			}
			else
			{
				SetTextInChild(Widget_Text2, data.text2);
			}
			SetTextureInChild(Widget_Texture, data.texture);
			SetAtlasInChild(Widget_AtlasTexture, data.atlasTexture);
		}
	}

	protected void SetTextInChild(GameObject child, string text)
	{
		if (child != null)
		{
			GluiText component = child.GetComponent<GluiText>();
			component.Text = text;
		}
	}

	protected void SetTextureInChild(GameObject child, Texture texture)
	{
		if (child != null)
		{
			GluiSprite component = child.GetComponent<GluiSprite>();
			component.Texture = (Texture2D)texture;
		}
	}

	protected void SetValueInChild(GameObject child, float value)
	{
		if (!(child != null))
		{
		}
	}

	protected void SetAtlasInChild(GameObject child, Rect atlasRect)
	{
		if (child != null)
		{
			GluiSprite component = child.GetComponent<GluiSprite>();
			Texture texture = component.Texture;
			if ((bool)texture)
			{
				atlasRect.y = 1f - (0f - atlasRect.y - atlasRect.height) / (0f - (float)texture.height);
				atlasRect.x /= texture.width;
				atlasRect.height /= texture.height;
				atlasRect.width /= texture.width;
			}
			component.AtlasRect = atlasRect;
		}
	}
}
