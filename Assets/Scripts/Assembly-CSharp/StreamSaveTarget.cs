using System;
using System.IO;

public class StreamSaveTarget : SaveTarget
{
	public Stream stream;

	public Stream backupStream;

	public override void Save(byte[] data)
	{
		if (stream != null)
		{
			stream.Write(data, 0, data.Length);
			stream.Close();
		}
		if (UseBackup && backupStream != null)
		{
			backupStream.Write(data, 0, data.Length);
			backupStream.Close();
		}
	}

	public override void Load(bool loadBackup, Action<byte[]> onComplete)
	{
		if (onComplete != null)
		{
			Stream stream = ((!loadBackup) ? this.stream : backupStream);
			if (stream != null)
			{
				byte[] array = new byte[stream.Length];
				stream.Seek(0L, SeekOrigin.Begin);
				stream.Read(array, 0, (int)stream.Length);
				onComplete(array);
			}
			else
			{
				onComplete(null);
			}
		}
	}

	public override void Delete()
	{
		if (stream != null)
		{
			stream.SetLength(0L);
		}
		if (backupStream != null)
		{
			backupStream.SetLength(0L);
		}
	}
}
