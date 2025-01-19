using System;
using System.Collections;
using System.Text;
using UnityEngine;

public static class JSON
{
	private static object DJgetValue(string input, ref int pos)
	{
		while (input[pos] != '"' && input[pos] != '{' && input[pos] != '[' && (input[pos] < '0' || input[pos] > '9') && input[pos] != 't' && input[pos] != 'f' && input[pos] != 'n' && input[pos] != '@')
		{
			pos++;
		}
		switch (input[pos])
		{
		case '"':
			return DJgetString(input, ref pos);
		case '{':
			return DJgetObject(input, ref pos);
		case '[':
			return DJgetArray(input, ref pos);
		case 't':
			if (input.Substring(pos, 4).CompareTo("true") == 0)
			{
				pos += 4;
				return true;
			}
			throw new Exception("Invalid JSON");
		case 'f':
			if (input.Substring(pos, 5).CompareTo("false") == 0)
			{
				pos += 5;
				return false;
			}
			throw new Exception("Invalid JSON");
		case 'n':
			if (input.Substring(pos, 4).CompareTo("null") == 0)
			{
				pos += 4;
				return null;
			}
			throw new Exception("Invalid JSON");
		case '@':
			return DJgetDateTime(input, ref pos);
		default:
			return DJgetNumber(input, ref pos);
		}
	}

