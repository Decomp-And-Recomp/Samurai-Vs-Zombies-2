using System;
using System.Collections.Generic;
using UnityEngine;

namespace Glu.Plugins.ASocial
{
	internal class ASocial
	{
		private static bool isInitialized;

		private static AndroidJavaClass _asocial;

		public static AndroidJavaClass asocial
		{
			get
			{
				if (_asocial == null)
				{
					_asocial = new AndroidJavaClass("com.glu.plugins.ASocial");
				}
				return _asocial;
			}
		}

		public static void Init()
		{
			if (!isInitialized)
			{
				isInitialized = true;
				ASocial_Init(Debug.isDebugBuild);
			}
		}

		private static void ASocial_Init(bool debug)
		{
			asocial.CallStatic("Init", debug);
		}

		public static AndroidJavaObject DictionaryToHashMap(Dictionary<Facebook.FeedType, string> dict)
		{
			JniHelper.PushLocalFrame();
			AndroidJavaObject androidJavaObject = null;
			try
			{
				androidJavaObject = new AndroidJavaObject("java.util.HashMap");
				IntPtr methodID = AndroidJNI.GetMethodID(androidJavaObject.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
				object[] array = new object[2];
				foreach (KeyValuePair<Facebook.FeedType, string> item in dict)
				{
					JniHelper.PushLocalFrame();
					try
					{
						using (AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("java.lang.String", item.Key.ToString().ToLower()))
						{
							using (AndroidJavaObject androidJavaObject3 = new AndroidJavaObject("java.lang.String", item.Value))
							{
								array[0] = androidJavaObject2;
								array[1] = androidJavaObject3;
								AndroidJNI.CallObjectMethod(androidJavaObject.GetRawObject(), methodID, AndroidJNIHelper.CreateJNIArgArray(array));
							}
						}
					}
					finally
					{
						JniHelper.PopLocalFrame();
					}
				}
				return androidJavaObject;
			}
			catch (Exception)
			{
				if (androidJavaObject != null)
				{
					androidJavaObject.Dispose();
				}
				throw;
			}
			finally
			{
				JniHelper.PopLocalFrame();
			}
		}

		public static AndroidJavaObject DictionaryToHashMap(Dictionary<Facebook.AppRequestType, string> dict)
		{
			JniHelper.PushLocalFrame();
			AndroidJavaObject androidJavaObject = null;
			try
			{
				androidJavaObject = new AndroidJavaObject("java.util.HashMap");
				IntPtr methodID = AndroidJNI.GetMethodID(androidJavaObject.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
				object[] array = new object[2];
				foreach (KeyValuePair<Facebook.AppRequestType, string> item in dict)
				{
					JniHelper.PushLocalFrame();
					try
					{
						using (AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("java.lang.String", item.Key.ToString().ToLower()))
						{
							using (AndroidJavaObject androidJavaObject3 = new AndroidJavaObject("java.lang.String", item.Value))
							{
								array[0] = androidJavaObject2;
								array[1] = androidJavaObject3;
								AndroidJNI.CallObjectMethod(androidJavaObject.GetRawObject(), methodID, AndroidJNIHelper.CreateJNIArgArray(array));
							}
						}
					}
					finally
					{
						JniHelper.PopLocalFrame();
					}
				}
				return androidJavaObject;
			}
			catch (Exception)
			{
				if (androidJavaObject != null)
				{
					androidJavaObject.Dispose();
				}
				throw;
			}
			finally
			{
				JniHelper.PopLocalFrame();
			}
		}
	}
}
