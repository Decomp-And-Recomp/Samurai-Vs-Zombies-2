using System.Collections.Generic;

namespace Gamespy.Matchmaking
{
	public class GameHost
	{
		private const byte PRIVATE_IP_FLAG = 2;

		private const byte ICMP_IP_FLAG = 8;

		private const byte NONSTANDARD_PORT_FLAG = 16;

		private const byte NONSTANDARD_PRIVATE_PORT_FLAG = 32;

		private const byte HAS_KEYS_FLAG = 64;

		public byte flags;

		public string ip = string.Empty;

		public int port;

		public string privateIp = string.Empty;

		public int privatePort;

		public string icmpIp = string.Empty;

		public List<Key> keys = new List<Key>();

		public int serverSize;

		public GameHost(byte serverFlags)
		{
			flags = serverFlags;
			serverSize = 5;
			if ((flags & 0x10) == 0)
			{
				port = -1;
				serverSize += 2;
			}
			if ((flags & 2) == 0)
			{
				privateIp = "-1";
				serverSize += 4;
			}
			if ((flags & 0x20) == 0)
			{
				privatePort = -1;
				serverSize += 2;
			}
			if ((flags & 8) == 0)
			{
				icmpIp = "-1";
				serverSize += 4;
			}
		}
	}
}
