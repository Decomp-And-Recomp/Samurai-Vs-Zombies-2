public static class CStringUtils
{
	private static string OpenTag = "<UnityIAP_Key";

	private static string CloseTag = "</UnityIAP_Key";

	public static bool IsIAPXmlFormat(string src)
	{
		if (src.IndexOf(OpenTag) >= 0)
		{
			return true;
		}
		return false;
	}

	public static string ExtractFirstValueFromStringForKey(string src, string key)
	{
		string text = src;
		if (text.IndexOf(HeadTag(key)) < 0)
		{
			return string.Empty;
		}
		text = text.Substring(text.IndexOf(HeadTag(key)));
		int num = 1;
		int num2 = 1;
		while (num2 > 0)
		{
			int num3 = text.IndexOf(OpenTag, num);
			int num4 = text.IndexOf(CloseTag, num);
			bool flag = true;
			if (num3 < num && num4 < num)
			{
				num = 0;
				num2 = 0;
				break;
			}
			if (num4 < num || (num3 >= num && num3 < num4))
			{
				num = num3 + 1;
				num2++;
				flag = false;
			}
			if (num3 < num || (num4 >= num && num4 < num3))
			{
				num = num4 + 1;
				num2--;
				flag = false;
			}
			if (flag)
			{
				num = 0;
				num2 = 0;
				break;
			}
		}
		if (num > 0)
		{
			text = text.Substring(0, num - 1);
			return text.Substring(text.IndexOf(">") + 1);
		}
		return string.Empty;
	}

	public static int GetAmountOfValuesFromStringForKey(string src, string key)
	{
		string text = src;
		int num = 0;
		while (text.IndexOf(HeadTag(key)) >= 0)
		{
			text = text.Substring(text.IndexOf(HeadTag(key)) + 1);
			num++;
		}
		return num;
	}

	public static string ExtractFromStringForKeyValue(string src, string key, int n)
	{
		string text = src;
		if (n <= 0)
		{
			text = string.Empty;
		}
		while (n > 0)
		{
			n--;
			if (n == 0)
			{
				text = ExtractFirstValueFromStringForKey(text, key);
				continue;
			}
			if (text.IndexOf(HeadTag(key)) >= 0)
			{
				text = text.Substring(text.IndexOf(HeadTag(key)) + 1);
				continue;
			}
			text = string.Empty;
			break;
		}
		return text;
	}

	private static string HeadTag(string key)
	{
		return "<UnityIAP_Key=" + key + ">";
	}
}
