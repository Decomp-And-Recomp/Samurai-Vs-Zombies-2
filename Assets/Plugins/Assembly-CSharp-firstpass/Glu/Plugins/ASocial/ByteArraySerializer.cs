using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Glu.Plugins.ASocial
{
	internal static class ByteArraySerializer
	{
		public static byte[] Serialize<T>(this T m)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				new BinaryFormatter().Serialize(memoryStream, m);
				return memoryStream.ToArray();
			}
		}

		public static T Deserialize<T>(this byte[] byteArray)
		{
			using (MemoryStream serializationStream = new MemoryStream(byteArray))
			{
				return (T)new BinaryFormatter().Deserialize(serializationStream);
			}
		}
	}
}
