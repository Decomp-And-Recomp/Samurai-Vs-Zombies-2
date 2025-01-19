using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace HTTPS
{
	public class HTTPSResponse
	{
		public int status = 200;

		public string message = "OK";

		public byte[] bytes;

		private List<byte[]> chunks;

		private Dictionary<string, List<string>> headers = new Dictionary<string, List<string>>();

		public string Text
		{
			get
			{
				if (bytes == null)
				{
					return string.Empty;
				}
				return Encoding.UTF8.GetString(bytes);
			}
		}

		public AssetBundleCreateRequest Asset
		{
			get
			{
				return AssetBundle.LoadFromMemoryAsync(bytes);
			}
		}

		public void AddHeader(string name, string value)
		{
			GetHeaders(name).Add(value.Trim());
		}

		public void SetHeader(string name, string value)
		{
			List<string> list = GetHeaders(name);
			list.Clear();
			list.Add(value.Trim());
		}

		public List<string> GetHeaders(string name)
		{
			name = name.Trim();
			foreach (string key in headers.Keys)
			{
				if (string.Compare(name, key, true) == 0)
				{
					return headers[key];
				}
			}
			List<string> list = new List<string>();
			headers[name] = list;
			return list;
		}

		public string GetHeader(string name)
		{
			List<string> list = GetHeaders(name);
			if (list.Count == 0)
			{
				return string.Empty;
			}
			return list[list.Count - 1];
		}

		private string ReadLine(Stream stream)
		{
			List<byte> list = new List<byte>();
			while (true)
			{
				byte b = (byte)stream.ReadByte();
				if (b == HTTPSRequest.EOL[1])
				{
					break;
				}
				list.Add(b);
			}
			return Encoding.ASCII.GetString(list.ToArray()).Trim();
		}

		private string[] ReadKeyValue(Stream stream)
		{
			string text = ReadLine(stream);
			if (text == string.Empty)
			{
				return null;
			}
			int num = text.IndexOf(':');
			if (num == -1)
			{
				return null;
			}
			return new string[2]
			{
				text.Substring(0, num).Trim(),
				text.Substring(num + 1).Trim()
			};
		}

		public AssetBundleCreateRequest TakeAsset()
		{
			byte[] array = TakeChunk();
			if (array == null)
			{
				return null;
			}
			return AssetBundle.LoadFromMemoryAsync(array);
		}

		public byte[] TakeChunk()
		{
			byte[] result = null;
			lock (chunks)
			{
				if (chunks.Count > 0)
				{
					result = chunks[0];
					chunks.RemoveAt(0);
					return result;
				}
				return result;
			}
		}

		public void ReadFromStream(Stream inputStream)
		{
			string[] array = ReadLine(inputStream).Split(' ');
			MemoryStream memoryStream = new MemoryStream();
			if (!int.TryParse(array[1], out status))
			{
				throw new HTTPException("Bad Status Code");
			}
			message = string.Join(" ", array, 2, array.Length - 2);
			headers.Clear();
			while (true)
			{
				string[] array2 = ReadKeyValue(inputStream);
				if (array2 == null)
				{
					break;
				}
				AddHeader(array2[0], array2[1]);
			}
			if (string.Compare(GetHeader("Transfer-Encoding"), "chunked", true) == 0)
			{
				chunks = new List<byte[]>();
				while (true)
				{
					string text = ReadLine(inputStream);
					if (text == "0")
					{
						break;
					}
					int num = int.Parse(text, NumberStyles.AllowHexSpecifier);
					for (int i = 0; i < num; i++)
					{
						memoryStream.WriteByte((byte)inputStream.ReadByte());
					}
					lock (chunks)
					{
						chunks.Add(memoryStream.ToArray());
					}
					memoryStream.SetLength(0L);
					inputStream.ReadByte();
					inputStream.ReadByte();
				}
				lock (chunks)
				{
					chunks.Add(new byte[0]);
				}
				while (true)
				{
					string[] array3 = ReadKeyValue(inputStream);
					if (array3 == null)
					{
						break;
					}
					AddHeader(array3[0], array3[1]);
				}
				List<byte> list = new List<byte>();
				foreach (byte[] chunk in chunks)
				{
					list.AddRange(chunk);
				}
				bytes = list.ToArray();
			}
			else
			{
				int result = 0;
				int.TryParse(GetHeader("Content-Length"), out result);
				for (int j = 0; j < result; j++)
				{
					memoryStream.WriteByte((byte)inputStream.ReadByte());
				}
				bytes = memoryStream.ToArray();
			}
		}
	}
}
