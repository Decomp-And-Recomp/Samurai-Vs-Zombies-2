using System;
using UnityEngine;

[Serializable]
public class DataAdaptor_Digit : DataAdaptorBase
{
	public GameObject text_Name;

	public override void SetData(object data)
	{
		string text = (string)data;
		SetGluiTextInChild(text_Name, text);
	}
}
