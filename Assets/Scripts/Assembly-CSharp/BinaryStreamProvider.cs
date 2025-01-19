using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class BinaryStreamProvider : SaveProvider
{
	public abstract class SaveHandler
	{
		public float Version { get; protected set; }

		public string Identifier { get; protected set; }

		public virtual void Save(BinaryStreamProvider sp, IEnumerable<SaveTarget> targets)
		{
		}

		public virtual void Load(BinaryStreamProvider sp, float handlerVersion, SaveTarget target)
		{
		}
	}

	public const float kVersionNumber = 1f;

	private static readonly string kVersionIdentifier = "ver";

	private static readonly string kEndIdentifier = "end";

	private BinaryWriter writer;

	private BinaryReader reader;

	private Dictionary<string, SaveHandler> saveHandlers = new Dictionary<string, SaveHandler>();

	public float VersionNumber { get; private set; }

	public void AddHandler(SaveHandler handler)
	{
		saveHandlers[handler.Identifier] = handler;
	}

	public void RemoveHandler(SaveHandler handler)
	{
		saveHandlers.Remove(handler.Identifier);
	}

	protected override bool Write(Stream dataStream, IEnumerable<SaveTarget> targets, DeviceData deviceData)
	{
		using (BinaryWriter binaryWriter = new BinaryWriter(dataStream))
		{
			writer = binaryWriter;
			WriteData(kVersionIdentifier);
			WriteData(1f);
			if (deviceData != null)
			{
				WriteData(deviceData.Count);
				foreach (DeviceDataEntry value in deviceData.Values)
				{
					WriteData(value);
				}
			}
			else
			{
				WriteData(0);
			}
			foreach (KeyValuePair<string, SaveHandler> saveHandler in saveHandlers)
			{
				MemoryStream memoryStream = new MemoryStream();
				using (BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream))
				{
					writer = binaryWriter2;
					try
					{
						WriteData(saveHandler.Key);
						WriteData(saveHandler.Value.Version);
						long position = memoryStream.Position;
						WriteData(0L);
						saveHandler.Value.Save(this, targets);
						long length = memoryStream.Length;
						memoryStream.Seek(position, SeekOrigin.Begin);
						WriteData(length);
						binaryWriter.Write(memoryStream.ToArray());
					}
					catch (Exception)
					{
					}
				}
			}
			writer = binaryWriter;
			WriteData(kEndIdentifier);
			WriteData(0);
			WriteData(0);
			WriteData(0);
			WriteData(0);
		}
		writer = null;
		return true;
	}

	protected override bool Read(SaveHeader dataHeader, Stream dataStream, SaveTarget target, DeviceData deviceData)
	{
		if (dataStream == null)
		{
			return false;
		}
		using (reader = new BinaryReader(dataStream))
		{
			while (reader.BaseStream.CanRead)
			{
				string text = reader.ReadString();
				if (text.Equals(kVersionIdentifier))
				{
					VersionNumber = reader.ReadSingle();
					if (VersionNumber != 1f)
					{
					}
					if (!dataHeader.UseDeviceData)
					{
						continue;
					}
					int num = ReadData_Int();
					for (int i = 0; i < num; i++)
					{
						Hashtable other = ReadData_Hashtable();
						if (deviceData != null)
						{
							DeviceDataEntry ifNewer = new DeviceDataEntry(other);
							deviceData.SetIfNewer(ifNewer);
						}
					}
					continue;
				}
				if (text.Equals(kEndIdentifier))
				{
					break;
				}
				if (saveHandlers.ContainsKey(text))
				{
					long position = dataStream.Position;
					float handlerVersion = reader.ReadSingle();
					long num2 = reader.ReadInt64();
					try
					{
						saveHandlers[text].Load(this, handlerVersion, target);
					}
					catch (Exception)
					{
						dataStream.Seek(position + num2, SeekOrigin.Begin);
					}
				}
			}
		}
		return true;
	}

	public void WriteData(bool value)
	{
		if (writer != null)
		{
			writer.Write(value);
		}
	}

	public void WriteData(char value)
	{
		if (writer != null)
		{
			writer.Write(value);
		}
	}

	public void WriteData(short value)
	{
		if (writer != null)
		{
			writer.Write(value);
		}
	}

	public void WriteData(int value)
	{
		if (writer != null)
		{
			writer.Write(value);
		}
	}

	public void WriteData(uint value)
	{
		if (writer != null)
		{
			writer.Write(value);
		}
	}

	public void WriteData(float value)
	{
		if (writer != null)
		{
			writer.Write(value);
		}
	}

	public void WriteData(double value)
	{
		if (writer != null)
		{
			writer.Write(value);
		}
	}

	public void WriteData(long value)
	{
		if (writer != null)
		{
			writer.Write(value);
		}
	}

	public void WriteData(string text)
	{
		if (writer != null)
		{
			writer.Write(text);
		}
	}

	public void WriteData(MemoryStream stream)
	{
		if (writer != null)
		{
			writer.Write(stream.ToArray());
		}
	}

	public void WriteData(DateTime? time)
	{
		if (time.HasValue)
		{
			WriteData(time.Value.ToBinary());
		}
		else
		{
			WriteData(0L);
		}
	}

	public void WriteData(TimeSpan time)
	{
		WriteData(time.TotalMilliseconds);
	}

	public void WriteData(Hashtable hashtable)
	{
		if (writer != null)
		{
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				binaryFormatter.Serialize(writer.BaseStream, hashtable);
			}
			catch (SerializationException)
			{
			}
		}
	}

	public void WriteData(byte[] data)
	{
		if (writer != null)
		{
			writer.Write(data.Length);
			writer.Write(data);
		}
	}

	public bool ReadData_Bool()
	{
		bool result = false;
		if (reader != null)
		{
			result = reader.ReadBoolean();
		}
		return result;
	}

	public char ReadData_Char()
	{
		char result = '\0';
		if (reader != null)
		{
			result = reader.ReadChar();
		}
		return result;
	}

	public short ReadData_Short()
	{
		short result = 0;
		if (reader != null)
		{
			result = reader.ReadInt16();
		}
		return result;
	}

	public int ReadData_Int()
	{
		int result = 0;
		if (reader != null)
		{
			result = reader.ReadInt32();
		}
		return result;
	}

	public uint ReadData_UInt()
	{
		uint result = 0u;
		if (reader != null)
		{
			result = reader.ReadUInt32();
		}
		return result;
	}

	public long ReadData_Long()
	{
		long result = 0L;
		if (reader != null)
		{
			result = reader.ReadInt64();
		}
		return result;
	}

	public float ReadData_Float()
	{
		float result = 0f;
		if (reader != null)
		{
			result = reader.ReadSingle();
		}
		return result;
	}

	public double ReadData_Double()
	{
		double result = 0.0;
		if (reader != null)
		{
			result = reader.ReadDouble();
		}
		return result;
	}

	public string ReadData_String()
	{
		string result = string.Empty;
		if (reader != null)
		{
			result = reader.ReadString();
		}
		return result;
	}

	public DateTime? ReadData_DateTime()
	{
		long num = ReadData_Long();
		if (num != 0L)
		{
			return DateTime.FromBinary(num);
		}
		return null;
	}

	public TimeSpan ReadData_TimeSpan()
	{
		return TimeSpan.FromMilliseconds(ReadData_Double());
	}

	public Hashtable ReadData_Hashtable()
	{
		Hashtable result = null;
		if (reader != null)
		{
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				result = (Hashtable)binaryFormatter.Deserialize(reader.BaseStream);
			}
			catch (SerializationException)
			{
			}
		}
		return result;
	}

	public byte[] ReadData_ByteArray()
	{
		if (reader != null)
		{
			int count = reader.ReadInt32();
			return reader.ReadBytes(count);
		}
		return null;
	}
}
