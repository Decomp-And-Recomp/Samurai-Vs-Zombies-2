using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class GluiSettings
{
	public struct FontConfig
	{
		public string name;

		public string[] languageOverrides;
	}

	public const string DefaultLocale = "enUS";

	public const string SaveAssetName = "Glui/GluiSettings";

	public const string BaseFontPath = "Glui/Fonts/";

	public const string BaseStringsPath = "Glui/Strings/";

	public static int MainLayer;

	public static bool HideDetails;

	public static Dictionary<string, GluiFont> Fonts;

	public static FontConfig[] FontsConfig;

	public static bool LoadAllFontsAtStartup;

	public static Dictionary<string, GluiLocale> Locales;

	public static string[] LocaleNames;

	public static string CurrentLocale;

	private static Dictionary<string, string> settingsTable;

	static GluiSettings()
	{
		MainLayer = 8;
		HideDetails = true;
		Fonts = new Dictionary<string, GluiFont>();
		FontsConfig = new FontConfig[0];
		LoadAllFontsAtStartup = true;
		Locales = new Dictionary<string, GluiLocale>();
		LocaleNames = new string[0];
		CurrentLocale = "enUS";
		Load();
		LoadAssets();
	}

	public static void LoadAssets()
	{
		if (LoadAllFontsAtStartup || Application.isEditor)
		{
			LoadAllGluiSettingsFonts();
		}
		LoadLocales();
	}

	public static void LoadAllGluiSettingsFonts()
	{
		for (int i = 0; i < FontsConfig.Length; i++)
		{
			LoadFontByName(FontsConfig[i].name);
		}
	}

	public static GluiFont LoadFontByName(string fontName)
	{
		GluiFont value;
		if (!Fonts.TryGetValue(fontName, out value))
		{
			value = new GluiFont(fontName);
			Fonts.Add(fontName, value);
		}
		return value;
	}

	public static void LoadFont(string fontName, GluiFont font)
	{
		if (!Fonts.ContainsKey(fontName))
		{
			Fonts.Add(fontName, font);
		}
	}

	public static GluiFont LoadOverrideFontForLanguage(string language)
	{
		if (string.IsNullOrEmpty(language))
		{
			return null;
		}
		for (int i = 0; i < FontsConfig.Length; i++)
		{
			if (FontsConfig[i].languageOverrides != null && !string.IsNullOrEmpty(Array.Find(FontsConfig[i].languageOverrides, (string l) => string.Equals(language, l))))
			{
				string name = FontsConfig[i].name;
				return LoadFontByName(name);
			}
		}
		return null;
	}

	private static void LoadLocales()
	{
		Locales = new Dictionary<string, GluiLocale>();
		for (int i = 0; i < LocaleNames.Length; i++)
		{
			GluiLocale value = new GluiLocale(LocaleNames[i]);
			Locales.Add(LocaleNames[i], value);
		}
	}

	public static void SaveLocales()
	{
		foreach (GluiLocale value in Locales.Values)
		{
			value.Save();
		}
	}

	public static void Load()
	{
		TextAsset textAsset = Resources.Load("Glui/GluiSettings") as TextAsset;
		if (textAsset == null)
		{
			return;
		}
		MemoryStream memoryStream = new MemoryStream(textAsset.bytes);
		if (memoryStream == null)
		{
			return;
		}
		StreamReader streamReader = new StreamReader(memoryStream);
		settingsTable = new Dictionary<string, string>();
		while (true)
		{
			string text = streamReader.ReadLine();
			if (string.IsNullOrEmpty(text))
			{
				break;
			}
			int num = text.IndexOf('=');
			string text2 = text.Substring(0, num);
			text2 = text2.Trim();
			string text3 = text.Substring(num + 1);
			text3 = text3.Trim();
			settingsTable.Add(text2, text3);
		}
		streamReader.Close();
		memoryStream.Close();
		MainLayer = LoadInt("MainLayer");
		HideDetails = LoadBool("HideDetails");
		if (DoesSettingExist("LoadAllFontsAtStartup"))
		{
			LoadAllFontsAtStartup = LoadBool("LoadAllFontsAtStartup");
		}
		else
		{
			LoadAllFontsAtStartup = true;
		}
		int num2 = LoadInt("FontCount");
		FontsConfig = new FontConfig[num2];
		for (int i = 0; i < num2; i++)
		{
			FontsConfig[i].name = LoadString("Font" + (i + 1));
			string text4 = LoadString("LanguageOverride" + (i + 1));
			if (!string.IsNullOrEmpty(text4))
			{
				FontsConfig[i].languageOverrides = text4.Split(',');
			}
			else
			{
				FontsConfig[i].languageOverrides = new string[0];
			}
		}
		int num3 = LoadInt("LocaleCount");
		LocaleNames = new string[num3];
		for (int j = 0; j < num3; j++)
		{
			LocaleNames[j] = LoadString("Locale" + (j + 1));
		}
		CurrentLocale = LoadString("CurrentLocale");
	}

	private static int LoadInt(string id)
	{
		string text = LoadString(id);
		if (!string.IsNullOrEmpty(text))
		{
			return int.Parse(text);
		}
		return 0;
	}

	private static bool LoadBool(string id)
	{
		string value = LoadString(id);
		if (!string.IsNullOrEmpty(value))
		{
			return bool.Parse(value);
		}
		return false;
	}

	private static string LoadString(string id)
	{
		string value;
		if (settingsTable.TryGetValue(id, out value))
		{
			return value;
		}
		return string.Empty;
	}

	private static bool DoesSettingExist(string id)
	{
		return LoadString(id) != string.Empty;
	}

	public static void Save(string path)
	{
		FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
		if (fileStream == null)
		{
			return;
		}
		StreamWriter streamWriter = new StreamWriter(fileStream);
		streamWriter.WriteLine("MainLayer = " + MainLayer);
		streamWriter.WriteLine("HideDetails = " + HideDetails);
		streamWriter.WriteLine("LoadAllFontsAtStartup = " + LoadAllFontsAtStartup);
		streamWriter.WriteLine("FontCount = " + FontsConfig.Length);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < FontsConfig.Length; i++)
		{
			streamWriter.WriteLine("Font" + (i + 1) + " = " + FontsConfig[i].name);
			if (FontsConfig[i].languageOverrides == null)
			{
				continue;
			}
			stringBuilder.Length = 0;
			stringBuilder.AppendFormat("LanguageOverride{0} = ", i + 1);
			for (int j = 0; j < FontsConfig[i].languageOverrides.Length; j++)
			{
				if (j != 0)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(FontsConfig[i].languageOverrides[j]);
			}
			streamWriter.WriteLine(stringBuilder.ToString());
		}
		streamWriter.WriteLine("LocaleCount = " + LocaleNames.Length);
		for (int k = 0; k < LocaleNames.Length; k++)
		{
			streamWriter.WriteLine("Locale" + (k + 1) + " = " + LocaleNames[k]);
		}
		streamWriter.WriteLine("CurrentLocale = " + CurrentLocale);
		streamWriter.Close();
		fileStream.Close();
	}
}
