using System;
using System.Net;
using System.Text;

namespace Gamespy.Common
{
	public static class TypeConverters
	{
		public static string ByteArrayToHexString(byte[] bytes)
		{
			string text = BitConverter.ToString(bytes);
			StringBuilder stringBuilder = new StringBuilder(text.Length / 2 - 2);
			string text2 = text;
			foreach (char c in text2)
			{
				if (c != '-')
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}

		public static byte[] HexStringToByteArray(string text)
		{
			byte[] array = new byte[text.Length / 2];
			int num = 0;
			int num2 = 0;
			text = text.ToUpper();
			while (num < text.Length)
			{
				num2 = ((text[num] < '0' || text[num] > '9') ? (text[num] - 65 + 10) : (text[num] - 48));
				array[num / 2] = Convert.ToByte(num2 * 16);
				num++;
				num2 = ((text[num] < '0' || text[num] > '9') ? (text[num] - 65 + 10) : (text[num] - 48));
				array[num / 2] += Convert.ToByte(num2);
				num++;
			}
			return array;
		}

		public static string LongToIP(long longIP)
		{
			return new IPAddress(longIP).ToString();
		}
	}
}
