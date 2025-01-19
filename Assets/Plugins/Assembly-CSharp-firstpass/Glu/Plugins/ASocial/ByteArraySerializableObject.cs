using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Glu.Plugins.ASocial
{
	[Serializable]
	internal class ByteArraySerializableObject : ISerializable
	{
		public string mPlayerID;

		public float mTimeSent;

		public float mTimeReceived;

		public string mGameObjectName;

		public string mMethodName;

		public string mComponentName;

		public string mOwnerIdentifier;

		public List<object> mParameterList;

		public ByteArraySerializableObject(string playerID, float timeSent, float timeReceived, string gameObjectName, string componentName, string methodName, string ownerID, object[] data)
		{
			mPlayerID = playerID;
			mTimeSent = timeSent;
			mTimeReceived = timeReceived;
			mGameObjectName = gameObjectName;
			mMethodName = methodName;
			mComponentName = componentName;
			mOwnerIdentifier = ownerID;
			mParameterList = new List<object>(data);
		}

		protected ByteArraySerializableObject(SerializationInfo info, StreamingContext context)
		{
			mPlayerID = (string)info.GetValue("mPlayerID", typeof(string));
			mGameObjectName = (string)info.GetValue("mGameObjectName", typeof(string));
			mMethodName = (string)info.GetValue("mMethodName", typeof(string));
			mComponentName = (string)info.GetValue("mComponentName", typeof(string));
			mTimeSent = (float)info.GetValue("mTimeSent", typeof(float));
			mTimeReceived = (float)info.GetValue("mTimeReceived", typeof(float));
			mOwnerIdentifier = (string)info.GetValue("mOwnerIdentifier", typeof(string));
			object[] array = (object[])info.GetValue("params", typeof(object[]));
			for (int i = 0; i < array.Length; i++)
			{
				object obj = array[i];
				if (obj != null)
				{
					try
					{
						if (obj.GetType() == typeof(GluVector2))
						{
							Vector2 vector = new Vector2(((GluVector2)obj).x, ((GluVector2)obj).y);
							array[i] = vector;
						}
						else if (obj.GetType() == typeof(GluVector3))
						{
							Vector3 vector2 = new Vector3(((GluVector3)obj).x, ((GluVector3)obj).y, ((GluVector3)obj).z);
							array[i] = vector2;
						}
						else if (obj.GetType() == typeof(GluVector4))
						{
							Vector4 vector3 = new Vector4(((GluVector4)obj).w, ((GluVector4)obj).x, ((GluVector4)obj).y, ((GluVector4)obj).z);
							array[i] = vector3;
						}
						else if (obj.GetType() == typeof(GluQuaternion))
						{
							Quaternion quaternion = new Quaternion(((GluQuaternion)obj).x, ((GluQuaternion)obj).y, ((GluQuaternion)obj).z, ((GluQuaternion)obj).w);
							array[i] = quaternion;
						}
						else if (obj.GetType() == typeof(string) && (string)obj == "NULL")
						{
							array[i] = null;
						}
					}
					catch (Exception ex)
					{
						Debug.LogError(ex.ToString());
					}
				}
				else
				{
					Debug.LogError("Parameter #" + (i + 1) + " is NULL, for method = " + mMethodName + " ,ComponentName " + mComponentName + " gameObject = " + mGameObjectName);
				}
			}
			mParameterList = new List<object>(array);
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("mPlayerID", mPlayerID);
			info.AddValue("mGameObjectName", mGameObjectName);
			info.AddValue("mMethodName", mMethodName);
			info.AddValue("mComponentName", mComponentName);
			info.AddValue("mTimeSent", mTimeSent);
			info.AddValue("mTimeReceived", mTimeReceived);
			info.AddValue("mOwnerIdentifier", mOwnerIdentifier);
			object[] array = mParameterList.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				object obj = array[i];
				if (obj != null)
				{
					try
					{
						if (obj.GetType() == typeof(Vector2))
						{
							GluVector2 gluVector = new GluVector2((Vector2)obj);
							array[i] = gluVector;
						}
						if (obj.GetType() == typeof(Vector3))
						{
							GluVector3 gluVector2 = new GluVector3((Vector3)obj);
							array[i] = gluVector2;
						}
						if (obj.GetType() == typeof(Vector4))
						{
							GluVector4 gluVector3 = new GluVector4((Vector4)obj);
							array[i] = gluVector3;
						}
						if (obj.GetType() == typeof(Quaternion))
						{
							GluQuaternion gluQuaternion = new GluQuaternion((Quaternion)obj);
							array[i] = gluQuaternion;
						}
					}
					catch (Exception ex)
					{
						Debug.LogError(ex.ToString());
					}
				}
				else
				{
					array[i] = "NULL";
				}
			}
			info.AddValue("params", array);
		}
	}
}
