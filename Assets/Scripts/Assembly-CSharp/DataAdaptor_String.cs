using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("")]
public class DataAdaptor_String : DataAdaptorBase
{
	public GameObject GluiText_String;

	public override void SetData(object data)
	{
		string text = data.ToString();
		string stringFromStringRef = StringUtils.GetStringFromStringRef(text);
		if (!string.IsNullOrEmpty(stringFromStringRef))
		{
			SetGluiTextInChild(GluiText_String, stringFromStringRef);
		}
		else
		{
			SetGluiTextInChild(GluiText_String, text);
		}
	}
}
