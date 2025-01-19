using System;

namespace Glu.Plugins.ASocial
{
	public class MessageArgs : EventArgs
	{
		public enum MessageType
		{
			Non_Reliable = 0,
			Reliable = 1
		}

		private MessageType status { get; set; }

		private byte[] message { get; set; }

		public MessageArgs(MessageType _status, byte[] _message)
		{
			status = _status;
			message = _message;
		}

		public MessageType GetStatus()
		{
			return status;
		}

		public byte[] GetMessage()
		{
			return message;
		}
	}
}
