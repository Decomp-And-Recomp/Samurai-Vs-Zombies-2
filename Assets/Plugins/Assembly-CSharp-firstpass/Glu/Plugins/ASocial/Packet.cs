using System;
using UnityEngine;

namespace Glu.Plugins.ASocial
{
	public class Packet
	{
		private string mParticipantID;

		private string mGameObjectName;

		private string mGameComponentName;

		private string mMethodName;

		private object[] mParameters;

		private float mTimeSent;

		private float mTimeReceived;

		private string mOwnerIdentifier;

		public static Action<object, byte[], bool> SendMessage;

		private Packet()
		{
		}

		private Packet(string playerID, string gameObjectName, string componentName, string methodName, string ownerID, object[] parameters)
		{
			mParticipantID = playerID;
			mGameObjectName = gameObjectName;
			mGameComponentName = componentName;
			mMethodName = methodName;
			mOwnerIdentifier = ownerID;
			mParameters = parameters;
		}

		private Packet(byte[] data)
		{
			ByteArraySerializableObject byteArraySerializableObject = data.Deserialize<ByteArraySerializableObject>();
			mParticipantID = byteArraySerializableObject.mPlayerID;
			mGameObjectName = byteArraySerializableObject.mGameObjectName;
			mGameComponentName = byteArraySerializableObject.mComponentName;
			mMethodName = byteArraySerializableObject.mMethodName;
			mOwnerIdentifier = byteArraySerializableObject.mOwnerIdentifier;
			mParameters = byteArraySerializableObject.mParameterList.ToArray();
			mTimeReceived = Time.realtimeSinceStartup;
			mTimeSent = byteArraySerializableObject.mTimeSent;
		}

		public static Packet BuildPacketToWriteForNetworkSyncObject(string playerID, string gameObjectName, string componentName, string methodName, string ownerID, params object[] parameters)
		{
			if (string.IsNullOrEmpty(playerID))
			{
				Debug.LogError("playerID is not set! WTF?");
				return null;
			}
			return new Packet(playerID, gameObjectName, componentName, methodName, ownerID, parameters);
		}

		public static Packet BuildPacketToWrite(string playerID, string gameObjectName, string componentName, string methodName, params object[] parameters)
		{
			if (string.IsNullOrEmpty(playerID))
			{
				Debug.LogError("playerID is not set! WTF?");
				return null;
			}
			return new Packet(playerID, gameObjectName, componentName, methodName, string.Empty, parameters);
		}

		public static Packet BuildPacketToRead(byte[] bytes)
		{
			if (bytes == null || bytes.Length == 0)
			{
				Debug.LogError("bytes is null or length is 0! WTF?");
				return null;
			}
			return new Packet(bytes);
		}

		public void SendPacket(bool reliable)
		{
			if (SendMessage == null)
			{
				Debug.LogError("SendPacket delegate not set! WTF? Will not be sending any messages at this time.");
				return;
			}
			ByteArraySerializableObject m = new ByteArraySerializableObject(mParticipantID, Time.realtimeSinceStartup, Time.realtimeSinceStartup, mGameObjectName, mGameComponentName, mMethodName, mOwnerIdentifier, mParameters);
			SendMessage(this, m.Serialize(), reliable);
		}

		public string GetParticipantsID()
		{
			return mParticipantID;
		}

		public string GetGameObjectName()
		{
			return mGameObjectName;
		}

		public string GetComponentName()
		{
			return mGameComponentName;
		}

		public string GetMethodName()
		{
			return mMethodName;
		}

		public object[] GetParameters()
		{
			return mParameters;
		}

		public float TimeSent()
		{
			return mTimeSent;
		}

		public float TimeReceived()
		{
			return mTimeReceived;
		}

		public string NetworkOwnerIdentifier()
		{
			return mOwnerIdentifier;
		}
	}
}
