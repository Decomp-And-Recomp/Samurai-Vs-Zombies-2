using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GluiFont
{
	public class Glyph
	{
		public ushort id;

		public short x;

		public short y;

		public short width;

		public short height;

		public short xOffset;

		public short yOffset;

		public short xAdvance;

		public byte page;

		public byte chnl;

		public Vector2 texPos = default(Vector2);

		public Vector2 texArea = default(Vector2);

		public Dictionary<ushort, short> kernings;
	}

	public readonly string name;

	public readonly Texture2D texture;

	public Dictionary<int, Glyph> glyphs;

	public readonly Vector4 glyphPadding;

	public readonly float lineHeight;

	public readonly float lineBase;

	public readonly float lineCapHeight;

	public readonly bool isPacked;

	public GluiFont(string fontName)
	{
		string path = "Glui/Fonts/" + fontName;
		TextAsset textAsset = Resources.Load(path, typeof(TextAsset)) as TextAsset;
		if (textAsset == null)
		{
			return;
		}
		MemoryStream memoryStream = new MemoryStream(textAsset.bytes);
		StreamReader streamReader = new StreamReader(memoryStream);
		string line = streamReader.ReadLine();
		name = ParseString(line, "face");
		name = name.Replace("\"", " ");
		name = name.Trim();
		string line2 = streamReader.ReadLine();
		lineHeight = ParseInt(line2, "lineHeight");
		glyphPadding = ParseVector4(line, "padding");
		lineBase = (float)ParseInt(line2, "base") - glyphPadding[0];
		int num = ParseInt(line2, "packed");
		isPacked = num != 0;
		lineCapHeight = 0f;
		string line3 = streamReader.ReadLine();
		string text = ParseString(line3, "file");
		text = text.Substring(text.IndexOf('"') + 1);
		text = text.Substring(0, text.IndexOf('.'));
		text = "Glui/Fonts/" + text;
		texture = Resources.Load(text, typeof(Texture2D)) as Texture2D;
		if (texture == null)
		{
			return;
		}
		string line4 = streamReader.ReadLine();
		int num2 = ParseInt(line4, "count");
		glyphs = new Dictionary<int, Glyph>();
		for (int i = 0; i < num2; i++)
		{
			string line5 = streamReader.ReadLine();
			Glyph glyph = new Glyph();
			glyph.id = (ushort)ParseInt(line5, "id");
			glyph.x = (short)ParseInt(line5, "x");
			glyph.y = (short)ParseInt(line5, "y");
			glyph.width = (short)ParseInt(line5, "width");
			glyph.height = (short)ParseInt(line5, "height");
			glyph.xOffset = (short)ParseInt(line5, "xoffset");
			glyph.yOffset = (short)ParseInt(line5, "yoffset");
			glyph.xAdvance = (short)ParseInt(line5, "xadvance");
			glyph.page = (byte)ParseInt(line5, "page");
			glyph.chnl = (byte)ParseInt(line5, "chnl");
			glyph.texPos.x = (float)glyph.x / (float)texture.width;
			glyph.texPos.y = (float)glyph.y / (float)texture.height;
			glyph.texArea.x = (float)glyph.width / (float)texture.width;
			glyph.texArea.y = (float)glyph.height / (float)texture.height;
			glyph.texPos.y = 1f - (glyph.texPos.y + glyph.texArea.y);
			glyphs.Add(glyph.id, glyph);
		}
		string line6 = streamReader.ReadLine();
		int num3 = ParseInt(line6, "count");
		for (int j = 0; j < num3; j++)
		{
			string line7 = streamReader.ReadLine();
			ushort key = (ushort)ParseInt(line7, "first");
			ushort key2 = (ushort)ParseInt(line7, "second");
			short value = (short)ParseInt(line7, "amount");
			Glyph value2;
			if (!glyphs.TryGetValue(key, out value2))
			{
				return;
			}
			if (value2.kernings == null)
			{
				value2.kernings = new Dictionary<ushort, short>();
			}
			value2.kernings.Add(key2, value);
		}
		streamReader.Close();
		memoryStream.Close();
	}

	public int StringWidth(string text, int kerningOffset, float glyphScale)
	{
		float num = 0f;
		int length = text.Length;
		for (int i = 0; i < length; i++)
		{
			Glyph value;
			if (!glyphs.TryGetValue(text[i], out value))
			{
				continue;
			}
			int num2 = value.xAdvance + kerningOffset;
			if (value.kernings != null && i < length - 1)
			{
				short value2 = 0;
				if (value.kernings.TryGetValue(text[i + 1], out value2))
				{
					num2 += value2;
				}
			}
			num += (float)num2;
		}
		return (int)(num * glyphScale);
	}

	public int StringWidth(string text)
	{
		return StringWidth(text, 0, 1f);
	}

	private int ParseInt(string line, string token)
	{
		string text = ParseString(line, token);
		if (text != string.Empty)
		{
			return int.Parse(text);
		}
		return 0;
	}

	private Vector4 ParseVector4(string line, string token)
	{
		string[] results;
		ParseStrings(line, token, out results);
		if (results.Length > 3)
		{
			return new Vector4(int.Parse(results[0]), int.Parse(results[1]), int.Parse(results[2]), int.Parse(results[3]));
		}
		return Vector4.zero;
	}

	private bool ParseInts(string line, string token, out int[] results)
	{
		string[] results2;
		if (ParseStrings(line, token, out results2))
		{
			results = new int[results2.Length];
			for (int i = 0; i < results2.Length; i++)
			{
				results[i] = int.Parse(results2[i]);
			}
			return true;
		}
		results = new int[0];
		return false;
	}

	private string ParseString(string line, string token)
	{
		if (string.IsNullOrEmpty(line))
		{
			return string.Empty;
		}
		string[] results;
		if (ParseStrings(line, token, out results))
		{
			return results[0];
		}
		return string.Empty;
	}

	private bool ParseStrings(string line, string token, out string[] results)
	{
		results = new string[0];
		int num = line.IndexOf(token);
		if (num == -1)
		{
			return false;
		}
		string text = line.Substring(num);
		num = text.IndexOf('=');
		if (num == -1)
		{
			return false;
		}
		text = text.Substring(num + 1);
		num = text.IndexOf(' ');
		if (num == -1)
		{
			num = text.Length;
		}
		text = text.Substring(0, num);
		text = text.Trim();
		results = text.Split(',');
		return true;
	}
}
