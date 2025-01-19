using System;

namespace Glu.Plugins.ASocial
{
	public class ReceivedPacketEventArgs : EventArgs
	{
		private Packet mReceivedPacket;

		public ReceivedPacketEventArgs(Packet p)
		{
			mReceivedPacket = p;
		}

		public Packet GetPacket()
		{
			return mReceivedPacket;
		}
	}
}
