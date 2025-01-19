using System;
using UnityEngine;

namespace Glu.Plugins.AUnityInstaller
{
	public static class UnityLauncherActivity
	{
		private static bool initialized;

		private static IntPtr addListener;

		private static IntPtr addStaticListener;

		private static IntPtr removeListener;

		private static IntPtr addActivityLifetimeListener;

		public static void AddStaticListener(string className)
		{
			if (className == null)
			{
				throw new ArgumentNullException("className");
			}
			AddStaticListenerImpl(className);
		}

		public static void AddListener(string name, IntPtr listener)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (listener == IntPtr.Zero)
			{
				throw new ArgumentException("listener == IntPtr.Zero", "listener");
			}
			AddListenerImpl(name, listener);
		}

		public static void RemoveListener(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			RemoveListenerImpl(name);
		}

		[Obsolete("Use UnityLauncherActivity.AddListener()")]
		public static void AddActivityListener(ActivityListener type, string name, IntPtr listener)
		{
			if (type != ActivityListener.Lifetime)
			{
				throw new NotSupportedException(string.Format("Type {0} is not supported", type));
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (listener == IntPtr.Zero)
			{
				throw new ArgumentException("listener == IntPtr.Zero", "listener");
			}
			AddActivityListenerImpl(type, name, listener);
		}

		[Obsolete("Use UnityLauncherActivity.RemoveListener()")]
		public static void RemoveActivityListener(ActivityListener type, string name)
		{
			if (type != ActivityListener.Lifetime)
			{
				throw new NotSupportedException(string.Format("Type {0} is not supported", type));
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			RemoveListenerImpl(name);
		}

		private static void Initialize()
		{
			if (!initialized)
			{
				AndroidJNI.PushLocalFrame(0);
				try
				{
					IntPtr helperClass = GetHelperClass();
					addListener = AndroidJNI.GetStaticMethodID(helperClass, "addListener", "(Ljava/lang/String;Lcom/glu/plugins/AUnityInstaller/AndroidActivityListener;)V");
					removeListener = AndroidJNI.GetStaticMethodID(helperClass, "removeListener", "(Ljava/lang/String;)V");
					addStaticListener = AndroidJNI.GetStaticMethodID(helperClass, "addStaticListener", "(Ljava/lang/String;)V");
					addActivityLifetimeListener = AndroidJNI.GetStaticMethodID(helperClass, "addActivityLifetimeListener", "(Ljava/lang/String;Lcom/glu/plugins/AUnityInstaller/ActivityLifetimeListener;)V");
				}
				finally
				{
					AndroidJNI.PopLocalFrame(IntPtr.Zero);
				}
				initialized = true;
			}
		}

		private static IntPtr GetHelperClass()
		{
			return AndroidJNI.FindClass("com/glu/plugins/AUnityInstaller/UnityLauncherActivityHelper");
		}

		private static void AddStaticListenerImpl(string className)
		{
			Initialize();
			AndroidJNI.PushLocalFrame(0);
			try
			{
				IntPtr helperClass = GetHelperClass();
				jvalue[] args = new jvalue[1]
				{
					new jvalue
					{
						l = AndroidJNI.NewStringUTF(className)
					}
				};
				AndroidJNI.CallStaticVoidMethod(helperClass, addStaticListener, args);
			}
			finally
			{
				AndroidJNI.PopLocalFrame(IntPtr.Zero);
			}
		}

		private static void AddListenerImpl(string name, IntPtr listener)
		{
			Initialize();
			AndroidJNI.PushLocalFrame(0);
			try
			{
				IntPtr helperClass = GetHelperClass();
				jvalue[] args = new jvalue[2]
				{
					new jvalue
					{
						l = AndroidJNI.NewStringUTF(name)
					},
					new jvalue
					{
						l = listener
					}
				};
				AndroidJNI.CallStaticVoidMethod(helperClass, addListener, args);
			}
			finally
			{
				AndroidJNI.PopLocalFrame(IntPtr.Zero);
			}
		}

		private static void RemoveListenerImpl(string name)
		{
			Initialize();
			AndroidJNI.PushLocalFrame(0);
			try
			{
				IntPtr helperClass = GetHelperClass();
				jvalue[] args = new jvalue[1]
				{
					new jvalue
					{
						l = AndroidJNI.NewStringUTF(name)
					}
				};
				AndroidJNI.CallStaticVoidMethod(helperClass, removeListener, args);
			}
			finally
			{
				AndroidJNI.PopLocalFrame(IntPtr.Zero);
			}
		}

		private static void AddActivityListenerImpl(ActivityListener type, string name, IntPtr listener)
		{
			Initialize();
			AndroidJNI.PushLocalFrame(0);
			try
			{
				IntPtr helperClass = GetHelperClass();
				jvalue[] args = new jvalue[2]
				{
					new jvalue
					{
						l = AndroidJNI.NewStringUTF(name)
					},
					new jvalue
					{
						l = listener
					}
				};
				AndroidJNI.CallStaticVoidMethod(helperClass, addActivityLifetimeListener, args);
			}
			finally
			{
				AndroidJNI.PopLocalFrame(IntPtr.Zero);
			}
		}
	}
}