	private static string DJgetString(string input, ref int pos)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (input[pos] != '"')
		{
			pos++;
		}
		pos++;
		while (input[pos] != '"')
		{
			if (input[pos] == '\\')
			{
				pos++;
				switch (input[pos])
				{
				case '"':
					stringBuilder.Append('"');
					break;
				case '\\':
					stringBuilder.Append('\\');
					break;
				case '/':
					stringBuilder.Append('/');
					break;
				case 'b':
					stringBuilder.Append('\b');
					break;
				case 'f':
					stringBuilder.Append('\f');
					break;
				case 'n':
					stringBuilder.Append('\n');
					break;
				case 'r':
					stringBuilder.Append('\r');
					break;
				case 't':
					stringBuilder.Append('\t');
					break;
				case 'u':
				{
					pos++;
					string value = char.ConvertFromUtf32(Convert.ToInt32(input.Substring(pos, 4), 16));
					pos += 3;
					stringBuilder.Append(value);
					break;
				}
				}
			}
			else
			{
				stringBuilder.Append(input[pos]);
			}
			pos++;
		}
		pos++;
		return stringBuilder.ToString();
	}

	private static double DJgetNumber(string input, ref int pos)
	{
		double num = 0.0;
		bool flag = false;
		while ((input[pos] < '0' || input[pos] > '9') && input[pos] != '-')
		{
			pos++;
		}
		if (input[pos] == '-')
		{
			flag = true;
			pos++;
		}
		while (input[pos] >= '0' && input[pos] <= '9')
		{
			num *= 10.0;
			num += (double)(input[pos] - 48);
			pos++;
		}
		double num2 = 1.0;
		if (input[pos] == '.')
		{
			pos++;
			while (input[pos] >= '0' && input[pos] <= '9')
			{
				num2 *= 10.0;
				num += (double)(input[pos] - 48) / num2;
				pos++;
			}
		}
		if (input[pos] == 'e' || input[pos] == 'E')
		{
			pos++;
			bool flag2 = false;
			int num3 = 0;
			if (input[pos] == '-')
			{
				flag2 = true;
				pos++;
			}
			if (input[pos] == '+')
			{
				pos++;
			}
			while (input[pos] >= '0' && input[pos] <= '9')
			{
				num3 *= 10;
				num3 += input[pos] - 48;
				pos++;
			}
			if (flag2)
			{
				num3 = -num3;
			}
			num *= (double)Mathf.Pow(10f, num3);
		}
		if (flag)
		{
			num = 0.0 - num;
		}
		return num;
	}

	private static Hashtable DJgetObject(string input, ref int pos)
	{
		Hashtable hashtable = new Hashtable();
		while (input[pos] != '{')
		{
			pos++;
		}
		pos++;
		while (input[pos] != '"' && input[pos] != '}')
		{
			pos++;
		}
		if (input[pos] != '}')
		{
			pos--;
			do
			{
				pos++;
				string key = DJgetString(input, ref pos);
				while (input[pos] != ':')
				{
					pos++;
				}
				pos++;
				object value = DJgetValue(input, ref pos);
				if (hashtable.Contains(key))
				{
					hashtable[key] = value;
				}
				else
				{
					hashtable.Add(key, value);
				}
				while (input[pos] != ',' && input[pos] != '}')
				{
					pos++;
				}
			}
			while (input[pos] == ',');
			pos++;
			return hashtable;
		}
		return hashtable;
	}

	private static ArrayList DJgetArray(string input, ref int pos)
	{
		ArrayList arrayList = new ArrayList();
		while (input[pos] != '[')
		{
			pos++;
		}
		pos++;
		while (input[pos] != ']' && input[pos] != '"' && input[pos] != '{' && input[pos] != '[' && (input[pos] < '0' || input[pos] > '9') && input[pos] != 't' && input[pos] != 'f' && input[pos] != 'n')
		{
			pos++;
		}
		if (input[pos] != ']')
		{
			pos--;
			do
			{
				pos++;
				object value = DJgetValue(input, ref pos);
				arrayList.Add(value);
				while (input[pos] != ',' && input[pos] != ']')
				{
					pos++;
				}
			}
			while (input[pos] == ',');
			pos++;
			return arrayList;
		}
		return arrayList;
	}

	private static DateTime DJgetDateTime(string input, ref int pos)
	{
		pos += 2;
		int num = pos;
		while (input[pos] != ']')
		{
			pos++;
		}
		DateTime result = DateTime.Parse(input.Substring(num, pos - num));
		pos++;
		return result;
	}

	public static object Decode(string input)
	{
		int pos = 0;
		try
		{
			return DJgetValue(input, ref pos);
		}
		catch (Exception)
		{
			return null;
		}
	}

	private static void EJgetObject(StringBuilder sb, Hashtable ht)
	{
		sb.Append('{');
		bool flag = true;
		foreach (object key in ht.Keys)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				sb.Append(", ");
			}
			EJgetString(sb, (string)key);
			sb.Append(": ");
			EJgetValue(sb, ht[key]);
		}
		sb.Append('}');
	}

	private static void EJgetArray(StringBuilder sb, ArrayList al)
	{
		sb.Append('[');
		bool flag = true;
		foreach (object item in al)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				sb.Append(", ");
			}
			EJgetValue(sb, item);
		}
		sb.Append(']');
	}

	private static void EJgetString(StringBuilder sb, string val)
	{
		sb.Append("\"");
		for (int i = 0; i < val.Length; i++)
		{
			switch (val[i])
			{
			case '"':
				sb.Append("\\\"");
				break;
			case '\\':
				if (i < val.Length - 1 && val[i + 1] == 'u')
				{
					sb.Append("\\u");
				}
				else
				{
					sb.Append("\\\\");
				}
				break;
			case '\b':
				sb.Append("\\b");
				break;
			case '\f':
				sb.Append("\\f");
				break;
			case '\n':
				sb.Append("\\n");
				break;
			case '\r':
				sb.Append("\\r");
				break;
			case '\t':
				sb.Append("\\t");
				break;
			default:
				sb.Append(val[i]);
				break;
			}
		}
		sb.Append("\"");
	}

	private static void EJgetDateTime(StringBuilder sb, DateTime val)
	{
		sb.Append("@[");
		sb.Append(val.ToString());
		sb.Append(']');
	}

	private static void EJgetValue(StringBuilder sb, object val)
	{
		if (val == null)
		{
			sb.Append("null");
			return;
		}
		if (val is Hashtable)
		{
			EJgetObject(sb, (Hashtable)val);
			return;
		}
		switch (val.GetType().ToString())
		{
		case "System.String":
			EJgetString(sb, (string)val);
			break;
		case "System.Double":
			sb.Append(val.ToString());
			break;
		case "System.Collections.ArrayList":
			EJgetArray(sb, (ArrayList)val);
			break;
		case "System.DateTime":
			EJgetDateTime(sb, (DateTime)val);
			break;
		case "System.Boolean":
			if ((bool)val)
			{
				sb.Append("true");
			}
			else
			{
				sb.Append("false");
			}
			break;
		default:
			sb.Append(val.ToString());
			break;
		}
	}

	public static string Encode(object input)
	{
		StringBuilder stringBuilder = new StringBuilder();
		EJgetValue(stringBuilder, input);
		return stringBuilder.ToString();
	}
}
