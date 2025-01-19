using UnityEngine;

[AddComponentMenu("Glui Modulator/Text Number Format")]
[ExecuteInEditMode]
public class GluiModulator_Text_NumberFormat : GluiModulator
{
	public enum NumberFormat
	{
		Commas_1000 = 0,
		Time_MinuteSecond_Colons = 1,
		Time_HourMinuteSecond_Colons = 2,
		Time_HourMinute_Longhand = 3
	}

	public NumberFormat format;

	protected override void OnTextChanged(ref string text, string originalText)
	{
		float result;
		if (float.TryParse(text, out result))
		{
			switch (format)
			{
			case NumberFormat.Commas_1000:
				text = string.Format("{0:N0}", result);
				break;
			case NumberFormat.Time_MinuteSecond_Colons:
			case NumberFormat.Time_HourMinuteSecond_Colons:
			case NumberFormat.Time_HourMinute_Longhand:
				text = formatTime(result, format);
				break;
			}
		}
	}

	private string formatTime(float timeInSeconds, NumberFormat format)
	{
		int num = (int)timeInSeconds;
		int num2 = (int)(timeInSeconds / 60f);
		int num3 = num2 / 60;
		num -= num2 * 60;
		string text = string.Empty;
		switch (format)
		{
		case NumberFormat.Time_MinuteSecond_Colons:
			text = text + num2 + ":";
			if (num < 10)
			{
				text += "0";
			}
			text += num;
			break;
		case NumberFormat.Time_HourMinuteSecond_Colons:
			num2 -= num3 * 60;
			text = num3 + ":";
			if (num2 < 10)
			{
				text += "0";
			}
			text = text + num2 + ":";
			if (num < 10)
			{
				text += "0";
			}
			text += num;
			break;
		case NumberFormat.Time_HourMinute_Longhand:
			num2 -= num3 * 60;
			if (num3 > 0)
			{
				text = num3 + StringUtils.GetStringFromStringRef("LocalizedStrings", "code_Hour_Short") + " ";
			}
			if (num2 > 0)
			{
				text = text + num2 + StringUtils.GetStringFromStringRef("LocalizedStrings", "code_Minute_Short") + " ";
			}
			if (num > 0)
			{
				text = text + num + StringUtils.GetStringFromStringRef("LocalizedStrings", "code_Second_Short");
			}
			break;
		}
		return text;
	}
}
