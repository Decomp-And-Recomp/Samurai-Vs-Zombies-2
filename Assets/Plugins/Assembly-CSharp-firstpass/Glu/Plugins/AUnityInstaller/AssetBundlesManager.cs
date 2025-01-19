using UnityEngine;

namespace Glu.Plugins.AUnityInstaller
{
	public class AssetBundlesManager
	{
		private static AndroidJavaClass unpacker;

		public static void Fixup(string uri, int version)
		{
			if (unpacker == null)
			{
				unpacker = new AndroidJavaClass("com.glu.plugins.assetbundles.UnityUnpacker");
			}
			unpacker.CallStatic("fixup", uri, version);
		}
	}
}
