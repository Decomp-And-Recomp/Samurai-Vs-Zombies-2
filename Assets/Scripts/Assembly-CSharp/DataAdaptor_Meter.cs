using System;
using UnityEngine;

[Serializable]
public class DataAdaptor_Meter : DataAdaptorBase
{
	public GameObject gluiMeter;

	public bool persistentValueIsFloat;

	public string persistentMaxValueName;

	public int constantMaxValue;

	public override void SetData(object data)
	{
		float num = 0f;
		if (persistentValueIsFloat)
		{
			num = (float)data;
		}
		else
		{
			int num2 = (int)data;
			int num3 = ((!string.IsNullOrEmpty(persistentMaxValueName)) ? ((int)SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData(persistentMaxValueName)) : constantMaxValue);
			num = (float)num2 / (float)num3;
		}
		SetGluiMeterInChild(gluiMeter, num);
	}
}
