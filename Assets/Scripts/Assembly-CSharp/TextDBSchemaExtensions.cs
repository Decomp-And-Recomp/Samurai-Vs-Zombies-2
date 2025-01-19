using System;

public static class TextDBSchemaExtensions
{
	public static float GetFloat(this TextDBSchema[] data, string key)
	{
		float result = 0f;
		TextDBSchema textDBSchema = Array.Find(data, (TextDBSchema d) => d.key.Equals(key));
		if (textDBSchema != null)
		{
			float.TryParse(textDBSchema.value, out result);
		}
		return result;
	}

	public static int GetInt(this TextDBSchema[] data, string key)
	{
		int result = 0;
		TextDBSchema textDBSchema = Array.Find(data, (TextDBSchema d) => d.key.Equals(key));
		if (textDBSchema != null)
		{
			int.TryParse(textDBSchema.value, out result);
		}
		return result;
	}

	public static bool GetBool(this TextDBSchema[] data, string key)
	{
		bool result = false;
		TextDBSchema textDBSchema = Array.Find(data, (TextDBSchema d) => d.key.Equals(key));
		if (textDBSchema != null)
		{
			bool.TryParse(textDBSchema.value, out result);
		}
		return result;
	}

	public static string GetString(this TextDBSchema[] data, string key)
	{
		TextDBSchema textDBSchema = Array.Find(data, (TextDBSchema d) => d.key.Equals(key));
		if (textDBSchema != null)
		{
			return textDBSchema.value;
		}
		return string.Empty;
	}

	public static int Count(this TextDBSchema[] data, string prefix)
	{
		int num = 0;
		foreach (TextDBSchema textDBSchema in data)
		{
			if (textDBSchema.key.StartsWith(prefix))
			{
				num++;
			}
		}
		return num;
	}

	public static int GetMaxLevels(this TextDBSchema[] data)
	{
		int num = 0;
		foreach (TextDBSchema textDBSchema in data)
		{
			string[] array = textDBSchema.key.Split(TextDBSchema.kChildSeperator);
			int result = 0;
			if (int.TryParse(array[0], out result) && result > num)
			{
				num = result;
			}
		}
		return num;
	}
}
