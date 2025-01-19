using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class StringUtils
{
	public enum TimeFormatType
	{
		MinuteSecond_Colons = 0,
		HourMinuteSecond_Colons = 1,
		HourMinute_Longhand = 2,
		DaysOrHMS = 3
	}

	private static CultureInfo cultureInfo;

	public static string GetStringFromStringRef(string table, string stringRef)
	{
		return GetStringFromStringRef(new DataBundleRecordKey(table, stringRef));
	}

	public static string GetStringFromStringRef(DataBundleRecordKey stringRef)
	{
		if (DataBundleRecordKey.IsNullOrEmpty(stringRef))
		{
			return string.Empty;
		}
		if (DataBundleRuntime.Instance == null)
		{
			return string.Empty;
		}
		string text = DataBundleRuntime.Instance.GetValue<string>(typeof(TaggedString), stringRef.ToString(), "stringValue", false);
		if (text == null)
		{
			return string.Empty;
		}
		if (text.Contains("Apple Store"))
		{
			text = (AJavaTools.Properties.IsBuildGoogle() ? text.Replace("Apple Store", "Google Play Store") : (AJavaTools.Properties.IsBuildAmazon() ? text.Replace("Apple Store", "Amazon Appstore") : ((!AJavaTools.Properties.IsBuildTStore()) ? text.Replace("Apple Store", "Google Play Store") : text.Replace("Apple Store", "T store"))));
		}
		else if (text.Contains("App Store"))
		{
			text = (AJavaTools.Properties.IsBuildGoogle() ? text.Replace("App Store", "Google Play Store") : (AJavaTools.Properties.IsBuildAmazon() ? text.Replace("App Store", "Amazon Appstore") : ((!AJavaTools.Properties.IsBuildTStore()) ? text.Replace("App Store", "Google Play Store") : text.Replace("App Store", "T store"))));
		}
		else if (text.Contains("iCloud"))
		{
			text = (AJavaTools.Properties.IsBuildGoogle() ? text.Replace("iCloud", GetStringFromStringRef("LocalizedStrings", "IDS_ICLOUD_ANDROID")) : (AJavaTools.Properties.IsBuildAmazon() ? text.Replace("iCloud", "Amazon Appstore") : ((!AJavaTools.Properties.IsBuildTStore()) ? text.Replace("iCloud", "Cloud") : text.Replace("iCloud", GetStringFromStringRef("LocalizedStrings", "IDS_ICLOUD_ANDROID")))));
		}
		if (stringRef.ToString().Contains("MenuFixedStrings.LegalText_Help"))
		{
			text = text + "\n\n" + AJavaTools.Properties.GetBuildTag();
		}
		text = text.Replace("\n", "<N>");
		text = text.Replace("\\n", "<N>");
		if (text.Contains("Wave<N>") && GameObject.Find("StoreScreen(Clone)") != null)
		{
			text = "Wave {0}";
		}
		return text;
	}

	public static Regex BuildSplitRegex(string delimiter, string qualifier, bool ignoreCase)
	{
		string pattern = string.Format("{0}(?=(?:[^{1}]*{1}[^{1}]*{1})*(?![^{1}]*{1}))", Regex.Escape(delimiter), Regex.Escape(qualifier));
		RegexOptions regexOptions = RegexOptions.Multiline;
		if (ignoreCase)
		{
			regexOptions |= RegexOptions.IgnoreCase;
		}
		return new Regex(pattern, regexOptions);
	}

	public static string FormatPriceString(float priceInDollars)
	{
		AcquireCultureInfo();
		return FilterCulturalCharacters(priceInDollars.ToString("#,###.##", cultureInfo.NumberFormat));
	}

	public static string FormatAmountString(int amount)
	{
		AcquireCultureInfo();
		return FilterCulturalCharacters(amount.ToString("N0", cultureInfo.NumberFormat));
	}

	public static string FormatAmountString(long amount)
	{
		AcquireCultureInfo();
		return FilterCulturalCharacters(amount.ToString("N0", cultureInfo.NumberFormat));
	}

	private static string FilterCulturalCharacters(string originalString)
	{
		return originalString.Replace('\u00a0', ' ');
	}

	public static void ClearCultureInfo()
	{
		cultureInfo = null;
	}

	private static void AcquireCultureInfo()
	{
		if (cultureInfo == null)
		{
			string name;
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				List<string> culturesFromCountryCode = GetCulturesFromCountryCode(NUF.GetCountry());
				name = FindClosestCultureCodeForLanguage(culturesFromCountryCode, NUF.GetLanguage());
			}
			else
			{
				name = "en-US";
			}
			cultureInfo = new CultureInfo(name);
		}
	}

	private static List<string> GetCulturesFromCountryCode(string countryCode)
	{
		List<string> list = new List<string>();
		CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
		CultureInfo[] array = cultures;
		foreach (CultureInfo cultureInfo in array)
		{
			if (!cultureInfo.IsNeutralCulture && cultureInfo.Name.Length > 2 && CompareCountryCode(cultureInfo.Name, countryCode))
			{
				list.Add(cultureInfo.Name);
			}
		}
		return list;
	}

	private static string FindClosestCultureCodeForLanguage(List<string> culturesList, string languageCode)
	{
		if (culturesList == null || culturesList.Count == 0)
		{
			return "en-US";
		}
		foreach (string cultures in culturesList)
		{
			if (CompareLanguageCode(cultures, languageCode))
			{
				return cultures;
			}
		}
		return culturesList[0];
	}

	private static bool CompareLanguageCode(string cultureCode, string languageCode)
	{
		string[] array = cultureCode.Split('-');
		if (array.Length > 0)
		{
			return string.Compare(array[0], languageCode, true) == 0;
		}
		return false;
	}

	private static bool CompareCountryCode(string cultureCode, string countryCode)
	{
		string[] array = cultureCode.Split('-');
		if (array.Length == 2)
		{
			return string.Compare(array[1], countryCode, true) == 0;
		}
		return false;
	}

	public static string FormatTime(float timeInSeconds, TimeFormatType format)
	{
		int num = (int)timeInSeconds;
		int num2 = (int)(timeInSeconds / 60f);
		int num3 = num2 / 60;
		num -= num2 * 60;
		string text = string.Empty;
		if (format == TimeFormatType.DaysOrHMS && num3 < 25)
		{
			format = TimeFormatType.HourMinuteSecond_Colons;
		}
		switch (format)
		{
		case TimeFormatType.MinuteSecond_Colons:
			text = text + num2 + ":";
			if (num < 10)
			{
				text += "0";
			}
			text += num;
			break;
		case TimeFormatType.HourMinuteSecond_Colons:
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
		case TimeFormatType.HourMinute_Longhand:
			num2 -= num3 * 60;
			if (num3 > 0)
			{
				text = num3 + GetStringFromStringRef("LocalizedStrings", "code_Hour_Short") + " ";
			}
			if (num2 > 0)
			{
				text = text + num2 + GetStringFromStringRef("LocalizedStrings", "code_Minute_Short") + " ";
			}
			if (num > 0)
			{
				text = text + num + GetStringFromStringRef("LocalizedStrings", "code_Second_Short");
			}
			break;
		case TimeFormatType.DaysOrHMS:
		{
			int num4 = num3 / 24;
			num3 -= num4 * 24;
			text = num4 + GetStringFromStringRef("MenuFixedStrings", "Menu_Days") + " ";
			if (num3 < 10)
			{
				text += "0";
			}
			text = text + num3 + GetStringFromStringRef("MenuFixedStrings", "Menu_Hours") + " ";
			break;
		}
		}
		return text;
	}

	public static string formatCurrency(float dollars)
	{
		return string.Format(dollars.ToString("N2"));
	}
}
