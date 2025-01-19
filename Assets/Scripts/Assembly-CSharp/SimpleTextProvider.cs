using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class SimpleTextProvider : SaveProvider
{
	private const string kVersion = "0.1";

	private static readonly string kSeperator = "\",\"";

	private static readonly string kDeviceDataToken = "DEVICEDATA:";

	private static readonly string kVersionKey = "kVersion";

	private Dictionary<string, string> strings = new Dictionary<string, string>();

	public Dictionary<string, string> Strings
	{
		get
		{
			return strings;
		}
	}

	public string Version
	{
		get
		{
			return strings[kVersionKey];
		}
	}

	public SimpleTextProvider()
	{
		strings[kVersionKey] = "0.1";
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, string> @string in strings)
		{
			stringBuilder.AppendFormat("\"{0}{1}{2}\"\n", @string.Key, kSeperator, @string.Value);
		}
		return stringBuilder.ToString();
	}

	protected override bool Write(Stream dataStream, IEnumerable<SaveTarget> targets, DeviceData deviceData)
	{
		if (dataStream == null)
		{
			return false;
		}
		using (StreamWriter streamWriter = new StreamWriter(dataStream))
		{
			WriteDeviceData(deviceData, streamWriter);
			streamWriter.Write(ToString());
		}
		return true;
	}

	protected override bool Read(SaveHeader dataHeader, Stream dataStream, SaveTarget target, DeviceData deviceData)
	{
		if (dataStream == null)
		{
			return false;
		}
		strings.Clear();
		using (StreamReader streamReader = new StreamReader(dataStream))
		{
			if (dataHeader.UseDeviceData)
			{
				ReadDeviceData(streamReader, deviceData);
			}
			while (!streamReader.EndOfStream)
			{
				string text = streamReader.ReadLine();
				int num = text.IndexOf(kSeperator);
				string key = text.Substring(1, num - 1);
				string value = text.Substring(num + 3, text.Length - num - 4);
				strings[key] = value;
			}
		}
		return true;
	}

	private void WriteDeviceData(DeviceData deviceData, StreamWriter writer)
	{
		if (deviceData == null)
		{
			return;
		}
		writer.Write(string.Format("{0}{1}\n", kDeviceDataToken, deviceData.Count));
		foreach (KeyValuePair<string, DeviceDataEntry> deviceDatum in deviceData)
		{
			writer.WriteLine(JSON.Encode(deviceDatum.Value));
		}
	}

	private void ReadDeviceData(StreamReader reader, DeviceData deviceData)
	{
		long position = reader.BaseStream.Position;
		string text = reader.ReadLine();
		if (text.StartsWith(kDeviceDataToken))
		{
			int result;
			if (!int.TryParse(text.Substring(kDeviceDataToken.Length, text.Length - kDeviceDataToken.Length), out result))
			{
				return;
			}
			for (int i = 0; i < result; i++)
			{
				text = reader.ReadLine();
				if (deviceData != null)
				{
					DeviceDataEntry ifNewer = new DeviceDataEntry(JSON.Decode(text) as Hashtable);
					deviceData.SetIfNewer(ifNewer);
				}
			}
		}
		else
		{
			reader.BaseStream.Seek(position, SeekOrigin.Begin);
		}
	}
}
