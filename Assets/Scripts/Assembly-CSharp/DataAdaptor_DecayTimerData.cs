using System;
using UnityEngine;

[Serializable]
public class DataAdaptor_DecayTimerData : DataAdaptorBase
{
	public GameObject GluiText_TimeToNext;

	public override void SetData(object data)
	{
		DecayTimer decayTimer = (DecayTimer)data;
		if (decayTimer != null)
		{
			SetGluiTextInChild(GluiText_TimeToNext, StringUtils.FormatTime(decayTimer.TimeToNextTick(), StringUtils.TimeFormatType.MinuteSecond_Colons));
		}
	}

	public override void SetDefaultData()
	{
	}
}
